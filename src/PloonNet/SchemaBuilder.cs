using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PloonNet;

/// <summary>
/// Builds PLOON schema from .NET objects
/// </summary>
internal class SchemaBuilder
{
    private readonly PloonConfig _config;

    public SchemaBuilder(PloonConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Build schema from a JSON element
    /// </summary>
    public SchemaNode BuildSchema(JsonElement element)
    {
        var schema = new SchemaNode();

        if (element.ValueKind == JsonValueKind.Object)
        {
            // Root is an object
            var firstProperty = element.EnumerateObject().FirstOrDefault();
            if (firstProperty.Value.ValueKind == JsonValueKind.Array)
            {
                schema.RootName = firstProperty.Name;
                schema.Count = firstProperty.Value.GetArrayLength();
                schema.Fields = AnalyzeArray(firstProperty.Value);
            }
            else
            {
                // Single object root
                schema.RootName = "root";
                schema.Fields = AnalyzeObject(element);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            schema.RootName = "root";
            schema.Count = element.GetArrayLength();
            schema.Fields = AnalyzeArray(element);
        }

        return schema;
    }

    /// <summary>
    /// Analyze an array to determine its field structure
    /// </summary>
    private List<SchemaField> AnalyzeArray(JsonElement arrayElement)
    {
        var fieldsMap = new Dictionary<string, SchemaField>();
        int totalItems = arrayElement.GetArrayLength();

        foreach (var item in arrayElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in item.EnumerateObject())
                {
                    if (!fieldsMap.ContainsKey(prop.Name))
                    {
                        fieldsMap[prop.Name] = AnalyzeProperty(prop.Name, prop.Value);
                        fieldsMap[prop.Name].IsOptional = false;
                    }
                }
            }
        }

        // Determine which fields are optional
        foreach (var item in arrayElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object)
            {
                var presentFields = new HashSet<string>(item.EnumerateObject().Select(p => p.Name));
                foreach (var fieldName in fieldsMap.Keys)
                {
                    if (!presentFields.Contains(fieldName))
                    {
                        fieldsMap[fieldName].IsOptional = true;
                    }
                }
            }
        }

        return fieldsMap.Values.ToList();
    }

    /// <summary>
    /// Analyze an object to determine its field structure
    /// </summary>
    private List<SchemaField> AnalyzeObject(JsonElement objectElement)
    {
        var fields = new List<SchemaField>();

        foreach (var prop in objectElement.EnumerateObject())
        {
            fields.Add(AnalyzeProperty(prop.Name, prop.Value));
        }

        return fields;
    }

    /// <summary>
    /// Analyze a single property
    /// </summary>
    private SchemaField AnalyzeProperty(string name, JsonElement value)
    {
        var field = new SchemaField { Name = name };

        switch (value.ValueKind)
        {
            case JsonValueKind.Array:
                field.Type = FieldType.Array;
                field.ArrayCount = value.GetArrayLength();
                if (value.GetArrayLength() > 0)
                {
                    var firstItem = value.EnumerateArray().First();
                    if (firstItem.ValueKind == JsonValueKind.Object)
                    {
                        field.Fields = AnalyzeArray(value);
                    }
                }
                break;

            case JsonValueKind.Object:
                field.Type = FieldType.Object;
                field.Fields = AnalyzeObject(value);
                break;

            default:
                field.Type = FieldType.Primitive;
                break;
        }

        return field;
    }

    /// <summary>
    /// Generate schema string from SchemaNode
    /// </summary>
    public string GenerateSchemaString(SchemaNode schema)
    {
        var result = _config.SchemaOpen;
        
        result += schema.RootName;
        
        if (schema.Count.HasValue)
        {
            result += _config.ArraySizeMarker + schema.Count.Value;
        }
        
        result += _config.SchemaClose;
        result += _config.FieldsOpen;
        result += GenerateFieldsString(schema.Fields);
        result += _config.FieldsClose;

        return result;
    }

    /// <summary>
    /// Generate fields string for schema
    /// </summary>
    private string GenerateFieldsString(List<SchemaField> fields)
    {
        var fieldStrings = new List<string>();

        foreach (var field in fields)
        {
            var fieldStr = field.Name;

            switch (field.Type)
            {
                case FieldType.Array:
                    fieldStr += _config.ArraySizeMarker;
                    if (field.Fields != null && field.Fields.Any())
                    {
                        fieldStr += _config.FieldsOpen;
                        fieldStr += GenerateFieldsString(field.Fields);
                        fieldStr += _config.FieldsClose;
                    }
                    break;

                case FieldType.Object:
                    if (field.Fields != null && field.Fields.Any())
                    {
                        fieldStr += _config.NestedObjectOpen;
                        fieldStr += GenerateFieldsString(field.Fields);
                        fieldStr += _config.NestedObjectClose;
                    }
                    break;

                case FieldType.Primitive:
                    // No additional syntax
                    break;
            }

            fieldStrings.Add(fieldStr);
        }

        return string.Join(_config.SchemaFieldSeparator, fieldStrings);
    }
}
