namespace PloonNet;

/// <summary>
/// Encodes data into PLOON format
/// </summary>
internal class DataEncoder(PloonConfig config, SchemaNode schema)
{
    private readonly PloonConfig _config = config;
    private readonly SchemaNode _schema = schema;

    /// <summary>
    /// Encode JSON element to PLOON data records
    /// </summary>
    public List<string> EncodeData(JsonElement element)
    {
        var records = new List<string>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            var firstProperty = element.EnumerateObject().FirstOrDefault();
            if (firstProperty.Value.ValueKind == JsonValueKind.Array)
            {
                EncodeArray(firstProperty.Value, _schema.Fields, 1, records);
            }
            else
            {
                EncodeObject(element, _schema.Fields, 1, records);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            EncodeArray(element, _schema.Fields, 1, records);
        }

        return records;
    }

    /// <summary>
    /// Encode an array
    /// </summary>
    private void EncodeArray(JsonElement arrayElement, List<SchemaField> fields, int depth, List<string> records)
    {
        int index = 1;
        foreach (var item in arrayElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object)
            {
                EncodeObjectItem(item, fields, depth, index, records);
            }
            index++;
        }
    }

    /// <summary>
    /// Encode a single object item in an array
    /// </summary>
    private void EncodeObjectItem(JsonElement item, List<SchemaField> fields, int depth, int index, List<string> records)
    {
        var primitiveFields = fields.Where(f => f.Type == FieldType.Primitive).ToList();

        if (primitiveFields.Count != 0)
        {
            var values = new List<string>();
            var path = $"{depth}{_config.PathSeparator}{index}";

            foreach (var field in primitiveFields)
            {
                if (item.TryGetProperty(field.Name, out var propValue))
                {
                    values.Add(FormatValue(propValue));
                }
                else if (field.IsOptional)
                {
                    values.Add(string.Empty);
                }
            }

            if (values.Count != 0)
            {
                records.Add(path + _config.FieldDelimiter + string.Join(_config.FieldDelimiter, values));
            }
        }

        // Handle nested objects
        foreach (var field in fields.Where(f => f.Type == FieldType.Object))
        {
            if (item.TryGetProperty(field.Name, out var nestedObj) && nestedObj.ValueKind == JsonValueKind.Object)
            {
                EncodeNestedObject(nestedObj, field.Fields ?? new List<SchemaField>(), depth + 1, records);
            }
        }

        // Handle nested arrays
        foreach (var field in fields.Where(f => f.Type == FieldType.Array))
        {
            if (item.TryGetProperty(field.Name, out var nestedArray) && nestedArray.ValueKind == JsonValueKind.Array)
            {
                EncodeArray(nestedArray, field.Fields ?? new List<SchemaField>(), depth + 1, records);
            }
        }
    }

    /// <summary>
    /// Encode a nested object (uses depth only, no index)
    /// </summary>
    private void EncodeNestedObject(JsonElement obj, List<SchemaField> fields, int depth, List<string> records)
    {
        var primitiveFields = fields.Where(f => f.Type == FieldType.Primitive).ToList();

        if (primitiveFields.Count != 0)
        {
            var values = new List<string>();
            var path = $"{depth} "; // Object path: depth + space

            foreach (var field in primitiveFields)
            {
                if (obj.TryGetProperty(field.Name, out var propValue))
                {
                    values.Add(FormatValue(propValue));
                }
                else if (field.IsOptional)
                {
                    values.Add(string.Empty);
                }
            }

            if (values.Count != 0)
            {
                records.Add(path + _config.FieldDelimiter + string.Join(_config.FieldDelimiter, values));
            }
        }

        // Handle nested objects within this object
        foreach (var field in fields.Where(f => f.Type == FieldType.Object))
        {
            if (obj.TryGetProperty(field.Name, out var nestedObj) && nestedObj.ValueKind == JsonValueKind.Object)
            {
                EncodeNestedObject(nestedObj, field.Fields ?? new List<SchemaField>(), depth + 1, records);
            }
        }

        // Handle arrays within this object
        foreach (var field in fields.Where(f => f.Type == FieldType.Array))
        {
            if (obj.TryGetProperty(field.Name, out var nestedArray) && nestedArray.ValueKind == JsonValueKind.Array)
            {
                EncodeArray(nestedArray, field.Fields ?? new List<SchemaField>(), depth + 1, records);
            }
        }
    }

    /// <summary>
    /// Encode a root-level object (non-array)
    /// </summary>
    private void EncodeObject(JsonElement obj, List<SchemaField> fields, int depth, List<string> records)
    {
        var primitiveFields = fields.Where(f => f.Type == FieldType.Primitive).ToList();

        if (primitiveFields.Count != 0)
        {
            var values = new List<string>();
            var path = $"{depth} "; // Object path

            foreach (var field in primitiveFields)
            {
                if (obj.TryGetProperty(field.Name, out var propValue))
                {
                    values.Add(FormatValue(propValue));
                }
            }

            if (values.Count != 0)
            {
                records.Add(path + _config.FieldDelimiter + string.Join(_config.FieldDelimiter, values));
            }
        }

        // Handle nested structures
        foreach (var field in fields.Where(f => f.Type == FieldType.Object))
        {
            if (obj.TryGetProperty(field.Name, out var nestedObj) && nestedObj.ValueKind == JsonValueKind.Object)
            {
                EncodeNestedObject(nestedObj, field.Fields ?? new List<SchemaField>(), depth + 1, records);
            }
        }

        foreach (var field in fields.Where(f => f.Type == FieldType.Array))
        {
            if (obj.TryGetProperty(field.Name, out var nestedArray) && nestedArray.ValueKind == JsonValueKind.Array)
            {
                EncodeArray(nestedArray, field.Fields ?? new List<SchemaField>(), depth + 1, records);
            }
        }
    }

    /// <summary>
    /// Format a value for output
    /// </summary>
    private string FormatValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => EscapeValue(value.GetString() ?? string.Empty),
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null => string.Empty,
        _ => string.Empty
    };

    /// <summary>
    /// Escape special characters in values
    /// </summary>
    private string EscapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder();
        foreach (var ch in value)
        {
            if (ch.ToString() == _config.FieldDelimiter ||
                ch.ToString() == _config.RecordSeparator ||
                ch.ToString() == _config.EscapeChar)
            {
                sb.Append(_config.EscapeChar);
            }
            sb.Append(ch);
        }
        return sb.ToString();
    }
}
