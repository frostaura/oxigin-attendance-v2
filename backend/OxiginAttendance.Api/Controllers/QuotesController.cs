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
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;
    private readonly ILogger<QuotesController> _logger;

    public QuotesController(IQuoteService quoteService, ILogger<QuotesController> logger)
    {
        _quoteService = quoteService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new quote for a job order
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<QuoteDto>> CreateQuote([FromBody] CreateQuoteDto createQuoteDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var quote = await _quoteService.CreateQuoteAsync(createQuoteDto.JobOrderId, userId);
            if (quote == null)
            {
                return BadRequest("Failed to create quote. Job order may not exist.");
            }

            var quoteDto = MapToQuoteDto(quote);
            return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quoteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quote for job order {JobOrderId}", createQuoteDto.JobOrderId);
            return StatusCode(500, "An error occurred while creating the quote");
        }
    }

    /// <summary>
    /// Get quote by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<QuoteDto>> GetQuote(int id)
    {
        try
        {
            var quote = await _quoteService.GetQuoteByIdAsync(id);
            if (quote == null)
            {
                return NotFound($"Quote with ID {id} not found");
            }

            var quoteDto = MapToQuoteDto(quote);
            return Ok(quoteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quote {QuoteId}", id);
            return StatusCode(500, "An error occurred while retrieving the quote");
        }
    }

    /// <summary>
    /// Get quotes for a specific job order
    /// </summary>
    [HttpGet("job-order/{jobOrderId}")]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetQuotesByJobOrder(int jobOrderId)
    {
        try
        {
            var quotes = await _quoteService.GetQuotesByJobOrderIdAsync(jobOrderId);
            var quoteDtos = quotes.Select(MapToQuoteDto).ToList();
            return Ok(quoteDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quotes for job order {JobOrderId}", jobOrderId);
            return StatusCode(500, "An error occurred while retrieving quotes");
        }
    }

    /// <summary>
    /// Update quote status (approve/reject)
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<QuoteDto>> UpdateQuoteStatus(int id, [FromBody] UpdateQuoteStatusDto updateDto)
    {
        try
        {
            if (!Enum.TryParse<QuoteStatus>(updateDto.Status, out var status))
            {
                return BadRequest($"Invalid status: {updateDto.Status}");
            }

            var quote = await _quoteService.UpdateQuoteStatusAsync(id, status, updateDto.ClientNotes);
            if (quote == null)
            {
                return NotFound($"Quote with ID {id} not found");
            }

            var quoteDto = MapToQuoteDto(quote);
            return Ok(quoteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quote {QuoteId} status", id);
            return StatusCode(500, "An error occurred while updating the quote status");
        }
    }

    /// <summary>
    /// Generate a new quote when job requirements change
    /// </summary>
    [HttpPost("generate-from-changes")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<QuoteDto>> GenerateQuoteFromChanges([FromBody] GenerateQuoteFromChangesDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var quote = await _quoteService.GenerateQuoteFromJobChangesAsync(dto.JobOrderId, dto.NewEstimatedHours, userId);
            if (quote == null)
            {
                return BadRequest("Failed to generate quote from job changes");
            }

            var quoteDto = MapToQuoteDto(quote);
            return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quoteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quote from job changes for job order {JobOrderId}", dto.JobOrderId);
            return StatusCode(500, "An error occurred while generating the quote");
        }
    }

    /// <summary>
    /// Delete a quote
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<IActionResult> DeleteQuote(int id)
    {
        try
        {
            var success = await _quoteService.DeleteQuoteAsync(id);
            if (!success)
            {
                return NotFound($"Quote with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quote {QuoteId}", id);
            return StatusCode(500, "An error occurred while deleting the quote");
        }
    }

    private static QuoteDto MapToQuoteDto(Quote quote)
    {
        return new QuoteDto
        {
            Id = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            JobOrderId = quote.JobOrderId,
            Amount = quote.Amount,
            Description = quote.Description,
            ValidUntil = quote.ValidUntil,
            Status = quote.Status.ToString(),
            ClientNotes = quote.ClientNotes,
            CreatedAt = quote.CreatedAt,
            JobOrder = quote.JobOrder != null ? new JobOrderDto
            {
                Id = quote.JobOrder.Id,
                OrderNumber = quote.JobOrder.OrderNumber,
                ClientId = quote.JobOrder.ClientId,
                EventName = quote.JobOrder.EventName,
                SiteName = quote.JobOrder.SiteName,
                SiteAddress = quote.JobOrder.SiteAddress,
                Description = quote.JobOrder.Description,
                PoNumber = quote.JobOrder.PoNumber,
                StartDate = quote.JobOrder.StartDate,
                EndDate = quote.JobOrder.EndDate,
                EstimatedHours = quote.JobOrder.EstimatedHours,
                Status = quote.JobOrder.Status.ToString(),
                CreatedAt = quote.JobOrder.CreatedAt
            } : null,
            LineItems = quote.LineItems?.Select(li => new QuoteLineItemDto
            {
                Id = li.Id,
                QuoteId = li.QuoteId,
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                Cost = li.Cost,
                TotalPrice = li.TotalPrice,
                TotalCost = li.TotalCost
            }).ToList() ?? new List<QuoteLineItemDto>()
        };
    }
}

public class GenerateQuoteFromChangesDto
{
    public int JobOrderId { get; set; }
    public decimal NewEstimatedHours { get; set; }
}