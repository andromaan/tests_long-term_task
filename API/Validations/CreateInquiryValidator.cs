using API.DTOs.Inquiry;
using FluentValidation;

namespace API.Validations;

public class CreateInquiryValidator : AbstractValidator<CreateInquiryDto>
{
    public CreateInquiryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Message).NotEmpty();
    }
}

