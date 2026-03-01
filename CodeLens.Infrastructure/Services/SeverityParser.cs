using CodeLens.Domain.Enums;

namespace CodeLens.Infrastructure.Services;

/// <summary>
/// Parses the severity string returned in the AI JSON response into the
/// <see cref="Severity"/> domain enum. Exposed as internal so it can be
/// tested directly by the unit test project.
/// </summary>
internal static class SeverityParser
{
    /// <summary>
    /// Maps a case-insensitive severity label from the AI response to
    /// <see cref="Severity"/>. Unrecognised values fall back to
    /// <see cref="Severity.Minor"/>.
    /// </summary>
    public static Severity Parse(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "critical"              => Severity.Critical,
            "major"                 => Severity.Major,
            "minor"                 => Severity.Minor,
            "info" or "information" => Severity.Info,
            _                       => Severity.Minor
        };
}
