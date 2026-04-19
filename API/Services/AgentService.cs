using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class AgentService(AppDbContext context) : IAgentService
{
    public async Task<IEnumerable<Agent>> GetAllAsync()
    {
        return await context.Agents.ToListAsync();
    }

    public async Task<Agent?> GetByIdAsync(int id)
    {
        return await context.Agents.FindAsync(id);
    }

    public async Task<Agent> CreateAsync(Agent agent)
    {
        context.Agents.Add(agent);
        await context.SaveChangesAsync();
        return agent;
    }

    public async Task<Agent?> UpdateAsync(int id, Agent agent)
    {
        var existing = await context.Agents.FindAsync(id);
        if (existing == null) return null;

        existing.FirstName = agent.FirstName;
        existing.LastName = agent.LastName;
        existing.Email = agent.Email;
        existing.Phone = agent.Phone;
        existing.LicenseNumber = agent.LicenseNumber;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Agents.FindAsync(id);
        if (existing == null) return false;

        context.Agents.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Property>> GetPropertiesByAgentIdAsync(int agentId)
    {
        return await context.Properties
            .Where(p => p.AgentId == agentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inquiry>> GetInquiriesByAgentIdAsync(int agentId)
    {
        return await context.Inquiries
            .Include(i => i.Property)
            .Where(i => i.Property != null && i.Property.AgentId == agentId)
            .ToListAsync();
    }
}
