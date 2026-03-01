namespace CodeLens.Web.Models.Reviews;

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

public sealed record ReviewIssueDto(
    Guid Id,
    Severity Severity,
    string Title,
    string Description,
    int? LineStart,
    int? LineEnd,
    string? SuggestedFix,
    string? Category);

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
