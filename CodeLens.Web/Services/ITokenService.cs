namespace CodeLens.Web.Services;

/// <summary>Manages JWT token persistence in the browser's localStorage.</summary>
public interface ITokenService
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task RemoveTokenAsync();
    Task<bool> IsTokenValidAsync();
}
