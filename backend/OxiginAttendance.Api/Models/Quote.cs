using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.Models;

public class Quote
{
    public int Id { get; set; }
    
    [Required]
    public string QuoteNumber { get; set; } = string.Empty;
    
    public int JobOrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime ValidUntil { get; set; }
    
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    
    public string? ClientNotes { get; set; }
    public string? CreatedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual JobOrder JobOrder { get; set; } = null!;
    public virtual ApplicationUser? CreatedBy { get; set; }
    public virtual ICollection<QuoteLineItem> LineItems { get; set; } = new List<QuoteLineItem>();
    public virtual Invoice? Invoice { get; set; }
}

public enum QuoteStatus
{
    Draft = 1,          // Quote being prepared
    Sent = 2,          // Quote sent to client
    Approved = 3,      // Client approved quote
    Rejected = 4,      // Client rejected quote
    Expired = 5        // Quote expired
}

public class QuoteLineItem
{
    public int Id { get; set; }
    public int QuoteId { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Cost { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public decimal TotalCost => Quantity * Cost;
    
    public int? ServiceItemId { get; set; }
    
    // Navigation properties
    public virtual Quote Quote { get; set; } = null!;
    public virtual ServiceItem? ServiceItem { get; set; }
}