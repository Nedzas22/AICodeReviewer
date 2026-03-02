using Microsoft.Extensions.Options;

namespace CodeLens.Infrastructure.Settings;

internal sealed class JwtSettingsValidator : IValidateOptions<JwtSettings>
{
    public const int MinimumSecretKeyLength = 32;

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
