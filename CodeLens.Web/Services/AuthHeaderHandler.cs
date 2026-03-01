using System.Net.Http.Headers;

namespace CodeLens.Web.Services;

/// <summary>
/// Delegating handler that attaches a JWT Bearer token to every outgoing
/// API request if one is present in localStorage.
/// </summary>
internal sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;

    public AuthHeaderHandler(ITokenService tokenService) => _tokenService = tokenService;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
