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
    private const int PROPERTIES_PER_AGENT = 46; // ~7000 properties total
    private const int INQUIRIES_PER_10_PROPERTIES = 4; // ~3000 inquiries total

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
        // Перевіримо, чи вже дані є в БД
        if (await _context.Agents.AnyAsync())
        {
            Console.WriteLine("Database already seeded. Skipping...");
            return;
        }

        Console.WriteLine("🌱 Starting database seeding...");

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
                LicenseNumber = $"LIC{i:000000}", // Unique license numbers
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
        var inquiries = new List<Inquiry>();

        // Генеруємо запити для випадково обраних об'єктів
        // ~43% від об'єктів отримуватимуть запити (3000 / 7000)
        var propertiesToQueryAgainst = properties
            .Where(_ => random.NextDouble() < 0.43) // 43% обраних об'єктів
            .ToList();

        foreach (var property in propertiesToQueryAgainst)
        {
            // Кожен обраний об'єкт отримує 1-4 запити
            int inquiryCount = random.Next(1, 5);

            for (int i = 0; i < inquiryCount; i++)
            {
                var inquiry = _fixture
                    .Build<Inquiry>()
                    .Without(i => i.Property)
                    .With(i => i.PropertyId, property.Id)
                    .With(i => i.CreatedAt, DateTime.UtcNow.AddDays(-random.Next(0, 30)))
                    .With(i => i.IsResponded, random.Next(2) == 0) // 50% of inquiries are responded
                    .Create();

                inquiries.Add(inquiry);
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
