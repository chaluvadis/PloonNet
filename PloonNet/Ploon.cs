namespace PloonNet;

/// <summary>
/// Main PLOON API for serialization and deserialization
/// </summary>
public static class Ploon
{
    /// <summary>
    /// Convert an object to PLOON format
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="options">Stringify options</param>
    /// <returns>PLOON formatted string</returns>
    public static string Stringify(object obj, StringifyOptions? options = null)
    {
        options ??= new StringifyOptions();
        var config = options.Config ?? (options.Format == PloonFormat.Compact ? PloonConfig.Compact : PloonConfig.Standard);

        // Avoid serialize->parse roundtrip by serializing directly to a JsonElement
        var jsonElement = JsonSerializer.SerializeToElement(obj);

        // Build schema
        var schemaBuilder = new SchemaBuilder(config);
        var schema = schemaBuilder.BuildSchema(jsonElement);
        var schemaString = schemaBuilder.GenerateSchemaString(schema);

        // Encode data
        var dataEncoder = new DataEncoder(config, schema);
        var records = dataEncoder.EncodeData(jsonElement);

        // Combine schema and data efficiently
        var sb = new StringBuilder(schemaString.Length + 64);
        sb.Append(schemaString);
        sb.Append(config.RecordSeparator);

        if (options.Format == PloonFormat.Standard)
        {
            sb.Append(config.RecordSeparator); // extra newline for readability
        }

        if (records != null)
        {
            bool first = true;
            foreach (var r in records)
            {
                if (!first) sb.Append(config.RecordSeparator);
                sb.Append(r);
                first = false;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Convert PLOON string to standard format (newlines)
    /// </summary>
    /// <param name="ploonString">PLOON string in compact format</param>
    /// <returns>PLOON string in standard format</returns>
    public static string Prettify(string ploonString)
        => string.IsNullOrWhiteSpace(ploonString) ? ploonString : ploonString.Replace(";", "\n");


    /// <summary>
    /// Convert PLOON string to compact format (semicolons)
    /// </summary>
    /// <param name="ploonString">PLOON string in standard format</param>
    /// <returns>PLOON string in compact format</returns>
    public static string Minify(string ploonString)
        => string.IsNullOrWhiteSpace(ploonString)
        ? ploonString
        : ploonString.Replace("\r\n", ";").Replace("\n", ";").Replace(";;", ";");


    /// <summary>
    /// Validate PLOON format string
    /// </summary>
    /// <param name="ploonString">PLOON string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(string ploonString) =>
        !string.IsNullOrWhiteSpace(ploonString)
        && ploonString.TrimStart().StartsWith('[')
        && ploonString.Contains("](");

    /// <summary>
    /// Convert JSON string to PLOON
    /// </summary>
    /// <param name="jsonString">JSON string</param>
    /// <param name="options">Stringify options</param>
    /// <returns>PLOON formatted string</returns>
    public static string FromJson(string jsonString, StringifyOptions? options = null)
    {
        var jsonElement = JsonDocument.Parse(jsonString).RootElement;
        return Stringify(jsonElement, options);
    }

    /// <summary>
    /// Parse PLOON string back to .NET object
    /// </summary>
    /// <param name="ploonString">PLOON formatted string</param>
    /// <param name="options">Parse options</param>
    /// <returns>Deserialized .NET object</returns>
    public static object? Parse(string ploonString, ParseOptions? options = null)
    {
        options ??= new ParseOptions();
        var config = options.Config ?? PloonConfig.Standard;
        if (string.IsNullOrWhiteSpace(ploonString))
            throw new ArgumentException("PLOON string cannot be null or empty", nameof(ploonString));

        // Enhanced validation
        ValidatePloonString(ploonString, config);

        // Parse the PLOON string
        var parser = new PloonParser(config, options.Strict);
        return parser.Parse(ploonString);
    }

    /// <summary>
    /// Convert an object to PLOON format asynchronously
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="options">Stringify options</param>
    /// <returns>PLOON formatted string</returns>
    public static Task<string> StringifyAsync(object obj, StringifyOptions? options = null)
        => Task.Run(() => Stringify(obj, options));

    /// <summary>
    /// Parse PLOON string back to .NET object asynchronously
    /// </summary>
    /// <param name="ploonString">PLOON formatted string</param>
    /// <param name="options">Parse options</param>
    /// <returns>Deserialized .NET object</returns>
    public static Task<object?> ParseAsync(string ploonString, ParseOptions? options = null)
        => Task.Run(() => Parse(ploonString, options));

    /// <summary>
    /// Enhanced validation with detailed error messages
    /// </summary>
    private static void ValidatePloonString(string ploonString, PloonConfig config)
    {
        if (string.IsNullOrWhiteSpace(ploonString))
            throw new ArgumentException("PLOON string cannot be null or empty", nameof(ploonString));

        var trimmed = ploonString.TrimStart();

        // Check for schema opening bracket
        if (!trimmed.StartsWith(config.SchemaOpen))
            throw new FormatException($"PLOON string must start with schema opening bracket '{config.SchemaOpen}'");

        // Check for schema closing and fields opening
        var schemaEndPattern = config.SchemaClose + config.FieldsOpen;
        if (!ploonString.Contains(schemaEndPattern))
            throw new FormatException($"PLOON string must contain schema closing bracket '{config.SchemaClose}' followed by fields opening parenthesis '{config.FieldsOpen}'");

        // Check for basic structure
        var fieldsCloseIndex = ploonString.IndexOf(config.FieldsClose);
        if (fieldsCloseIndex == -1)
            throw new FormatException($"PLOON string must contain fields closing parenthesis '{config.FieldsClose}'");

        // Ensure there's data after the schema
        var dataStart = fieldsCloseIndex + config.FieldsClose.Length;
        if (dataStart >= ploonString.Length)
            throw new FormatException("PLOON string must contain data records after the schema");
    }
}
