using System.Security.Claims;
using CodeLens.Application.Common.Interfaces;

namespace CodeLens.Api.Extensions;

/// <summary>
/// Reads the authenticated user's identity from the current <see cref="HttpContext"/>
/// using the claims extracted from the incoming JWT by the Bearer authentication middleware.
/// Requires <c>MapInboundClaims = false</c> on the JwtBearer options so raw claim names
/// (e.g., "sub", "email") are preserved.
/// </summary>
internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initialises the service with the HTTP context accessor.</summary>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public string? UserId => Principal?.FindFirstValue("sub");

    /// <inheritdoc />
    public string? Email => Principal?.FindFirstValue("email");

    /// <inheritdoc />
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
