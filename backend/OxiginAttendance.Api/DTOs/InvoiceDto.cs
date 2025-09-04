using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.DTOs;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? QuoteId { get; set; }
    public int JobOrderId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public QuoteDto? Quote { get; set; }
    public JobOrderDto? JobOrder { get; set; }
    public UserDto? Client { get; set; }
    public List<InvoiceLineItemDto> LineItems { get; set; } = new();
}

public class InvoiceLineItemDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public ServiceItemDto? ServiceItem { get; set; }
}

public class CreateInvoiceFromQuoteDto
{
    [Required]
    public int QuoteId { get; set; }
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateInvoiceStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class ServiceItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal BaseCost { get; set; }
    public int? ShiftHours { get; set; }
    public bool IsActive { get; set; }
}

public class CreateServiceItemDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public string Type { get; set; } = string.Empty;
    [Required]
    public decimal BasePrice { get; set; }
    [Required]
    public decimal BaseCost { get; set; }
    public int? ShiftHours { get; set; }
}

public class EmployeeRateDto
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public int ServiceItemId { get; set; }
    public decimal PayRate { get; set; }
    public decimal ChargeRate { get; set; }
    public decimal CostRate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public UserDto? Employee { get; set; }
    public ServiceItemDto? ServiceItem { get; set; }
}

public class CreateEmployeeRateDto
{
    [Required]
    public string EmployeeId { get; set; } = string.Empty;
    [Required]
    public int ServiceItemId { get; set; }
    [Required]
    public decimal PayRate { get; set; }
    [Required]
    public decimal ChargeRate { get; set; }
    [Required]
    public decimal CostRate { get; set; }
    public DateTime? EffectiveDate { get; set; }
}

public class BulkEmployeeRateUpdateDto
{
    [Required]
    public List<CreateEmployeeRateDto> Rates { get; set; } = new();
}