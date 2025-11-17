using System.Collections.Generic;
using System.Text.Json;

namespace PloonNet;

/// <summary>
/// Type of a schema field
/// </summary>
internal enum FieldType
{
    Primitive,
    Array,
    Object
}

/// <summary>
/// Represents a field in the PLOON schema
/// </summary>
internal class SchemaField
{
    public string Name { get; set; } = string.Empty;
    public FieldType Type { get; set; }
    public List<SchemaField>? Fields { get; set; }  // For nested objects
    public int? ArrayCount { get; set; }  // For arrays
    public bool IsOptional { get; set; }
}

/// <summary>
/// Represents the root schema node
/// </summary>
internal class SchemaNode
{
    public string RootName { get; set; } = string.Empty;
    public int? Count { get; set; }
    public List<SchemaField> Fields { get; set; } = new();
}

/// <summary>
/// Represents a data record with path information
/// </summary>
internal class DataRecord
{
    public string Path { get; set; } = string.Empty;
    public List<object?> Values { get; set; } = new();
}
