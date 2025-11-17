namespace Ploon.Net;

/// <summary>
/// Parses PLOON format back to .NET objects
/// </summary>
internal class PloonParser(PloonConfig config, bool strict = true)
{
    private readonly PloonConfig _config = config;
    private readonly bool _strict = strict;

    /// <summary>
    /// Parse PLOON string to .NET object
    /// </summary>
    public object? Parse(string ploonString)
    {
        // Split into schema and data parts
        var (schemaPart, dataPart) = SplitSchemaAndData(ploonString);

        // Parse schema
        var schema = ParseSchema(schemaPart);

        // Parse data records
        var records = ParseDataRecords(dataPart);

        // Strict mode validations
        if (_strict)
        {
            ValidatePathNotation(records);
            ValidateSchemaConsistency(schema, records);
        }

        // Reconstruct object
        return ReconstructObject(schema, records);
    }

    /// <summary>
    /// Split PLOON string into schema and data parts
    /// </summary>
    private (string schema, string data) SplitSchemaAndData(string ploonString)
    {
        var fieldsCloseIndex = ploonString.IndexOf(_config.FieldsClose);
        if (fieldsCloseIndex == -1)
            throw new FormatException("Invalid PLOON format: missing fields closing parenthesis");

        var schemaEnd = fieldsCloseIndex + _config.FieldsClose.Length;
        var schema = ploonString[..schemaEnd];

        // Skip record separators after schema
        var dataStart = schemaEnd;
        while (dataStart < ploonString.Length &&
               (ploonString[dataStart] == _config.RecordSeparator[0] ||
                char.IsWhiteSpace(ploonString[dataStart])))
        {
            dataStart++;
        }

        var data = dataStart < ploonString.Length ? ploonString[dataStart..] : string.Empty;
        return (schema, data);
    }

    /// <summary>
    /// Parse schema string into SchemaNode
    /// </summary>
    private SchemaNode ParseSchema(string schemaString)
    {
        // Extract root name and count
        var schemaOpenIndex = schemaString.IndexOf(_config.SchemaOpen);
        var schemaCloseIndex = schemaString.IndexOf(_config.SchemaClose);

        if (schemaOpenIndex == -1 || schemaCloseIndex == -1)
            throw new FormatException("Invalid schema format");

        var schemaContent = schemaString[(schemaOpenIndex + _config.SchemaOpen.Length)..schemaCloseIndex];
        var parts = schemaContent.Split(_config.ArraySizeMarker);

        var schema = new SchemaNode
        {
            RootName = parts[0]
        };

        if (parts.Length > 1 && int.TryParse(parts[1], out var count))
        {
            schema.Count = count;
        }

        // Parse fields
        var fieldsStart = schemaString.IndexOf(_config.FieldsOpen, schemaCloseIndex);
        var fieldsEnd = schemaString.IndexOf(_config.FieldsClose, fieldsStart);

        if (fieldsStart == -1 || fieldsEnd == -1)
            throw new FormatException("Invalid fields format");

        var fieldsString = schemaString[(fieldsStart + _config.FieldsOpen.Length)..fieldsEnd];
        schema.Fields = ParseFields(fieldsString);

        return schema;
    }

    /// <summary>
    /// Parse fields string into list of SchemaField
    /// </summary>
    private List<SchemaField> ParseFields(string fieldsString)
    {
        var fields = new List<SchemaField>();
        var fieldStrings = fieldsString.Split(_config.SchemaFieldSeparator);

        foreach (var fieldStr in fieldStrings)
        {
            if (string.IsNullOrWhiteSpace(fieldStr)) continue;

            var field = new SchemaField { Name = fieldStr.Trim() };

            // Check for array marker
            if (field.Name.Contains(_config.ArraySizeMarker))
            {
                field.Type = FieldType.Array;
                var arrayParts = field.Name.Split(_config.ArraySizeMarker);
                field.Name = arrayParts[0];

                // Parse nested fields if present
                if (arrayParts.Length > 1 && arrayParts[1].StartsWith(_config.FieldsOpen))
                {
                    var nestedFieldsEnd = arrayParts[1].IndexOf(_config.FieldsClose);
                    if (nestedFieldsEnd != -1)
                    {
                        var nestedFieldsStr = arrayParts[1][(_config.FieldsOpen.Length)..nestedFieldsEnd];
                        field.Fields = ParseFields(nestedFieldsStr);
                    }
                }
            }
            // Check for object marker
            else if (field.Name.Contains(_config.NestedObjectOpen))
            {
                field.Type = FieldType.Object;
                var objStart = field.Name.IndexOf(_config.NestedObjectOpen);
                var objEnd = field.Name.IndexOf(_config.NestedObjectClose);

                if (objStart != -1 && objEnd != -1 && objEnd > objStart)
                {
                    var nestedFieldsStr = field.Name[(objStart + _config.NestedObjectOpen.Length)..objEnd];
                    field.Name = field.Name[..objStart];
                    field.Fields = ParseFields(nestedFieldsStr);
                }
            }
            else
            {
                field.Type = FieldType.Primitive;
            }

            fields.Add(field);
        }

        return fields;
    }

    /// <summary>
    /// Parse data records into list of DataRecord
    /// </summary>
    private List<DataRecord> ParseDataRecords(string dataString)
    {
        var records = new List<DataRecord>();

        if (string.IsNullOrWhiteSpace(dataString)) return records;

        var recordStrings = dataString.Split(_config.RecordSeparator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var recordStr in recordStrings)
        {
            if (string.IsNullOrWhiteSpace(recordStr)) continue;

            var record = ParseDataRecord(recordStr);
            if (record != null)
            {
                records.Add(record);
            }
        }

        return records;
    }

    /// <summary>
    /// Parse single data record
    /// </summary>
    private DataRecord? ParseDataRecord(string recordString)
    {
        // Find the first field delimiter to separate path from values
        var firstDelimiterIndex = recordString.IndexOf(_config.FieldDelimiter);
        if (firstDelimiterIndex == -1) return null;

        var path = recordString[..firstDelimiterIndex].Trim();
        var valuesPart = recordString[(firstDelimiterIndex + _config.FieldDelimiter.Length)..];

        var values = SplitUnescaped(valuesPart, _config.FieldDelimiter[0])
            .Select(v => UnescapeValue(v.Trim()))
            .ToList();

        return new DataRecord
        {
            Path = path,
            Values = [.. values.Cast<object?>()]
        };
    }

    /// <summary>
    /// Split string on delimiter while ignoring escaped delimiters
    /// </summary>
    private List<string> SplitUnescaped(string input, char delimiter)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool escaped = false;

        foreach (char c in input)
        {
            if (escaped)
            {
                current.Append(c);
                escaped = false;
            }
            else if (c == _config.EscapeChar[0])
            {
                escaped = true;
                current.Append(c);
            }
            else if (c == delimiter)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }

    /// <summary>
    /// Unescape special characters in values
    /// </summary>
    private string UnescapeValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        return value.Replace(_config.EscapeChar + _config.FieldDelimiter, _config.FieldDelimiter)
                   .Replace(_config.EscapeChar + _config.RecordSeparator, _config.RecordSeparator)
                   .Replace(_config.EscapeChar + _config.EscapeChar, _config.EscapeChar);
    }

    /// <summary>
    /// Reconstruct .NET object from schema and data records
    /// </summary>
    private object? ReconstructObject(SchemaNode schema, List<DataRecord> records)
    {
        if (schema.Count.HasValue)
        {
            // Root is an array
            return ReconstructArray(schema.Fields, records, 1);
        }
        else
        {
            // Root is an object
            return ReconstructObject(schema.Fields, records, 1);
        }
    }

    /// <summary>
    /// Reconstruct array from records
    /// </summary>
    private object ReconstructArray(List<SchemaField> fields, List<DataRecord> records, int depth)
    {
        var arrayRecords = records.Where(r => IsArrayPath(r.Path, depth)).ToList();
        var maxIndex = arrayRecords.Any() ? arrayRecords.Max(r => GetArrayIndex(r.Path)) : 0;

        var result = new List<Dictionary<string, object?>>();

        for (int i = 1; i <= maxIndex; i++)
        {
            var itemRecords = records.Where(r => GetArrayIndex(r.Path) == i && GetPathDepth(r.Path) == depth).ToList();
            if (itemRecords.Any())
            {
                var item = ReconstructObjectItem(fields, itemRecords, depth, i);
                result.Add(item);
            }
        }

        return result;
    }

    /// <summary>
    /// Reconstruct single object item
    /// </summary>
    private Dictionary<string, object?> ReconstructObjectItem(List<SchemaField> fields, List<DataRecord> records, int depth, int index)
    {
        var result = new Dictionary<string, object?>();

        // Find the main record for this item
        var mainRecord = records.FirstOrDefault(r => GetArrayIndex(r.Path) == index && GetPathDepth(r.Path) == depth);
        if (mainRecord == null) return result;

        var primitiveFields = fields.Where(f => f.Type == FieldType.Primitive).ToList();
        for (int i = 0; i < primitiveFields.Count && i < mainRecord.Values.Count; i++)
        {
            result[primitiveFields[i].Name] = mainRecord.Values[i];
        }

        // Handle nested objects
        foreach (var field in fields.Where(f => f.Type == FieldType.Object))
        {
            var nestedRecords = records.Where(r => IsObjectPath(r.Path, depth + 1)).ToList();
            if (nestedRecords.Any())
            {
                result[field.Name] = ReconstructNestedObject(field.Fields ?? new List<SchemaField>(), nestedRecords, depth + 1);
            }
        }

        // Handle nested arrays
        foreach (var field in fields.Where(f => f.Type == FieldType.Array))
        {
            var nestedRecords = records.Where(r => IsArrayPath(r.Path, depth + 1)).ToList();
            if (nestedRecords.Any())
            {
                result[field.Name] = ReconstructArray(field.Fields ?? new List<SchemaField>(), nestedRecords, depth + 1);
            }
        }

        return result;
    }

    /// <summary>
    /// Reconstruct nested object
    /// </summary>
    private Dictionary<string, object?> ReconstructNestedObject(List<SchemaField> fields, List<DataRecord> records, int depth)
    {
        var result = new Dictionary<string, object?>();

        var objectRecord = records.FirstOrDefault(r => IsObjectPath(r.Path, depth));
        if (objectRecord == null) return result;

        var primitiveFields = fields.Where(f => f.Type == FieldType.Primitive).ToList();
        for (int i = 0; i < primitiveFields.Count && i < objectRecord.Values.Count; i++)
        {
            result[primitiveFields[i].Name] = objectRecord.Values[i];
        }

        // Handle further nesting
        foreach (var field in fields.Where(f => f.Type == FieldType.Object))
        {
            var nestedRecords = records.Where(r => IsObjectPath(r.Path, depth + 1)).ToList();
            if (nestedRecords.Any())
            {
                result[field.Name] = ReconstructNestedObject(field.Fields ?? new List<SchemaField>(), nestedRecords, depth + 1);
            }
        }

        foreach (var field in fields.Where(f => f.Type == FieldType.Array))
        {
            var nestedRecords = records.Where(r => IsArrayPath(r.Path, depth + 1)).ToList();
            if (nestedRecords.Any())
            {
                result[field.Name] = ReconstructArray(field.Fields ?? new List<SchemaField>(), nestedRecords, depth + 1);
            }
        }

        return result;
    }

    /// <summary>
    /// Reconstruct root-level object
    /// </summary>
    private Dictionary<string, object?> ReconstructObject(List<SchemaField> fields, List<DataRecord> records, int depth)
    {
        var result = new Dictionary<string, object?>();

        var objectRecord = records.FirstOrDefault(r => IsObjectPath(r.Path, depth));
        if (objectRecord != null)
        {
            var primitiveFields = fields.Where(f => f.Type == FieldType.Primitive).ToList();
            for (int i = 0; i < primitiveFields.Count && i < objectRecord.Values.Count; i++)
            {
                result[primitiveFields[i].Name] = objectRecord.Values[i];
            }
        }

        // Handle nested structures
        foreach (var field in fields.Where(f => f.Type == FieldType.Object))
        {
            var nestedRecords = records.Where(r => IsObjectPath(r.Path, depth + 1)).ToList();
            if (nestedRecords.Any())
            {
                result[field.Name] = ReconstructNestedObject(field.Fields ?? new List<SchemaField>(), nestedRecords, depth + 1);
            }
        }

        foreach (var field in fields.Where(f => f.Type == FieldType.Array))
        {
            var nestedRecords = records.Where(r => IsArrayPath(r.Path, depth + 1)).ToList();
            if (nestedRecords.Any())
            {
                result[field.Name] = ReconstructArray(field.Fields ?? new List<SchemaField>(), nestedRecords, depth + 1);
            }
        }

        return result;
    }

    /// <summary>
    /// Check if path is an array path at given depth
    /// </summary>
    private bool IsArrayPath(string path, int depth)
    {
        var parts = path.Split(_config.PathSeparator);
        return parts.Length == 2 &&
               int.TryParse(parts[0], out var pathDepth) &&
               pathDepth == depth &&
               int.TryParse(parts[1], out _);
    }

    /// <summary>
    /// Check if path is an object path at given depth
    /// </summary>
    private bool IsObjectPath(string path, int depth)
    {
        return path.Trim() == depth.ToString();
    }

    /// <summary>
    /// Get depth from path
    /// </summary>
    private int GetPathDepth(string path)
    {
        var parts = path.Split(_config.PathSeparator);
        if (parts.Length >= 1 && int.TryParse(parts[0], out var depth))
        {
            return depth;
        }
        return 0;
    }

    /// <summary>
    /// Get array index from path
    /// </summary>
    private int GetArrayIndex(string path)
    {
        var parts = path.Split(_config.PathSeparator);
        if (parts.Length == 2 && int.TryParse(parts[1], out var index))
        {
            return index;
        }
        return 0;
    }

    /// <summary>
    /// Validate schema consistency in strict mode
    /// </summary>
    private void ValidateSchemaConsistency(SchemaNode schema, List<DataRecord> records)
    {
        // For arrays, ensure all items have consistent field structure
        if (schema.Count.HasValue && schema.Count.Value > 0)
        {
            var arrayRecords = records.Where(r => IsArrayPath(r.Path, 1)).ToList();
            if (arrayRecords.Any())
            {
                // Group records by array index
                var recordsByIndex = arrayRecords.GroupBy(r => GetArrayIndex(r.Path))
                                                 .ToDictionary(g => g.Key, g => g.ToList());

                // Check that all array items have the same number of primitive fields at root level
                var expectedFieldCount = schema.Fields.Count(f => f.Type == FieldType.Primitive);
                foreach (var kvp in recordsByIndex)
                {
                    var itemRecords = kvp.Value;
                    var mainRecord = itemRecords.FirstOrDefault(r => GetPathDepth(r.Path) == 1);
                    if (mainRecord != null && mainRecord.Values.Count != expectedFieldCount)
                    {
                        throw new FormatException(
                            $"Schema consistency violation: Array item at index {kvp.Key} has {mainRecord.Values.Count} fields, expected {expectedFieldCount}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Validate path notation in strict mode
    /// </summary>
    private void ValidatePathNotation(List<DataRecord> records)
    {
        foreach (var record in records)
        {
            var path = record.Path.Trim();

            // Check basic path format
            if (string.IsNullOrEmpty(path))
            {
                throw new FormatException("Empty path found in data record");
            }

            // Paths should either be "depth" (object) or "depth:index" (array)
            var parts = path.Split(_config.PathSeparator);
            if (parts.Length == 1)
            {
                // Object path: should be just a number
                if (!int.TryParse(parts[0], out _))
                {
                    throw new FormatException($"Invalid object path format: '{path}'. Expected numeric depth.");
                }
            }
            else if (parts.Length == 2)
            {
                // Array path: should be "depth:index"
                if (!int.TryParse(parts[0], out _) || !int.TryParse(parts[1], out _))
                {
                    throw new FormatException($"Invalid array path format: '{path}'. Expected 'depth{_config.PathSeparator}index' format.");
                }
            }
            else
            {
                throw new FormatException($"Invalid path format: '{path}'. Expected 'depth' or 'depth{_config.PathSeparator}index' format.");
            }
        }
    }
}
