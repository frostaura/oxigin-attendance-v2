using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxiginAttendance.Api.DTOs;
using OxiginAttendance.Api.Models;
using OxiginAttendance.Api.Services;

namespace OxiginAttendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceItemsController : ControllerBase
{
    private readonly IServiceItemService _serviceItemService;
    private readonly ILogger<ServiceItemsController> _logger;

    public ServiceItemsController(IServiceItemService serviceItemService, ILogger<ServiceItemsController> logger)
    {
        _serviceItemService = serviceItemService;
        _logger = logger;
    }

    /// <summary>
    /// Create service item (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ServiceItemDto>> CreateServiceItem([FromBody] CreateServiceItemDto createDto)
    {
        try
        {
            if (!Enum.TryParse<ServiceItemType>(createDto.Type, out var serviceItemType))
            {
                return BadRequest($"Invalid service item type: {createDto.Type}");
            }

            var serviceItem = await _serviceItemService.CreateServiceItemAsync(
                createDto.Name, 
                serviceItemType,
                createDto.BasePrice,
                createDto.BaseCost,
                createDto.ShiftHours);

            if (serviceItem == null)
            {
                return BadRequest("Failed to create service item");
            }

            var serviceItemDto = MapToServiceItemDto(serviceItem);
            return CreatedAtAction(nameof(GetServiceItem), new { id = serviceItem.Id }, serviceItemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service item {Name}", createDto.Name);
            return StatusCode(500, "An error occurred while creating the service item");
        }
    }

    /// <summary>
    /// Get service item by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceItemDto>> GetServiceItem(int id)
    {
        try
        {
            var serviceItems = await _serviceItemService.GetServiceItemsAsync();
            var serviceItem = serviceItems.FirstOrDefault(si => si.Id == id);
            
            if (serviceItem == null)
            {
                return NotFound($"Service item with ID {id} not found");
            }

            var serviceItemDto = MapToServiceItemDto(serviceItem);
            return Ok(serviceItemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service item {ServiceItemId}", id);
            return StatusCode(500, "An error occurred while retrieving the service item");
        }
    }

    /// <summary>
    /// Get all service items
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceItemDto>>> GetServiceItems()
    {
        try
        {
            var serviceItems = await _serviceItemService.GetServiceItemsAsync();
            var serviceItemDtos = serviceItems.Select(MapToServiceItemDto).ToList();
            return Ok(serviceItemDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service items");
            return StatusCode(500, "An error occurred while retrieving service items");
        }
    }

    /// <summary>
    /// Get shift hour service items (6, 8, 10, 12, 14, 16 hours)
    /// </summary>
    [HttpGet("shift-hours")]
    public async Task<ActionResult<IEnumerable<ServiceItemDto>>> GetShiftHourServiceItems()
    {
        try
        {
            var serviceItems = await _serviceItemService.GetShiftHourServiceItemsAsync();
            var serviceItemDtos = serviceItems.Select(MapToServiceItemDto).ToList();
            return Ok(serviceItemDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift hour service items");
            return StatusCode(500, "An error occurred while retrieving shift hour service items");
        }
    }

    /// <summary>
    /// Update service item (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ServiceItemDto>> UpdateServiceItem(int id, [FromBody] UpdateServiceItemDto updateDto)
    {
        try
        {
            var serviceItem = await _serviceItemService.UpdateServiceItemAsync(
                id, 
                updateDto.Name, 
                updateDto.BasePrice, 
                updateDto.BaseCost);

            if (serviceItem == null)
            {
                return NotFound($"Service item with ID {id} not found");
            }

            var serviceItemDto = MapToServiceItemDto(serviceItem);
            return Ok(serviceItemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service item {ServiceItemId}", id);
            return StatusCode(500, "An error occurred while updating the service item");
        }
    }

    /// <summary>
    /// Delete service item (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteServiceItem(int id)
    {
        try
        {
            var success = await _serviceItemService.DeleteServiceItemAsync(id);
            if (!success)
            {
                return NotFound($"Service item with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service item {ServiceItemId}", id);
            return StatusCode(500, "An error occurred while deleting the service item");
        }
    }

    private static ServiceItemDto MapToServiceItemDto(ServiceItem serviceItem)
    {
        return new ServiceItemDto
        {
            Id = serviceItem.Id,
            Name = serviceItem.Name,
            Description = serviceItem.Description,
            Type = serviceItem.Type.ToString(),
            BasePrice = serviceItem.BasePrice,
            BaseCost = serviceItem.BaseCost,
            ShiftHours = serviceItem.ShiftHours,
            IsActive = serviceItem.IsActive
        };
    }
}

public class UpdateServiceItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal BaseCost { get; set; }
}