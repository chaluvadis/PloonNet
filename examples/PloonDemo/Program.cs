using PloonNet;

var data = new
{
    products = new[]
    {
        new { id = 1, name = "Shirt", price = 29.99 },
        new { id = 2, name = "Pants", price = 49.99 }
    }
};

Console.WriteLine("=== Standard Format ===");
var standard = Ploon.Stringify(data);
Console.WriteLine(standard);

Console.WriteLine("\n=== Compact Format ===");
var compact = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });
Console.WriteLine(compact);

Console.WriteLine("\n=== Nested Example ===");
var nestedData = new
{
    orders = new[]
    {
        new
        {
            id = 101,
            customer = new { name = "Alice", email = "alice@example.com" },
            items = new[]
            {
                new { name = "Laptop", price = 999 },
                new { name = "Mouse", price = 25 }
            }
        }
    }
};
Console.WriteLine(Ploon.Stringify(nestedData));
