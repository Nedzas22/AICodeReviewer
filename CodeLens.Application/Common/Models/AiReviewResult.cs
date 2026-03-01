using CodeLens.Domain.Enums;

namespace CodeLens.Application.Common.Models;

/// <summary>
/// Represents the fully parsed, structured response from the AI review engine.
/// This is an internal Application model — it is never returned directly to clients.
/// </summary>
public sealed class AiReviewResult
{
    /// <summary>Gets the overall narrative summary written by the AI.</summary>
    public string Summary { get; init; } = default!;

    /// <summary>Gets the overall code quality score between 0 (worst) and 100 (best).</summary>
    public int OverallScore { get; init; }

    /// <summary>Gets the AI model identifier that produced this result (e.g., "gpt-4o").</summary>
    public string Model { get; init; } = default!;

    /// <summary>Gets the list of individual issues identified by the AI.</summary>
    public IReadOnlyList<AiReviewIssue> Issues { get; init; } = [];
}

/// <summary>
/// Represents a single issue parsed from the AI's structured JSON response.
/// </summary>
public sealed class AiReviewIssue
{
    /// <summary>Gets the severity of the issue.</summary>
    public Severity Severity { get; init; }

    /// <summary>Gets the short headline title for the issue.</summary>
    public string Title { get; init; } = default!;

    /// <summary>Gets the detailed description of the problem and its impact.</summary>
    public string Description { get; init; } = default!;

    /// <summary>Gets the 1-based line number where the issue starts, if applicable.</summary>
    public int? LineStart { get; init; }

    /// <summary>Gets the 1-based line number where the issue ends, if applicable.</summary>
    public int? LineEnd { get; init; }

    /// <summary>Gets the AI-suggested code fix or refactoring snippet, if provided.</summary>
    public string? SuggestedFix { get; init; }

    /// <summary>Gets the issue category (e.g., "Security", "Performance", "Style").</summary>
    public string? Category { get; init; }
}
