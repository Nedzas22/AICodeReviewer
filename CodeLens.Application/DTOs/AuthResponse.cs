namespace CodeLens.Application.DTOs;

/// <summary>Returned to the client after a successful login or registration.</summary>
/// <param name="Token">The signed JWT access token.</param>
/// <param name="UserId">The Identity user ID.</param>
/// <param name="Email">The authenticated user's email.</param>
/// <param name="DisplayName">The authenticated user's display name.</param>
/// <param name="ExpiresAt">UTC timestamp when the token expires.</param>
public sealed record AuthResponse(
    string Token,
    string UserId,
    string Email,
    string DisplayName,
    DateTime ExpiresAt);
