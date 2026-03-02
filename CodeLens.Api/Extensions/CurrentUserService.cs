using System.Security.Claims;
using CodeLens.Application.Common.Interfaces;

namespace CodeLens.Api.Extensions;

internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue("sub");
    public string? Email => Principal?.FindFirstValue("email");
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
