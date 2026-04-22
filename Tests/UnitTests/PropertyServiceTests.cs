using API.Data;
using API.Models;
using API.Services;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Tests.UnitTests;

public class PropertyServiceTests
{
    private readonly AppDbContext _context;
    private readonly PropertyService _sut;
    private readonly Fixture _fixture;

    public PropertyServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new PropertyService(_context);

        _fixture = new Fixture();
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_UpdateStatus_When_PropertyExists()
    {
        // Arrange
        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Available)
            .Create();

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        var newStatus = PropertyStatus.Sold;

        // Act
        var result = await _sut.ChangeStatusAsync(property.Id, newStatus);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(newStatus);

        var savedProperty = await _context.Properties.FindAsync(property.Id);
        savedProperty.ShouldNotBeNull();
        savedProperty.Status.ShouldBe(newStatus);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_ReturnNull_When_PropertyDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;
        var newStatus = PropertyStatus.Rented;

        // Act
        var result = await _sut.ChangeStatusAsync(nonExistentId, newStatus);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SubmitInquiryAsync_Should_ThrowException_When_PropertyIsSold()
    {
        // Arrange
        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Sold)
            .Create();

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        var inquiry = _fixture.Build<Inquiry>().Without(i => i.Property).Create();

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.SubmitInquiryAsync(property.Id, inquiry)
        );

        exception.Message.ShouldBe("Cannot submit an inquiry for a sold or rented property.");
    }

    [Fact]
    public async Task SubmitInquiryAsync_Should_ThrowException_When_PropertyIsRented()
    {
        // Arrange
        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Rented)
            .Create();

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        var inquiry = _fixture.Build<Inquiry>().Without(i => i.Property).Create();

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.SubmitInquiryAsync(property.Id, inquiry)
        );

        exception.Message.ShouldBe("Cannot submit an inquiry for a sold or rented property.");
    }

    [Fact]
    public async Task SubmitInquiryAsync_Should_AddInquiry_When_PropertyIsAvailable()
    {
        // Arrange
        var property = _fixture
            .Build<Property>()
            .Without(p => p.Agent)
            .Without(p => p.Inquiries)
            .With(p => p.Status, PropertyStatus.Available)
            .Create();

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        var inquiry = _fixture
            .Build<Inquiry>()
            .Without(i => i.Id)
            .Without(i => i.Property)
            .With(i => i.PropertyId, property.Id)
            .Create();

        // Act
        var result = await _sut.SubmitInquiryAsync(property.Id, inquiry);

        // Assert
        result.ShouldNotBeNull();
        result.PropertyId.ShouldBe(property.Id);

        var savedInquiry = await _context.Inquiries.FirstOrDefaultAsync(i => i.Id == result.Id);
        savedInquiry.ShouldNotBeNull();
        savedInquiry.Message.ShouldBe(inquiry.Message);
    }
}
