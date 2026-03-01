using CodeLens.Domain.Enums;

namespace CodeLens.Application.DTOs;

/// <summary>
/// DTO for a single issue found during a code review.
/// </summary>
/// <param name="Id">The issue's unique identifier.</param>
/// <param name="Severity">How critical the issue is.</param>
/// <param name="Title">Short headline for the issue.</param>
/// <param name="Description">Detailed explanation of the problem and its impact.</param>
/// <param name="LineStart">1-based start line number, or <c>null</c> if not line-specific.</param>
/// <param name="LineEnd">1-based end line number, or <c>null</c> if single-line or not applicable.</param>
/// <param name="SuggestedFix">AI-generated fix snippet, or <c>null</c> if none provided.</param>
/// <param name="Category">Category label (e.g., "Security", "Performance"), or <c>null</c>.</param>
public sealed record ReviewIssueDto(
    Guid Id,
    Severity Severity,
    string Title,
    string Description,
    int? LineStart,
    int? LineEnd,
    string? SuggestedFix,
    string? Category);
