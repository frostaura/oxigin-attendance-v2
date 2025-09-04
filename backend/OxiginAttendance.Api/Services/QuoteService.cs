using Microsoft.EntityFrameworkCore;
using OxiginAttendance.Api.Data;
using OxiginAttendance.Api.Models;

namespace OxiginAttendance.Api.Services;

public interface IQuoteService
{
    Task<Quote?> CreateQuoteAsync(int jobOrderId, string createdByUserId);
    Task<Quote?> GetQuoteByIdAsync(int quoteId);
    Task<IEnumerable<Quote>> GetQuotesByJobOrderIdAsync(int jobOrderId);
    Task<Quote?> UpdateQuoteStatusAsync(int quoteId, QuoteStatus status, string? clientNotes = null);
    Task<Quote?> GenerateQuoteFromJobChangesAsync(int jobOrderId, decimal newEstimatedHours, string createdByUserId);
    Task<bool> DeleteQuoteAsync(int quoteId);
}

public class QuoteService : IQuoteService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuoteService> _logger;

    public QuoteService(ApplicationDbContext context, ILogger<QuoteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Quote?> CreateQuoteAsync(int jobOrderId, string createdByUserId)
    {
        try
        {
            var jobOrder = await _context.JobOrders.FindAsync(jobOrderId);
            if (jobOrder == null)
            {
                _logger.LogWarning("JobOrder {JobOrderId} not found", jobOrderId);
                return null;
            }

            // Generate quote number
            var quoteNumber = await GenerateQuoteNumberAsync();

            var quote = new Quote
            {
                QuoteNumber = quoteNumber,
                JobOrderId = jobOrderId,
                Amount = 0, // Will be calculated based on line items
                ValidUntil = DateTime.UtcNow.AddDays(30), // Default 30 days validity
                Status = QuoteStatus.Draft,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return quote;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quote for job order {JobOrderId}", jobOrderId);
            return null;
        }
    }

    public async Task<Quote?> GetQuoteByIdAsync(int quoteId)
    {
        return await _context.Quotes
            .Include(q => q.JobOrder)
            .Include(q => q.CreatedBy)
            .Include(q => q.LineItems)
                .ThenInclude(li => li.ServiceItem)
            .FirstOrDefaultAsync(q => q.Id == quoteId);
    }

    public async Task<IEnumerable<Quote>> GetQuotesByJobOrderIdAsync(int jobOrderId)
    {
        return await _context.Quotes
            .Include(q => q.CreatedBy)
            .Include(q => q.LineItems)
                .ThenInclude(li => li.ServiceItem)
            .Where(q => q.JobOrderId == jobOrderId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Quote?> UpdateQuoteStatusAsync(int quoteId, QuoteStatus status, string? clientNotes = null)
    {
        try
        {
            var quote = await _context.Quotes.FindAsync(quoteId);
            if (quote == null)
            {
                return null;
            }

            quote.Status = status;
            if (clientNotes != null)
            {
                quote.ClientNotes = clientNotes;
            }
            quote.UpdatedAt = DateTime.UtcNow;

            // If quote is approved, update job order status
            if (status == QuoteStatus.Approved)
            {
                var jobOrder = await _context.JobOrders.FindAsync(quote.JobOrderId);
                if (jobOrder != null)
                {
                    jobOrder.Status = OrderStatus.Approved;
                    jobOrder.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return quote;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quote {QuoteId} status to {Status}", quoteId, status);
            return null;
        }
    }

    public async Task<Quote?> GenerateQuoteFromJobChangesAsync(int jobOrderId, decimal newEstimatedHours, string createdByUserId)
    {
        try
        {
            // Create a new quote when job requirements change
            var quote = await CreateQuoteAsync(jobOrderId, createdByUserId);
            if (quote == null) return null;

            // Update the job order with new estimated hours
            var jobOrder = await _context.JobOrders.FindAsync(jobOrderId);
            if (jobOrder != null)
            {
                jobOrder.EstimatedHours = newEstimatedHours;
                jobOrder.Status = OrderStatus.Quoted;
                jobOrder.UpdatedAt = DateTime.UtcNow;
            }

            // TODO: Add logic to calculate quote line items based on service items and employee rates
            // This would include shift hours calculations and individual employee pricing

            await _context.SaveChangesAsync();
            return quote;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quote from job changes for JobOrder {JobOrderId}", jobOrderId);
            return null;
        }
    }

    public async Task<bool> DeleteQuoteAsync(int quoteId)
    {
        try
        {
            var quote = await _context.Quotes.FindAsync(quoteId);
            if (quote == null) return false;

            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quote {QuoteId}", quoteId);
            return false;
        }
    }

    private async Task<string> GenerateQuoteNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var lastQuote = await _context.Quotes
            .Where(q => q.QuoteNumber.StartsWith($"Q{year}-"))
            .OrderByDescending(q => q.QuoteNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastQuote != null)
        {
            var lastNumberStr = lastQuote.QuoteNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"Q{year}-{nextNumber:D4}";
    }
}