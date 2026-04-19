using API.DTOs.Property;
using API.DTOs.Inquiry;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Property>>> GetProperties(
        [FromQuery] string? city,
        [FromQuery] PropertyType? type,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? bedrooms)
    {
        var properties = await _propertyService.GetPropertiesAsync(city, type, minPrice, maxPrice, bedrooms);
        return Ok(properties);
    }

    [HttpPost]
    public async Task<ActionResult<Property>> CreateProperty([FromBody] CreatePropertyDto dto)
    {
        var property = dto.ToModel();
        var createdProperty = await _propertyService.CreateAsync(property);
        return CreatedAtAction(nameof(GetProperty), new { id = createdProperty.Id }, createdProperty);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Property>> GetProperty(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property == null) return NotFound();
        return Ok(property);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Property>> UpdateProperty(int id, [FromBody] CreatePropertyDto dto)
    {
        var property = dto.ToModel();
        var updatedProperty = await _propertyService.UpdateAsync(id, property);
        if (updatedProperty == null) return NotFound();
        return Ok(updatedProperty);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<Property>> ChangeStatus(int id, [FromBody] PropertyStatus status)
    {
        var updatedProperty = await _propertyService.ChangeStatusAsync(id, status);
        if (updatedProperty == null) return NotFound();
        return Ok(updatedProperty);
    }

    [HttpPost("{id}/inquiries")]
    public async Task<ActionResult<Inquiry>> SubmitInquiry(int id, [FromBody] CreateInquiryDto dto)
    {
        var inquiry = dto.ToModel(id);
        var submittedInquiry = await _propertyService.SubmitInquiryAsync(id, inquiry);
        if (submittedInquiry == null) return BadRequest("Could not submit inquiry. The property might not exist, or it is unavailable.");
        return Created("", submittedInquiry); // Using simplified created response
    }
}
