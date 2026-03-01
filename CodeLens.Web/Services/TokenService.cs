using Blazored.LocalStorage;
using CodeLens.Web.Helpers;

namespace CodeLens.Web.Services;

/// <inheritdoc />
internal sealed class TokenService : ITokenService
{
    private const string TokenKey = "codelens_token";
    private readonly ILocalStorageService _localStorage;

    public TokenService(ILocalStorageService localStorage) => _localStorage = localStorage;

    public async Task<string?> GetTokenAsync() =>
        await _localStorage.GetItemAsStringAsync(TokenKey);

    public async Task SetTokenAsync(string token) =>
        await _localStorage.SetItemAsStringAsync(TokenKey, token);

    public async Task RemoveTokenAsync() =>
        await _localStorage.RemoveItemAsync(TokenKey);

    public async Task<bool> IsTokenValidAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token) && JwtHelper.IsTokenValid(token);
    }
}
