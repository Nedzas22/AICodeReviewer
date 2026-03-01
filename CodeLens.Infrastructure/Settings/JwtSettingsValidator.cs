using Microsoft.Extensions.Options;

namespace CodeLens.Infrastructure.Settings;

/// <summary>
/// Validates <see cref="JwtSettings"/> at application startup.
/// Throws an <see cref="OptionsValidationException"/> if <see cref="JwtSettings.SecretKey"/>
/// is shorter than <see cref="MinimumSecretKeyLength"/> characters, preventing the application
/// from starting with a cryptographically weak signing key.
/// </summary>
internal sealed class JwtSettingsValidator : IValidateOptions<JwtSettings>
{
    /// <summary>Minimum acceptable length for the HMAC-SHA256 signing key.</summary>
    public const int MinimumSecretKeyLength = 32;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, JwtSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.SecretKey) ||
            options.SecretKey.Length < MinimumSecretKeyLength)
        {
            return ValidateOptionsResult.Fail(
                $"JwtSettings.SecretKey must be at least {MinimumSecretKeyLength} characters long " +
                $"to ensure a cryptographically strong HMAC-SHA256 signature. " +
                $"Current length: {options.SecretKey?.Length ?? 0}.");
        }

        return ValidateOptionsResult.Success;
    }
}
