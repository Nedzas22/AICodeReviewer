using CodeLens.Domain.Common;
using CodeLens.Domain.Enums;

namespace CodeLens.Domain.Entities;

/// <summary>
/// Aggregate root representing a user's code review request and its AI-generated results.
/// All state transitions are controlled through behaviour methods to maintain invariants.
/// </summary>
public sealed class CodeReview : AuditableEntity
{
    private readonly List<ReviewIssue> _issues = [];

    // EF Core requires a parameterless constructor
    private CodeReview() { }

    /// <summary>Gets the identity of the user who submitted this review.</summary>
    public string UserId { get; private set; } = default!;

    /// <summary>Gets the user-supplied title for this review.</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Gets the raw source code submitted for review.</summary>
    public string SourceCode { get; private set; } = default!;

    /// <summary>Gets the programming language of the submitted code.</summary>
    public ProgrammingLanguage Language { get; private set; }

    /// <summary>Gets the current lifecycle status of this review.</summary>
    public ReviewStatus Status { get; private set; }

    /// <summary>Gets the AI model identifier used to produce this review (e.g., "gpt-4o").</summary>
    public string? AiModel { get; private set; }

    /// <summary>Gets the overall summary narrative produced by the AI.</summary>
    public string? Summary { get; private set; }

    /// <summary>
    /// Gets the overall quality score from 0–100 assigned by the AI.
    /// <c>null</c> until the review is completed.
    /// </summary>
    public int? OverallScore { get; private set; }

    /// <summary>Gets the UTC timestamp when the review was completed, or <c>null</c> if still in progress.</summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>Gets an optional error message when <see cref="Status"/> is <see cref="ReviewStatus.Failed"/>.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the total number of issues found in this review.
    /// Persisted to the database so list queries do not require an <c>Issues</c> join.
    /// </summary>
    public int IssueCount { get; private set; }

    /// <summary>Gets the read-only collection of issues found in this review.</summary>
    public IReadOnlyCollection<ReviewIssue> Issues => _issues.AsReadOnly();

    // ──────────────────────────────────────────────────────────────────────────
    // Factory
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="CodeReview"/> in <see cref="ReviewStatus.Pending"/> state.
    /// </summary>
    /// <param name="userId">The Identity user ID of the submitter.</param>
    /// <param name="title">A short descriptive title chosen by the user.</param>
    /// <param name="sourceCode">The code to be reviewed.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <returns>A new <see cref="CodeReview"/> ready for processing.</returns>
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

    // ──────────────────────────────────────────────────────────────────────────
    // Behaviour methods (state transitions)
    // State machine: Pending → Completed
    //                Pending → Failed
    //                Completed / Failed → Pending  (via ResetForRerun)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Completes the review with AI-generated results and issues.
    /// Transitions status to <see cref="ReviewStatus.Completed"/>.
    /// </summary>
    /// <param name="summary">Overall narrative summary from the AI.</param>
    /// <param name="overallScore">Quality score between 0 and 100.</param>
    /// <param name="aiModel">The model identifier used (e.g., "gpt-4o").</param>
    /// <param name="issues">All issues identified by the AI.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="overallScore"/> is outside [0, 100].
    /// </exception>
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

    /// <summary>
    /// Marks the review as failed, storing the reason for display in the UI.
    /// </summary>
    /// <param name="errorMessage">Human-readable reason for the failure.</param>
    public void Fail(string errorMessage)
    {
        Status = ReviewStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Resets a completed or failed review back to <see cref="ReviewStatus.Pending"/>
    /// so it can be re-submitted to the AI engine.
    /// </summary>
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
