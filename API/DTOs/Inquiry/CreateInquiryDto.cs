namespace API.DTOs.Inquiry;

public record CreateInquiryDto(
    string Name,
    string Email,
    string Phone,
    string Message)
{
    public Models.Inquiry ToModel(int propertyId) => new Models.Inquiry
    {
        PropertyId = propertyId,
        Name = Name,
        Email = Email,
        Phone = Phone,
        Message = Message,
        CreatedAt = DateTime.UtcNow,
        IsResponded = false
    };
}

