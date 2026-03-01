using CodeLens.Domain.Common;
using CodeLens.Domain.Enums;

namespace CodeLens.Domain.Entities;

/// <summary>
/// Represents a single issue or suggestion identified by the AI during a code review.
/// Belongs to a <see cref="CodeReview"/> aggregate.
/// </summary>
public sealed class ReviewIssue : BaseEntity
{
    // EF Core requires a parameterless constructor
    private ReviewIssue() { }

    /// <summary>Gets the ID of the parent <see cref="CodeReview"/> aggregate.</summary>
    public Guid CodeReviewId { get; private set; }

    /// <summary>Gets the severity level of this issue.</summary>
    public Severity Severity { get; private set; }

    /// <summary>Gets the short title or headline for this issue.</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Gets the detailed description of the issue and its impact.</summary>
    public string Description { get; private set; } = default!;

    /// <summary>Gets the 1-based line number where the issue starts, or <c>null</c> if not line-specific.</summary>
    public int? LineStart { get; private set; }

    /// <summary>Gets the 1-based line number where the issue ends, or <c>null</c> if single-line.</summary>
    public int? LineEnd { get; private set; }

    /// <summary>
    /// Gets the AI-suggested code fix or refactoring, if available.
    /// May contain a code snippet.
    /// </summary>
    public string? SuggestedFix { get; private set; }

    /// <summary>
    /// Gets the issue category (e.g., "Security", "Performance", "Maintainability", "Style").
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Factory method to create a validated <see cref="ReviewIssue"/>.
    /// </summary>
    /// <param name="codeReviewId">The parent review's ID.</param>
    /// <param name="severity">How severe the issue is.</param>
    /// <param name="title">Short headline for the issue.</param>
    /// <param name="description">Detailed explanation of the problem.</param>
    /// <param name="lineStart">Optional 1-based start line number.</param>
    /// <param name="lineEnd">Optional 1-based end line number.</param>
    /// <param name="suggestedFix">Optional AI-generated fix snippet.</param>
    /// <param name="category">Optional category label.</param>
    /// <returns>A new, valid <see cref="ReviewIssue"/> instance.</returns>
    public static ReviewIssue Create(
        Guid codeReviewId,
        Severity severity,
        string title,
        string description,
        int? lineStart = null,
        int? lineEnd = null,
        string? suggestedFix = null,
        string? category = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new ReviewIssue
        {
            CodeReviewId = codeReviewId,
            Severity = severity,
            Title = title.Trim(),
            Description = description.Trim(),
            LineStart = lineStart,
            LineEnd = lineEnd,
            SuggestedFix = suggestedFix?.Trim(),
            Category = category?.Trim()
        };
    }
}
