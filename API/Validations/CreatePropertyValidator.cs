using API.DTOs.Property;
using FluentValidation;

namespace API.Validations;

public class CreatePropertyValidator : AbstractValidator<CreatePropertyDto>
{
    public CreatePropertyValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be positive");
        RuleFor(x => x.Area).GreaterThan(0).WithMessage("Area must be positive");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Bedrooms must be non-negative");
        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Bathrooms must be non-negative");

        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.AgentId).GreaterThan(0);
    }
}
