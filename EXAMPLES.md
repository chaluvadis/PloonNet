# PloonNet Usage Examples

This file demonstrates various usage patterns for PloonNet.

## Basic Usage

### Convert Object to PLOON

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

var ploon = Ploon.Stringify(data);
Console.WriteLine(ploon);
```

Output:
```
[users#2](id,name)

1:1|1|Alice
1:2|2|Bob
```

## Format Conversion

### Standard to Compact

```csharp
var standard = Ploon.Stringify(data);
var compact = Ploon.Minify(standard);
```

### Compact to Standard

```csharp
var compact = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });
var standard = Ploon.Prettify(compact);
```

## JSON Integration

### Convert JSON to PLOON

```csharp
var json = @"{""products"":[{""id"":1,""name"":""Item""}]}";
var ploon = Ploon.FromJson(json);
```

## Advanced Structures

### Nested Objects

```csharp
var orders = new
{
    orders = new[]
    {
        new
        {
            id = 1,
            customer = new
            {
                name = "Alice",
                address = new { city = "NYC", zip = "10001" }
            }
        }
    }
};

var ploon = Ploon.Stringify(orders);
```

Output demonstrates dual path notation:
```
[orders#1](id,customer{name,address{city,zip}})

1:1|1          ← Order (array, depth:index)
2 |Alice       ← Customer (object, depth only)
3 |NYC|10001   ← Address (nested object, depth only)
```

### Nested Arrays

```csharp
var catalog = new
{
    products = new[]
    {
        new
        {
            id = 1,
            tags = new[] { "electronics", "featured", "sale" }
        }
    }
};

// Note: Currently arrays of primitives are not fully optimized
// Best for arrays of objects
```

### Mixed Structures

```csharp
var ecommerce = new
{
    orders = new[]
    {
        new
        {
            id = 101,
            customer = new { name = "Bob", email = "bob@test.com" },
            items = new[]
            {
                new { name = "Monitor", qty = 1, price = 299.99 },
                new { name = "Keyboard", qty = 2, price = 75.00 }
            }
        }
    }
};

var ploon = Ploon.Stringify(ecommerce);
```

## Custom Configuration

### Create Custom Delimiter Configuration

```csharp
var config = new PloonConfig
{
    FieldDelimiter = "|",
    PathSeparator = ":",
    RecordSeparator = "\n"
};

var options = new StringifyOptions
{
    Format = PloonFormat.Standard,
    Config = config
};

var ploon = Ploon.Stringify(data, options);
```

## Validation

### Validate PLOON String

```csharp
string ploonString = "[users#2](id,name)\n\n1:1|1|Alice";
if (Ploon.IsValid(ploonString))
{
    Console.WriteLine("Valid PLOON format");
}
```

## Performance Comparison

### Measure Size Reduction

```csharp
using System.Text.Json;

var data = new
{
    employees = new[]
    {
        new { id = 1, name = "Alice", dept = "Engineering", salary = 95000 },
        new { id = 2, name = "Bob", dept = "Sales", salary = 75000 },
        new { id = 3, name = "Charlie", dept = "Marketing", salary = 80000 }
    }
};

var json = JsonSerializer.Serialize(data);
var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

Console.WriteLine($"JSON:  {json.Length} chars");
Console.WriteLine($"PLOON: {ploon.Length} chars");
Console.WriteLine($"Reduction: {((json.Length - ploon.Length) * 100.0 / json.Length):F1}%");
```

## LLM Optimization

### Optimizing Data for LLM Prompts

```csharp
// Large dataset for LLM analysis
var analyticsData = new
{
    metrics = new[]
    {
        new { date = "2024-01-01", views = 1250, clicks = 45, conversions = 12 },
        new { date = "2024-01-02", views = 1380, clicks = 52, conversions = 15 },
        // ... more data
    }
};

// Use compact format for minimal tokens
var compactPloon = Ploon.Stringify(analyticsData, 
    new StringifyOptions { Format = PloonFormat.Compact });

// Include in LLM prompt
var prompt = $"Analyze this data:\n{compactPloon}\n\nProvide insights.";
```

## Error Handling

### Handle Invalid Input

```csharp
try
{
    var ploon = Ploon.Stringify(data);
}
catch (Exception ex)
{
    Console.WriteLine($"Serialization failed: {ex.Message}");
}
```

## Tips & Tricks

1. **Use Compact format for production**: Minimizes bandwidth and token usage
2. **Use Standard format for debugging**: Easier to read and understand
3. **Validate external PLOON**: Always validate PLOON strings from external sources
4. **Structure matters**: PLOON excels with repetitive, structured data
5. **Field order**: Fields appear in alphabetical order in the schema
6. **Path notation**: Remember arrays use `depth:index`, objects use `depth ` (with space)

## Common Patterns

### REST API Response Optimization

```csharp
public IActionResult GetUsers()
{
    var users = _context.Users.ToList();
    var data = new { users };
    
    // Return compact PLOON for efficiency
    var ploon = Ploon.Stringify(data, 
        new StringifyOptions { Format = PloonFormat.Compact });
    
    return Content(ploon, "application/x-ploon");
}
```

### Data Export

```csharp
var exportData = new { records = GetLargeDataset() };
var ploon = Ploon.Stringify(exportData);

// Save to file
await File.WriteAllTextAsync("export.ploon", ploon);
```

### Configuration Files (Compact)

```csharp
var config = new
{
    settings = new[]
    {
        new { key = "apiUrl", value = "https://api.example.com" },
        new { key = "timeout", value = "30" }
    }
};

var ploonConfig = Ploon.Stringify(config, 
    new StringifyOptions { Format = PloonFormat.Compact });
```
