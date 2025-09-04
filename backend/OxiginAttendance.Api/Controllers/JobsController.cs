using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OxiginAttendance.Api.Data;
using OxiginAttendance.Api.DTOs;
using OxiginAttendance.Api.Models;
using System.Security.Claims;

namespace OxiginAttendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<JobsController> _logger;

    public JobsController(ApplicationDbContext context, ILogger<JobsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get jobs (Manager/Admin see all, Crew Boss sees assigned jobs, Employee sees assigned jobs)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetJobs()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrManager = User.IsInRole("Administrator") || User.IsInRole("Manager");
            var isCrewBoss = User.IsInRole("CrewBoss");

            IQueryable<Job> query = _context.Jobs
                .Include(j => j.JobOrder)
                .Include(j => j.CrewBoss);

            if (!isAdminOrManager)
            {
                if (isCrewBoss)
                {
                    // Crew Boss sees assigned jobs
                    query = query.Where(j => j.CrewBossId == userId);
                }
                else
                {
                    // Employee sees jobs they're assigned to
                    query = query.Where(j => j.JobAssignments.Any(ja => ja.EmployeeId == userId && ja.IsActive));
                }
            }

            var jobs = await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
            var jobDtos = jobs.Select(MapToJobDto).ToList();

            return Ok(jobDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jobs for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return StatusCode(500, "An error occurred while retrieving jobs");
        }
    }

    /// <summary>
    /// Get job by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JobDto>> GetJob(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrManager = User.IsInRole("Administrator") || User.IsInRole("Manager");

            var job = await _context.Jobs
                .Include(j => j.JobOrder)
                .Include(j => j.CrewBoss)
                .Include(j => j.JobAssignments)
                    .ThenInclude(ja => ja.Employee)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return NotFound($"Job with ID {id} not found");
            }

            // Check authorization
            if (!isAdminOrManager && job.CrewBossId != userId && 
                !job.JobAssignments.Any(ja => ja.EmployeeId == userId && ja.IsActive))
            {
                return Forbid("You don't have access to this job");
            }

            var jobDto = MapToJobDto(job);
            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job {JobId}", id);
            return StatusCode(500, "An error occurred while retrieving the job");
        }
    }

    /// <summary>
    /// Create job from approved job order (Manager/Admin only)
    /// </summary>
    [HttpPost("from-order/{jobOrderId}")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<JobDto>> CreateJobFromOrder(int jobOrderId, [FromBody] CreateJobDto createJobDto)
    {
        try
        {
            var jobOrder = await _context.JobOrders
                .Include(jo => jo.Quotes)
                .FirstOrDefaultAsync(jo => jo.Id == jobOrderId);

            if (jobOrder == null)
            {
                return NotFound($"Job order with ID {jobOrderId} not found");
            }

            if (jobOrder.Status != OrderStatus.Approved)
            {
                return BadRequest("Job order must be approved before creating a job");
            }

            // Check if job already exists for this order
            var existingJob = await _context.Jobs.FirstOrDefaultAsync(j => j.JobOrderId == jobOrderId);
            if (existingJob != null)
            {
                return BadRequest("Job already exists for this order");
            }

            var jobNumber = await GenerateJobNumberAsync();

            var job = new Job
            {
                JobNumber = jobNumber,
                JobOrderId = jobOrderId,
                CrewBossId = createJobDto.CrewBossId,
                Status = JobStatus.Assigned,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(job);

            // Update job order status
            jobOrder.Status = OrderStatus.InProgress;
            jobOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var jobDto = MapToJobDto(job);
            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job from order {JobOrderId}", jobOrderId);
            return StatusCode(500, "An error occurred while creating the job");
        }
    }

    /// <summary>
    /// Update job status (Crew Boss can update assigned jobs, Manager/Admin can update any)
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<JobDto>> UpdateJobStatus(int id, [FromBody] UpdateJobStatusDto updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrManager = User.IsInRole("Administrator") || User.IsInRole("Manager");

            var job = await _context.Jobs
                .Include(j => j.JobOrder)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return NotFound($"Job with ID {id} not found");
            }

            // Check authorization
            if (!isAdminOrManager && job.CrewBossId != userId)
            {
                return Forbid("You can only update status of jobs assigned to you");
            }

            if (!Enum.TryParse<JobStatus>(updateDto.Status, out var status))
            {
                return BadRequest($"Invalid status: {updateDto.Status}");
            }

            job.Status = status;
            job.UpdatedAt = DateTime.UtcNow;

            if (status == JobStatus.InProgress && !job.ActualStartTime.HasValue)
            {
                job.ActualStartTime = DateTime.UtcNow;
            }
            else if (status == JobStatus.Completed)
            {
                job.ActualEndTime = DateTime.UtcNow;
                job.CompletionNotes = updateDto.Notes;
                
                // Update job order status
                if (job.JobOrder != null)
                {
                    job.JobOrder.Status = OrderStatus.Completed;
                    job.JobOrder.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            var jobDto = MapToJobDto(job);
            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId} status", id);
            return StatusCode(500, "An error occurred while updating the job status");
        }
    }

    /// <summary>
    /// Assign employee to job (Crew Boss or Manager/Admin only)
    /// </summary>
    [HttpPost("{id}/assign-employee")]
    [Authorize(Roles = "CrewBoss,Manager,Administrator")]
    public async Task<ActionResult> AssignEmployeeToJob(int id, [FromBody] AssignEmployeeDto assignDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrManager = User.IsInRole("Administrator") || User.IsInRole("Manager");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
            {
                return NotFound($"Job with ID {id} not found");
            }

            // Check authorization
            if (!isAdminOrManager && job.CrewBossId != userId)
            {
                return Forbid("You can only assign employees to jobs assigned to you");
            }

            var employee = await _context.Users.FindAsync(assignDto.EmployeeId);
            if (employee == null)
            {
                return BadRequest("Employee not found");
            }

            // Check if already assigned
            var existingAssignment = await _context.JobAssignments
                .FirstOrDefaultAsync(ja => ja.JobId == id && ja.EmployeeId == assignDto.EmployeeId && ja.IsActive);

            if (existingAssignment != null)
            {
                return BadRequest("Employee is already assigned to this job");
            }

            var assignment = new JobAssignment
            {
                JobId = id,
                EmployeeId = assignDto.EmployeeId,
                AssignedByUserId = userId,
                AssignedDate = DateTime.UtcNow,
                Notes = assignDto.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.JobAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee assigned to job successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning employee to job {JobId}", id);
            return StatusCode(500, "An error occurred while assigning employee to job");
        }
    }

    private async Task<string> GenerateJobNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var lastJob = await _context.Jobs
            .Where(j => j.JobNumber.StartsWith($"JOB{year}-"))
            .OrderByDescending(j => j.JobNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastJob != null)
        {
            var lastNumberStr = lastJob.JobNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"JOB{year}-{nextNumber:D4}";
    }

    private static JobDto MapToJobDto(Job job)
    {
        return new JobDto
        {
            Id = job.Id,
            JobNumber = job.JobNumber,
            JobOrderId = job.JobOrderId,
            CrewBossId = job.CrewBossId,
            Status = job.Status.ToString(),
            ActualStartTime = job.ActualStartTime,
            ActualEndTime = job.ActualEndTime,
            CompletionNotes = job.CompletionNotes,
            CreatedAt = job.CreatedAt,
            JobOrder = job.JobOrder != null ? new JobOrderDto
            {
                Id = job.JobOrder.Id,
                OrderNumber = job.JobOrder.OrderNumber,
                EventName = job.JobOrder.EventName,
                SiteName = job.JobOrder.SiteName,
                SiteAddress = job.JobOrder.SiteAddress,
                Description = job.JobOrder.Description,
                StartDate = job.JobOrder.StartDate,
                EndDate = job.JobOrder.EndDate,
                Status = job.JobOrder.Status.ToString()
            } : null,
            CrewBoss = job.CrewBoss != null ? new UserDto
            {
                Id = job.CrewBoss.Id,
                Email = job.CrewBoss.Email ?? "",
                FirstName = job.CrewBoss.FirstName,
                LastName = job.CrewBoss.LastName,
                EmployeeId = job.CrewBoss.EmployeeId
            } : null
        };
    }
}

public class CreateJobDto
{
    public string? CrewBossId { get; set; }
}

public class UpdateJobStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class AssignEmployeeDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string? Notes { get; set; }
}