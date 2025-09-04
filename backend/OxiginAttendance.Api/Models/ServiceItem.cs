using System.ComponentModel.DataAnnotations;

namespace OxiginAttendance.Api.Models;

public class ServiceItem
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public ServiceItemType Type { get; set; }
    
    // Base pricing for service items
    public decimal BasePrice { get; set; }
    public decimal BaseCost { get; set; }
    
    // For shift hours (6, 8, 10, 12, 14, 16 hours)
    public int? ShiftHours { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<EmployeeRate> EmployeeRates { get; set; } = new List<EmployeeRate>();
}

public enum ServiceItemType
{
    ShiftHours = 1,         // 6, 8, 10, 12, 14, 16 hour shifts
    NormalTime = 2,         // Normal time for permanent staff
    Overtime = 3,           // Overtime for permanent staff
    DoubleTime = 4,         // Double time for permanent staff
    Equipment = 5,          // Equipment rental
    Other = 6               // Other services
}

// Individual employee rates for different service items
public class EmployeeRate
{
    public int Id { get; set; }
    
    public string EmployeeId { get; set; } = string.Empty;
    public int ServiceItemId { get; set; }
    
    // Individual employee pricing (hidden from non-admin)
    public decimal PayRate { get; set; }        // What employee gets paid
    public decimal ChargeRate { get; set; }     // What client is charged
    public decimal CostRate { get; set; }       // Company cost
    
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ApplicationUser Employee { get; set; } = null!;
    public virtual ServiceItem ServiceItem { get; set; } = null!;
}