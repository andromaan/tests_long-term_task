using API.Models;

namespace API.Services;

public interface IPropertyService
{
    Task<IEnumerable<Property>> GetPropertiesAsync(string? city, PropertyType? type, decimal? minPrice, decimal? maxPrice, int? bedrooms);
    Task<Property?> GetByIdAsync(int id);
    Task<Property> CreateAsync(Property property);
    Task<Property?> UpdateAsync(int id, Property property);
    Task<Property?> ChangeStatusAsync(int id, PropertyStatus status);
    Task<bool> DeleteAsync(int id);
    Task<Inquiry?> SubmitInquiryAsync(int propertyId, Inquiry inquiry);
}

