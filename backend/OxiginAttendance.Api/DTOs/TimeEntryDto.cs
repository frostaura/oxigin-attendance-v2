using OxiginAttendance.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.DTOs;

public class TimeEntryDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public TimeSpan? TotalHours { get; set; }
    public TimeSpan? BreakTime { get; set; }
    public TimeSpan? OvertimeHours { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public TimeEntryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDto? User { get; set; }
}

public class ClockInDto
{
    public string? Notes { get; set; }
    public string? Location { get; set; }
}

public class ClockOutDto
{
    public int TimeEntryId { get; set; }
    public string? Notes { get; set; }
    public TimeSpan? BreakTime { get; set; }
}

public class TimeEntryCreateDto
{
    [Required]
    public DateTime ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public TimeSpan? BreakTime { get; set; }
}

public class TimeEntryUpdateDto
{
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public TimeSpan? BreakTime { get; set; }
    public TimeEntryStatus? Status { get; set; }
}

public class TimeReportDto
{
    public string UserId { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan TotalWorkedHours { get; set; }
    public TimeSpan TotalOvertimeHours { get; set; }
    public int TotalDaysWorked { get; set; }
    public List<TimeEntryDto> TimeEntries { get; set; } = new List<TimeEntryDto>();
}