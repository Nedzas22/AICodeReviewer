using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using CodeLens.Web.Helpers;

namespace CodeLens.Web.Services;

/// <summary>
/// Reads the JWT from localStorage on each auth state check and builds the
/// <see cref="ClaimsPrincipal"/> from the token's payload claims.
/// </summary>
public sealed class JwtAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ITokenService _tokenService;

    public JwtAuthStateProvider(ITokenService tokenService) => _tokenService = tokenService;

    /// <inheritdoc />
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenService.GetTokenAsync();

        if (string.IsNullOrEmpty(token) || !JwtHelper.IsTokenValid(token))
        {
            await _tokenService.RemoveTokenAsync();
            return AnonymousState;
        }

        var claims = JwtHelper.ParseClaims(token);
        var identity = new ClaimsIdentity(claims, "jwt", "sub", null);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    /// <summary>Notify all subscribers that the user has authenticated with the given token.</summary>
    public async Task NotifyUserAuthenticatedAsync(string token)
    {
        var claims = JwtHelper.ParseClaims(token);
        var identity = new ClaimsIdentity(claims, "jwt", "sub", null);
        var principal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        await Task.CompletedTask;
    }

    /// <summary>Notify all subscribers that the user has signed out.</summary>
    public void NotifyUserLoggedOut() =>
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
}
