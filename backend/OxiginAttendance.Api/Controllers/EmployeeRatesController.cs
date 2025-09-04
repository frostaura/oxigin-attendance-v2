using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxiginAttendance.Api.DTOs;
using OxiginAttendance.Api.Models;
using OxiginAttendance.Api.Services;

namespace OxiginAttendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeRatesController : ControllerBase
{
    private readonly IEmployeeRateService _employeeRateService;
    private readonly ILogger<EmployeeRatesController> _logger;

    public EmployeeRatesController(IEmployeeRateService employeeRateService, ILogger<EmployeeRatesController> logger)
    {
        _employeeRateService = employeeRateService;
        _logger = logger;
    }

    /// <summary>
    /// Create employee rate (Admin only - pricing is hidden from non-admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<EmployeeRateDto>> CreateEmployeeRate([FromBody] CreateEmployeeRateDto createDto)
    {
        try
        {
            var employeeRate = await _employeeRateService.CreateEmployeeRateAsync(
                createDto.EmployeeId, 
                createDto.ServiceItemId,
                createDto.PayRate,
                createDto.ChargeRate,
                createDto.CostRate);

            if (employeeRate == null)
            {
                return BadRequest("Failed to create employee rate. Employee or service item may not exist.");
            }

            var employeeRateDto = MapToEmployeeRateDto(employeeRate);
            return CreatedAtAction(nameof(GetEmployeeRate), new { id = employeeRate.Id }, employeeRateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee rate for employee {EmployeeId}", createDto.EmployeeId);
            return StatusCode(500, "An error occurred while creating the employee rate");
        }
    }

    /// <summary>
    /// Get employee rate by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<EmployeeRateDto>> GetEmployeeRate(int id)
    {
        try
        {
            var employeeRates = await _employeeRateService.GetEmployeeRatesAsync("");
            var employeeRate = employeeRates.FirstOrDefault(er => er.Id == id);
            
            if (employeeRate == null)
            {
                return NotFound($"Employee rate with ID {id} not found");
            }

            var employeeRateDto = MapToEmployeeRateDto(employeeRate);
            return Ok(employeeRateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee rate {RateId}", id);
            return StatusCode(500, "An error occurred while retrieving the employee rate");
        }
    }

    /// <summary>
    /// Get employee rates for a specific employee (Admin only)
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<EmployeeRateDto>>> GetEmployeeRates(string employeeId)
    {
        try
        {
            var employeeRates = await _employeeRateService.GetEmployeeRatesAsync(employeeId);
            var employeeRateDtos = employeeRates.Select(MapToEmployeeRateDto).ToList();
            return Ok(employeeRateDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee rates for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving employee rates");
        }
    }

    /// <summary>
    /// Get active rate for employee and service item (Admin only)
    /// </summary>
    [HttpGet("employee/{employeeId}/service/{serviceItemId}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<EmployeeRateDto>> GetActiveEmployeeRate(string employeeId, int serviceItemId)
    {
        try
        {
            var employeeRate = await _employeeRateService.GetActiveEmployeeRateAsync(employeeId, serviceItemId);
            if (employeeRate == null)
            {
                return NotFound($"No active rate found for employee {employeeId} and service item {serviceItemId}");
            }

            var employeeRateDto = MapToEmployeeRateDto(employeeRate);
            return Ok(employeeRateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active employee rate for employee {EmployeeId} and service item {ServiceItemId}", employeeId, serviceItemId);
            return StatusCode(500, "An error occurred while retrieving the employee rate");
        }
    }

    /// <summary>
    /// Bulk update employee rates (Admin only - for client increases or new client bulk uploads)
    /// </summary>
    [HttpPost("bulk-update")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> BulkUpdateEmployeeRates([FromBody] BulkEmployeeRateUpdateDto bulkUpdateDto)
    {
        try
        {
            var updates = bulkUpdateDto.Rates.Select(r => new BulkEmployeeRateUpdate
            {
                EmployeeId = r.EmployeeId,
                ServiceItemId = r.ServiceItemId,
                PayRate = r.PayRate,
                ChargeRate = r.ChargeRate,
                CostRate = r.CostRate,
                EffectiveDate = r.EffectiveDate ?? DateTime.UtcNow
            }).ToList();

            var success = await _employeeRateService.BulkUpdateEmployeeRatesAsync(updates);
            if (!success)
            {
                return BadRequest("Bulk update failed");
            }

            return Ok(new { Message = $"Successfully updated {updates.Count} employee rates" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk update of employee rates");
            return StatusCode(500, "An error occurred during bulk update");
        }
    }

    /// <summary>
    /// Delete employee rate (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteEmployeeRate(int id)
    {
        try
        {
            var success = await _employeeRateService.DeleteEmployeeRateAsync(id);
            if (!success)
            {
                return NotFound($"Employee rate with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee rate {RateId}", id);
            return StatusCode(500, "An error occurred while deleting the employee rate");
        }
    }

    private static EmployeeRateDto MapToEmployeeRateDto(EmployeeRate employeeRate)
    {
        return new EmployeeRateDto
        {
            Id = employeeRate.Id,
            EmployeeId = employeeRate.EmployeeId,
            ServiceItemId = employeeRate.ServiceItemId,
            PayRate = employeeRate.PayRate,
            ChargeRate = employeeRate.ChargeRate,
            CostRate = employeeRate.CostRate,
            EffectiveDate = employeeRate.EffectiveDate,
            ExpiryDate = employeeRate.ExpiryDate,
            IsActive = employeeRate.IsActive,
            Employee = employeeRate.Employee != null ? new UserDto
            {
                Id = employeeRate.Employee.Id,
                Email = employeeRate.Employee.Email ?? "",
                FirstName = employeeRate.Employee.FirstName,
                LastName = employeeRate.Employee.LastName,
                EmployeeId = employeeRate.Employee.EmployeeId,
                Department = employeeRate.Employee.Department,
                JobTitle = employeeRate.Employee.JobTitle
            } : null,
            ServiceItem = employeeRate.ServiceItem != null ? new ServiceItemDto
            {
                Id = employeeRate.ServiceItem.Id,
                Name = employeeRate.ServiceItem.Name,
                Description = employeeRate.ServiceItem.Description,
                Type = employeeRate.ServiceItem.Type.ToString(),
                BasePrice = employeeRate.ServiceItem.BasePrice,
                BaseCost = employeeRate.ServiceItem.BaseCost,
                ShiftHours = employeeRate.ServiceItem.ShiftHours,
                IsActive = employeeRate.ServiceItem.IsActive
            } : null
        };
    }
}