namespace CodeLens.Application.Common.Interfaces;

/// <summary>
/// Contract for generating and validating JWT access tokens.
/// Implementation lives in the Infrastructure layer.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT for an authenticated user.
    /// </summary>
    /// <param name="userId">The Identity user ID (subject claim).</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <returns>
    /// A tuple containing the compact serialised JWT string and the UTC expiry timestamp
    /// derived from <c>JwtSettings.ExpirationHours</c>.
    /// </returns>
    (string Token, DateTime ExpiresAt) GenerateToken(string userId, string email, string displayName);

    /// <summary>
    /// Validates a JWT and extracts the user ID if the token is valid.
    /// </summary>
    /// <param name="token">The compact JWT string to validate.</param>
    /// <returns>
    /// <c>(true, userId)</c> when the token is valid;
    /// <c>(false, null)</c> when it is expired or tampered.
    /// </returns>
    (bool IsValid, string? UserId) ValidateToken(string token);
}
