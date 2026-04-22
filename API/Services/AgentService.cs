using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class AgentService : IAgentService
{
    private readonly AppDbContext _context;

    public AgentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Agent>> GetAllAsync()
    {
        return await _context.Agents.ToListAsync();
    }

    public async Task<Agent?> GetByIdAsync(int id)
    {
        return await _context.Agents.FindAsync(id);
    }

    public async Task<Agent> CreateAsync(Agent agent)
    {
        var licenseExists = await _context.Agents.AnyAsync(a =>
            a.LicenseNumber == agent.LicenseNumber
        );
        if (licenseExists)
        {
            throw new ArgumentException("LicenseNumber must be unique.");
        }

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();
        return agent;
    }

    public async Task<Agent?> UpdateAsync(int id, Agent agent)
    {
        var existingAgent = await _context.Agents.FindAsync(id);
        if (existingAgent == null)
            return null;

        var licenseExists = await _context.Agents.AnyAsync(a =>
            a.LicenseNumber == agent.LicenseNumber && a.Id != id
        );
        if (licenseExists)
        {
            throw new ArgumentException("LicenseNumber must be unique.");
        }

        existingAgent.FirstName = agent.FirstName;
        existingAgent.LastName = agent.LastName;
        existingAgent.Email = agent.Email;
        existingAgent.Phone = agent.Phone;
        existingAgent.LicenseNumber = agent.LicenseNumber;

        await _context.SaveChangesAsync();
        return existingAgent;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Agents.FindAsync(id);
        if (existing == null)
            return false;

        _context.Agents.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Property>> GetPropertiesByAgentIdAsync(int agentId)
    {
        return await _context.Properties.Where(p => p.AgentId == agentId).ToListAsync();
    }

    public async Task<IEnumerable<Inquiry>> GetInquiriesByAgentIdAsync(int agentId)
    {
        return await _context
            .Inquiries.Include(i => i.Property)
            .Where(i => i.Property != null && i.Property.AgentId == agentId)
            .ToListAsync();
    }
}
