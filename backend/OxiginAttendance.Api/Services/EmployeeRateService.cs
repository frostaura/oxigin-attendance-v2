using Microsoft.EntityFrameworkCore;
using OxiginAttendance.Api.Data;
using OxiginAttendance.Api.Models;

namespace OxiginAttendance.Api.Services;

public interface IEmployeeRateService
{
    Task<EmployeeRate?> CreateEmployeeRateAsync(string employeeId, int serviceItemId, decimal payRate, decimal chargeRate, decimal costRate);
    Task<IEnumerable<EmployeeRate>> GetEmployeeRatesAsync(string employeeId);
    Task<EmployeeRate?> GetActiveEmployeeRateAsync(string employeeId, int serviceItemId);
    Task<bool> BulkUpdateEmployeeRatesAsync(List<BulkEmployeeRateUpdate> updates);
    Task<bool> DeleteEmployeeRateAsync(int rateId);
}

public class BulkEmployeeRateUpdate
{
    public string EmployeeId { get; set; } = string.Empty;
    public int ServiceItemId { get; set; }
    public decimal PayRate { get; set; }
    public decimal ChargeRate { get; set; }
    public decimal CostRate { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
}

public class EmployeeRateService : IEmployeeRateService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeRateService> _logger;

    public EmployeeRateService(ApplicationDbContext context, ILogger<EmployeeRateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmployeeRate?> CreateEmployeeRateAsync(string employeeId, int serviceItemId, decimal payRate, decimal chargeRate, decimal costRate)
    {
        try
        {
            // Check if employee exists
            var employee = await _context.Users.FindAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found", employeeId);
                return null;
            }

            // Check if service item exists
            var serviceItem = await _context.ServiceItems.FindAsync(serviceItemId);
            if (serviceItem == null)
            {
                _logger.LogWarning("ServiceItem {ServiceItemId} not found", serviceItemId);
                return null;
            }

            // Deactivate any existing active rates for this employee/service item combination
            var existingRates = await _context.EmployeeRates
                .Where(er => er.EmployeeId == employeeId && 
                            er.ServiceItemId == serviceItemId && 
                            er.IsActive && 
                            (er.ExpiryDate == null || er.ExpiryDate > DateTime.UtcNow))
                .ToListAsync();

            foreach (var rate in existingRates)
            {
                rate.IsActive = false;
                rate.ExpiryDate = DateTime.UtcNow;
                rate.UpdatedAt = DateTime.UtcNow;
            }

            var employeeRate = new EmployeeRate
            {
                EmployeeId = employeeId,
                ServiceItemId = serviceItemId,
                PayRate = payRate,
                ChargeRate = chargeRate,
                CostRate = costRate,
                EffectiveDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EmployeeRates.Add(employeeRate);
            await _context.SaveChangesAsync();

            return employeeRate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee rate for employee {EmployeeId} and service item {ServiceItemId}", employeeId, serviceItemId);
            return null;
        }
    }

    public async Task<IEnumerable<EmployeeRate>> GetEmployeeRatesAsync(string employeeId)
    {
        return await _context.EmployeeRates
            .Include(er => er.ServiceItem)
            .Where(er => er.EmployeeId == employeeId && er.IsActive)
            .OrderBy(er => er.ServiceItem.Name)
            .ToListAsync();
    }

    public async Task<EmployeeRate?> GetActiveEmployeeRateAsync(string employeeId, int serviceItemId)
    {
        return await _context.EmployeeRates
            .Include(er => er.ServiceItem)
            .Where(er => er.EmployeeId == employeeId && 
                        er.ServiceItemId == serviceItemId && 
                        er.IsActive &&
                        er.EffectiveDate <= DateTime.UtcNow &&
                        (er.ExpiryDate == null || er.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(er => er.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> BulkUpdateEmployeeRatesAsync(List<BulkEmployeeRateUpdate> updates)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            foreach (var update in updates)
            {
                await CreateEmployeeRateAsync(update.EmployeeId, update.ServiceItemId, 
                                            update.PayRate, update.ChargeRate, update.CostRate);
            }

            await transaction.CommitAsync();
            _logger.LogInformation("Successfully bulk updated {Count} employee rates", updates.Count);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during bulk update of employee rates");
            return false;
        }
    }

    public async Task<bool> DeleteEmployeeRateAsync(int rateId)
    {
        try
        {
            var employeeRate = await _context.EmployeeRates.FindAsync(rateId);
            if (employeeRate == null) return false;

            employeeRate.IsActive = false;
            employeeRate.ExpiryDate = DateTime.UtcNow;
            employeeRate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee rate {RateId}", rateId);
            return false;
        }
    }
}

public interface IServiceItemService
{
    Task<ServiceItem?> CreateServiceItemAsync(string name, ServiceItemType type, decimal basePrice, decimal baseCost, int? shiftHours = null);
    Task<IEnumerable<ServiceItem>> GetServiceItemsAsync();
    Task<IEnumerable<ServiceItem>> GetShiftHourServiceItemsAsync();
    Task<ServiceItem?> UpdateServiceItemAsync(int id, string name, decimal basePrice, decimal baseCost);
    Task<bool> DeleteServiceItemAsync(int id);
}

public class ServiceItemService : IServiceItemService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceItemService> _logger;

    public ServiceItemService(ApplicationDbContext context, ILogger<ServiceItemService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceItem?> CreateServiceItemAsync(string name, ServiceItemType type, decimal basePrice, decimal baseCost, int? shiftHours = null)
    {
        try
        {
            var serviceItem = new ServiceItem
            {
                Name = name,
                Type = type,
                BasePrice = basePrice,
                BaseCost = baseCost,
                ShiftHours = shiftHours,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ServiceItems.Add(serviceItem);
            await _context.SaveChangesAsync();

            return serviceItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service item {Name}", name);
            return null;
        }
    }

    public async Task<IEnumerable<ServiceItem>> GetServiceItemsAsync()
    {
        return await _context.ServiceItems
            .Where(si => si.IsActive)
            .OrderBy(si => si.Type)
            .ThenBy(si => si.ShiftHours)
            .ThenBy(si => si.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceItem>> GetShiftHourServiceItemsAsync()
    {
        return await _context.ServiceItems
            .Where(si => si.IsActive && si.Type == ServiceItemType.ShiftHours && si.ShiftHours.HasValue)
            .OrderBy(si => si.ShiftHours)
            .ToListAsync();
    }

    public async Task<ServiceItem?> UpdateServiceItemAsync(int id, string name, decimal basePrice, decimal baseCost)
    {
        try
        {
            var serviceItem = await _context.ServiceItems.FindAsync(id);
            if (serviceItem == null) return null;

            serviceItem.Name = name;
            serviceItem.BasePrice = basePrice;
            serviceItem.BaseCost = baseCost;
            serviceItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return serviceItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service item {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteServiceItemAsync(int id)
    {
        try
        {
            var serviceItem = await _context.ServiceItems.FindAsync(id);
            if (serviceItem == null) return false;

            serviceItem.IsActive = false;
            serviceItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service item {Id}", id);
            return false;
        }
    }
}