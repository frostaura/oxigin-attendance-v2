using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxiginAttendance.Api.DTOs;
using OxiginAttendance.Api.Services;
using System.Security.Claims;

namespace OxiginAttendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeEntryController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly ILogger<TimeEntryController> _logger;

    public TimeEntryController(ITimeEntryService timeEntryService, ILogger<TimeEntryController> logger)
    {
        _timeEntryService = timeEntryService;
        _logger = logger;
    }

    [HttpPost("clock-in")]
    public async Task<ActionResult<TimeEntryDto>> ClockIn([FromBody] ClockInDto clockInDto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _timeEntryService.ClockInAsync(userId, clockInDto);
        if (result == null)
        {
            return BadRequest(new { message = "Unable to clock in. You may already be clocked in." });
        }

        return Ok(result);
    }

    [HttpPost("clock-out")]
    public async Task<ActionResult<TimeEntryDto>> ClockOut([FromBody] ClockOutDto clockOutDto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _timeEntryService.ClockOutAsync(userId, clockOutDto);
        if (result == null)
        {
            return BadRequest(new { message = "Unable to clock out. No active time entry found." });
        }

        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<TimeEntryDto>> GetActiveTimeEntry()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _timeEntryService.GetActiveTimeEntryAsync(userId);
        if (result == null)
        {
            return Ok(new { message = "No active time entry found" });
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<TimeEntryDto>>> GetTimeEntries(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _timeEntryService.GetTimeEntriesAsync(userId, startDate, endDate);
        return Ok(result);
    }

    [HttpGet("report")]
    public async Task<ActionResult<TimeReportDto>> GetTimeReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _timeEntryService.GetTimeReportAsync(userId, startDate, endDate);
        return Ok(result);
    }

    [HttpGet("report/all")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<List<TimeReportDto>>> GetAllEmployeesTimeReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _timeEntryService.GetAllEmployeesTimeReportAsync(startDate, endDate);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<TimeEntryDto>> CreateTimeEntry(
        [FromBody] TimeEntryCreateDto createDto,
        [FromQuery] string? userId = null)
    {
        var targetUserId = userId ?? GetCurrentUserId();
        if (string.IsNullOrEmpty(targetUserId))
        {
            return Unauthorized();
        }

        var result = await _timeEntryService.CreateTimeEntryAsync(targetUserId, createDto);
        if (result == null)
        {
            return BadRequest(new { message = "Unable to create time entry" });
        }

        return CreatedAtAction(nameof(GetTimeEntry), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TimeEntryDto>> GetTimeEntry(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var timeEntries = await _timeEntryService.GetTimeEntriesAsync(userId);
        var timeEntry = timeEntries.FirstOrDefault(te => te.Id == id);

        if (timeEntry == null)
        {
            return NotFound();
        }

        return Ok(timeEntry);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<TimeEntryDto>> UpdateTimeEntry(int id, [FromBody] TimeEntryUpdateDto updateDto)
    {
        var result = await _timeEntryService.UpdateTimeEntryAsync(id, updateDto);
        if (result == null)
        {
            return NotFound(new { message = "Time entry not found" });
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult> DeleteTimeEntry(int id)
    {
        var result = await _timeEntryService.DeleteTimeEntryAsync(id);
        if (!result)
        {
            return NotFound(new { message = "Time entry not found" });
        }

        return NoContent();
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}