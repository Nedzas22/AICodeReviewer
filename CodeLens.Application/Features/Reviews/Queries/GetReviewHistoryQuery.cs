using FluentValidation;
using MediatR;
using CodeLens.Application.Common.Models;
using CodeLens.Application.DTOs;

namespace CodeLens.Application.Features.Reviews.Queries;

/// <summary>
/// Returns a paged list of the authenticated user's review history, newest first.
/// </summary>
/// <param name="Page">1-based page number (defaults to 1).</param>
/// <param name="PageSize">Number of items per page (defaults to 10, max 50).</param>
public sealed record GetReviewHistoryQuery(int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<CodeReviewDto>>;

/// <summary>Validates <see cref="GetReviewHistoryQuery"/> pagination parameters.</summary>
public sealed class GetReviewHistoryQueryValidator : AbstractValidator<GetReviewHistoryQuery>
{
    /// <summary>Defines pagination guard rules.</summary>
    public GetReviewHistoryQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("PageSize must be between 1 and 50.");
    }
}
