using API.Data;
using API.Models;
using AutoFixture;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public interface IDataSeedService
{
    Task SeedAsync();
}

public class DataSeedService : IDataSeedService
{
    private readonly AppDbContext _context;
    private readonly Fixture _fixture;

    private const int AGENT_COUNT = 150;
    private const int PROPERTIES_PER_AGENT = 46;
    private const int INQUIRIES_PER_10_PROPERTIES = 4;

    private readonly string[] CITIES =
    [
        "Kyiv",
        "Lviv",
        "Odesa",
        "Dnipro",
        "Kharkiv",
        "Rivne",
        "Vinnytsia",
        "Zaporizhzhia",
        "Mykolaiv",
        "Chernivtsi",
    ];

    public DataSeedService(AppDbContext context)
    {
        _context = context;
        _fixture = new Fixture();
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public async Task SeedAsync()
    {
        if (await _context.Agents.AnyAsync())
        {
            Console.WriteLine("Database already seeded. Skipping...");
            return;
        }

        try
        {
            // 1. Генеруємо агентів (~150)
            Console.WriteLine($"Creating {AGENT_COUNT} agents...");
            var agents = GenerateAgents(AGENT_COUNT);
            _context.Agents.AddRange(agents);
            await _context.SaveChangesAsync();

            // 2. Генеруємо об'єкти нерухомості (~7000)
            Console.WriteLine($"Creating ~{AGENT_COUNT * PROPERTIES_PER_AGENT} properties...");
            var properties = GenerateProperties(agents);
            _context.Properties.AddRange(properties);
            await _context.SaveChangesAsync();

            // 3. Генеруємо запити (~3000)
            Console.WriteLine(
                $"Creating ~{(properties.Count / 10) * INQUIRIES_PER_10_PROPERTIES} inquiries..."
            );
            var inquiries = GenerateInquiries(properties);
            _context.Inquiries.AddRange(inquiries);
            await _context.SaveChangesAsync();

            Console.WriteLine(
                "Database seeding completed successfully! "
                    + $"({agents.Count} agents, {properties.Count} properties, {inquiries.Count} inquiries)"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during seeding: {ex.Message}");
            throw;
        }
    }

    private List<Agent> GenerateAgents(int count)
    {
        var agents = new List<Agent>();
        var firstNames = new[]
        {
            "John",
            "Jane",
            "Michael",
            "Sarah",
            "David",
            "Emily",
            "James",
            "Lisa",
            "Robert",
            "Maria",
        };
        var lastNames = new[]
        {
            "Smith",
            "Johnson",
            "Williams",
            "Brown",
            "Jones",
            "Garcia",
            "Miller",
            "Davis",
            "Rodriguez",
            "Martinez",
        };

        for (int i = 0; i < count; i++)
        {
            var agent = new Agent
            {
                FirstName = firstNames[i % firstNames.Length],
                LastName = lastNames[i % lastNames.Length],
                Email = $"agent_{i}@realestate.com",
                Phone = $"+380{random.Next(50, 99)}{random.Next(1000000, 9999999)}",
                LicenseNumber = $"LIC{i:000000}",
            };
            agents.Add(agent);
        }

        return agents;
    }

    private List<Property> GenerateProperties(List<Agent> agents)
    {
        var properties = new List<Property>();
        var propertyTypes = new[]
        {
            PropertyType.Apartment,
            PropertyType.House,
            PropertyType.Commercial,
        };

        foreach (var agent in agents)
        {
            for (int i = 0; i < PROPERTIES_PER_AGENT; i++)
            {
                var property = _fixture
                    .Build<Property>()
                    .Without(p => p.Id)
                    .Without(p => p.Agent)
                    .Without(p => p.Inquiries)
                    .With(p => p.AgentId, agent.Id)
                    .With(p => p.City, CITIES[random.Next(CITIES.Length)])
                    .With(p => p.Type, propertyTypes[random.Next(propertyTypes.Length)])
                    .With(p => p.Price, decimal.CreateChecked(random.Next(50000, 1000000)))
                    .With(p => p.Area, decimal.CreateChecked(random.Next(30, 500)))
                    .With(p => p.Bedrooms, random.Next(0, 5))
                    .With(p => p.Bathrooms, random.Next(1, 4))
                    .With(p => p.Status, GetRandomStatus())
                    .With(p => p.ListedAt, DateTime.UtcNow.AddDays(-random.Next(0, 365)))
                    .Create();

                properties.Add(property);
            }
        }

        return properties;
    }

    private List<Inquiry> GenerateInquiries(List<Property> properties)
    {
        int total = (properties.Count / 10) * INQUIRIES_PER_10_PROPERTIES; // 2760
        var shuffled = properties.OrderBy(_ => Random.Shared.Next()).ToList();

        var inquiries = new List<Inquiry>();
        int created = 0;

        foreach (var property in shuffled)
        {
            if (created >= total)
                break;

            int count = Math.Min(Random.Shared.Next(1, 5), total - created);
            for (int i = 0; i < count; i++)
            {
                inquiries.Add(
                    new Inquiry
                    {
                        PropertyId = property.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30)),
                        IsResponded = Random.Shared.Next(2) == 0,
                    }
                );
                created++;
            }
        }
        return inquiries;
    }

    private PropertyStatus GetRandomStatus()
    {
        // 80% Available, 15% Sold, 5% Rented
        var roll = random.Next(100);
        if (roll < 80)
            return PropertyStatus.Available;
        if (roll < 95)
            return PropertyStatus.Sold;
        return PropertyStatus.Rented;
    }

    private static readonly Random random = new Random();
}
