using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.Models;

public class Job
{
    public int Id { get; set; }
    
    [Required]
    public string JobNumber { get; set; } = string.Empty;
    
    public int JobOrderId { get; set; }
    public string? CrewBossId { get; set; }
    
    public JobStatus Status { get; set; } = JobStatus.Assigned;
    
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public string? CompletionNotes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual JobOrder JobOrder { get; set; } = null!;
    public virtual ApplicationUser? CrewBoss { get; set; }
    public virtual ICollection<JobAssignment> JobAssignments { get; set; } = new List<JobAssignment>();
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}

public enum JobStatus
{
    Assigned = 1,       // Job assigned to crew boss
    InProgress = 2,     // Job is active
    Completed = 3,      // Job completed successfully
    Cancelled = 4       // Job cancelled
}

public class JobAssignment
{
    public int Id { get; set; }
    
    public int JobId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public string? AssignedByUserId { get; set; }
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Job Job { get; set; } = null!;
    public virtual ApplicationUser Employee { get; set; } = null!;
    public virtual ApplicationUser? AssignedBy { get; set; }
}