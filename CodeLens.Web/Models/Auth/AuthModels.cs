namespace CodeLens.Web.Models.Auth;

public sealed record AuthResponse(
    string Token,
    string UserId,
    string Email,
    string DisplayName,
    DateTime ExpiresAt);

public sealed record UserDto(string UserId, string Email, string DisplayName);
