using MediatR;
using CodeLens.Application.DTOs;

namespace CodeLens.Application.Features.Reviews.Queries;

/// <summary>
/// Fetches the full detail of a single code review including all issues.
/// Ownership is enforced in the handler — a user cannot fetch another user's review.
/// </summary>
/// <param name="ReviewId">The GUID of the review to retrieve.</param>
public sealed record GetReviewByIdQuery(Guid ReviewId) : IRequest<CodeReviewDetailDto>;
