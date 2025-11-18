using System.Text;

namespace PloonNet.Example;

// The top-level complex model. The model + nested objects together contain well over 50 distinct fields.
// You can inspect the nested classes to see many property types (strings, ints, enums, nullable types, lists, dictionaries, byte arrays, GUIDs, DateTimes, decimals, doubles, etc.).
public class ComplexRecord
{
    public Guid Id { get; set; }                         // 1
    public int Index { get; set; }                       // 2
    public string Name { get; set; }                     // 3
    public string Description { get; set; }              // 4
    public DateTime CreatedAt { get; set; }              // 5
    public DateTime? UpdatedAt { get; set; }             // 6
    public bool IsActive { get; set; }                   // 7
    public StatusEnum Status { get; set; }               // 8
    public double Rating { get; set; }                   // 9
    public decimal Score { get; set; }                   // 10
    public int Count { get; set; }                       // 11
    public TimeSpan Duration { get; set; }               // 12
    public byte[] BinaryData { get; set; }               // 13
    public Coordinates Location { get; set; }            // 14-16 (3 fields inside)
    public Person Owner { get; set; }                    // many fields inside
    public List<Address> Addresses { get; set; }         // collection with nested fields
    public List<Order> Orders { get; set; }              // collection with nested fields
    public Dictionary<string, string> Attributes { get; set; } // map
    public Dictionary<string, double> Metrics { get; set; }    // map
    public List<string> Tags { get; set; }
    public HashSet<int> FlagIds { get; set; }
    public NestedStats Stats { get; set; }               // nested stats object
    public string[] Aliases { get; set; }
    public int? OptionalInt { get; set; }
    public Guid CorrelationId { get; set; }
    public char CategoryCode { get; set; }
    public IReadOnlyList<string> ReadOnlyNotes { get; set; }
}

public enum StatusEnum
{
    Unknown,
    New,
    Active,
    Suspended,
    Archived
}

public class Coordinates
{
    public double Latitude { get; set; }      // 1
    public double Longitude { get; set; }     // 2
    public double? Altitude { get; set; }     // 3
}

public class Person
{
    public Guid PersonId { get; set; }        // 1
    public string FirstName { get; set; }     // 2
    public string LastName { get; set; }      // 3
    public string Email { get; set; }         // 4
    public int Age { get; set; }              // 5
    public DateTime BirthDate { get; set; }   // 6
    public bool Verified { get; set; }        // 7
    public List<Phone> Phones { get; set; }   // collection
    public string MiddleName { get; set; }    // 8
    public decimal Salary { get; set; }       // 9
    public IEnumerable<string> Roles { get; set; }
}

public class Phone
{
    public string CountryCode { get; set; }
    public string Number { get; set; }
    public PhoneType Type { get; set; }
}

public enum PhoneType
{
    Mobile,
    Home,
    Work
}

public class Address
{
    public string Street { get; set; }        // 1
    public string City { get; set; }          // 2
    public string State { get; set; }         // 3
    public string PostalCode { get; set; }    // 4
    public string Country { get; set; }       // 5
    public bool Primary { get; set; }         // 6
}

public class Order
{
    public Guid OrderId { get; set; }
    public DateTime PlacedAt { get; set; }
    public List<OrderLine> Lines { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
}

public class OrderLine
{
    public string SKU { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public Dictionary<string, string> Meta { get; set; }
}

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

public class NestedStats
{
    public long Views { get; set; }
    public long Clicks { get; set; }
    public IDictionary<string, int> Breakdowns { get; set; }
    public double ConversionRate { get; set; }
}

static class Generator
{
    private static readonly string[] SampleWords =
    [
        "alpha","beta","gamma","delta","epsilon","omega","sigma","lambda","kappa","theta",
        "sun","moon","star","river","mountain","forest","ocean","valley","meadow","field"
    ];

    public static List<ComplexRecord> Generate(int size)
    {
        var rnd = new Random(1000);
        var complextRecords = new List<ComplexRecord>();
        for (int i = 0; i < size; i++)
        {
            complextRecords.Add(Generate(i, rnd));
        }
        return complextRecords;
    }

    private static ComplexRecord Generate(int index, Random rnd)
    {
        var rec = new ComplexRecord
        {
            Id = Guid.NewGuid(),
            Index = index,
            Name = $"Record {index} - {RandomWord(rnd)}",
            Description = RandomSentence(rnd, 6, 15),
            CreatedAt = RandomDate(rnd, daysBack: 3650),
            UpdatedAt = rnd.NextDouble() < 0.8 ? (DateTime?)RandomDate(rnd, daysBack: 365) : null,
            IsActive = rnd.Next(0, 2) == 1,
            Status = RandomEnum<StatusEnum>(rnd),
            Rating = Math.Round(rnd.NextDouble() * 5.0, 2),
            Score = Math.Round((decimal)rnd.NextDouble() * 1000m, 4),
            Count = rnd.Next(0, 10000),
            Duration = TimeSpan.FromSeconds(rnd.Next(0, 3600 * 24)),
            BinaryData = RandomBytes(rnd, 32),
            Location = new Coordinates
            {
                Latitude = rnd.NextDouble() * 180 - 90,
                Longitude = rnd.NextDouble() * 360 - 180,
                Altitude = rnd.Next(0, 2) == 1 ? (double?)rnd.Next(0, 10000) : null
            },
            Owner = GeneratePerson(rnd),
            Addresses = GenerateAddresses(rnd),
            Orders = GenerateOrders(rnd),
            Attributes = GenerateAttributes(rnd),
            Metrics = GenerateMetrics(rnd),
            Tags = GenerateTags(rnd),
            FlagIds = new HashSet<int> { rnd.Next(0, 100), rnd.Next(100, 200) },
            Stats = new NestedStats
            {
                Views = rnd.Next(0, 1_000_000),
                Clicks = rnd.Next(0, 100_000),
                Breakdowns = new Dictionary<string, int>
                    {
                        { "mobile", rnd.Next(0, 100_000) },
                        { "desktop", rnd.Next(0, 100_000) },
                        { "tablet", rnd.Next(0, 100_000) }
                    },
                ConversionRate = Math.Round(rnd.NextDouble(), 4)
            },
            Aliases = new[] { RandomWord(rnd), RandomWord(rnd) },
            OptionalInt = rnd.Next(0, 5) == 0 ? (int?)null : rnd.Next(0, 1000),
            CorrelationId = Guid.NewGuid(),
            CategoryCode = (char)('A' + rnd.Next(0, 26)),
            ReadOnlyNotes = new List<string> { RandomSentence(rnd, 2, 6), RandomSentence(rnd, 1, 4) }
        };

        return rec;
    }

    private static Person GeneratePerson(Random rnd)
    {
        return new Person
        {
            PersonId = Guid.NewGuid(),
            FirstName = RandomWord(rnd, 4),
            LastName = RandomWord(rnd, 6),
            MiddleName = rnd.Next(0, 4) == 0 ? null : RandomWord(rnd, 3),
            Email = $"{RandomWord(rnd, 6)}.{RandomWord(rnd, 4)}@example.com",
            Age = rnd.Next(18, 80),
            BirthDate = DateTime.UtcNow.AddYears(-rnd.Next(18, 80)).AddDays(-rnd.Next(0, 365)),
            Verified = rnd.Next(0, 2) == 1,
            Phones = new List<Phone>
                {
                    new Phone { CountryCode = "+1", Number = $"{rnd.Next(200,999)}-{rnd.Next(100,999)}-{rnd.Next(1000,9999)}", Type = PhoneType.Mobile },
                    new Phone { CountryCode = "+1", Number = $"{rnd.Next(200,999)}-{rnd.Next(100,999)}-{rnd.Next(1000,9999)}", Type = PhoneType.Work }
                },
            Salary = Math.Round((decimal)rnd.NextDouble() * 200_000m, 2),
            Roles = new[] { "user", rnd.Next(0, 10) == 0 ? "admin" : "editor" }
        };
    }

    private static List<Address> GenerateAddresses(Random rnd)
    {
        var count = rnd.Next(1, 4);
        var list = new List<Address>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new Address
            {
                Street = $"{rnd.Next(1, 9999)} {RandomWord(rnd)} St",
                City = RandomWord(rnd),
                State = "State" + rnd.Next(1, 50),
                PostalCode = $"{rnd.Next(10000, 99999)}",
                Country = "USA",
                Primary = i == 0
            });
        }
        return list;
    }

    private static List<Order> GenerateOrders(Random rnd)
    {
        var count = rnd.Next(0, 5);
        var orders = new List<Order>(count);
        for (int i = 0; i < count; i++)
        {
            var lines = new List<OrderLine>();
            for (int l = 0; l < rnd.Next(1, 4); l++)
            {
                lines.Add(new OrderLine
                {
                    SKU = $"SKU-{rnd.Next(1000, 9999)}",
                    Quantity = rnd.Next(1, 10),
                    Price = Math.Round((decimal)rnd.NextDouble() * 500m, 2),
                    Meta = new Dictionary<string, string> { { "color", RandomWord(rnd) }, { "size", $"{rnd.Next(1, 100)}" } }
                });
            }

            decimal total = 0;
            foreach (var ln in lines) total += ln.Price * ln.Quantity;

            orders.Add(new Order
            {
                OrderId = Guid.NewGuid(),
                PlacedAt = RandomDate(rnd, daysBack: 3650),
                Lines = lines,
                Status = RandomEnum<OrderStatus>(rnd),
                Total = Math.Round(total, 2)
            });
        }

        return orders;
    }

    private static Dictionary<string, string> GenerateAttributes(Random rnd)
    {
        var d = new Dictionary<string, string>
            {
                { "featureA", rnd.Next(0,2)==1 ? "enabled" : "disabled" },
                { "tier", rnd.Next(0,5).ToString() },
                { "release", $"r{rnd.Next(1,10)}.{rnd.Next(0,9)}" }
            };
        return d;
    }

    private static Dictionary<string, double> GenerateMetrics(Random rnd)
    {
        return new Dictionary<string, double>
            {
                { "cpu", Math.Round(rnd.NextDouble()*100,2) },
                { "memoryGb", Math.Round(rnd.NextDouble()*64,2) },
                { "latencyMs", Math.Round(rnd.NextDouble()*300,2) }
            };
    }

    private static List<string> GenerateTags(Random rnd)
    {
        var count = rnd.Next(1, 6);
        var list = new List<string>(count);
        for (int i = 0; i < count; i++) list.Add(RandomWord(rnd));
        return list;
    }

    private static byte[] RandomBytes(Random rnd, int size)
    {
        var b = new byte[size];
        rnd.NextBytes(b);
        return b;
    }

    private static T RandomEnum<T>(Random rnd) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        return values[rnd.Next(values.Length)];
    }

    private static DateTime RandomDate(Random rnd, int daysBack = 365)
    {
        var days = rnd.Next(0, daysBack);
        var seconds = rnd.Next(0, 86400);
        return DateTime.UtcNow.AddDays(-days).AddSeconds(-seconds);
    }

    private static string RandomWord(Random rnd, int length = -1)
    {
        if (length <= 0) return SampleWords[rnd.Next(SampleWords.Length)];
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append((char)('a' + rnd.Next(0, 26)));
        }
        return sb.ToString();
    }

    private static string RandomSentence(Random rnd, int minWords = 3, int maxWords = 8)
    {
        int w = rnd.Next(minWords, maxWords + 1);
        var parts = new List<string>(w);
        for (int i = 0; i < w; i++) parts.Add(RandomWord(rnd));
        return string.Join(' ', parts);
    }
}
