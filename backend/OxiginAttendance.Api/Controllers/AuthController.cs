using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxiginAttendance.Api.DTOs;
using OxiginAttendance.Api.Services;
using System.Security.Claims;

namespace OxiginAttendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
        {
            return BadRequest(new { message = "Registration failed. User may already exist." });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("validate")]
    [Authorize]
    public ActionResult ValidateToken()
    {
        return Ok(new { message = "Token is valid", userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value });
    }
}