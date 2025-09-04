using Microsoft.EntityFrameworkCore;
using OxiginAttendance.Api.Data;
using OxiginAttendance.Api.DTOs;
using OxiginAttendance.Api.Models;

namespace OxiginAttendance.Api.Services;

public interface ITimeEntryService
{
    Task<TimeEntryDto?> ClockInAsync(string userId, ClockInDto clockInDto);
    Task<TimeEntryDto?> ClockOutAsync(string userId, ClockOutDto clockOutDto);
    Task<TimeEntryDto?> GetActiveTimeEntryAsync(string userId);
    Task<List<TimeEntryDto>> GetTimeEntriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<TimeReportDto> GetTimeReportAsync(string userId, DateTime startDate, DateTime endDate);
    Task<List<TimeReportDto>> GetAllEmployeesTimeReportAsync(DateTime startDate, DateTime endDate);
    Task<TimeEntryDto?> CreateTimeEntryAsync(string userId, TimeEntryCreateDto createDto);
    Task<TimeEntryDto?> UpdateTimeEntryAsync(int timeEntryId, TimeEntryUpdateDto updateDto);
    Task<bool> DeleteTimeEntryAsync(int timeEntryId);
}

public class TimeEntryService : ITimeEntryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TimeEntryService> _logger;

    public TimeEntryService(ApplicationDbContext context, ILogger<TimeEntryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TimeEntryDto?> ClockInAsync(string userId, ClockInDto clockInDto)
    {
        try
        {
            // Check if user already has an active time entry
            var activeEntry = await _context.TimeEntries
                .FirstOrDefaultAsync(te => te.UserId == userId && te.Status == TimeEntryStatus.Active);

            if (activeEntry != null)
            {
                _logger.LogWarning("User {UserId} already has an active time entry", userId);
                return null; // User already clocked in
            }

            var timeEntry = new TimeEntry
            {
                UserId = userId,
                ClockInTime = DateTime.UtcNow,
                Notes = clockInDto.Notes,
                Location = clockInDto.Location,
                LocationCoordinates = clockInDto.LocationCoordinates,
                JobId = clockInDto.JobId,
                Status = TimeEntryStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();

            return await MapToTimeEntryDtoAsync(timeEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clocking in user {UserId}", userId);
            return null;
        }
    }

    public async Task<TimeEntryDto?> ClockOutAsync(string userId, ClockOutDto clockOutDto)
    {
        try
        {
            var timeEntry = await _context.TimeEntries
                .Include(te => te.User)
                .FirstOrDefaultAsync(te => te.Id == clockOutDto.TimeEntryId 
                    && te.UserId == userId 
                    && te.Status == TimeEntryStatus.Active);

            if (timeEntry == null)
            {
                _logger.LogWarning("Active time entry {TimeEntryId} not found for user {UserId}", clockOutDto.TimeEntryId, userId);
                return null;
            }

            var clockOutTime = DateTime.UtcNow;
            timeEntry.ClockOutTime = clockOutTime;
            timeEntry.BreakTime = clockOutDto.BreakTime ?? TimeSpan.Zero;
            timeEntry.Status = TimeEntryStatus.Completed;
            timeEntry.UpdatedAt = clockOutTime;

            if (!string.IsNullOrEmpty(clockOutDto.Notes))
            {
                timeEntry.Notes = string.IsNullOrEmpty(timeEntry.Notes) 
                    ? clockOutDto.Notes 
                    : $"{timeEntry.Notes}; {clockOutDto.Notes}";
            }

            // Calculate total hours
            var totalTime = clockOutTime - timeEntry.ClockInTime;
            timeEntry.TotalHours = totalTime - timeEntry.BreakTime.Value;

            // Calculate overtime (assuming 8 hours is standard work day)
            var standardWorkDay = TimeSpan.FromHours(8);
            timeEntry.OvertimeHours = timeEntry.TotalHours > standardWorkDay 
                ? timeEntry.TotalHours - standardWorkDay 
                : TimeSpan.Zero;

            await _context.SaveChangesAsync();

            return await MapToTimeEntryDtoAsync(timeEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clocking out user {UserId} for time entry {TimeEntryId}", userId, clockOutDto.TimeEntryId);
            return null;
        }
    }

    public async Task<TimeEntryDto?> GetActiveTimeEntryAsync(string userId)
    {
        try
        {
            var timeEntry = await _context.TimeEntries
                .Include(te => te.User)
                .FirstOrDefaultAsync(te => te.UserId == userId && te.Status == TimeEntryStatus.Active);

            return timeEntry != null ? await MapToTimeEntryDtoAsync(timeEntry) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active time entry for user {UserId}", userId);
            return null;
        }
    }

    public async Task<List<TimeEntryDto>> GetTimeEntriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.TimeEntries
                .Include(te => te.User)
                .Where(te => te.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(te => te.ClockInTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(te => te.ClockInTime <= endDate.Value.AddDays(1).AddTicks(-1));
            }

            var timeEntries = await query
                .OrderByDescending(te => te.ClockInTime)
                .ToListAsync();

            var timeEntryDtos = new List<TimeEntryDto>();
            foreach (var entry in timeEntries)
            {
                var dto = await MapToTimeEntryDtoAsync(entry);
                if (dto != null)
                {
                    timeEntryDtos.Add(dto);
                }
            }

            return timeEntryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time entries for user {UserId}", userId);
            return new List<TimeEntryDto>();
        }
    }

    public async Task<TimeReportDto> GetTimeReportAsync(string userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var timeEntries = await GetTimeEntriesAsync(userId, startDate, endDate);
            var user = await _context.Users.FindAsync(userId);

            var report = new TimeReportDto
            {
                UserId = userId,
                User = user != null ? new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmployeeId = user.EmployeeId,
                    Department = user.Department,
                    JobTitle = user.JobTitle
                } : null,
                StartDate = startDate,
                EndDate = endDate,
                TimeEntries = timeEntries,
                TotalDaysWorked = timeEntries.Count(te => te.Status == TimeEntryStatus.Completed),
                TotalWorkedHours = TimeSpan.FromTicks(timeEntries
                    .Where(te => te.TotalHours.HasValue)
                    .Sum(te => te.TotalHours!.Value.Ticks)),
                TotalOvertimeHours = TimeSpan.FromTicks(timeEntries
                    .Where(te => te.OvertimeHours.HasValue)
                    .Sum(te => te.OvertimeHours!.Value.Ticks))
            };

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating time report for user {UserId}", userId);
            return new TimeReportDto { UserId = userId, StartDate = startDate, EndDate = endDate };
        }
    }

    public async Task<List<TimeReportDto>> GetAllEmployeesTimeReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            var reports = new List<TimeReportDto>();

            foreach (var user in users)
            {
                var report = await GetTimeReportAsync(user.Id, startDate, endDate);
                reports.Add(report);
            }

            return reports.OrderBy(r => r.User?.LastName).ThenBy(r => r.User?.FirstName).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating all employees time report");
            return new List<TimeReportDto>();
        }
    }

    public async Task<TimeEntryDto?> CreateTimeEntryAsync(string userId, TimeEntryCreateDto createDto)
    {
        try
        {
            var timeEntry = new TimeEntry
            {
                UserId = userId,
                ClockInTime = createDto.ClockInTime,
                ClockOutTime = createDto.ClockOutTime,
                Notes = createDto.Notes,
                Location = createDto.Location,
                BreakTime = createDto.BreakTime ?? TimeSpan.Zero,
                Status = createDto.ClockOutTime.HasValue ? TimeEntryStatus.Completed : TimeEntryStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (createDto.ClockOutTime.HasValue)
            {
                var totalTime = createDto.ClockOutTime.Value - createDto.ClockInTime;
                timeEntry.TotalHours = totalTime - timeEntry.BreakTime.Value;
                
                var standardWorkDay = TimeSpan.FromHours(8);
                timeEntry.OvertimeHours = timeEntry.TotalHours > standardWorkDay 
                    ? timeEntry.TotalHours - standardWorkDay 
                    : TimeSpan.Zero;
            }

            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();

            return await MapToTimeEntryDtoAsync(timeEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating time entry for user {UserId}", userId);
            return null;
        }
    }

    public async Task<TimeEntryDto?> UpdateTimeEntryAsync(int timeEntryId, TimeEntryUpdateDto updateDto)
    {
        try
        {
            var timeEntry = await _context.TimeEntries
                .Include(te => te.User)
                .FirstOrDefaultAsync(te => te.Id == timeEntryId);

            if (timeEntry == null)
            {
                return null;
            }

            if (updateDto.ClockInTime.HasValue)
                timeEntry.ClockInTime = updateDto.ClockInTime.Value;
            
            if (updateDto.ClockOutTime.HasValue)
                timeEntry.ClockOutTime = updateDto.ClockOutTime.Value;
            
            if (updateDto.Notes != null)
                timeEntry.Notes = updateDto.Notes;
            
            if (updateDto.Location != null)
                timeEntry.Location = updateDto.Location;
            
            if (updateDto.BreakTime.HasValue)
                timeEntry.BreakTime = updateDto.BreakTime.Value;
            
            if (updateDto.Status.HasValue)
                timeEntry.Status = updateDto.Status.Value;

            timeEntry.UpdatedAt = DateTime.UtcNow;

            // Recalculate hours if both clock in and out times are available
            if (timeEntry.ClockOutTime.HasValue)
            {
                var totalTime = timeEntry.ClockOutTime.Value - timeEntry.ClockInTime;
                timeEntry.TotalHours = totalTime - (timeEntry.BreakTime ?? TimeSpan.Zero);
                
                var standardWorkDay = TimeSpan.FromHours(8);
                timeEntry.OvertimeHours = timeEntry.TotalHours > standardWorkDay 
                    ? timeEntry.TotalHours - standardWorkDay 
                    : TimeSpan.Zero;
            }

            await _context.SaveChangesAsync();

            return await MapToTimeEntryDtoAsync(timeEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating time entry {TimeEntryId}", timeEntryId);
            return null;
        }
    }

    public async Task<bool> DeleteTimeEntryAsync(int timeEntryId)
    {
        try
        {
            var timeEntry = await _context.TimeEntries.FindAsync(timeEntryId);
            if (timeEntry == null)
            {
                return false;
            }

            _context.TimeEntries.Remove(timeEntry);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting time entry {TimeEntryId}", timeEntryId);
            return false;
        }
    }

    private async Task<TimeEntryDto?> MapToTimeEntryDtoAsync(TimeEntry timeEntry)
    {
        try
        {
            // Load user and job if not already loaded
            if (timeEntry.User == null)
            {
                await _context.Entry(timeEntry)
                    .Reference(te => te.User)
                    .LoadAsync();
            }
            
            if (timeEntry.Job == null && timeEntry.JobId.HasValue)
            {
                await _context.Entry(timeEntry)
                    .Reference(te => te.Job)
                    .LoadAsync();
            }

            return new TimeEntryDto
            {
                Id = timeEntry.Id,
                UserId = timeEntry.UserId,
                ClockInTime = timeEntry.ClockInTime,
                ClockOutTime = timeEntry.ClockOutTime,
                TotalHours = timeEntry.TotalHours,
                BreakTime = timeEntry.BreakTime,
                OvertimeHours = timeEntry.OvertimeHours,
                Notes = timeEntry.Notes,
                Location = timeEntry.Location,
                PhotoPath = timeEntry.PhotoPath,
                LocationCoordinates = timeEntry.LocationCoordinates,
                FacialRecognitionVerified = timeEntry.FacialRecognitionVerified,
                JobId = timeEntry.JobId,
                Status = timeEntry.Status,
                CreatedAt = timeEntry.CreatedAt,
                UpdatedAt = timeEntry.UpdatedAt,
                User = timeEntry.User != null ? new UserDto
                {
                    Id = timeEntry.User.Id,
                    Email = timeEntry.User.Email ?? "",
                    FirstName = timeEntry.User.FirstName,
                    LastName = timeEntry.User.LastName,
                    EmployeeId = timeEntry.User.EmployeeId,
                    Department = timeEntry.User.Department,
                    JobTitle = timeEntry.User.JobTitle
                } : null,
                Job = timeEntry.Job != null ? new JobDto
                {
                    Id = timeEntry.Job.Id,
                    JobNumber = timeEntry.Job.JobNumber,
                    JobOrderId = timeEntry.Job.JobOrderId,
                    CrewBossId = timeEntry.Job.CrewBossId,
                    Status = timeEntry.Job.Status.ToString(),
                    ActualStartTime = timeEntry.Job.ActualStartTime,
                    ActualEndTime = timeEntry.Job.ActualEndTime,
                    CompletionNotes = timeEntry.Job.CompletionNotes,
                    CreatedAt = timeEntry.Job.CreatedAt
                } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping time entry {TimeEntryId} to DTO", timeEntry.Id);
            return null;
        }
    }
}