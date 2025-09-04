using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxiginAttendance.Api.Services;

namespace OxiginAttendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class BulkUploadController : ControllerBase
{
    private readonly IBulkEmployeeService _bulkEmployeeService;
    private readonly ILogger<BulkUploadController> _logger;

    public BulkUploadController(IBulkEmployeeService bulkEmployeeService, ILogger<BulkUploadController> logger)
    {
        _bulkEmployeeService = bulkEmployeeService;
        _logger = logger;
    }

    /// <summary>
    /// Bulk upload employees from CSV file (Admin only)
    /// Expected CSV format: Email, FirstName, LastName, EmployeeId (optional), Department, JobTitle, HireDate (optional), Role (optional)
    /// </summary>
    [HttpPost("employees")]
    public async Task<ActionResult<BulkUploadResult>> BulkUploadEmployees(IFormFile csvFile)
    {
        try
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            var result = await _bulkEmployeeService.BulkUploadEmployeesAsync(csvFile);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk employee upload");
            return StatusCode(500, new { message = "An error occurred during bulk employee upload" });
        }
    }

    /// <summary>
    /// Bulk upload employee rates from CSV file (Admin only)
    /// Expected CSV format: EmployeeId, ServiceItemName, PayRate, ChargeRate, CostRate, EffectiveDate (optional)
    /// </summary>
    [HttpPost("employee-rates")]
    public async Task<ActionResult<BulkUploadResult>> BulkUploadEmployeeRates(IFormFile csvFile)
    {
        try
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            var result = await _bulkEmployeeService.BulkUploadEmployeeRatesAsync(csvFile);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk employee rate upload");
            return StatusCode(500, new { message = "An error occurred during bulk employee rate upload" });
        }
    }

    /// <summary>
    /// Generate a new unique employee number (Admin only)
    /// </summary>
    [HttpGet("generate-employee-number")]
    public async Task<ActionResult<string>> GenerateEmployeeNumber()
    {
        try
        {
            var employeeNumber = await _bulkEmployeeService.GenerateEmployeeNumberAsync();
            return Ok(new { employeeNumber });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating employee number");
            return StatusCode(500, new { message = "An error occurred while generating employee number" });
        }
    }

    /// <summary>
    /// Check if an employee number is unique (Admin only)
    /// </summary>
    [HttpGet("check-employee-number/{employeeNumber}")]
    public async Task<ActionResult<bool>> CheckEmployeeNumberUnique(string employeeNumber)
    {
        try
        {
            var isUnique = await _bulkEmployeeService.IsEmployeeNumberUniqueAsync(employeeNumber);
            return Ok(new { employeeNumber, isUnique });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking employee number uniqueness");
            return StatusCode(500, new { message = "An error occurred while checking employee number" });
        }
    }

    /// <summary>
    /// Download sample CSV template for employee bulk upload
    /// </summary>
    [HttpGet("templates/employees")]
    public IActionResult DownloadEmployeeTemplate()
    {
        var csv = "Email,FirstName,LastName,EmployeeId,Department,JobTitle,HireDate,Role\n";
        csv += "john.doe@example.com,John,Doe,,Security,Security Guard,2024-01-01,Employee\n";
        csv += "jane.smith@example.com,Jane,Smith,EMP20240001,Operations,Team Leader,2024-01-15,Employee\n";
        
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", "employee_bulk_upload_template.csv");
    }

    /// <summary>
    /// Download sample CSV template for employee rates bulk upload
    /// </summary>
    [HttpGet("templates/employee-rates")]
    public IActionResult DownloadEmployeeRatesTemplate()
    {
        var csv = "EmployeeId,ServiceItemName,PayRate,ChargeRate,CostRate,EffectiveDate\n";
        csv += "EMP20240001,8 Hour Shift,25.00,40.00,30.00,2024-01-01\n";
        csv += "EMP20240001,Normal Time,25.00,40.00,30.00,2024-01-01\n";
        csv += "EMP20240001,Overtime,37.50,60.00,45.00,2024-01-01\n";
        
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", "employee_rates_bulk_upload_template.csv");
    }
}