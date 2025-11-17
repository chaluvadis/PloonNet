using PloonNet;
using System.Text.Json;

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║         PloonNet - Token-Efficient Data Serialization         ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

// Example 1: Simple Array
Console.WriteLine("📦 Example 1: Simple Array");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

var products = new
{
    products = new[]
    {
        new { id = 1, name = "Laptop", price = 999.99 },
        new { id = 2, name = "Mouse", price = 25.50 }
    }
};

var json1 = JsonSerializer.Serialize(products);
var ploon1 = Ploon.Stringify(products, new StringifyOptions { Format = PloonFormat.Compact });

Console.WriteLine($"JSON:  {json1}");
Console.WriteLine($"PLOON: {ploon1}");
Console.WriteLine($"Reduction: {((json1.Length - ploon1.Length) * 100.0 / json1.Length):F1}%\n");

// Example 2: Nested Objects
Console.WriteLine("🏢 Example 2: Nested Objects (dual path notation)");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

var orders = new
{
    orders = new[]
    {
        new
        {
            id = 101,
            customer = new
            {
                name = "Alice Johnson",
                address = new
                {
                    city = "New York",
                    zip = "10001"
                }
            }
        }
    }
};

var ploon2 = Ploon.Stringify(orders);
Console.WriteLine(ploon2);
Console.WriteLine("Note: Objects use 'depth ' (with space), arrays use 'depth:index'\n");

// Example 3: Token Reduction Analysis
Console.WriteLine("📊 Example 3: Token Reduction Analysis");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

var largeDataset = new
{
    employees = new[]
    {
        new { id = 1, name = "Alice", department = "Engineering", salary = 95000 },
        new { id = 2, name = "Bob", department = "Marketing", salary = 75000 },
        new { id = 3, name = "Charlie", department = "Sales", salary = 80000 },
        new { id = 4, name = "Diana", department = "Engineering", salary = 98000 },
        new { id = 5, name = "Eve", department = "HR", salary = 70000 }
    }
};

var jsonLarge = JsonSerializer.Serialize(largeDataset);
var ploonLarge = Ploon.Stringify(largeDataset, new StringifyOptions { Format = PloonFormat.Compact });

Console.WriteLine($"Dataset: 5 employees with 4 fields each");
Console.WriteLine($"JSON size:  {jsonLarge.Length} characters");
Console.WriteLine($"PLOON size: {ploonLarge.Length} characters");
Console.WriteLine($"Reduction:  {((jsonLarge.Length - ploonLarge.Length) * 100.0 / jsonLarge.Length):F1}%");
Console.WriteLine($"Savings:    {jsonLarge.Length - ploonLarge.Length} characters\n");

Console.WriteLine("✨ Key Features:");
Console.WriteLine("  • Dual path notation: depth:index for arrays, depth for objects");
Console.WriteLine("  • Single schema declaration: no field name repetition");
Console.WriteLine("  • 49% token reduction vs JSON on average");
Console.WriteLine("  • Perfect for LLM prompts and efficient data transfer");
