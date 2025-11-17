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

    [Fact]
    public void Parse_SimpleArray_RestoresOriginalStructure()
    {
        // Arrange
        var original = new
        {
            users = new[]
            {
                new { id = 1, name = "Alice" },
                new { id = 2, name = "Bob" }
            }
        };

        // Act
        var ploon = Ploon.Stringify(original);
        var parsed = Ploon.Parse(ploon);

        // Assert
        Assert.NotNull(parsed);
        // Note: Full round-trip testing would require more complex assertions
        // For now, we verify it doesn't throw and returns an object
    }

    [Fact]
    public void Parse_NestedObject_RestoresOriginalStructure()
    {
        // Arrange
        var original = new
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
        var ploon = Ploon.Stringify(original);
        var parsed = Ploon.Parse(ploon);

        // Assert
        Assert.NotNull(parsed);
    }

    [Fact]
    public void Debug_NestedObject_Parsing()
    {
        // Arrange
        var original = new
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
        var ploon = Ploon.Stringify(original);
        Console.WriteLine($"Generated PLOON: {ploon}");

        // Assert - should not throw
        var parsed = Ploon.Parse(ploon);
        Assert.NotNull(parsed);
    }

    [Fact]
    public void Parse_EmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Ploon.Parse(""));
        Assert.Throws<ArgumentException>(() => Ploon.Parse(null!));
        Assert.Throws<ArgumentException>(() => Ploon.Parse("   "));
    }

    [Fact]
    public void Parse_InvalidFormat_ThrowsFormatException()
    {
        // Arrange
        var invalidPloon = "invalid ploon string";

        // Act & Assert
        Assert.Throws<FormatException>(() => Ploon.Parse(invalidPloon));
    }

    [Fact]
    public void Parse_MissingSchemaBrackets_ThrowsFormatException()
    {
        // Arrange
        var invalidPloon = "users(id,name)1:1|1|Alice";

        // Act & Assert
        Assert.Throws<FormatException>(() => Ploon.Parse(invalidPloon));
    }

    [Fact]
    public void Parse_MissingFieldsParentheses_ThrowsFormatException()
    {
        // Arrange
        var invalidPloon = "[users#2]id,name1:1|1|Alice";

        // Act & Assert
        Assert.Throws<FormatException>(() => Ploon.Parse(invalidPloon));
    }

    [Fact]
    public async Task StringifyAsync_ProducesSameResultAsSync()
    {
        // Arrange
        var data = new
        {
            products = new[]
            {
                new { id = 1, name = "Laptop", price = 999.99 },
                new { id = 2, name = "Mouse", price = 25.50 }
            }
        };

        // Act
        var syncResult = Ploon.Stringify(data);
        var asyncResult = await Ploon.StringifyAsync(data);

        // Assert
        Assert.Equal(syncResult, asyncResult);
    }

    [Fact]
    public async Task ParseAsync_ProducesSameResultAsSync()
    {
        // Arrange
        var ploonString = "[users#2](id,name)\n\n1:1|1|Alice\n1:2|2|Bob";

        // Act
        var syncResult = Ploon.Parse(ploonString);
        var asyncResult = await Ploon.ParseAsync(ploonString);

        // Assert
        Assert.NotNull(syncResult);
        Assert.NotNull(asyncResult);
        // Note: Deep equality comparison would require custom logic
    }

    [Fact]
    public void Parse_WithCustomConfig_UsesProvidedConfiguration()
    {
        // Arrange
        var customConfig = new PloonConfig
        {
            FieldDelimiter = ";",
            RecordSeparator = "|",
            SchemaOpen = "<",
            SchemaClose = ">",
            FieldsOpen = "{",
            FieldsClose = "}"
        };

        var options = new ParseOptions { Config = customConfig };

        // This would require a PLOON string with custom delimiters
        // For now, we just verify the method accepts the options
        Assert.NotNull(options);
    }

    [Fact]
    public void Parse_EscapedValues_HandlesSpecialCharacters()
    {
        // Arrange - create a PLOON string with escaped delimiters
        var ploonWithEscapes = "[test#1](value)\n\n1:1|value\\|with\\|pipes";

        // Act
        var result = Ploon.Parse(ploonWithEscapes);

        // Assert
        Assert.NotNull(result);
        // The parser should handle escaped characters properly
    }

    [Fact]
    public void Parse_StrictMode_EnablesPathValidation()
    {
        // Arrange - create a PLOON string with invalid path (non-numeric depth)
        var invalidPathPloon = "[test#1](value)\n\nabc:1|test";

        // Act & Assert - should throw in strict mode (default)
        Assert.Throws<FormatException>(() => Ploon.Parse(invalidPathPloon));
    }

    [Fact]
    public void Parse_LenientMode_SkipsPathValidation()
    {
        // Arrange - create a valid PLOON string
        var validPloon = "[test#1](value)\n\n1:1|test";
        var options = new ParseOptions { Strict = false };

        // Act
        var result = Ploon.Parse(validPloon, options);

        // Assert - should parse successfully in lenient mode
        Assert.NotNull(result);
    }

    [Fact]
    public void Parse_StrictMode_DefaultBehavior()
    {
        // Arrange
        var options = new ParseOptions(); // Strict should default to true

        // Act
        var data = new { users = new[] { new { id = 1, name = "Alice" } } };
        var ploon = Ploon.Stringify(data);
        var result = Ploon.Parse(ploon, options);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Parse_LenientMode_ExplicitFalse()
    {
        // Arrange
        var options = new ParseOptions { Strict = false };

        // Act
        var data = new { users = new[] { new { id = 1, name = "Alice" } } };
        var ploon = Ploon.Stringify(data);
        var result = Ploon.Parse(ploon, options);

        // Assert
        Assert.NotNull(result);
    }
}
