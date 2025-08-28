using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    
    [Required]
    public string Reason { get; set; } = string.Empty;
    
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ApplicationUser? ApprovedBy { get; set; }
}

public enum LeaveType
{
    SickLeave = 1,
    VacationLeave = 2,
    PersonalLeave = 3,
    EmergencyLeave = 4,
    MaternityLeave = 5,
    PaternityLeave = 6
}

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}