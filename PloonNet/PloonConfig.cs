namespace Ploon.Net;

/// <summary>
/// Configuration for PLOON serialization and deserialization
/// </summary>
public class PloonConfig
{
    /// <summary>
    /// Field delimiter (separates values in a record)
    /// </summary>
    public string FieldDelimiter { get; set; } = "|";

    /// <summary>
    /// Path separator (separates depth and index in paths)
    /// </summary>
    public string PathSeparator { get; set; } = ":";

    /// <summary>
    /// Array size marker (indicates array length in schema)
    /// </summary>
    public string ArraySizeMarker { get; set; } = "#";

    /// <summary>
    /// Record separator (separates records)
    /// </summary>
    public string RecordSeparator { get; set; } = "\n";

    /// <summary>
    /// Escape character (escapes special characters)
    /// </summary>
    public string EscapeChar { get; set; } = "\\";

    /// <summary>
    /// Schema opening bracket
    /// </summary>
    public string SchemaOpen { get; set; } = "[";

    /// <summary>
    /// Schema closing bracket
    /// </summary>
    public string SchemaClose { get; set; } = "]";

    /// <summary>
    /// Fields opening parenthesis
    /// </summary>
    public string FieldsOpen { get; set; } = "(";

    /// <summary>
    /// Fields closing parenthesis
    /// </summary>
    public string FieldsClose { get; set; } = ")";

    /// <summary>
    /// Nested object opening brace
    /// </summary>
    public string NestedObjectOpen { get; set; } = "{";

    /// <summary>
    /// Nested object closing brace
    /// </summary>
    public string NestedObjectClose { get; set; } = "}";

    /// <summary>
    /// Schema field separator (separates field names in schema)
    /// </summary>
    public string SchemaFieldSeparator { get; set; } = ",";

    /// <summary>
    /// Standard format configuration (human-readable with newlines)
    /// </summary>
    public static PloonConfig Standard => new()
    {
        RecordSeparator = "\n"
    };

    /// <summary>
    /// Compact format configuration (machine-optimized with semicolons)
    /// </summary>
    public static PloonConfig Compact => new()
    {
        RecordSeparator = ";"
    };

    /// <summary>
    /// Clone this configuration
    /// </summary>
    public PloonConfig Clone() => new()
    {
        FieldDelimiter = FieldDelimiter,
        PathSeparator = PathSeparator,
        ArraySizeMarker = ArraySizeMarker,
        RecordSeparator = RecordSeparator,
        EscapeChar = EscapeChar,
        SchemaOpen = SchemaOpen,
        SchemaClose = SchemaClose,
        FieldsOpen = FieldsOpen,
        FieldsClose = FieldsClose,
        NestedObjectOpen = NestedObjectOpen,
        NestedObjectClose = NestedObjectClose,
        SchemaFieldSeparator = SchemaFieldSeparator
    };
}
