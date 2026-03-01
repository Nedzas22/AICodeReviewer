using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CodeLens.Application.Common.Exceptions;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.DTOs;
using CodeLens.Domain.Entities;
using CodeLens.Domain.Interfaces;

namespace CodeLens.Application.Features.Reviews.Commands;

/// <summary>
/// Handles <see cref="ReRunReviewCommand"/>.
/// Resets the review aggregate, re-submits to the AI engine, and persists the new results.
/// </summary>
public sealed class ReRunReviewCommandHandler : IRequestHandler<ReRunReviewCommand, CodeReviewDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiReviewService _aiReviewService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<ReRunReviewCommandHandler> _logger;

    /// <summary>Initialises the handler with all required dependencies.</summary>
    public ReRunReviewCommandHandler(
        IUnitOfWork unitOfWork,
        IAiReviewService aiReviewService,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<ReRunReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _aiReviewService = aiReviewService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CodeReviewDetailDto> Handle(ReRunReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenException("You must be authenticated to re-run a review.");

        var review = await _unitOfWork.CodeReviews.GetByIdAsync(request.ReviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(CodeReview), request.ReviewId);

        if (review.UserId != userId)
            throw new ForbiddenException("You do not have permission to re-run this review.");

        review.ResetForRerun();

        IReadOnlyList<ReviewIssue> newIssues = [];

        try
        {
            var aiResult = await _aiReviewService.AnalyseAsync(
                review.SourceCode, review.Language, cancellationToken);

            newIssues = aiResult.Issues
                .Select(i => ReviewIssue.Create(
                    review.Id, i.Severity, i.Title, i.Description,
                    i.LineStart, i.LineEnd, i.SuggestedFix, i.Category))
                .ToList();

            review.Complete(aiResult.Summary, aiResult.OverallScore, aiResult.Model, newIssues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI re-run failed for {ReviewId}", review.Id);
            review.Fail($"AI analysis failed: {ex.Message}");
        }

        // Explicitly replace issues to avoid EF change-tracking ambiguity on re-owned entities
        await _unitOfWork.CodeReviews.ReplaceIssuesAsync(review.Id, newIssues, cancellationToken);
        _unitOfWork.CodeReviews.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CodeReviewDetailDto>(review);
    }
}
