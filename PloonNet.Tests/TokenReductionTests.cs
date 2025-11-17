using System;
using System.Text.Json;
using Xunit;

namespace PloonNet.Tests;

public class TokenReductionTests
{
    [Fact]
    public void TokenReduction_SimpleArray_ShowsReduction()
    {
        // Arrange
        var data = new
        {
            products = new[]
            {
                new { id = 1, name = "Laptop", price = 999.99, stock = 50 },
                new { id = 2, name = "Mouse", price = 25.50, stock = 150 },
                new { id = 3, name = "Keyboard", price = 75.00, stock = 75 },
                new { id = 4, name = "Monitor", price = 299.99, stock = 30 },
                new { id = 5, name = "Webcam", price = 89.99, stock = 60 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(data);
        var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

        // Assert - PLOON should be significantly smaller
        Assert.True(ploon.Length < json.Length, 
            $"PLOON ({ploon.Length} chars) should be smaller than JSON ({json.Length} chars)");
        
        var reduction = ((double)(json.Length - ploon.Length) / json.Length) * 100;
        Console.WriteLine($"JSON size: {json.Length} characters");
        Console.WriteLine($"PLOON size: {ploon.Length} characters");
        Console.WriteLine($"Reduction: {reduction:F1}%");
        
        // PLOON should achieve at least 40% reduction for repetitive data
        Assert.True(reduction > 40, $"Expected >40% reduction, got {reduction:F1}%");
    }

    [Fact]
    public void TokenReduction_NestedStructure_ShowsReduction()
    {
        // Arrange - more complex nested structure
        var data = new
        {
            orders = new[]
            {
                new
                {
                    id = 1001,
                    customer = new { name = "Alice Johnson", email = "alice@example.com" },
                    items = new[]
                    {
                        new { name = "Laptop", quantity = 1, price = 999.99 },
                        new { name = "Mouse", quantity = 2, price = 25.50 }
                    }
                },
                new
                {
                    id = 1002,
                    customer = new { name = "Bob Smith", email = "bob@example.com" },
                    items = new[]
                    {
                        new { name = "Monitor", quantity = 1, price = 299.99 },
                        new { name = "Keyboard", quantity = 1, price = 75.00 },
                        new { name = "Webcam", quantity = 1, price = 89.99 }
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(data);
        var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

        // Assert
        var reduction = ((double)(json.Length - ploon.Length) / json.Length) * 100;
        Console.WriteLine($"\nNested Structure:");
        Console.WriteLine($"JSON size: {json.Length} characters");
        Console.WriteLine($"PLOON size: {ploon.Length} characters");
        Console.WriteLine($"Reduction: {reduction:F1}%");
        
        Assert.True(reduction > 30, $"Expected >30% reduction for nested data, got {reduction:F1}%");
    }

    [Fact]
    public void DualPathNotation_ArraysUseColonFormat()
    {
        // Arrange
        var data = new
        {
            items = new[]
            {
                new { id = 1, name = "Item1" },
                new { id = 2, name = "Item2" }
            }
        };

        // Act
        var result = Ploon.Stringify(data);

        // Assert - Array paths should use "depth:index" format
        Assert.Contains("1:1|", result); // First item
        Assert.Contains("1:2|", result); // Second item
    }

    [Fact]
    public void DualPathNotation_ObjectsUseDepthOnlyFormat()
    {
        // Arrange
        var data = new
        {
            orders = new[]
            {
                new
                {
                    id = 101,
                    customer = new { name = "Alice" }
                }
            }
        };

        // Act
        var result = Ploon.Stringify(data);

        // Assert - Object paths should use "depth " (space) format
        Assert.Contains("1:1|101", result); // Order (array element)
        Assert.Contains("2 |Alice", result); // Customer (object - note the space after 2)
    }

    [Fact]
    public void SingleSchemaDeclaration_NoFieldRepetition()
    {
        // Arrange
        var data = new
        {
            users = new[]
            {
                new { id = 1, name = "Alice", email = "alice@test.com" },
                new { id = 2, name = "Bob", email = "bob@test.com" },
                new { id = 3, name = "Charlie", email = "charlie@test.com" }
            }
        };

        // Act
        var result = Ploon.Stringify(data);

        // Assert - Field names should only appear once in schema
        var idCount = CountOccurrences(result, "\"id\"");
        var nameCount = CountOccurrences(result, "\"name\"");
        var emailCount = CountOccurrences(result, "\"email\"");
        
        // In PLOON, field names appear only in schema, not in data
        Assert.Equal(0, idCount); // No quotes around field names in PLOON
        Assert.Equal(0, nameCount);
        Assert.Equal(0, emailCount);
        
        // But schema should define them once
        Assert.Contains("(id,name,email)", result);
    }

    private int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
