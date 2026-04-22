using API.DTOs.Agent;
using API.Models;
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
    public async Task<ActionResult<IEnumerable<Agent>>> GetAllAgents()
    {
        var agents = await _agentService.GetAllAsync();
        return Ok(agents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Agent>> GetAgent(int id)
    {
        var agent = await _agentService.GetByIdAsync(id);
        if (agent == null) return NotFound();
        return Ok(agent);
    }

    [HttpPost]
    public async Task<ActionResult<Agent>> CreateAgent([FromBody] CreateAgentDto dto)
    {
        var agent = dto.ToModel();
        var createdAgent = await _agentService.CreateAsync(agent);
        return CreatedAtAction(nameof(GetAgent), new { id = createdAgent.Id }, createdAgent);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Agent>> UpdateAgent(int id, [FromBody] UpdateAgentDto dto)
    {
        var agent = dto.ToModel();
        var updatedAgent = await _agentService.UpdateAsync(id, agent);
        if (updatedAgent == null) return NotFound();
        return Ok(updatedAgent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgent(int id)
    {
        var result = await _agentService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/properties")]
    public async Task<ActionResult<IEnumerable<Property>>> GetPropertiesByAgent(int id)
    {
        var properties = await _agentService.GetPropertiesByAgentIdAsync(id);
        return Ok(properties);
    }

    [HttpGet("{id}/inquiries")]
    public async Task<ActionResult<IEnumerable<Inquiry>>> GetInquiriesByAgent(int id)
    {
        var inquiries = await _agentService.GetInquiriesByAgentIdAsync(id);
        return Ok(inquiries);
    }
}

