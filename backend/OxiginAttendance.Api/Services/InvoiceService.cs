using Microsoft.EntityFrameworkCore;
using OxiginAttendance.Api.Data;
using OxiginAttendance.Api.Models;

namespace OxiginAttendance.Api.Services;

public interface IInvoiceService
{
    Task<Invoice?> CreateInvoiceFromQuoteAsync(int quoteId, string createdByUserId);
    Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
    Task<IEnumerable<Invoice>> GetInvoicesByClientIdAsync(string clientId);
    Task<Invoice?> UpdateInvoiceStatusAsync(int invoiceId, InvoiceStatus status);
    Task<bool> DeleteInvoiceAsync(int invoiceId);
}

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(ApplicationDbContext context, ILogger<InvoiceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Invoice?> CreateInvoiceFromQuoteAsync(int quoteId, string createdByUserId)
    {
        try
        {
            var quote = await _context.Quotes
                .Include(q => q.JobOrder)
                .Include(q => q.LineItems)
                    .ThenInclude(li => li.ServiceItem)
                .FirstOrDefaultAsync(q => q.Id == quoteId);

            if (quote == null)
            {
                _logger.LogWarning("Quote {QuoteId} not found", quoteId);
                return null;
            }

            if (quote.Status != QuoteStatus.Approved)
            {
                _logger.LogWarning("Quote {QuoteId} is not approved, cannot create invoice", quoteId);
                return null;
            }

            // Check if invoice already exists for this quote
            var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.QuoteId == quoteId);
            if (existingInvoice != null)
            {
                _logger.LogWarning("Invoice already exists for quote {QuoteId}", quoteId);
                return existingInvoice;
            }

            var invoiceNumber = await GenerateInvoiceNumberAsync();

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                QuoteId = quoteId,
                JobOrderId = quote.JobOrderId,
                ClientId = quote.JobOrder.ClientId,
                SubTotal = quote.Amount,
                TaxAmount = CalculateTax(quote.Amount), // TODO: Implement tax calculation logic
                TotalAmount = quote.Amount + CalculateTax(quote.Amount),
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30), // Default 30 days payment terms
                Status = InvoiceStatus.Draft,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Copy quote line items to invoice line items
            foreach (var quoteLineItem in quote.LineItems)
            {
                var invoiceLineItem = new InvoiceLineItem
                {
                    InvoiceId = invoice.Id,
                    Description = quoteLineItem.Description,
                    Quantity = quoteLineItem.Quantity,
                    UnitPrice = quoteLineItem.UnitPrice,
                    ServiceItemId = quoteLineItem.ServiceItemId
                };

                _context.InvoiceLineItems.Add(invoiceLineItem);
            }

            await _context.SaveChangesAsync();

            return await GetInvoiceByIdAsync(invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice from quote {QuoteId}", quoteId);
            return null;
        }
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
    {
        return await _context.Invoices
            .Include(i => i.Quote)
            .Include(i => i.JobOrder)
            .Include(i => i.Client)
            .Include(i => i.CreatedBy)
            .Include(i => i.LineItems)
                .ThenInclude(li => li.ServiceItem)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByClientIdAsync(string clientId)
    {
        return await _context.Invoices
            .Include(i => i.JobOrder)
            .Include(i => i.LineItems)
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> UpdateInvoiceStatusAsync(int invoiceId, InvoiceStatus status)
    {
        try
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return null;
            }

            invoice.Status = status;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return invoice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId} status to {Status}", invoiceId, status);
            return null;
        }
    }

    public async Task<bool> DeleteInvoiceAsync(int invoiceId)
    {
        try
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice {InvoiceId}", invoiceId);
            return false;
        }
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith($"INV{year}-"))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumberStr = lastInvoice.InvoiceNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"INV{year}-{nextNumber:D4}";
    }

    private decimal CalculateTax(decimal amount)
    {
        // TODO: Implement proper tax calculation based on business rules
        // For now, using a simple 10% tax rate
        return amount * 0.10m;
    }
}