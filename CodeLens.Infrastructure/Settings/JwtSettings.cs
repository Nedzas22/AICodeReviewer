namespace CodeLens.Infrastructure.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpirationHours { get; init; } = 24;
}
