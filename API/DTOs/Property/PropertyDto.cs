using API.Models;

namespace API.DTOs.Property;

public record PropertyDto(
    int Id,
    string Title,
    string Description,
    string Address,
    string City,
    decimal Price,
    decimal Area,
    int Bedrooms,
    int Bathrooms,
    PropertyType Type,
    PropertyStatus Status,
    int AgentId,
    DateTime ListedAt)
{
    public static PropertyDto FromModel(Models.Property property) => new(
        property.Id,
        property.Title,
        property.Description,
        property.Address,
        property.City,
        property.Price,
        property.Area,
        property.Bedrooms,
        property.Bathrooms,
        property.Type,
        property.Status,
        property.AgentId,
        property.ListedAt);
}

