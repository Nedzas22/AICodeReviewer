using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using CodeLens.Application.Features.Auth.Commands;

namespace CodeLens.Api.Controllers;

/// <summary>
/// Handles user registration, login, and profile retrieval.
/// Register and login are anonymous; <c>GET /me</c> requires a valid JWT.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    /// <summary>Rate-limit policy name applied to the anonymous auth endpoints.</summary>
    public const string RateLimitPolicy = "auth-fixed-window";

    private readonly ISender _sender;

    /// <summary>Initialises the controller with the MediatR sender.</summary>
    public AuthController(ISender sender) => _sender = sender;

    /// <summary>
    /// Creates a new user account and returns a signed JWT on success.
    /// </summary>
    /// <param name="command">Registration details (email, password, display name).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="201">Account created — JWT returned in body.</response>
    /// <response code="400">Validation failure (password strength, email format, etc.).</response>
    /// <response code="409">An account with the supplied email already exists.</response>
    /// <response code="429">Too many requests — rate limit exceeded.</response>
    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicy)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Authenticates an existing user and returns a signed JWT on success.
    /// Error messages are intentionally generic to prevent user enumeration.
    /// </summary>
    /// <param name="command">Login credentials (email and password).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Authentication successful — JWT returned in body.</response>
    /// <response code="400">Invalid credentials or validation failure.</response>
    /// <response code="429">Too many requests — rate limit exceeded.</response>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicy)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user, read from JWT claims.
    /// No database round-trip is required.
    /// </summary>
    /// <response code="200">User profile returned.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId      = User.FindFirstValue("sub")!;
        var email       = User.FindFirstValue("email")!;
        var displayName = User.FindFirstValue("display_name") ?? string.Empty;

        return Ok(new UserDto(userId, email, displayName));
    }
}
