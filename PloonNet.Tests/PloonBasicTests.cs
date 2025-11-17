namespace PloonNet.Tests;

public class PloonBasicTests
{
    [Fact]
    public void Stringify_SimpleArray_GeneratesCorrectPloon()
    {
        // Arrange
        var data = new
        {
            users = new[]
            {
                new { id = 1, name = "Alice" },
                new { id = 2, name = "Bob" }
            }
        };

        // Act
        var result = Ploon.Stringify(data);

        // Assert
        Assert.Contains("[users#2](id,name)", result);
        Assert.Contains("1:1|1|Alice", result);
        Assert.Contains("1:2|2|Bob", result);
    }

    [Fact]
    public void Stringify_SimpleArrayCompact_GeneratesCorrectPloon()
    {
        // Arrange
        var data = new
        {
            products = new[]
            {
                new { id = 1, name = "Laptop", price = 999 },
                new { id = 2, name = "Mouse", price = 25 }
            }
        };

        var options = new StringifyOptions { Format = PloonFormat.Compact };

        // Act
        var result = Ploon.Stringify(data, options);

        // Assert
        Assert.Contains("[products#2](id,name,price)", result);
        Assert.Contains(";1:1|1|Laptop|999;", result);
        Assert.Contains(";1:2|2|Mouse|25", result);
    }

    [Fact]
    public void Stringify_NestedObject_GeneratesCorrectPloon()
    {
        // Arrange
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

        // Act
        var result = Ploon.Stringify(data);

        // Assert
        Assert.Contains("[orders#1]", result);
        Assert.Contains("1:1|101", result); // Order with id
        Assert.Contains("2 |Alice", result); // Customer (object - depth only)
        Assert.Contains("3 |NYC|10001", result); // Address (nested object - depth only)
    }

    [Fact]
    public void Stringify_NestedArray_GeneratesCorrectPloon()
    {
        // Arrange
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

        // Act
        var result = Ploon.Stringify(data);

        // Assert
        Assert.Contains("[products#1]", result);
        Assert.Contains("1:1|1|Shirt", result); // Product
        Assert.Contains("2:1|Red|#FF0000", result); // First color (nested array)
        Assert.Contains("2:2|Blue|#0000FF", result); // Second color (nested array)
    }

    [Fact]
    public void Stringify_MixedNestedStructure_GeneratesCorrectPloon()
    {
        // Arrange
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

        // Act
        var result = Ploon.Stringify(data);

        // Assert
        Assert.Contains("[orders#1]", result);
        Assert.Contains("1:1|101", result); // Order
        Assert.Contains("2 |Alice", result); // Customer (object)
        Assert.Contains("2:1|Laptop|999", result); // First item (array)
        Assert.Contains("2:2|Mouse|25", result); // Second item (array)
    }

    [Fact]
    public void Minify_ConvertsStandardToCompact()
    {
        // Arrange
        var standard = "[users#2](id,name)\n\n1:1|1|Alice\n1:2|2|Bob";

        // Act
        var compact = Ploon.Minify(standard);

        // Assert
        Assert.Contains(";", compact);
        Assert.DoesNotContain("\n", compact);
    }

    [Fact]
    public void Prettify_ConvertsCompactToStandard()
    {
        // Arrange
        var compact = "[users#2](id,name);;1:1|1|Alice;1:2|2|Bob";

        // Act
        var standard = Ploon.Prettify(compact);

        // Assert
        Assert.Contains("\n", standard);
        Assert.DoesNotContain(";", standard);
    }

    [Fact]
    public void IsValid_ValidPloonString_ReturnsTrue()
    {
        // Arrange
        var ploon = "[users#2](id,name)\n\n1:1|1|Alice";

        // Act
        var result = Ploon.IsValid(ploon);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidPloonString_ReturnsFalse()
    {
        // Arrange
        var invalid = "not a ploon string";

        // Act
        var result = Ploon.IsValid(invalid);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FromJson_ConvertsJsonToPloon()
    {
        // Arrange
        var json = @"{""users"":[{""id"":1,""name"":""Alice""},{""id"":2,""name"":""Bob""}]}";

        // Act
        var result = Ploon.FromJson(json);

        // Assert
        Assert.Contains("[users#2](id,name)", result);
        Assert.Contains("1:1|1|Alice", result);
        Assert.Contains("1:2|2|Bob", result);
    }
}
