using System;
using System.Text.Json;

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

        // Convert object to JsonElement for processing
        var json = JsonSerializer.Serialize(obj);
        var jsonElement = JsonDocument.Parse(json).RootElement;

        // Build schema
        var schemaBuilder = new SchemaBuilder(config);
        var schema = schemaBuilder.BuildSchema(jsonElement);
        var schemaString = schemaBuilder.GenerateSchemaString(schema);

        // Encode data
        var dataEncoder = new DataEncoder(config, schema);
        var records = dataEncoder.EncodeData(jsonElement);

        // Combine schema and data
        var result = schemaString;
        result += config.RecordSeparator;
        
        if (options.Format == PloonFormat.Standard)
        {
            result += config.RecordSeparator; // Extra newline for readability
        }
        
        result += string.Join(config.RecordSeparator, records);

        return result;
    }

    /// <summary>
    /// Convert PLOON string to standard format (newlines)
    /// </summary>
    /// <param name="ploonString">PLOON string in compact format</param>
    /// <returns>PLOON string in standard format</returns>
    public static string Prettify(string ploonString)
    {
        if (string.IsNullOrEmpty(ploonString))
            return ploonString;

        // Replace semicolons with newlines
        return ploonString.Replace(";", "\n");
    }

    /// <summary>
    /// Convert PLOON string to compact format (semicolons)
    /// </summary>
    /// <param name="ploonString">PLOON string in standard format</param>
    /// <returns>PLOON string in compact format</returns>
    public static string Minify(string ploonString)
    {
        if (string.IsNullOrEmpty(ploonString))
            return ploonString;

        // Replace newlines with semicolons and remove extra whitespace
        var result = ploonString
            .Replace("\r\n", ";")
            .Replace("\n", ";")
            .Replace(";;", ";");

        return result;
    }

    /// <summary>
    /// Validate PLOON format string
    /// </summary>
    /// <param name="ploonString">PLOON string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(string ploonString)
    {
        if (string.IsNullOrEmpty(ploonString))
            return false;

        try
        {
            // Basic validation: check for schema brackets
            if (!ploonString.TrimStart().StartsWith("["))
                return false;

            if (!ploonString.Contains("]("))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

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
}
