using API.DTOs.Agent;
using API.DTOs.Inquiry;
using API.DTOs.Property;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentsController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAllAgents()
    {
        var agents = await _agentService.GetAllAsync();
        return Ok(agents.Select(a => AgentDto.FromModel(a)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AgentDto>> GetAgent(int id)
    {
        var agent = await _agentService.GetByIdAsync(id);
        if (agent == null)
            return NotFound();
        return Ok(AgentDto.FromModel(agent));
    }

    [HttpPost]
    public async Task<ActionResult<AgentDto>> CreateAgent([FromBody] CreateAgentDto dto)
    {
        var agent = dto.ToModel();
        var createdAgent = await _agentService.CreateAsync(agent);
        return CreatedAtAction(
            nameof(GetAgent),
            new { id = createdAgent.Id },
            AgentDto.FromModel(createdAgent)
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AgentDto>> UpdateAgent(int id, [FromBody] UpdateAgentDto dto)
    {
        var agent = dto.ToModel();
        var updatedAgent = await _agentService.UpdateAsync(id, agent);
        if (updatedAgent == null)
            return NotFound();
        return Ok(AgentDto.FromModel(updatedAgent));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgent(int id)
    {
        var result = await _agentService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/properties")]
    public async Task<ActionResult<IEnumerable<PropertyDto>>> GetPropertiesByAgent(int id)
    {
        var properties = await _agentService.GetPropertiesByAgentIdAsync(id);
        return Ok(properties.Select(p => PropertyDto.FromModel(p)));
    }

    [HttpGet("{id}/inquiries")]
    public async Task<ActionResult<IEnumerable<InquiryDto>>> GetInquiriesByAgent(int id)
    {
        var inquiries = await _agentService.GetInquiriesByAgentIdAsync(id);
        return Ok(inquiries.Select(i => InquiryDto.FromModel(i)));
    }
}
