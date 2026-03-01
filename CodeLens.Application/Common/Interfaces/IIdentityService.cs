using CodeLens.Application.DTOs;

namespace CodeLens.Application.Common.Interfaces;

/// <summary>
/// Abstracts ASP.NET Core Identity operations so that the Application layer
/// does not take a hard dependency on Identity packages.
/// Implementation lives in the Infrastructure layer.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="email">The user's email address (used as username).</param>
    /// <param name="password">The plain-text password to hash and store.</param>
    /// <param name="displayName">The user's chosen display name.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>
    /// <c>(true, userId, [])</c> on success;
    /// <c>(false, null, errors)</c> with Identity error messages on failure.
    /// </returns>
    Task<(bool Success, string? UserId, IReadOnlyList<string> Errors)> RegisterAsync(
        string email,
        string password,
        string displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates credentials and returns user information on success.
    /// </summary>
    /// <param name="email">The email address to authenticate.</param>
    /// <param name="password">The plain-text password to check.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>
    /// <c>(true, userId, email, displayName)</c> on success;
    /// <c>(false, null, null, null)</c> on failure.
    /// </returns>
    Task<(bool Success, string? UserId, string? Email, string? DisplayName)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a <see cref="UserDto"/> by user ID, or <c>null</c> if not found.</summary>
    Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a <see cref="UserDto"/> by email address, or <c>null</c> if not found.</summary>
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
}
