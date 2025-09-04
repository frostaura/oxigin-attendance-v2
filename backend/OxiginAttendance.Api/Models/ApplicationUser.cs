using Microsoft.AspNetCore.Identity;

namespace OxiginAttendance.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<JobOrder> ClientOrders { get; set; } = new List<JobOrder>();
    public virtual ICollection<JobAssignment> JobAssignments { get; set; } = new List<JobAssignment>();
    public virtual ICollection<Job> CrewBossJobs { get; set; } = new List<Job>();
    public virtual ICollection<EmployeeRate> EmployeeRates { get; set; } = new List<EmployeeRate>();
}