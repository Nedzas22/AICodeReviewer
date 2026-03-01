namespace CodeLens.Domain.Enums;

/// <summary>
/// Represents the severity level of a code issue identified during review.
/// Higher values indicate more critical problems.
/// </summary>
public enum Severity
{
    /// <summary>Informational note — no action required, purely educational.</summary>
    Info = 0,

    /// <summary>Minor issue — low impact; fix when convenient (style, naming).</summary>
    Minor = 1,

    /// <summary>Major issue — functional or maintainability concern that should be addressed.</summary>
    Major = 2,

    /// <summary>Critical issue — security vulnerability, data loss risk, or showstopper bug.</summary>
    Critical = 3
}
