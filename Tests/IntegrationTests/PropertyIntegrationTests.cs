using System.Net.Http.Json;
using API.DTOs.Inquiry;
using API.DTOs.Property;
using API.Models;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tests.Common;

namespace Tests.IntegrationTests;

public class PropertyIntegrationTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly Fixture _fixture;
    private Agent _testAgent = null!;

    public PropertyIntegrationTests(IntegrationTestWebFactory factory)
        : base(factory)
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    private async Task<Agent> CreateTestAgentAsync()
    {
        var agent = _fixture.Build<Agent>().Without(a => a.Properties).Create();

        Context.Agents.Add(agent);
        await Context.SaveChangesAsync();
        return agent;
    }

    [Fact]
    public async Task Can_Post_Property_Successfully()
    {
        // Arrange
        var dto = _fixture
            .Build<CreatePropertyDto>()
            .With(p => p.Price, 200000)
            .With(p => p.Area, 120)
            .With(p => p.Bedrooms, 3)
            .With(p => p.Bathrooms, 2)
            .With(p => p.AgentId, _testAgent.Id)
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync("/api/properties", dto);

        // Assert
        var err = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.ShouldBeTrue(err);
        var returnedProperty = await response.Content.ReadFromJsonAsync<PropertyDto>();
        returnedProperty.ShouldNotBeNull();
        returnedProperty.Title.ShouldBe(dto.Title);
        returnedProperty.Status.ShouldBe(PropertyStatus.Available); // Default status
    }

    [Fact]
    public async Task Can_Search_Properties_With_Multiple_Filters()
    {
        // Arrange
        // Create random properties
        var properties = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.AgentId, _testAgent.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
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
            .With(p => p.AgentId, _testAgent.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .Create();

        properties.Add(targetProperty);
        Context.Properties.AddRange(properties);
        await Context.SaveChangesAsync();

        // Act - query matching parameters
        var response = await Client.GetAsync(
            $"/api/properties?city=Kyiv&type=1&minPrice=100000&maxPrice=200000&bedrooms=4"
        ); // 1 is House

        // Assert
        var err = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.ShouldBeTrue(err);
        var returnedProperties = await response.Content.ReadFromJsonAsync<List<PropertyDto>>();
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
        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Available)
            .With(p => p.AgentId, _testAgent.Id)
            .With(p => p.ListedAt, DateTime.UtcNow)
            .Create();

        Context.Properties.Add(property);
        await Context.SaveChangesAsync();

        var inquiryDto = _fixture
            .Build<CreateInquiryDto>()
            .With(i => i.Email, "test@example.com")
            .With(i => i.Phone, "1234567890")
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/properties/{property.Id}/inquiries",
            inquiryDto
        );

        // Assert
        var err = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.ShouldBeTrue(err);
        var returnedInquiry = await response.Content.ReadFromJsonAsync<InquiryDto>();
        returnedInquiry.ShouldNotBeNull();
        returnedInquiry.Message.ShouldBe(inquiryDto.Message);

        // Verify in DB
        var savedInquiry = await Context.Inquiries.FirstOrDefaultAsync(i =>
            i.Id == returnedInquiry.Id
        );
        savedInquiry.ShouldNotBeNull();
        savedInquiry.PropertyId.ShouldBe(property.Id);
    }

    public async Task InitializeAsync()
    {
        await ClearDatabaseAsync();
        _testAgent = await CreateTestAgentAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
