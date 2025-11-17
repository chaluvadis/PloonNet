# 🚀 PloonNet

**DotNet driver for PLOON (Path-Level Object Oriented Notation)**

PloonNet achieves **49% token reduction vs JSON** and **14% better than TOON** through dual path notation (depth:index for arrays, depth for objects) and single schema declaration, optimized for deeply nested structures with full nested object support.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 📊 Why PLOON?

When sending data to LLMs, every token counts. PLOON optimizes hierarchical data by:

- **Path-based hierarchy**: Eliminates nesting overhead (no indentation!)
- **Dual path notation**: `depth:index` for arrays, `depth ` (space) for objects
- **Single schema declaration**: Zero key repetition
- **Dual format strategy**: Human-readable + machine-optimized

### Key Benefits

| Metric | vs JSON | vs TOON |
|--------|---------|---------|
| **File Size** | 66.2% ↓ | 36.0% ↓ |
| **Token Count** | 49.1% ↓ | 14.1% ↓ |

---

## 📦 Installation

```bash
# Add to your .NET project
dotnet add package PloonNet
```

Or add to your `.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="PloonNet" Version="1.0.0" />
</ItemGroup>
```

---

## 🚀 Quick Start

```csharp
using PloonNet;

// Your data
var data = new
{
    products = new[]
    {
        new { id = 1, name = "Shirt", price = 29.99 },
        new { id = 2, name = "Pants", price = 49.99 }
    }
};

// Convert to PLOON Standard (human-readable)
var ploon = Ploon.Stringify(data);
Console.WriteLine(ploon);
// [products#2](id,name,price)
//
// 1:1|1|Shirt|29.99
// 1:2|2|Pants|49.99

// Minify for production (token-optimized)
var compact = Ploon.Stringify(data, new StringifyOptions 
{ 
    Format = PloonFormat.Compact 
});
Console.WriteLine(compact);
// [products#2](id,name,price);1:1|1|Shirt|29.99;1:2|2|Pants|49.99
```

---

## 📖 API Reference

### Core Functions

#### `Stringify(object, options?)`

Convert .NET object to PLOON string.

```csharp
var ploon = Ploon.Stringify(data, new StringifyOptions
{
    Format = PloonFormat.Standard,  // or PloonFormat.Compact
    Config = PloonConfig.Standard   // or PloonConfig.Compact or custom
});
```

#### `Minify(string)`

Convert Standard format → Compact format (newlines → semicolons).

```csharp
var compact = Ploon.Minify(standardPloon);
```

#### `Prettify(string)`

Convert Compact format → Standard format (semicolons → newlines).

```csharp
var readable = Ploon.Prettify(compactPloon);
```

#### `IsValid(string)`

Validate PLOON format string.

```csharp
bool valid = Ploon.IsValid(ploonString);
```

#### `FromJson(string, options?)`

Convert JSON string directly to PLOON.

```csharp
var json = @"{""users"":[{""id"":1,""name"":""Alice""}]}";
var ploon = Ploon.FromJson(json);
```

---

## 📐 Understanding Path Notation

PLOON uses **dual path notation** to distinguish between arrays and objects:

### Array Paths: `depth:index`

Used for array elements with an index component:
- `1:1` - First item at depth 1
- `1:2` - Second item at depth 1
- `2:1` - First item at depth 2 (nested in `1:1`)
- `3:2` - Second item at depth 3

### Object Paths: `depth ` (depth + space)

Used for object elements without an index:
- `2 ` - Object at depth 2
- `3 ` - Object at depth 3
- `4 ` - Object at depth 4

### When to Use Each

**Arrays** (`#` in schema): Use `depth:index` format
```
[products#2](id,name)    ← Array marker #
1:1|1|Laptop             ← Array path
1:2|2|Mouse              ← Array path
```

**Objects** (`{}` in schema): Use `depth ` format
```
[orders#1](customer{name},id)    ← Object marker {}
1:1|101                          ← Array path (order)
2 |Alice                         ← Object path (customer)
```

**Mixed structures** combine both notations seamlessly:
```
[orders#1](customer{email,name},id,items#(name,price))

1:1|101                  ← Order (array element)
2 |alice@example.com|Alice  ← Customer (object)
2:1|Laptop|999           ← Item 1 (array element)
2:2|Mouse|25             ← Item 2 (array element)
```

---

## 📋 Format Examples

### Simple Array

**Input C#:**
```csharp
var data = new
{
    users = new[]
    {
        new { id = 1, name = "Alice" },
        new { id = 2, name = "Bob" }
    }
};
```

**PLOON Output:**
```
[users#2](id,name)

1:1|1|Alice
1:2|2|Bob
```

### Nested Objects

**Input C#:**
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
                address = new
                {
                    city = "NYC",
                    zip = "10001"
                }
            }
        }
    }
};
```

**PLOON Output:**
```
[orders#1](id,customer{name,address{city,zip}})

1:1|101
2 |Alice
3 |NYC|10001
```

### Nested Arrays

**Input C#:**
```csharp
var data = new
{
    products = new[]
    {
        new
        {
            id = 1,
            name = "Shirt",
            colors = new[]
            {
                new { name = "Red", hex = "#FF0000" },
                new { name = "Blue", hex = "#0000FF" }
            }
        }
    }
};
```

**PLOON Output:**
```
[products#1](id,name,colors#(name,hex))

1:1|1|Shirt
2:1|Red|#FF0000
2:2|Blue|#0000FF
```

---

## 🔧 Configuration

Customize PLOON format with `PloonConfig`:

```csharp
var customConfig = new PloonConfig
{
    FieldDelimiter = "|",      // default
    PathSeparator = ":",       // default
    ArraySizeMarker = "#",     // default
    RecordSeparator = "\n",    // "\n" for standard, ";" for compact
    SchemaFieldSeparator = "," // default
};

var options = new StringifyOptions
{
    Format = PloonFormat.Standard,
    Config = customConfig
};

var ploon = Ploon.Stringify(data, options);
```

---

## 🎯 Use Cases

### 1. Optimize LLM Prompts

```csharp
// Convert large dataset for GPT-4
var ploon = Ploon.Stringify(companyData, new StringifyOptions 
{ 
    Format = PloonFormat.Compact 
});

// Result: 49% fewer tokens = lower costs!
```

### 2. Efficient Data Serialization

```csharp
// Serialize complex nested structures efficiently
var nested = new { /* deeply nested data */ };
var ploon = Ploon.Stringify(nested);

// Much smaller than JSON while maintaining full fidelity
```

---

## 🧪 Testing

Run the test suite:

```bash
dotnet test
```

Run the demo:

```bash
cd examples/PloonDemo
dotnet run
```

---

## 🤝 Credits

Inspired by [TOON Format](https://github.com/toon-format/toon). PLOON offers an alternative approach using path-based hierarchy instead of indentation, achieving comparable token efficiency with different trade-offs.

Reference implementation: [ploon-js](https://github.com/ulpi-io/ploon-js)

---

## 📄 License

MIT © PloonNet Contributors

---

**Made with ❤️ for .NET and LLM optimization**
