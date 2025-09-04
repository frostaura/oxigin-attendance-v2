using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.DTOs;

public class QuoteDto
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public int JobOrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ClientNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public JobOrderDto? JobOrder { get; set; }
    public List<QuoteLineItemDto> LineItems { get; set; } = new();
}

public class QuoteLineItemDto
{
    public int Id { get; set; }
    public int QuoteId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Cost { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalCost { get; set; }
    public ServiceItemDto? ServiceItem { get; set; }
}

public class CreateQuoteDto
{
    [Required]
    public int JobOrderId { get; set; }
    public string? Description { get; set; }
    public DateTime? ValidUntil { get; set; }
    public List<CreateQuoteLineItemDto> LineItems { get; set; } = new();
}

public class CreateQuoteLineItemDto
{
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public decimal Quantity { get; set; }
    [Required]
    public decimal UnitPrice { get; set; }
    [Required]
    public decimal Cost { get; set; }
    public int? ServiceItemId { get; set; }
}

public class UpdateQuoteStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
    public string? ClientNotes { get; set; }
}

public class JobOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public string? Description { get; set; }
    public string? PoNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserDto? Client { get; set; }
}

public class CreateJobOrderDto
{
    [Required]
    public string EventName { get; set; } = string.Empty;
    [Required]
    public string SiteName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public string? Description { get; set; }
    public string? PoNumber { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public decimal? EstimatedHours { get; set; }
}