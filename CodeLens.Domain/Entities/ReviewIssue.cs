using CodeLens.Domain.Common;
using CodeLens.Domain.Enums;

namespace CodeLens.Domain.Entities;

public sealed class ReviewIssue : BaseEntity
{
    // EF Core requires a parameterless constructor
    private ReviewIssue() { }

    public Guid CodeReviewId { get; private set; }
    public Severity Severity { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public int? LineStart { get; private set; }
    public int? LineEnd { get; private set; }
    public string? SuggestedFix { get; private set; }
    public string? Category { get; private set; }

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
