using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.Models;

public class JobOrder
{
    public int Id { get; set; }
    
    [Required]
    public string OrderNumber { get; set; } = string.Empty;
    
    [Required]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    public string EventName { get; set; } = string.Empty;
    
    [Required]
    public string SiteName { get; set; } = string.Empty;
    
    public string? SiteAddress { get; set; }
    public string? Description { get; set; }
    public string? PoNumber { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ApplicationUser Client { get; set; } = null!;
    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public virtual Job? Job { get; set; }
}

public enum OrderStatus
{
    Pending = 1,        // Order submitted, awaiting quote
    Quoted = 2,         // Quote generated and sent to client
    Approved = 3,       // Quote approved by client
    InProgress = 4,     // Job is active
    Completed = 5,      // Job completed successfully
    Cancelled = 6       // Order cancelled
}