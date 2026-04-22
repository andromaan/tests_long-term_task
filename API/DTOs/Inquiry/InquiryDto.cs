namespace API.DTOs.Inquiry;

public record InquiryDto(
    int Id,
    int PropertyId,
    string Name,
    string Email,
    string Phone,
    string Message,
    DateTime CreatedAt,
    bool IsResponded
)
{
    public static InquiryDto FromModel(Models.Inquiry inquiry) =>
        new(
            inquiry.Id,
            inquiry.PropertyId,
            inquiry.Name,
            inquiry.Email,
            inquiry.Phone,
            inquiry.Message,
            inquiry.CreatedAt,
            inquiry.IsResponded
        );
}
