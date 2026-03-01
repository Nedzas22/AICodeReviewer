namespace CodeLens.Application.DTOs;

/// <summary>Lightweight user profile DTO used in identity lookups and responses.</summary>
/// <param name="UserId">The Identity user ID (GUID string).</param>
/// <param name="Email">The user's email address.</param>
/// <param name="DisplayName">The user's chosen display name.</param>
public sealed record UserDto(
    string UserId,
    string Email,
    string DisplayName);
