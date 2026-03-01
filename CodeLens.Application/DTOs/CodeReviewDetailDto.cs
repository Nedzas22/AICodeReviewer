using CodeLens.Domain.Enums;

namespace CodeLens.Application.DTOs;

/// <summary>
/// Full detail DTO for a code review — used on the review detail page.
/// Includes source code, all issues, and the AI model used.
/// </summary>
/// <param name="Id">The review's unique identifier.</param>
/// <param name="Title">The user-supplied title.</param>
/// <param name="SourceCode">The original code submitted for review.</param>
/// <param name="Language">The programming language of the reviewed code.</param>
/// <param name="Status">The current lifecycle status.</param>
/// <param name="OverallScore">The AI quality score (0–100), or <c>null</c> if not completed.</param>
/// <param name="Summary">The AI narrative summary, or <c>null</c> if not completed.</param>
/// <param name="AiModel">The AI model identifier used (e.g., "gpt-4o").</param>
/// <param name="IssueCount">Total number of issues found.</param>
/// <param name="CreatedAt">UTC timestamp of submission.</param>
/// <param name="CompletedAt">UTC timestamp of completion, or <c>null</c>.</param>
/// <param name="ErrorMessage">Failure reason when status is <see cref="ReviewStatus.Failed"/>.</param>
/// <param name="Issues">The full list of issues identified by the AI.</param>
public sealed record CodeReviewDetailDto(
    Guid Id,
    string Title,
    string SourceCode,
    ProgrammingLanguage Language,
    ReviewStatus Status,
    int? OverallScore,
    string? Summary,
    string? AiModel,
    int IssueCount,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    IReadOnlyList<ReviewIssueDto> Issues);
