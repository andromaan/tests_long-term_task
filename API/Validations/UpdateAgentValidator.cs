using API.DTOs.Agent;
using FluentValidation;

namespace API.Validations;

public class UpdateAgentValidator : AbstractValidator<UpdateAgentDto>
{
    public UpdateAgentValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
    }
}
