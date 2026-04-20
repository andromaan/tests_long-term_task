using API.DTOs.Property;
using API.Validations;
using AutoFixture;
using FluentValidation.TestHelper;

namespace Tests.UnitTests;

public class UpdatePropertyValidatorTests
{
    private readonly UpdatePropertyValidator _sut;
    private readonly Fixture _fixture;

    public UpdatePropertyValidatorTests()
    {
        _sut = new UpdatePropertyValidator();
        _fixture = new Fixture();
    }

    [Theory]
    [InlineData(1, 100, 1, 1)] // Positive price, area, proper bedrooms/bathrooms
    [InlineData(100000, 50, 0, 0)] // Edge case: zero bedrooms/bathrooms is valid
    public void Should_Pass_Validation_When_PriceAndAreaArePositive(decimal price, decimal area, int bedrooms, int bathrooms)
    {
        // Arrange
        var dto = _fixture.Build<UpdatePropertyDto>()
            .With(p => p.Price, price)
            .With(p => p.Area, area)
            .With(p => p.Bedrooms, bedrooms)
            .With(p => p.Bathrooms, bathrooms)
            .Create();

        // Act
        var result = _sut.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
        result.ShouldNotHaveValidationErrorFor(x => x.Area);
        result.ShouldNotHaveValidationErrorFor(x => x.Bedrooms);
        result.ShouldNotHaveValidationErrorFor(x => x.Bathrooms);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-100, 50)]
    [InlineData(50000, 0)]
    [InlineData(50000, -10)]
    public void Should_Fail_Validation_When_PriceOrAreaAreNotPositive(decimal price, decimal area)
    {
        // Arrange
        var dto = _fixture.Build<UpdatePropertyDto>()
            .With(p => p.Price, price)
            .With(p => p.Area, area)
            .With(p => p.Bedrooms, 2)
            .With(p => p.Bathrooms, 1)
            .Create();

        // Act
        var result = _sut.TestValidate(dto);

        // Assert        
        if (price <= 0)
            result.ShouldHaveValidationErrorFor(x => x.Price);
            
        if (area <= 0)
            result.ShouldHaveValidationErrorFor(x => x.Area);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(2, -1)]
    [InlineData(-5, -5)]
    public void Should_Fail_Validation_When_BedroomsOrBathroomsAreNegative(int bedrooms, int bathrooms)
    {
        // Arrange
        var dto = _fixture.Build<UpdatePropertyDto>()
            .With(p => p.Price, 100000)
            .With(p => p.Area, 100)
            .With(p => p.Bedrooms, bedrooms)
            .With(p => p.Bathrooms, bathrooms)
            .Create();

        // Act
        var result = _sut.TestValidate(dto);

        // Assert
        if (bedrooms < 0)
            result.ShouldHaveValidationErrorFor(x => x.Bedrooms);
            
        if (bathrooms < 0)
            result.ShouldHaveValidationErrorFor(x => x.Bathrooms);
    }
}
