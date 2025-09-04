using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.Models;

public class TimeEntry
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public TimeSpan? TotalHours { get; set; }
    public TimeSpan? BreakTime { get; set; }
    public TimeSpan? OvertimeHours { get; set; }
    
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public string? PhotoPath { get; set; }          // Path to check-in photo
    public string? LocationCoordinates { get; set; } // GPS coordinates
    public bool? FacialRecognitionVerified { get; set; }
    
    // Job assignment
    public int? JobId { get; set; }
    
    public TimeEntryStatus Status { get; set; } = TimeEntryStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Job? Job { get; set; }
}

public enum TimeEntryStatus
{
    Active = 1,
    Completed = 2,
    Cancelled = 3
}