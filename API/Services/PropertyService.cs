using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _context;

    public PropertyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Property>> GetPropertiesAsync(string? city, PropertyType? type, decimal? minPrice, decimal? maxPrice, int? bedrooms)
    {
        var query = _context.Properties.AsQueryable();

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
        return await _context.Properties
            .Include(p => p.Agent)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Property> CreateAsync(Property property)
    {
        var agentExists = await _context.Agents.AnyAsync(a => a.Id == property.AgentId);
        if (!agentExists) throw new ArgumentException("Agent does not exist.");

        property.ListedAt = DateTime.UtcNow;
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<Property?> UpdateAsync(int id, Property property)
    {
        var existingProperty = await _context.Properties.FindAsync(id);
        if (existingProperty == null) return null;

        var agentExists = await _context.Agents.AnyAsync(a => a.Id == property.AgentId);
        if (!agentExists) throw new ArgumentException("Agent does not exist.");

        existingProperty.Title = property.Title;
        existingProperty.Description = property.Description;
        existingProperty.Address = property.Address;
        existingProperty.City = property.City;
        existingProperty.Price = property.Price;
        existingProperty.Area = property.Area;
        existingProperty.Bedrooms = property.Bedrooms;
        existingProperty.Bathrooms = property.Bathrooms;
        existingProperty.Type = property.Type;
        existingProperty.Status = property.Status;
        existingProperty.AgentId = property.AgentId;

        await _context.SaveChangesAsync();
        return existingProperty;
    }

    public async Task<Property?> ChangeStatusAsync(int id, PropertyStatus status)
    {
        var existing = await _context.Properties.FindAsync(id);
        if (existing == null) return null;

        existing.Status = status;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Properties.FindAsync(id);
        if (existing == null) return false;

        _context.Properties.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Inquiry?> SubmitInquiryAsync(int propertyId, Inquiry inquiry)
    {
        var property = await _context.Properties.FindAsync(propertyId);
        if (property == null) return null;

        if (property.Status == PropertyStatus.Sold || property.Status == PropertyStatus.Rented)
        {
            throw new InvalidOperationException("Cannot submit an inquiry for a sold or rented property.");
        }

        inquiry.PropertyId = propertyId;
        _context.Inquiries.Add(inquiry);
        await _context.SaveChangesAsync();
        return inquiry;
    }
}
