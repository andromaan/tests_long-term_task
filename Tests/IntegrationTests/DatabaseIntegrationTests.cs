using System.Net.Http.Json;
using API.DTOs.Inquiry;
using API.DTOs.Property;
using API.Models;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tests.Common;

namespace Tests.IntegrationTests;

public class DatabaseIntegrationTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly Fixture _fixture;
    private Agent _agent1 = null!;
    private Agent _agent2 = null!;

    public DatabaseIntegrationTests(IntegrationTestWebFactory factory)
        : base(factory)
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task Complex_Query_With_Multiple_Filters_Returns_Correct_Results()
    {
        // Arrange - Create properties with different criteria
        var kyivApartments = _fixture
            .Build<Property>()
            .Without(p => p.Id)
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.City, "Kyiv")
            .With(p => p.Type, PropertyType.Apartment)
            .With(p => p.Price, 150000)
            .With(p => p.Bedrooms, 2)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(3)
            .ToList();

        var kyivHouses = _fixture
            .Build<Property>()
            .Without(p => p.Id)
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.City, "Kyiv")
            .With(p => p.Type, PropertyType.House)
            .With(p => p.Price, 350000)
            .With(p => p.Bedrooms, 4)
            .With(p => p.AgentId, _agent2.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(2)
            .ToList();

        var lvivApartments = _fixture
            .Build<Property>()
            .Without(p => p.Id)
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.City, "Lviv")
            .With(p => p.Type, PropertyType.Apartment)
            .With(p => p.Price, 100000)
            .With(p => p.Bedrooms, 2)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(2)
            .ToList();

        var allProperties = kyivApartments.Concat(kyivHouses).Concat(lvivApartments).ToList();
        Context.Properties.AddRange(allProperties);
        await Context.SaveChangesAsync();

        // Act - Query Kyiv houses with 4+ bedrooms in price range 300000-400000
        var response = await Client.GetAsync(
            "/api/properties?city=Kyiv&type=1&minPrice=300000&maxPrice=400000&bedrooms=4"
        );

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var properties = await response.Content.ReadFromJsonAsync<List<PropertyDto>>();
        properties.ShouldNotBeNull();
        properties.Count.ShouldBe(2);
        properties.ShouldAllBe(p =>
            p.City == "Kyiv" && p.Type == PropertyType.House && p.Bedrooms == 4
        );
    }

    [Fact]
    public async Task Agent_Has_Multiple_Properties_And_Can_Retrieve_Them()
    {
        // Arrange - Agent with multiple properties
        var properties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(5)
            .ToList();

        Context.Properties.AddRange(properties);
        await Context.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/agents/{_agent1.Id}/properties");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var agentProperties = await response.Content.ReadFromJsonAsync<List<PropertyDto>>();
        agentProperties.ShouldNotBeNull();
        agentProperties.Count.ShouldBe(5);
        agentProperties.ShouldAllBe(p => p.AgentId == _agent1.Id);
    }

    [Fact]
    public async Task Multiple_Agents_Maintain_Separate_Property_Lists()
    {
        // Arrange - Two agents with properties
        var agent1Properties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(3)
            .ToList();

        var agent2Properties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.AgentId, _agent2.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(4)
            .ToList();

        Context.Properties.AddRange(agent1Properties.Concat(agent2Properties));
        await Context.SaveChangesAsync();

        // Act
        var response1 = await Client.GetAsync($"/api/agents/{_agent1.Id}/properties");
        var response2 = await Client.GetAsync($"/api/agents/{_agent2.Id}/properties");

        // Assert
        response1.IsSuccessStatusCode.ShouldBeTrue();
        response2.IsSuccessStatusCode.ShouldBeTrue();

        var properties1 = await response1.Content.ReadFromJsonAsync<List<PropertyDto>>();
        var properties2 = await response2.Content.ReadFromJsonAsync<List<PropertyDto>>();

        properties1.ShouldNotBeNull();
        properties2.ShouldNotBeNull();
        properties1.Count.ShouldBe(3);
        properties2.Count.ShouldBe(4);

        properties1.ShouldAllBe(p => p.AgentId == _agent1.Id);
        properties2.ShouldAllBe(p => p.AgentId == _agent2.Id);
    }

    [Fact]
    public async Task Property_Can_Have_Multiple_Inquiries_Tracked()
    {
        // Arrange
        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Available)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .Create();

        Context.Properties.Add(property);
        await Context.SaveChangesAsync();

        // Create multiple inquiries directly in DB
        var inquiries = _fixture
            .Build<Inquiry>()
            .Without(i => i.Property)
            .With(i => i.PropertyId, property.Id)
            .With(i => i.CreatedAt, DateTime.UtcNow)
            .CreateMany(3)
            .ToList();

        Context.Inquiries.AddRange(inquiries);
        await Context.SaveChangesAsync();

        // Act - Verify all inquiries are saved in DB
        var savedInquiries = await Context
            .Inquiries.Where(i => i.PropertyId == property.Id)
            .ToListAsync();

        // Assert
        savedInquiries.Count.ShouldBe(3);
        savedInquiries.ShouldAllBe(i => i.PropertyId == property.Id);
        savedInquiries.Select(i => i.Id).ShouldBe(inquiries.Select(i => i.Id), ignoreOrder: true);
    }

    [Fact]
    public async Task Inquiries_Cannot_Be_Submitted_For_Sold_Property()
    {
        // Arrange
        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Sold)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .Create();

        Context.Properties.Add(property);
        await Context.SaveChangesAsync();

        var inquiryDto = _fixture.Build<CreateInquiryDto>().Create();

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/properties/{property.Id}/inquiries",
            inquiryDto
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);

        // Verify no inquiry was created
        var inquiries = await Context
            .Inquiries.Where(i => i.PropertyId == property.Id)
            .ToListAsync();
        inquiries.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Inquiries_Cannot_Be_Submitted_For_Rented_Property()
    {
        // Arrange
        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Rented)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .Create();

        Context.Properties.Add(property);
        await Context.SaveChangesAsync();

        var inquiryDto = _fixture.Build<CreateInquiryDto>().Create();

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/properties/{property.Id}/inquiries",
            inquiryDto
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Filter_By_Price_Range_Returns_Properties_Within_Range()
    {
        // Arrange
        var cheapProperties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Price, 50000)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(2)
            .ToList();

        var mediumProperties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Price, 200000)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(3)
            .ToList();

        var expensiveProperties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Price, 500000)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(2)
            .ToList();

        var allProperties = cheapProperties
            .Concat(mediumProperties)
            .Concat(expensiveProperties)
            .ToList();
        Context.Properties.AddRange(allProperties);
        await Context.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/properties?minPrice=150000&maxPrice=250000");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var properties = await response.Content.ReadFromJsonAsync<List<PropertyDto>>();
        properties.ShouldNotBeNull();
        properties.Count.ShouldBe(3);
        properties.ShouldAllBe(p => p.Price >= 150000 && p.Price <= 250000);
    }

    [Fact]
    public async Task Filter_By_Bedrooms_Returns_Properties_With_Exact_Bedroom_Count()
    {
        // Arrange
        var studio = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Bedrooms, 0)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(1)
            .ToList();

        var oneBedroom = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Bedrooms, 1)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(2)
            .ToList();

        var threeBedroom = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Bedrooms, 3)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(3)
            .ToList();

        var allProperties = studio.Concat(oneBedroom).Concat(threeBedroom).ToList();
        Context.Properties.AddRange(allProperties);
        await Context.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/properties?bedrooms=3");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var properties = await response.Content.ReadFromJsonAsync<List<PropertyDto>>();
        properties.ShouldNotBeNull();
        properties.Count.ShouldBe(3);
        properties.ShouldAllBe(p => p.Bedrooms == 3);
    }

    [Fact]
    public async Task Complex_Combined_Filters_Narrow_Results_Correctly()
    {
        // Arrange - Create a diverse set of properties
        var properties = new List<Property>();

        // Kyiv, Houses, 300k-500k, 3+ bedrooms
        properties.AddRange(
            _fixture
                .Build<Property>()
                .Without(p => p.Agent)
                .Without(p => p.Inquiries)
                .With(p => p.City, "Kyiv")
                .With(p => p.Type, PropertyType.House)
                .With(p => p.Price, 400000)
                .With(p => p.Bedrooms, 4)
                .With(p => p.AgentId, _agent1.Id)
                .With(p => p.ListedAt, DateTime.UtcNow)
                .CreateMany(2)
        );

        // Kyiv, Apartments, 100k-200k, 2 bedrooms
        properties.AddRange(
            _fixture
                .Build<Property>()
                .Without(p => p.Agent)
                .Without(p => p.Inquiries)
                .With(p => p.City, "Kyiv")
                .With(p => p.Type, PropertyType.Apartment)
                .With(p => p.Price, 150000)
                .With(p => p.Bedrooms, 2)
                .With(p => p.AgentId, _agent1.Id)
                .With(p => p.ListedAt, DateTime.UtcNow)
                .CreateMany(3)
        );

        // Lviv, Houses, 300k-500k, 3+ bedrooms
        properties.AddRange(
            _fixture
                .Build<Property>()
                .Without(p => p.Agent)
                .Without(p => p.Inquiries)
                .With(p => p.City, "Lviv")
                .With(p => p.Type, PropertyType.House)
                .With(p => p.Price, 400000)
                .With(p => p.Bedrooms, 4)
                .With(p => p.AgentId, _agent2.Id)
                .With(p => p.ListedAt, DateTime.UtcNow)
                .CreateMany(2)
        );

        Context.Properties.AddRange(properties);
        await Context.SaveChangesAsync();

        // Act - Query: Kyiv + Houses + 350k-450k + 4 bedrooms
        var response = await Client.GetAsync(
            "/api/properties?city=Kyiv&type=1&minPrice=350000&maxPrice=450000&bedrooms=4"
        );

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var foundProperties = await response.Content.ReadFromJsonAsync<List<PropertyDto>>();
        foundProperties.ShouldNotBeNull();
        foundProperties.Count.ShouldBe(2);
        foundProperties.ShouldAllBe(p =>
            p.City == "Kyiv"
            && p.Type == PropertyType.House
            && p.Price >= 350000
            && p.Price <= 450000
            && p.Bedrooms == 4
        );
    }

    [Fact]
    public async Task Agent_Can_Retrieve_All_Inquiries_For_Their_Properties()
    {
        // Arrange - Create properties for agent1 and inquiries in DB
        var agent1Properties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .CreateMany(2)
            .ToList();

        Context.Properties.AddRange(agent1Properties);
        await Context.SaveChangesAsync();

        // Create inquiries directly in DB
        var inquiries = new List<Inquiry>();
        foreach (var property in agent1Properties)
        {
            inquiries.AddRange(
                _fixture
                    .Build<Inquiry>()
                    .Without(i => i.Property)
                    .With(i => i.PropertyId, property.Id)
                    .With(i => i.CreatedAt, DateTime.UtcNow)
                    .CreateMany(2)
            );
        }

        Context.Inquiries.AddRange(inquiries);
        await Context.SaveChangesAsync();

        // Act - Get all inquiries for agent1 from DB
        var agentInquiries = await Context
            .Inquiries.Include(i => i.Property)
            .Where(i => i.Property != null && i.Property.AgentId == _agent1.Id)
            .ToListAsync();

        // Assert
        agentInquiries.Count.ShouldBe(4);
        agentInquiries.ShouldAllBe(i => i.Property!.AgentId == _agent1.Id);
    }

    [Fact]
    public async Task Different_Agents_Maintain_Separate_Inquiry_Lists()
    {
        // Arrange - Create properties for both agents
        var agent1Property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.AgentId, _agent1.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .Create();

        var agent2Property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.AgentId, _agent2.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .Create();

        Context.Properties.AddRange(agent1Property, agent2Property);
        await Context.SaveChangesAsync();

        // Create inquiries directly in DB
        var agent1Inquiries = _fixture
            .Build<Inquiry>()
            .Without(i => i.Property)
            .With(i => i.PropertyId, agent1Property.Id)
            .With(i => i.CreatedAt, DateTime.UtcNow)
            .CreateMany(2)
            .ToList();

        var agent2Inquiry = _fixture
            .Build<Inquiry>()
            .Without(i => i.Property)
            .With(i => i.PropertyId, agent2Property.Id)
            .With(i => i.CreatedAt, DateTime.UtcNow)
            .Create();

        Context.Inquiries.AddRange(agent1Inquiries);
        Context.Inquiries.Add(agent2Inquiry);
        await Context.SaveChangesAsync();

        // Act - Get inquiries for each agent from DB
        var agent1InquiriesFromDb = await Context
            .Inquiries.Include(i => i.Property)
            .Where(i => i.Property != null && i.Property.AgentId == _agent1.Id)
            .ToListAsync();

        var agent2InquiriesFromDb = await Context
            .Inquiries.Include(i => i.Property)
            .Where(i => i.Property != null && i.Property.AgentId == _agent2.Id)
            .ToListAsync();

        // Assert
        agent1InquiriesFromDb.Count.ShouldBe(2);
        agent2InquiriesFromDb.Count.ShouldBe(1);
        agent1InquiriesFromDb.ShouldAllBe(i => i.Property!.AgentId == _agent1.Id);
        agent2InquiriesFromDb.ShouldAllBe(i => i.Property!.AgentId == _agent2.Id);
    }

    public async Task InitializeAsync()
    {
        // Recreate context for this test to ensure clean state
        Context = CreateNewContext();

        await ClearDatabaseAsync();

        // Create test agents
        _agent1 = new Agent
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "1234567890",
            LicenseNumber = "LIC001",
        };

        _agent2 = new Agent
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "0987654321",
            LicenseNumber = "LIC002",
        };

        Context.Agents.AddRange(_agent1, _agent2);
        await SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
