using System.Net.Http.Json;
using API.Data;
using API.DTOs.Inquiry;
using API.DTOs.Property;
using API.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Tests.IntegrationTests;

public class PropertyIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Fixture _fixture;

    public PropertyIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                );

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var root = new InMemoryDatabaseRoot();

                var provider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<AppDbContext>(
                    (container, options) =>
                    {
                        options
                            .UseInMemoryDatabase("IntegrationTestsDb", root)
                            .UseInternalServiceProvider(provider);
                    }
                );
            });
        });

        _client = _factory.CreateClient();
        _fixture = new Fixture();
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    private async Task<Agent> CreateTestAgentAsync()
    {
        var agent = _fixture.Build<Agent>().Without(a => a.Properties).Create();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Agents.Add(agent);
        await context.SaveChangesAsync();
        return agent;
    }

    [Fact]
    public async Task Can_Post_Property_Successfully()
    {
        // Arrange
        var agent = await CreateTestAgentAsync();

        var dto = _fixture
            .Build<CreatePropertyDto>()
            .With(p => p.Price, 200000)
            .With(p => p.Area, 120)
            .With(p => p.Bedrooms, 3)
            .With(p => p.Bathrooms, 2)
            .With(p => p.AgentId, agent.Id)
            .Create();

        // Act
        var response = await _client.PostAsJsonAsync("/api/properties", dto);

        // Assert
        var err = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.ShouldBeTrue(err);
        var returnedProperty = await response.Content.ReadFromJsonAsync<Property>();
        returnedProperty.ShouldNotBeNull();
        returnedProperty.Title.ShouldBe(dto.Title);
        returnedProperty.Status.ShouldBe(PropertyStatus.Available); // Default status
    }

    [Fact]
    public async Task Can_Search_Properties_With_Multiple_Filters()
    {
        // Arrange
        var agent = await CreateTestAgentAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create random properties
        var properties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.AgentId, agent.Id)
            .CreateMany(5)
            .ToList();

        // Target property to find
        var targetProperty = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.City, "Kyiv")
            .With(p => p.Type, PropertyType.House)
            .With(p => p.Price, 150000)
            .With(p => p.Bedrooms, 4)
            .With(p => p.AgentId, agent.Id)
            .Create();

        properties.Add(targetProperty);
        context.Properties.AddRange(properties);
        await context.SaveChangesAsync();

        // Act - query matching parameters
        var response = await _client.GetAsync(
            $"/api/properties?city=Kyiv&type=1&minPrice=100000&maxPrice=200000&bedrooms=4"
        ); // 1 is House

        // Assert
        var err = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.ShouldBeTrue(err);
        var returnedProperties = await response.Content.ReadFromJsonAsync<List<Property>>();
        returnedProperties.ShouldNotBeNull();

        var found = returnedProperties.ShouldHaveSingleItem();
        found.City.ShouldBe("Kyiv");
        found.Type.ShouldBe(PropertyType.House);
        found.Bedrooms.ShouldBe(4);
    }

    [Fact]
    public async Task Can_Submit_Inquiry_For_Available_Property()
    {
        // Arrange
        var agent = await CreateTestAgentAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Available)
            .With(p => p.AgentId, agent.Id)
            .Create();

        context.Properties.Add(property);
        await context.SaveChangesAsync();

        var inquiryDto = _fixture
            .Build<CreateInquiryDto>()
            .With(i => i.Email, "test@example.com")
            .With(i => i.Phone, "1234567890")
            .Create();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/properties/{property.Id}/inquiries",
            inquiryDto
        );

        // Assert
        var err = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.ShouldBeTrue(err);
        var returnedInquiry = await response.Content.ReadFromJsonAsync<Inquiry>();
        returnedInquiry.ShouldNotBeNull();
        returnedInquiry.Message.ShouldBe(inquiryDto.Message);

        // Verify in DB
        var savedInquiry = await context.Inquiries.FirstOrDefaultAsync(i =>
            i.Id == returnedInquiry.Id
        );
        savedInquiry.ShouldNotBeNull();
        savedInquiry.PropertyId.ShouldBe(property.Id);
    }
}
