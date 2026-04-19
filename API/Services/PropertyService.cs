using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class PropertyService(AppDbContext context) : IPropertyService
{
    public async Task<IEnumerable<Property>> GetPropertiesAsync(string? city, PropertyType? type, decimal? minPrice, decimal? maxPrice, int? bedrooms)
    {
        var query = context.Properties.AsQueryable();

        if (!string.IsNullOrEmpty(city))
            query = query.Where(p => p.City.Contains(city));
        if (type.HasValue)
            query = query.Where(p => p.Type == type.Value);
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);
        if (bedrooms.HasValue)
            query = query.Where(p => p.Bedrooms == bedrooms.Value);

        return await query.ToListAsync();
    }

    public async Task<Property?> GetByIdAsync(int id)
    {
        return await context.Properties
            .Include(p => p.Agent)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Property> CreateAsync(Property property)
    {
        property.ListedAt = DateTime.UtcNow;
        context.Properties.Add(property);
        await context.SaveChangesAsync();
        return property;
    }

    public async Task<Property?> UpdateAsync(int id, Property property)
    {
        var existing = await context.Properties.FindAsync(id);
        if (existing == null) return null;

        existing.Title = property.Title;
        existing.Description = property.Description;
        existing.Address = property.Address;
        existing.City = property.City;
        existing.Price = property.Price;
        existing.Area = property.Area;
        existing.Bedrooms = property.Bedrooms;
        existing.Bathrooms = property.Bathrooms;
        existing.Type = property.Type;
        existing.Status = property.Status;
        existing.AgentId = property.AgentId;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<Property?> ChangeStatusAsync(int id, PropertyStatus status)
    {
        var existing = await context.Properties.FindAsync(id);
        if (existing == null) return null;

        existing.Status = status;
        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Properties.FindAsync(id);
        if (existing == null) return false;

        context.Properties.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Inquiry?> SubmitInquiryAsync(int propertyId, Inquiry inquiry)
    {
        var property = await context.Properties.FindAsync(propertyId);
        if (property == null || property.Status != PropertyStatus.Available)
        {
            return null; // Can only submit inquiry for available properties
        }

        inquiry.PropertyId = propertyId;
        inquiry.CreatedAt = DateTime.UtcNow;
        inquiry.IsResponded = false;

        context.Inquiries.Add(inquiry);
        await context.SaveChangesAsync();
        return inquiry;
    }
}

