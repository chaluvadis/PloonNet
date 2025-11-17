using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace PloonNet.Tests;

/// <summary>
/// Benchmark tests to verify PLOON's token reduction claims
/// </summary>
public class BenchmarkTests
{
    private readonly ITestOutputHelper _output;

    public BenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Benchmark_SmallDataset_5Items()
    {
        var data = new
        {
            products = new[]
            {
                new { id = 1, name = "Item1", price = 10.99, stock = 100 },
                new { id = 2, name = "Item2", price = 20.99, stock = 50 },
                new { id = 3, name = "Item3", price = 30.99, stock = 75 },
                new { id = 4, name = "Item4", price = 40.99, stock = 25 },
                new { id = 5, name = "Item5", price = 50.99, stock = 150 }
            }
        };

        var json = JsonSerializer.Serialize(data);
        var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

        var reduction = ((double)(json.Length - ploon.Length) / json.Length) * 100;

        _output.WriteLine("=== Small Dataset (5 items) ===");
        _output.WriteLine($"JSON:      {json.Length} chars");
        _output.WriteLine($"PLOON:     {ploon.Length} chars");
        _output.WriteLine($"Reduction: {reduction:F1}%");

        Assert.True(reduction > 30);
    }

    [Fact]
    public void Benchmark_MediumDataset_20Items()
    {
        var items = new List<object>();
        for (int i = 1; i <= 20; i++)
        {
            items.Add(new
            {
                id = i,
                name = $"Product{i}",
                category = "Electronics",
                price = 10.99 * i,
                stock = 100 - i
            });
        }

        var data = new { products = items.ToArray() };
        var json = JsonSerializer.Serialize(data);
        var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

        var reduction = ((double)(json.Length - ploon.Length) / json.Length) * 100;

        _output.WriteLine("=== Medium Dataset (20 items) ===");
        _output.WriteLine($"JSON:      {json.Length} chars");
        _output.WriteLine($"PLOON:     {ploon.Length} chars");
        _output.WriteLine($"Reduction: {reduction:F1}%");

        Assert.True(reduction > 40);
    }

    [Fact]
    public void Benchmark_LargeDataset_100Items()
    {
        var items = new List<object>();
        for (int i = 1; i <= 100; i++)
        {
            items.Add(new
            {
                id = i,
                name = $"Product{i}",
                category = i % 5 == 0 ? "Electronics" : "Accessories",
                price = 10.99 * i,
                stock = 100 - (i % 50),
                rating = 4.5
            });
        }

        var data = new { products = items.ToArray() };
        var json = JsonSerializer.Serialize(data);
        var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

        var reduction = ((double)(json.Length - ploon.Length) / json.Length) * 100;

        _output.WriteLine("=== Large Dataset (100 items) ===");
        _output.WriteLine($"JSON:      {json.Length} chars");
        _output.WriteLine($"PLOON:     {ploon.Length} chars");
        _output.WriteLine($"Reduction: {reduction:F1}%");

        Assert.True(reduction > 45);
    }

    [Fact]
    public void Benchmark_DeeplyNestedStructure()
    {
        var data = new
        {
            companies = new[]
            {
                new
                {
                    id = 1,
                    name = "TechCorp",
                    headquarters = new
                    {
                        country = "USA",
                        city = "San Francisco",
                        address = new
                        {
                            street = "123 Main St",
                            zip = "94105"
                        }
                    },
                    departments = new[]
                    {
                        new
                        {
                            name = "Engineering",
                            employees = new[]
                            {
                                new { name = "Alice", role = "Senior Dev", salary = 150000 },
                                new { name = "Bob", role = "Junior Dev", salary = 90000 }
                            }
                        },
                        new
                        {
                            name = "Sales",
                            employees = new[]
                            {
                                new { name = "Charlie", role = "Manager", salary = 120000 },
                                new { name = "Diana", role = "Rep", salary = 80000 }
                            }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(data);
        var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

        var reduction = ((double)(json.Length - ploon.Length) / json.Length) * 100;

        _output.WriteLine("=== Deeply Nested Structure ===");
        _output.WriteLine($"JSON:      {json.Length} chars");
        _output.WriteLine($"PLOON:     {ploon.Length} chars");
        _output.WriteLine($"Reduction: {reduction:F1}%");

        Assert.True(reduction > 25);
    }

    [Fact]
    public void Benchmark_WideStructure_ManyFields()
    {
        var data = new
        {
            records = new[]
            {
                new
                {
                    id = 1,
                    field1 = "value1",
                    field2 = "value2",
                    field3 = "value3",
                    field4 = "value4",
                    field5 = "value5",
                    field6 = 100,
                    field7 = 200,
                    field8 = 300,
                    field9 = true,
                    field10 = false
                },
                new
                {
                    id = 2,
                    field1 = "value1",
                    field2 = "value2",
                    field3 = "value3",
                    field4 = "value4",
                    field5 = "value5",
                    field6 = 150,
                    field7 = 250,
                    field8 = 350,
                    field9 = false,
                    field10 = true
                }
            }
        };

        var json = JsonSerializer.Serialize(data);
        var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

        var reduction = ((double)(json.Length - ploon.Length) / json.Length) * 100;

        _output.WriteLine("=== Wide Structure (Many Fields) ===");
        _output.WriteLine($"JSON:      {json.Length} chars");
        _output.WriteLine($"PLOON:     {ploon.Length} chars");
        _output.WriteLine($"Reduction: {reduction:F1}%");

        // Wide structures benefit greatly from no field name repetition
        Assert.True(reduction > 35);
    }

    [Fact]
    public void Benchmark_VerifyTargetReduction_49Percent()
    {
        // Create a representative dataset that should achieve ~49% reduction
        var items = new List<object>();
        for (int i = 1; i <= 50; i++)
        {
            items.Add(new
            {
                id = i,
                name = $"ProductName{i}",
                description = $"Description{i}",
                category = "Category",
                price = 99.99,
                quantity = 10,
                active = true
            });
        }

        var data = new { products = items.ToArray() };
        var json = JsonSerializer.Serialize(data);
        var ploon = Ploon.Stringify(data, new StringifyOptions { Format = PloonFormat.Compact });

        var reduction = ((double)(json.Length - ploon.Length) / json.Length) * 100;

        _output.WriteLine("=== Target Reduction Verification (50 items, 7 fields) ===");
        _output.WriteLine($"JSON:      {json.Length} chars");
        _output.WriteLine($"PLOON:     {ploon.Length} chars");
        _output.WriteLine($"Reduction: {reduction:F1}%");
        _output.WriteLine($"Target:    49.0%");

        // Should be close to 49% for this type of dataset
        Assert.True(reduction > 45, $"Expected >45% reduction, got {reduction:F1}%");
    }
}
