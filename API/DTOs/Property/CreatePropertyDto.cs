using API.Models;

namespace API.DTOs.Property;

public record CreatePropertyDto(
    string Title,
    string Description,
    string Address,
    string City,
    decimal Price,
    decimal Area,
    int Bedrooms,
    int Bathrooms,
    PropertyType Type,
    int AgentId)
{
    public Models.Property ToModel() => new Models.Property
    {
        Title = Title,
        Description = Description,
        Address = Address,
        City = City,
        Price = Price,
        Area = Area,
        Bedrooms = Bedrooms,
        Bathrooms = Bathrooms,
        Type = Type,
        AgentId = AgentId,
        Status = PropertyStatus.Available,
        ListedAt = DateTime.UtcNow
    };
}

