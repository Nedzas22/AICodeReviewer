namespace CodeLens.Infrastructure.Settings;

/// <summary>
/// Strongly-typed configuration for JWT token generation.
/// Bind to <c>appsettings.json</c> section <c>"JwtSettings"</c>.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>The <c>appsettings.json</c> section key for this settings class.</summary>
    public const string SectionName = "JwtSettings";

    /// <summary>Gets the HMAC-SHA256 signing secret (minimum 32 characters recommended).</summary>
    public string SecretKey { get; init; } = default!;

    /// <summary>Gets the token issuer claim value (e.g., "https://api.codelens.io").</summary>
    public string Issuer { get; init; } = default!;

    /// <summary>Gets the token audience claim value (e.g., "codelens-app").</summary>
    public string Audience { get; init; } = default!;

    /// <summary>Gets the token lifetime in hours. Defaults to 24.</summary>
    public int ExpirationHours { get; init; } = 24;
}
