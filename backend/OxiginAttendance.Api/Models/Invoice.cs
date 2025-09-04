using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.Models;

public class Invoice
{
    public int Id { get; set; }
    
    [Required]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    public int? QuoteId { get; set; }
    public int JobOrderId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    public string? Notes { get; set; }
    public string? CreatedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Quote? Quote { get; set; }
    public virtual JobOrder JobOrder { get; set; } = null!;
    public virtual ApplicationUser Client { get; set; } = null!;
    public virtual ApplicationUser? CreatedBy { get; set; }
    public virtual ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
}

public enum InvoiceStatus
{
    Draft = 1,          // Invoice being prepared
    Sent = 2,           // Invoice sent to client
    Paid = 3,           // Invoice has been paid
    Overdue = 4,        // Invoice is past due date
    Cancelled = 5       // Invoice cancelled
}

public class InvoiceLineItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    
    public int? ServiceItemId { get; set; }
    
    // Navigation properties
    public virtual Invoice Invoice { get; set; } = null!;
    public virtual ServiceItem? ServiceItem { get; set; }
}