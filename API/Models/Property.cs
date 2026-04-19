namespace API.Models;

public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    
    public PropertyType Type { get; set; }
    public PropertyStatus Status { get; set; }
    
    public int AgentId { get; set; }
    public Agent? Agent { get; set; }
    
    public DateTime ListedAt { get; set; }
    
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
