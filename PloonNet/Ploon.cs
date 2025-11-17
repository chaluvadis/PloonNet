namespace Ploon.Net;

/// <summary>
/// Main API for serialization and deserialization
/// </summary>
public static class Ploon
{
    /// <summary>
    /// Convert an object to format
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="options">Stringify options</param>
    /// <returns>formatted string</returns>
    public static string Stringify(object obj, StringifyOptions? options = null)
    {
        options ??= new StringifyOptions();
        var config = options.Config ?? (options.Format == PloonFormat.Compact ? PloonConfig.Compact : PloonConfig.Standard);

        // Avoid serialize->parse roundtrip by serializing directly to a JsonElement
        var jsonElement = JsonSerializer.SerializeToElement(obj);

        var (schemaString, records) = GenerateSchemaAndRecords(jsonElement, config);

        return CombineSchemaAndRecords(schemaString, records, options, config, ct: null);
    }

    /// <summary>
    /// Convert string to standard format (newlines)
    /// </summary>
    /// <param name="ploonString">string in compact format</param>
    /// <returns>string in standard format</returns>
    public static string Prettify(string ploonString)
        => string.IsNullOrWhiteSpace(ploonString) ? ploonString : ploonString.Replace(";", "\n");

    /// <summary>
    /// Convert string to compact format (semicolons)
    /// </summary>
    /// <param name="ploonString">string in standard format</param>
    /// <returns>String in compact format</returns>
    public static string Minify(string ploonString)
        => string.IsNullOrWhiteSpace(ploonString)
        ? ploonString
        : ploonString.Replace("\r\n", ";").Replace("\n", ";").Replace(";;", ";");

    /// <summary>
    /// Validate format string
    /// </summary>
    /// <param name="ploonString">string to validate</param>
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
    /// <returns>formatted string</returns>
    public static string FromJson(string jsonString, StringifyOptions? options = null)
    {
        var jsonElement = JsonDocument.Parse(jsonString).RootElement;
        return Stringify(jsonElement, options);
    }

    /// <summary>
    /// Parse string back to .NET object
    /// </summary>
    /// <param name="ploonString">formatted string</param>
    /// <param name="options">Parse options</param>
    /// <returns>Deserialized .NET object</returns>
    public static object? Parse(string ploonString, ParseOptions? options = null)
    {
        options ??= new ParseOptions();
        var config = options.Config ?? PloonConfig.Standard;
        if (string.IsNullOrWhiteSpace(ploonString))
            throw new ArgumentException("String cannot be null or empty", nameof(ploonString));

        // Enhanced validation
        ValidatePloonString(ploonString, config);

        var parser = CreateParser(config, options.Strict);
        return parser.Parse(ploonString);
    }

    /// <summary>
    /// Convert JSON string to asynchronously
    /// </summary>
    /// <param name="jsonString">JSON string</param>
    /// <param name="options">Stringify options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>formatted string</returns>
    public static async ValueTask<string> FromJsonAsync(
        string jsonString,
        StringifyOptions? options = null,
        CancellationToken ct = default)
    {
        var jsonElement = await ParseJsonAsync(jsonString).ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
        return await StringifyAsync(jsonElement, options, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Convert an object to format asynchronously with enhanced performance
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="options">Stringify options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>formatted string</returns>
    public static async ValueTask<string> StringifyAsync(
        object obj,
        StringifyOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new StringifyOptions();
        var config = options.Config ?? (options.Format == PloonFormat.Compact ? PloonConfig.Compact : PloonConfig.Standard);

        ct.ThrowIfCancellationRequested();

        // Use async JSON serialization for better performance with large objects
        var jsonElement = await SerializeToElementAsync(obj).ConfigureAwait(false);

        // Yield control for large datasets to improve responsiveness
        await Task.Yield();
        ct.ThrowIfCancellationRequested();

        var (schemaString, records) = GenerateSchemaAndRecords(jsonElement, config);

        ct.ThrowIfCancellationRequested();

        return CombineSchemaAndRecords(schemaString, records, options, config, ct);
    }

    /// <summary>
    /// Parse string back to .NET object asynchronously with cancellation support
    /// </summary>
    /// <param name="ploonString">formatted string</param>
    /// <param name="options">Parse options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Deserialized .NET object</returns>
    public static async ValueTask<object?> ParseAsync(
        string ploonString,
        ParseOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new ParseOptions();
        var config = options.Config ?? PloonConfig.Standard;

        if (string.IsNullOrWhiteSpace(ploonString))
            throw new ArgumentException("String cannot be null or empty", nameof(ploonString));

        ct.ThrowIfCancellationRequested();

        // Enhanced validation
        ValidatePloonString(ploonString, config);

        ct.ThrowIfCancellationRequested();

        var parser = CreateParser(config, options.Strict);

        // Allow cancellation during long parsing operations
        await Task.Yield();
        ct.ThrowIfCancellationRequested();

        return parser.Parse(ploonString);
    }

    /// <summary>
    /// Enhanced validation with detailed error messages
    /// </summary>
    private static void ValidatePloonString(string ploonString, PloonConfig config)
    {
        if (string.IsNullOrWhiteSpace(ploonString))
            throw new ArgumentException("string cannot be null or empty", nameof(ploonString));

        var trimmed = ploonString.TrimStart();

        // Check for schema opening bracket
        if (!trimmed.StartsWith(config.SchemaOpen))
            throw new FormatException($"string must start with schema opening bracket '{config.SchemaOpen}'");

        // Check for schema closing and fields opening
        var schemaEndPattern = config.SchemaClose + config.FieldsOpen;
        if (!ploonString.Contains(schemaEndPattern))
            throw new FormatException($"string must contain schema closing bracket '{config.SchemaClose}' followed by fields opening parenthesis '{config.FieldsOpen}'");

        // Check for basic structure
        var fieldsCloseIndex = ploonString.IndexOf(config.FieldsClose);
        if (fieldsCloseIndex == -1)
            throw new FormatException($"string must contain fields closing parenthesis '{config.FieldsClose}'");

        // Ensure there's data after the schema
        var dataStart = fieldsCloseIndex + config.FieldsClose.Length;
        if (dataStart >= ploonString.Length)
            throw new FormatException("string must contain data records after the schema");
    }

    /// <summary>
    /// Serialize object to JSON element asynchronously
    /// </summary>
    public static async ValueTask<JsonElement> SerializeToElementAsync(object obj, JsonSerializerOptions? options = null)
    {
        if (obj is JsonElement element)
            return element;

        // Use JsonSerializer to produce bytes then parse asynchronously from a stream
        var jsonString = JsonSerializer.Serialize(obj, options);
        using var document = await JsonDocument.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(jsonString))).ConfigureAwait(false);
        return document.RootElement;
    }

    /// <summary>
    /// Parse JSON string to JsonElement asynchronously
    /// </summary>
    public static async ValueTask<JsonElement> ParseJsonAsync(string jsonString)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
        return document.RootElement;
    }

    /// <summary>
    /// Process large datasets in chunks asynchronously
    /// </summary>
    public static async IAsyncEnumerable<T> ProcessInChunksAsync<T>(
        IEnumerable<T> items,
        int chunkSize = 1000,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var list = items.ToList();
        for (int i = 0; i < list.Count; i += chunkSize)
        {
            ct.ThrowIfCancellationRequested();

            // Yield control to allow other async operations
            if (i > 0)
                await Task.Yield();

            var chunk = list.Skip(i).Take(chunkSize);
            foreach (var item in chunk)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Create a parser instance (extracted to reduce duplication)
    /// </summary>
    private static PloonParser CreateParser(PloonConfig config, bool strict) => new(config, strict);

    /// <summary>
    /// Generate schema string and data records for a JsonElement.
    /// Extracted from Stringify/StringifyAsync to remove duplication.
    /// </summary>
    private static (string schemaString, List<string>? records) GenerateSchemaAndRecords(
        JsonElement jsonElement,
        PloonConfig config)
    {
        // Build schema
        var schemaBuilder = new SchemaBuilder(config);
        var schema = schemaBuilder.BuildSchema(jsonElement);
        var schemaString = schemaBuilder.GenerateSchemaString(schema);

        // Encode data
        var dataEncoder = new DataEncoder(config, schema);
        var recordsEnumerable = dataEncoder.EncodeData(jsonElement);

        List<string>? records = recordsEnumerable != null ? recordsEnumerable.ToList() : null;
        return (schemaString, records);
    }

    /// <summary>
    /// Combine schema and records into the final string.
    /// Shared code used by both sync and async flows.
    /// If a ct is provided, it will be honored between record appends.
    /// </summary>
    private static string CombineSchemaAndRecords(
        string schemaString,
        IEnumerable<string>? records,
        StringifyOptions options,
        PloonConfig config,
        CancellationToken? ct)
    {
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
                if (ct.HasValue)
                    ct.Value.ThrowIfCancellationRequested();

                if (!first) sb.Append(config.RecordSeparator);
                sb.Append(r);
                first = false;
            }
        }

        return sb.ToString();
    }
}