using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OxiginAttendance.Api.Models;

namespace OxiginAttendance.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<JobOrder> JobOrders { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteLineItem> QuoteLineItems { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobAssignment> JobAssignments { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
    public DbSet<ServiceItem> ServiceItems { get; set; }
    public DbSet<EmployeeRate> EmployeeRates { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure TimeEntry relationships
        builder.Entity<TimeEntry>()
            .HasOne(te => te.User)
            .WithMany(u => u.TimeEntries)
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure LeaveRequest relationships
        builder.Entity<LeaveRequest>()
            .HasOne(lr => lr.User)
            .WithMany(u => u.LeaveRequests)
            .HasForeignKey(lr => lr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LeaveRequest>()
            .HasOne(lr => lr.ApprovedBy)
            .WithMany()
            .HasForeignKey(lr => lr.ApprovedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure JobOrder relationships
        builder.Entity<JobOrder>()
            .HasOne(jo => jo.Client)
            .WithMany(u => u.ClientOrders)
            .HasForeignKey(jo => jo.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<JobOrder>()
            .HasOne(jo => jo.Job)
            .WithOne(j => j.JobOrder)
            .HasForeignKey<Job>(j => j.JobOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Quote relationships
        builder.Entity<Quote>()
            .HasOne(q => q.JobOrder)
            .WithMany(jo => jo.Quotes)
            .HasForeignKey(q => q.JobOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Quote>()
            .HasOne(q => q.CreatedBy)
            .WithMany()
            .HasForeignKey(q => q.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Quote>()
            .HasOne(q => q.Invoice)
            .WithOne(i => i.Quote)
            .HasForeignKey<Invoice>(i => i.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure QuoteLineItem relationships
        builder.Entity<QuoteLineItem>()
            .HasOne(qli => qli.Quote)
            .WithMany(q => q.LineItems)
            .HasForeignKey(qli => qli.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuoteLineItem>()
            .HasOne(qli => qli.ServiceItem)
            .WithMany()
            .HasForeignKey(qli => qli.ServiceItemId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Job relationships
        builder.Entity<Job>()
            .HasOne(j => j.CrewBoss)
            .WithMany(u => u.CrewBossJobs)
            .HasForeignKey(j => j.CrewBossId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure JobAssignment relationships
        builder.Entity<JobAssignment>()
            .HasOne(ja => ja.Job)
            .WithMany(j => j.JobAssignments)
            .HasForeignKey(ja => ja.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<JobAssignment>()
            .HasOne(ja => ja.Employee)
            .WithMany(u => u.JobAssignments)
            .HasForeignKey(ja => ja.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<JobAssignment>()
            .HasOne(ja => ja.AssignedBy)
            .WithMany()
            .HasForeignKey(ja => ja.AssignedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure TimeEntry to Job relationship
        builder.Entity<TimeEntry>()
            .HasOne(te => te.Job)
            .WithMany(j => j.TimeEntries)
            .HasForeignKey(te => te.JobId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Invoice relationships
        builder.Entity<Invoice>()
            .HasOne(i => i.JobOrder)
            .WithMany()
            .HasForeignKey(i => i.JobOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invoice>()
            .HasOne(i => i.Client)
            .WithMany()
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invoice>()
            .HasOne(i => i.CreatedBy)
            .WithMany()
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure InvoiceLineItem relationships
        builder.Entity<InvoiceLineItem>()
            .HasOne(ili => ili.Invoice)
            .WithMany(i => i.LineItems)
            .HasForeignKey(ili => ili.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InvoiceLineItem>()
            .HasOne(ili => ili.ServiceItem)
            .WithMany()
            .HasForeignKey(ili => ili.ServiceItemId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure EmployeeRate relationships
        builder.Entity<EmployeeRate>()
            .HasOne(er => er.Employee)
            .WithMany(u => u.EmployeeRates)
            .HasForeignKey(er => er.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EmployeeRate>()
            .HasOne(er => er.ServiceItem)
            .WithMany(si => si.EmployeeRates)
            .HasForeignKey(er => er.ServiceItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure EmailLog relationships
        builder.Entity<EmailLog>()
            .HasOne(el => el.Quote)
            .WithMany()
            .HasForeignKey(el => el.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EmailLog>()
            .HasOne(el => el.Invoice)
            .WithMany()
            .HasForeignKey(el => el.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EmailLog>()
            .HasOne(el => el.Job)
            .WithMany()
            .HasForeignKey(el => el.JobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EmailLog>()
            .HasOne(el => el.JobOrder)
            .WithMany()
            .HasForeignKey(el => el.JobOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure precision for decimal/time calculations
        builder.Entity<TimeEntry>()
            .Property(te => te.TotalHours)
            .HasColumnType("interval");

        builder.Entity<TimeEntry>()
            .Property(te => te.BreakTime)
            .HasColumnType("interval");

        builder.Entity<TimeEntry>()
            .Property(te => te.OvertimeHours)
            .HasColumnType("interval");

        // Configure decimal precision for financial data
        builder.Entity<Quote>()
            .Property(q => q.Amount)
            .HasPrecision(18, 2);

        builder.Entity<QuoteLineItem>()
            .Property(qli => qli.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<QuoteLineItem>()
            .Property(qli => qli.Cost)
            .HasPrecision(18, 2);

        builder.Entity<Invoice>()
            .Property(i => i.SubTotal)
            .HasPrecision(18, 2);

        builder.Entity<Invoice>()
            .Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<Invoice>()
            .Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<InvoiceLineItem>()
            .Property(ili => ili.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<ServiceItem>()
            .Property(si => si.BasePrice)
            .HasPrecision(18, 2);

        builder.Entity<ServiceItem>()
            .Property(si => si.BaseCost)
            .HasPrecision(18, 2);

        builder.Entity<EmployeeRate>()
            .Property(er => er.PayRate)
            .HasPrecision(18, 2);

        builder.Entity<EmployeeRate>()
            .Property(er => er.ChargeRate)
            .HasPrecision(18, 2);

        builder.Entity<EmployeeRate>()
            .Property(er => er.CostRate)
            .HasPrecision(18, 2);

        // Add indexes for performance
        builder.Entity<TimeEntry>()
            .HasIndex(te => te.ClockInTime)
            .HasDatabaseName("IX_TimeEntry_ClockInTime");

        builder.Entity<TimeEntry>()
            .HasIndex(te => te.UserId)
            .HasDatabaseName("IX_TimeEntry_UserId");

        builder.Entity<TimeEntry>()
            .HasIndex(te => te.JobId)
            .HasDatabaseName("IX_TimeEntry_JobId");

        builder.Entity<LeaveRequest>()
            .HasIndex(lr => lr.StartDate)
            .HasDatabaseName("IX_LeaveRequest_StartDate");

        builder.Entity<LeaveRequest>()
            .HasIndex(lr => lr.Status)
            .HasDatabaseName("IX_LeaveRequest_Status");

        builder.Entity<JobOrder>()
            .HasIndex(jo => jo.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_JobOrder_OrderNumber");

        builder.Entity<JobOrder>()
            .HasIndex(jo => jo.ClientId)
            .HasDatabaseName("IX_JobOrder_ClientId");

        builder.Entity<Quote>()
            .HasIndex(q => q.QuoteNumber)
            .IsUnique()
            .HasDatabaseName("IX_Quote_QuoteNumber");

        builder.Entity<Quote>()
            .HasIndex(q => q.JobOrderId)
            .HasDatabaseName("IX_Quote_JobOrderId");

        builder.Entity<Job>()
            .HasIndex(j => j.JobNumber)
            .IsUnique()
            .HasDatabaseName("IX_Job_JobNumber");

        builder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("IX_Invoice_InvoiceNumber");

        builder.Entity<EmployeeRate>()
            .HasIndex(er => new { er.EmployeeId, er.ServiceItemId, er.EffectiveDate })
            .HasDatabaseName("IX_EmployeeRate_Employee_ServiceItem_Date");

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.EmployeeId)
            .IsUnique()
            .HasDatabaseName("IX_ApplicationUser_EmployeeId");

        // Configure automatic timestamp updates
        builder.Entity<ApplicationUser>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<TimeEntry>()
            .Property(te => te.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<LeaveRequest>()
            .Property(lr => lr.UpdatedAt)
            .HasDefaultValueSql("NOW()");
    }
}