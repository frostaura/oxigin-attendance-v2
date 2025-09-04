using OxiginAttendance.Api.DTOs;

namespace OxiginAttendance.Api.Services;

public interface IFileUploadService
{
    Task<string?> UploadPhotoAsync(IFormFile photo, string folderName);
    Task<bool> DeletePhotoAsync(string photoPath);
    string GetPhotoUrl(string photoPath);
}

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;
    private readonly string _uploadsPath;

    public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
        _uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");
        
        // Ensure uploads directory exists
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }

    public async Task<string?> UploadPhotoAsync(IFormFile photo, string folderName)
    {
        try
        {
            if (photo == null || photo.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Invalid file type: {FileExtension}", fileExtension);
                return null;
            }

            // Validate file size (max 10MB)
            if (photo.Length > 10 * 1024 * 1024)
            {
                _logger.LogWarning("File too large: {FileSize} bytes", photo.Length);
                return null;
            }

            // Create folder if it doesn't exist
            var folderPath = Path.Combine(_uploadsPath, folderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // Return relative path
            return Path.Combine(folderName, fileName).Replace("\\", "/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo");
            return null;
        }
    }

    public async Task<bool> DeletePhotoAsync(string photoPath)
    {
        try
        {
            if (string.IsNullOrEmpty(photoPath))
                return false;

            var fullPath = Path.Combine(_uploadsPath, photoPath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo: {PhotoPath}", photoPath);
            return false;
        }
    }

    public string GetPhotoUrl(string photoPath)
    {
        if (string.IsNullOrEmpty(photoPath))
            return string.Empty;

        return $"/uploads/{photoPath}";
    }
}

public interface IEnhancedCheckInService
{
    Task<TimeEntryDto?> ClockInWithPhotosAsync(string userId, ClockInWithPhotoDto clockInDto);
    Task<bool> VerifyLocationAsync(string coordinates, int? jobId);
    Task<bool> ProcessFacialRecognitionAsync(IFormFile photo, string userId);
}

public class EnhancedCheckInService : IEnhancedCheckInService
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<EnhancedCheckInService> _logger;

    public EnhancedCheckInService(
        ITimeEntryService timeEntryService,
        IFileUploadService fileUploadService,
        ILogger<EnhancedCheckInService> logger)
    {
        _timeEntryService = timeEntryService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<TimeEntryDto?> ClockInWithPhotosAsync(string userId, ClockInWithPhotoDto clockInDto)
    {
        try
        {
            // Upload photos if provided
            string? photoPath = null;
            if (clockInDto.Photo != null)
            {
                photoPath = await _fileUploadService.UploadPhotoAsync(clockInDto.Photo, "checkin-photos");
            }

            string? sitePhotoPath = null;
            if (clockInDto.SitePhoto != null)
            {
                sitePhotoPath = await _fileUploadService.UploadPhotoAsync(clockInDto.SitePhoto, "site-photos");
            }

            string? groupPhotoPath = null;
            if (clockInDto.GroupPhoto != null)
            {
                groupPhotoPath = await _fileUploadService.UploadPhotoAsync(clockInDto.GroupPhoto, "group-photos");
            }

            // Verify location if coordinates provided
            if (!string.IsNullOrEmpty(clockInDto.LocationCoordinates))
            {
                var locationValid = await VerifyLocationAsync(clockInDto.LocationCoordinates, clockInDto.JobId);
                if (!locationValid)
                {
                    _logger.LogWarning("Location verification failed for user {UserId}", userId);
                    // Continue anyway but log the issue
                }
            }

            // Process facial recognition if required
            bool facialRecognitionVerified = false;
            if (clockInDto.RequireFacialRecognition && clockInDto.Photo != null)
            {
                facialRecognitionVerified = await ProcessFacialRecognitionAsync(clockInDto.Photo, userId);
            }

            // Create enhanced clock in DTO
            var enhancedClockInDto = new ClockInDto
            {
                Notes = clockInDto.Notes,
                Location = clockInDto.Location,
                LocationCoordinates = clockInDto.LocationCoordinates,
                JobId = clockInDto.JobId
            };

            // Clock in using the enhanced service
            var timeEntry = await _timeEntryService.ClockInAsync(userId, enhancedClockInDto);
            
            // TODO: Update time entry with photo paths and facial recognition status
            // This would require updating the TimeEntryService to support these fields

            return timeEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced clock in for user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> VerifyLocationAsync(string coordinates, int? jobId)
    {
        try
        {
            // TODO: Implement actual location verification logic
            // This could involve:
            // 1. Parsing GPS coordinates
            // 2. Checking against job site location
            // 3. Verifying within acceptable radius
            
            _logger.LogInformation("Location verification for coordinates: {Coordinates}, JobId: {JobId}", coordinates, jobId);
            
            // For now, always return true (placeholder implementation)
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying location");
            return false;
        }
    }

    public async Task<bool> ProcessFacialRecognitionAsync(IFormFile photo, string userId)
    {
        try
        {
            // TODO: Implement actual facial recognition logic
            // This could involve:
            // 1. Calling Azure Face API or similar service
            // 2. Comparing with stored user photo
            // 3. Returning verification result
            
            _logger.LogInformation("Facial recognition processing for user: {UserId}", userId);
            
            // For now, always return true (placeholder implementation)
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing facial recognition");
            return false;
        }
    }
}