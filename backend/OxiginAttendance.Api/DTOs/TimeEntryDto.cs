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
    public string? PhotoPath { get; set; }
    public string? LocationCoordinates { get; set; }
    public bool? FacialRecognitionVerified { get; set; }
    public int? JobId { get; set; }
    public TimeEntryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDto? User { get; set; }
    public JobDto? Job { get; set; }
}

public class ClockInDto
{
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public string? LocationCoordinates { get; set; }  // GPS coordinates
    public int? JobId { get; set; }  // Job assignment
}

public class ClockInWithPhotoDto
{
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public string? LocationCoordinates { get; set; }
    public int? JobId { get; set; }
    public IFormFile? Photo { get; set; }  // Employee photo during check-in
    public IFormFile? SitePhoto { get; set; }  // Photo with crew boss/manager at site
    public IFormFile? GroupPhoto { get; set; }  // Group photo of all employees on site
    public bool RequireFacialRecognition { get; set; } = false;
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

public class JobDto
{
    public int Id { get; set; }
    public string JobNumber { get; set; } = string.Empty;
    public int JobOrderId { get; set; }
    public string? CrewBossId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public string? CompletionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public JobOrderDto? JobOrder { get; set; }
    public UserDto? CrewBoss { get; set; }
}