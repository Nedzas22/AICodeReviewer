using MediatR;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.DTOs;

namespace CodeLens.Application.Features.Reviews.Commands;

/// <summary>
/// Re-runs the AI analysis on an existing completed or failed review.
/// The review ID is supplied by the client; ownership is verified in the handler.
/// </summary>
/// <param name="ReviewId">The GUID of the review to re-run.</param>
public sealed record ReRunReviewCommand(Guid ReviewId) : IRequest<CodeReviewDetailDto>, IAiRequest;
