using System.Text.Json;
using System.Text.Json.Serialization;
using CodeLens.Web.Models.Auth;
using CodeLens.Web.Models.Common;

namespace CodeLens.Web.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task LogoutAsync();
}

internal sealed class AuthService : IAuthService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly ITokenService _tokenService;
    private readonly JwtAuthStateProvider _authStateProvider;

    public AuthService(
        HttpClient http,
        ITokenService tokenService,
        JwtAuthStateProvider authStateProvider)
    {
        _http = http;
        _tokenService = tokenService;
        _authStateProvider = authStateProvider;
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", request, ct);
        return await HandleAuthResponseAsync(response, ct);
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/register", request, ct);
        return await HandleAuthResponseAsync(response, ct);
    }

    public async Task LogoutAsync()
    {
        await _tokenService.RemoveTokenAsync();
        _authStateProvider.NotifyUserLoggedOut();
    }

    private async Task<ServiceResult<AuthResponse>> HandleAuthResponseAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOpts, ct);
            await _tokenService.SetTokenAsync(result!.Token);
            await _authStateProvider.NotifyUserAuthenticatedAsync(result.Token);
            return ServiceResult<AuthResponse>.Success(result);
        }

        var error = await TryReadErrorAsync(response, ct);
        return ServiceResult<AuthResponse>.Failure(
            error.detail ?? error.title ?? $"Request failed ({(int)response.StatusCode}).",
            error.errors);
    }

    private static async Task<(string? title, string? detail, IReadOnlyDictionary<string, string[]>? errors)>
        TryReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            var title  = doc.RootElement.TryGetProperty("title",  out var t) ? t.GetString() : null;
            var detail = doc.RootElement.TryGetProperty("detail", out var d) ? d.GetString() : null;

            Dictionary<string, string[]>? errors = null;
            if (doc.RootElement.TryGetProperty("errors", out var e) &&
                e.ValueKind == JsonValueKind.Object)
            {
                errors = e.EnumerateObject()
                    .ToDictionary(
                        p => p.Name,
                        p => p.Value.EnumerateArray().Select(v => v.GetString()!).ToArray());
            }

            return (title, detail, errors);
        }
        catch
        {
            return (null, null, null);
        }
    }
}
