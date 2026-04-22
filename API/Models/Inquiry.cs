namespace API.Models;

public class Inquiry
{
    public int Id { get; set; }

    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public bool IsResponded { get; set; }
}
