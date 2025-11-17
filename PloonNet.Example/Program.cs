using PloonNet;
using System.Text.Json;
Console.WriteLine("------------------------ PloonNet - Token-Efficient Data Serialization ------------------------");

// Example 0: No Array
Console.WriteLine("Example 0: No Array");

var product = new
{
    id = 1,
    name = "Laptop",
    price = 999.99
};

var json0 = JsonSerializer.Serialize(product);
var ploon0 = Ploon.Stringify(product, new StringifyOptions { Format = PloonFormat.Compact });

Console.WriteLine($"""
        JSON:  {json0}
        PLOON: {ploon0}
        Reduction: {((json0.Length - ploon0.Length) * 100.0 / json0.Length):F1}% \n
""");


// Example 1: Simple Array
Console.WriteLine("Example 1: Simple Array");

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

Console.WriteLine($"""
        JSON:  {json1}
        PLOON: {ploon1}
        Reduction: {((json1.Length - ploon1.Length) * 100.0 / json1.Length):F1}%
""");

// Example 2: Nested Objects
Console.WriteLine("Example 2: Nested Objects (dual path notation)");

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
Console.WriteLine("Example 3: Token Reduction Analysis");

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

Console.WriteLine($"""
        Dataset: 5 employees with 4 fields each
        JSON size:  {jsonLarge.Length} characters
        PLOON size: {ploonLarge.Length} characters
        Reduction:  {((jsonLarge.Length - ploonLarge.Length) * 100.0 / jsonLarge.Length):F1}%
        Savings:    {jsonLarge.Length - ploonLarge.Length} characters
    """);