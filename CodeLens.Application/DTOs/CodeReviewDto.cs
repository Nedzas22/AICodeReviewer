using CodeLens.Domain.Enums;

namespace CodeLens.Application.DTOs;

/// <summary>
/// Summary DTO for a code review — used in list/history views.
/// Does not include the full source code or issue details to keep payloads small.
/// </summary>
/// <param name="Id">The review's unique identifier.</param>
/// <param name="Title">The user-supplied title.</param>
/// <param name="Language">The programming language of the reviewed code.</param>
/// <param name="Status">The current lifecycle status.</param>
/// <param name="OverallScore">The AI quality score (0–100), or <c>null</c> if not completed.</param>
/// <param name="Summary">The AI narrative summary, or <c>null</c> if not completed.</param>
/// <param name="IssueCount">Total number of issues found.</param>
/// <param name="CreatedAt">UTC timestamp of submission.</param>
/// <param name="CompletedAt">UTC timestamp of completion, or <c>null</c>.</param>
/// <param name="ErrorMessage">Failure reason when status is <see cref="ReviewStatus.Failed"/>.</param>
public sealed record CodeReviewDto(
    Guid Id,
    string Title,
    ProgrammingLanguage Language,
    ReviewStatus Status,
    int? OverallScore,
    string? Summary,
    int IssueCount,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage);
