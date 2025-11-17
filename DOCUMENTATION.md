# PloonNet API Documentation

## Overview

PloonNet is a .NET implementation of PLOON (Path-Level Object Oriented Notation), a highly efficient serialization format designed to minimize token usage when transmitting data to Large Language Models (LLMs) and for general data interchange.

## Installation

```bash
dotnet add package PloonNet
```

## Quick Start

```csharp
using PloonNet;

var data = new 
{ 
    users = new[] 
    { 
        new { id = 1, name = "Alice" },
        new { id = 2, name = "Bob" }
    } 
};

// Standard format (human-readable)
var ploon = Ploon.Stringify(data);

// Compact format (token-optimized)
var compact = Ploon.Stringify(data, new StringifyOptions 
{ 
    Format = PloonFormat.Compact 
});
```

## Core API

### `Ploon.Stringify(object, StringifyOptions?)`

Converts a .NET object to PLOON format.

**Parameters:**
- `obj`: The object to serialize (any .NET object compatible with System.Text.Json)
- `options`: Optional configuration (format, custom config)

**Returns:** String in PLOON format

**Example:**
```csharp
var result = Ploon.Stringify(data, new StringifyOptions
{
    Format = PloonFormat.Standard,  // or PloonFormat.Compact
    Config = PloonConfig.Standard   // or custom config
});
```

### `Ploon.Minify(string)`

Converts Standard format to Compact format (newlines → semicolons).

**Parameters:**
- `ploonString`: PLOON string in standard format

**Returns:** PLOON string in compact format

**Example:**
```csharp
var standard = "[users#2](id,name)\n\n1:1|1|Alice\n1:2|2|Bob";
var compact = Ploon.Minify(standard);
// Result: "[users#2](id,name);;1:1|1|Alice;1:2|2|Bob"
```

### `Ploon.Prettify(string)`

Converts Compact format to Standard format (semicolons → newlines).

**Parameters:**
- `ploonString`: PLOON string in compact format

**Returns:** PLOON string in standard format

**Example:**
```csharp
var compact = "[users#2](id,name);;1:1|1|Alice;1:2|2|Bob";
var standard = Ploon.Prettify(compact);
```

### `Ploon.IsValid(string)`

Validates a PLOON format string.

**Parameters:**
- `ploonString`: String to validate

**Returns:** `true` if valid PLOON format, `false` otherwise

**Example:**
```csharp
bool valid = Ploon.IsValid("[users#2](id,name)\n\n1:1|1|Alice");
```

### `Ploon.FromJson(string, StringifyOptions?)`

Converts JSON string directly to PLOON format.

**Parameters:**
- `jsonString`: Valid JSON string
- `options`: Optional configuration

**Returns:** PLOON formatted string

**Example:**
```csharp
var json = @"{""users"":[{""id"":1,""name"":""Alice""}]}";
var ploon = Ploon.FromJson(json);
```

## Configuration

### PloonConfig

Customize PLOON output format with `PloonConfig`:

**Properties:**
- `FieldDelimiter`: Separates values in a record (default: `|`)
- `PathSeparator`: Separates depth and index in paths (default: `:`)
- `ArraySizeMarker`: Indicates array length in schema (default: `#`)
- `RecordSeparator`: Separates records (default: `\n` for standard, `;` for compact)
- `EscapeChar`: Escapes special characters (default: `\`)
- `SchemaOpen`: Schema opening bracket (default: `[`)
- `SchemaClose`: Schema closing bracket (default: `]`)
- `FieldsOpen`: Fields opening parenthesis (default: `(`)
- `FieldsClose`: Fields closing parenthesis (default: `)`)
- `NestedObjectOpen`: Nested object opening brace (default: `{`)
- `NestedObjectClose`: Nested object closing brace (default: `}`)
- `SchemaFieldSeparator`: Separates field names in schema (default: `,`)

**Built-in Configs:**
```csharp
PloonConfig.Standard  // Human-readable format with newlines
PloonConfig.Compact   // Token-optimized format with semicolons
```

**Custom Config Example:**
```csharp
var customConfig = new PloonConfig
{
    FieldDelimiter = "|",
    RecordSeparator = "\n",
    // ... other properties
};

var options = new StringifyOptions
{
    Format = PloonFormat.Standard,
    Config = customConfig
};
```

## Format Specification

### Dual Path Notation

PLOON uses different path formats for arrays and objects:

**Arrays** (use `depth:index`):
```
1:1    First item at depth 1
1:2    Second item at depth 1
2:1    First item at depth 2
```

**Objects** (use `depth ` with space):
```
2      Object at depth 2
3      Object at depth 3
```

### Schema Declaration

The schema declares the structure once at the top:

**Simple Array:**
```
[products#2](id,name,price)
```

**Nested Object:**
```
[orders#1](id,customer{name,email})
```

**Nested Array:**
```
[products#1](id,colors#(name,hex))
```

**Mixed Structure:**
```
[orders#1](id,customer{name},items#(name,price))
```

### Data Records

Records follow the schema with path prefixes:

**Array Records:**
```
1:1|101|ProductA|99.99
1:2|102|ProductB|149.99
```

**Object Records:**
```
2 |Alice|alice@example.com
```

## Examples

### Simple Array

```csharp
var data = new
{
    products = new[]
    {
        new { id = 1, name = "Laptop", price = 999.99 },
        new { id = 2, name = "Mouse", price = 25.50 }
    }
};

var ploon = Ploon.Stringify(data);
```

**Output:**
```
[products#2](id,name,price)

1:1|1|Laptop|999.99
1:2|2|Mouse|25.5
```

### Nested Objects

```csharp
var data = new
{
    orders = new[]
    {
        new
        {
            id = 101,
            customer = new
            {
                name = "Alice",
                email = "alice@example.com"
            }
        }
    }
};

var ploon = Ploon.Stringify(data);
```

**Output:**
```
[orders#1](id,customer{name,email})

1:1|101
2 |Alice|alice@example.com
```

### Nested Arrays

```csharp
var data = new
{
    products = new[]
    {
        new
        {
            id = 1,
            colors = new[]
            {
                new { name = "Red", hex = "#FF0000" },
                new { name = "Blue", hex = "#0000FF" }
            }
        }
    }
};

var ploon = Ploon.Stringify(data);
```

**Output:**
```
[products#1](id,colors#(name,hex))

1:1|1
2:1|Red|#FF0000
2:2|Blue|#0000FF
```

### Mixed Structure

```csharp
var data = new
{
    orders = new[]
    {
        new
        {
            id = 101,
            customer = new { name = "Alice" },
            items = new[]
            {
                new { name = "Laptop", price = 999 },
                new { name = "Mouse", price = 25 }
            }
        }
    }
};

var ploon = Ploon.Stringify(data);
```

**Output:**
```
[orders#1](id,customer{name},items#(name,price))

1:1|101
2 |Alice
2:1|Laptop|999
2:2|Mouse|25
```

## Performance

PloonNet achieves significant size reductions compared to JSON:

| Dataset Type | Reduction |
|--------------|-----------|
| Simple arrays | 29-45% |
| Medium datasets | 41-45% |
| Large datasets (100+ items) | 45-49% |
| Wide structures (many fields) | 38-50% |

**Example:**
```csharp
var data = new
{
    employees = new[]
    {
        new { id = 1, name = "Alice", dept = "Eng", salary = 95000 },
        new { id = 2, name = "Bob", dept = "Sales", salary = 75000 },
        // ... 3 more employees
    }
};

var json = JsonSerializer.Serialize(data);   // 326 characters
var ploon = Ploon.Stringify(data, 
    new StringifyOptions { Format = PloonFormat.Compact }); // 171 characters
// Reduction: 47.5%
```

## Best Practices

1. **Use Compact format for LLM prompts**: Minimizes token usage
2. **Use Standard format for debugging**: More human-readable
3. **Validate input**: Use `Ploon.IsValid()` when parsing external data
4. **Consider structure**: PLOON excels with repetitive, structured data

## Limitations

- Currently supports serialization only (no deserialization yet)
- Works best with tabular/structured data
- Requires consistent field structure within arrays

## Contributing

Contributions are welcome! Please see the GitHub repository for guidelines.

## License

MIT License - See LICENSE file for details

## Credits

Inspired by [TOON Format](https://github.com/toon-format/toon) and the [ploon-js](https://github.com/ulpi-io/ploon-js) reference implementation.
