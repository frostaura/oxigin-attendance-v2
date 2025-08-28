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

        // Add indexes for performance
        builder.Entity<TimeEntry>()
            .HasIndex(te => te.ClockInTime)
            .HasDatabaseName("IX_TimeEntry_ClockInTime");

        builder.Entity<TimeEntry>()
            .HasIndex(te => te.UserId)
            .HasDatabaseName("IX_TimeEntry_UserId");

        builder.Entity<LeaveRequest>()
            .HasIndex(lr => lr.StartDate)
            .HasDatabaseName("IX_LeaveRequest_StartDate");

        builder.Entity<LeaveRequest>()
            .HasIndex(lr => lr.Status)
            .HasDatabaseName("IX_LeaveRequest_Status");

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