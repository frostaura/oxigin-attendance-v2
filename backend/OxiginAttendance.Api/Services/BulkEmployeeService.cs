using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OxiginAttendance.Api.Data;
using OxiginAttendance.Api.DTOs;
using OxiginAttendance.Api.Models;
using System.Text.Json;

namespace OxiginAttendance.Api.Services;

public interface IBulkEmployeeService
{
    Task<BulkUploadResult> BulkUploadEmployeesAsync(IFormFile csvFile);
    Task<BulkUploadResult> BulkUploadEmployeeRatesAsync(IFormFile csvFile);
    Task<string> GenerateEmployeeNumberAsync();
    Task<bool> IsEmployeeNumberUniqueAsync(string employeeNumber);
}

public class BulkUploadResult
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Message { get; set; }
}

public class EmployeeCsvRecord
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime? HireDate { get; set; }
    public string? Role { get; set; } = "Employee";
}

public class EmployeeRateCsvRecord
{
    public string EmployeeId { get; set; } = string.Empty;
    public string ServiceItemName { get; set; } = string.Empty;
    public decimal PayRate { get; set; }
    public decimal ChargeRate { get; set; }
    public decimal CostRate { get; set; }
    public DateTime? EffectiveDate { get; set; }
}

public class BulkEmployeeService : IBulkEmployeeService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmployeeRateService _employeeRateService;
    private readonly ILogger<BulkEmployeeService> _logger;

    public BulkEmployeeService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IEmployeeRateService employeeRateService,
        ILogger<BulkEmployeeService> logger)
    {
        _userManager = userManager;
        _context = context;
        _employeeRateService = employeeRateService;
        _logger = logger;
    }

    public async Task<BulkUploadResult> BulkUploadEmployeesAsync(IFormFile csvFile)
    {
        var result = new BulkUploadResult();
        
        try
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                result.Errors.Add("No file provided");
                return result;
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add("File must be a CSV file");
                return result;
            }

            var employees = await ParseEmployeeCsvAsync(csvFile);
            result.TotalRecords = employees.Count;

            foreach (var employeeRecord in employees)
            {
                try
                {
                    // Generate employee number if not provided
                    if (string.IsNullOrEmpty(employeeRecord.EmployeeId))
                    {
                        employeeRecord.EmployeeId = await GenerateEmployeeNumberAsync();
                    }
                    else
                    {
                        // Check if employee number is unique
                        var isUnique = await IsEmployeeNumberUniqueAsync(employeeRecord.EmployeeId);
                        if (!isUnique)
                        {
                            result.Errors.Add($"Employee ID {employeeRecord.EmployeeId} already exists for {employeeRecord.Email}");
                            result.FailedRecords++;
                            continue;
                        }
                    }

                    // Check if user already exists
                    var existingUser = await _userManager.FindByEmailAsync(employeeRecord.Email);
                    if (existingUser != null)
                    {
                        result.Warnings.Add($"User with email {employeeRecord.Email} already exists, skipping");
                        continue;
                    }

                    var user = new ApplicationUser
                    {
                        UserName = employeeRecord.Email,
                        Email = employeeRecord.Email,
                        FirstName = employeeRecord.FirstName,
                        LastName = employeeRecord.LastName,
                        EmployeeId = employeeRecord.EmployeeId,
                        Department = employeeRecord.Department,
                        JobTitle = employeeRecord.JobTitle,
                        HireDate = employeeRecord.HireDate ?? DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        EmailConfirmed = true
                    };

                    // Default password - should be changed on first login
                    var defaultPassword = "TempPassword123!";
                    var createResult = await _userManager.CreateAsync(user, defaultPassword);
                    
                    if (createResult.Succeeded)
                    {
                        // Assign role
                        var role = !string.IsNullOrEmpty(employeeRecord.Role) ? employeeRecord.Role : "Employee";
                        await _userManager.AddToRoleAsync(user, role);
                        
                        result.SuccessfulRecords++;
                        _logger.LogInformation("Successfully created user {Email} with employee ID {EmployeeId}", 
                            employeeRecord.Email, employeeRecord.EmployeeId);
                    }
                    else
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        result.Errors.Add($"Failed to create user {employeeRecord.Email}: {errors}");
                        result.FailedRecords++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating employee {Email}", employeeRecord.Email);
                    result.Errors.Add($"Error creating employee {employeeRecord.Email}: {ex.Message}");
                    result.FailedRecords++;
                }
            }

            result.Success = result.SuccessfulRecords > 0;
            result.Message = $"Processed {result.TotalRecords} records. {result.SuccessfulRecords} successful, {result.FailedRecords} failed.";
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk employee upload");
            result.Errors.Add($"General error: {ex.Message}");
            return result;
        }
    }

    public async Task<BulkUploadResult> BulkUploadEmployeeRatesAsync(IFormFile csvFile)
    {
        var result = new BulkUploadResult();
        
        try
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                result.Errors.Add("No file provided");
                return result;
            }

            var rates = await ParseEmployeeRateCsvAsync(csvFile);
            result.TotalRecords = rates.Count;

            var bulkUpdates = new List<BulkEmployeeRateUpdate>();

            foreach (var rateRecord in rates)
            {
                try
                {
                    // Find employee
                    var employee = await _context.Users
                        .FirstOrDefaultAsync(u => u.EmployeeId == rateRecord.EmployeeId);
                    
                    if (employee == null)
                    {
                        result.Errors.Add($"Employee with ID {rateRecord.EmployeeId} not found");
                        result.FailedRecords++;
                        continue;
                    }

                    // Find service item
                    var serviceItem = await _context.ServiceItems
                        .FirstOrDefaultAsync(si => si.Name == rateRecord.ServiceItemName && si.IsActive);
                    
                    if (serviceItem == null)
                    {
                        result.Errors.Add($"Service item '{rateRecord.ServiceItemName}' not found for employee {rateRecord.EmployeeId}");
                        result.FailedRecords++;
                        continue;
                    }

                    bulkUpdates.Add(new BulkEmployeeRateUpdate
                    {
                        EmployeeId = employee.Id,
                        ServiceItemId = serviceItem.Id,
                        PayRate = rateRecord.PayRate,
                        ChargeRate = rateRecord.ChargeRate,
                        CostRate = rateRecord.CostRate,
                        EffectiveDate = rateRecord.EffectiveDate ?? DateTime.UtcNow
                    });

                    result.SuccessfulRecords++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing rate for employee {EmployeeId}", rateRecord.EmployeeId);
                    result.Errors.Add($"Error processing rate for employee {rateRecord.EmployeeId}: {ex.Message}");
                    result.FailedRecords++;
                }
            }

            if (bulkUpdates.Any())
            {
                var bulkSuccess = await _employeeRateService.BulkUpdateEmployeeRatesAsync(bulkUpdates);
                if (!bulkSuccess)
                {
                    result.Errors.Add("Bulk rate update operation failed");
                    result.Success = false;
                }
                else
                {
                    result.Success = true;
                }
            }

            result.Message = $"Processed {result.TotalRecords} rate records. {result.SuccessfulRecords} successful, {result.FailedRecords} failed.";
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk employee rate upload");
            result.Errors.Add($"General error: {ex.Message}");
            return result;
        }
    }

    public async Task<string> GenerateEmployeeNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"EMP{year}";
        
        // Get the highest existing employee number for this year
        var lastEmployee = await _context.Users
            .Where(u => u.EmployeeId.StartsWith(prefix))
            .OrderByDescending(u => u.EmployeeId)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastEmployee != null)
        {
            var lastNumberStr = lastEmployee.EmployeeId.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task<bool> IsEmployeeNumberUniqueAsync(string employeeNumber)
    {
        return !await _context.Users.AnyAsync(u => u.EmployeeId == employeeNumber);
    }

    private async Task<List<EmployeeCsvRecord>> ParseEmployeeCsvAsync(IFormFile csvFile)
    {
        var employees = new List<EmployeeCsvRecord>();
        
        using var reader = new StreamReader(csvFile.OpenReadStream());
        
        // Skip header row
        var header = await reader.ReadLineAsync();
        
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var values = line.Split(',');
            
            if (values.Length >= 6)
            {
                employees.Add(new EmployeeCsvRecord
                {
                    Email = values[0]?.Trim() ?? "",
                    FirstName = values[1]?.Trim() ?? "",
                    LastName = values[2]?.Trim() ?? "",
                    EmployeeId = values[3]?.Trim(),
                    Department = values[4]?.Trim() ?? "",
                    JobTitle = values[5]?.Trim() ?? "",
                    HireDate = values.Length > 6 && DateTime.TryParse(values[6]?.Trim(), out var hireDate) ? hireDate : null,
                    Role = values.Length > 7 ? values[7]?.Trim() : "Employee"
                });
            }
        }
        
        return employees;
    }

    private async Task<List<EmployeeRateCsvRecord>> ParseEmployeeRateCsvAsync(IFormFile csvFile)
    {
        var rates = new List<EmployeeRateCsvRecord>();
        
        using var reader = new StreamReader(csvFile.OpenReadStream());
        
        // Skip header row
        var header = await reader.ReadLineAsync();
        
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var values = line.Split(',');
            
            if (values.Length >= 5)
            {
                rates.Add(new EmployeeRateCsvRecord
                {
                    EmployeeId = values[0]?.Trim() ?? "",
                    ServiceItemName = values[1]?.Trim() ?? "",
                    PayRate = decimal.TryParse(values[2]?.Trim(), out var payRate) ? payRate : 0,
                    ChargeRate = decimal.TryParse(values[3]?.Trim(), out var chargeRate) ? chargeRate : 0,
                    CostRate = decimal.TryParse(values[4]?.Trim(), out var costRate) ? costRate : 0,
                    EffectiveDate = values.Length > 5 && DateTime.TryParse(values[5]?.Trim(), out var effectiveDate) ? effectiveDate : null
                });
            }
        }
        
        return rates;
    }
}