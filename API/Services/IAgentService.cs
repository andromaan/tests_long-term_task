using API.Models;

namespace API.Services;

public interface IAgentService
{
    Task<IEnumerable<Agent>> GetAllAsync();
    Task<Agent?> GetByIdAsync(int id);
    Task<Agent> CreateAsync(Agent agent);
    Task<Agent?> UpdateAsync(int id, Agent agent);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Property>> GetPropertiesByAgentIdAsync(int agentId);
    Task<IEnumerable<Inquiry>> GetInquiriesByAgentIdAsync(int agentId);
}
