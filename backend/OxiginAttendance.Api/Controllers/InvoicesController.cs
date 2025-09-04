using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxiginAttendance.Api.DTOs;
using OxiginAttendance.Api.Models;
using OxiginAttendance.Api.Services;
using System.Security.Claims;

namespace OxiginAttendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// Create invoice from approved quote
    /// </summary>
    [HttpPost("from-quote")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<InvoiceDto>> CreateInvoiceFromQuote([FromBody] CreateInvoiceFromQuoteDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var invoice = await _invoiceService.CreateInvoiceFromQuoteAsync(createDto.QuoteId, userId);
            if (invoice == null)
            {
                return BadRequest("Failed to create invoice. Quote may not exist or not approved.");
            }

            var invoiceDto = MapToInvoiceDto(invoice);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoiceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice from quote {QuoteId}", createDto.QuoteId);
            return StatusCode(500, "An error occurred while creating the invoice");
        }
    }

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
    {
        try
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound($"Invoice with ID {id} not found");
            }

            var invoiceDto = MapToInvoiceDto(invoice);
            return Ok(invoiceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", id);
            return StatusCode(500, "An error occurred while retrieving the invoice");
        }
    }

    /// <summary>
    /// Get invoices for a client
    /// </summary>
    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoicesByClient(string clientId)
    {
        try
        {
            // Check if user is authorized to view client invoices
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrManager = User.IsInRole("Administrator") || User.IsInRole("Manager");
            
            if (!isAdminOrManager && currentUserId != clientId)
            {
                return Forbid("You can only view your own invoices");
            }

            var invoices = await _invoiceService.GetInvoicesByClientIdAsync(clientId);
            var invoiceDtos = invoices.Select(MapToInvoiceDto).ToList();
            return Ok(invoiceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices for client {ClientId}", clientId);
            return StatusCode(500, "An error occurred while retrieving invoices");
        }
    }

    /// <summary>
    /// Update invoice status
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<InvoiceDto>> UpdateInvoiceStatus(int id, [FromBody] UpdateInvoiceStatusDto updateDto)
    {
        try
        {
            if (!Enum.TryParse<InvoiceStatus>(updateDto.Status, out var status))
            {
                return BadRequest($"Invalid status: {updateDto.Status}");
            }

            var invoice = await _invoiceService.UpdateInvoiceStatusAsync(id, status);
            if (invoice == null)
            {
                return NotFound($"Invoice with ID {id} not found");
            }

            var invoiceDto = MapToInvoiceDto(invoice);
            return Ok(invoiceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId} status", id);
            return StatusCode(500, "An error occurred while updating the invoice status");
        }
    }

    /// <summary>
    /// Delete an invoice
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        try
        {
            var success = await _invoiceService.DeleteInvoiceAsync(id);
            if (!success)
            {
                return NotFound($"Invoice with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);
            return StatusCode(500, "An error occurred while deleting the invoice");
        }
    }

    private static InvoiceDto MapToInvoiceDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            QuoteId = invoice.QuoteId,
            JobOrderId = invoice.JobOrderId,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            Quote = invoice.Quote != null ? new QuoteDto
            {
                Id = invoice.Quote.Id,
                QuoteNumber = invoice.Quote.QuoteNumber,
                Amount = invoice.Quote.Amount,
                Status = invoice.Quote.Status.ToString()
            } : null,
            JobOrder = invoice.JobOrder != null ? new JobOrderDto
            {
                Id = invoice.JobOrder.Id,
                OrderNumber = invoice.JobOrder.OrderNumber,
                EventName = invoice.JobOrder.EventName,
                SiteName = invoice.JobOrder.SiteName,
                Status = invoice.JobOrder.Status.ToString()
            } : null,
            Client = invoice.Client != null ? new UserDto
            {
                Id = invoice.Client.Id,
                Email = invoice.Client.Email ?? "",
                FirstName = invoice.Client.FirstName,
                LastName = invoice.Client.LastName,
                EmployeeId = invoice.Client.EmployeeId
            } : null,
            LineItems = invoice.LineItems?.Select(li => new InvoiceLineItemDto
            {
                Id = li.Id,
                InvoiceId = li.InvoiceId,
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                TotalPrice = li.TotalPrice
            }).ToList() ?? new List<InvoiceLineItemDto>()
        };
    }
}