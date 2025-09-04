using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.Models;

public class EmailLog
{
    public int Id { get; set; }
    
    [Required]
    public string ToEmail { get; set; } = string.Empty;
    
    public string? FromEmail { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    
    public EmailType Type { get; set; }
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    
    // Related entities
    public int? QuoteId { get; set; }
    public int? InvoiceId { get; set; }
    public int? JobId { get; set; }
    public int? JobOrderId { get; set; }
    
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Quote? Quote { get; set; }
    public virtual Invoice? Invoice { get; set; }
    public virtual Job? Job { get; set; }
    public virtual JobOrder? JobOrder { get; set; }
}

public enum EmailType
{
    Quote = 1,
    Invoice = 2,
    Timesheet = 3,
    JobNotification = 4,
    System = 5
}

public enum EmailStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Cancelled = 4
}