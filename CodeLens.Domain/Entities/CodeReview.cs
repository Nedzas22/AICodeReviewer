using CodeLens.Domain.Common;
using CodeLens.Domain.Enums;

namespace CodeLens.Domain.Entities;

public sealed class CodeReview : AuditableEntity
{
    private readonly List<ReviewIssue> _issues = [];

    // EF Core requires a parameterless constructor
    private CodeReview() { }

    public string UserId { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string SourceCode { get; private set; } = default!;
    public ProgrammingLanguage Language { get; private set; }
    public ReviewStatus Status { get; private set; }
    public string? AiModel { get; private set; }
    public string? Summary { get; private set; }
    public int? OverallScore { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int IssueCount { get; private set; }
    public IReadOnlyCollection<ReviewIssue> Issues => _issues.AsReadOnly();

    // ──────────────────────────────────────────────────────────────────────────
    public static CodeReview Create(
        string userId,
        string title,
        string sourceCode,
        ProgrammingLanguage language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCode);

        return new CodeReview
        {
            UserId = userId,
            Title = title.Trim(),
            SourceCode = sourceCode,
            Language = language,
            Status = ReviewStatus.Pending
        };
    }

    public void Complete(
        string summary,
        int overallScore,
        string aiModel,
        IEnumerable<ReviewIssue> issues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(aiModel);

        if (overallScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(overallScore), "Score must be between 0 and 100.");

        Summary = summary.Trim();
        OverallScore = overallScore;
        AiModel = aiModel;
        Status = ReviewStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        _issues.Clear();
        _issues.AddRange(issues);
        IssueCount = _issues.Count;
    }

    public void Fail(string errorMessage)
    {
        Status = ReviewStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetForRerun()
    {
        Status = ReviewStatus.Pending;
        Summary = null;
        OverallScore = null;
        AiModel = null;
        CompletedAt = null;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
        _issues.Clear();
        IssueCount = 0;
    }
}
