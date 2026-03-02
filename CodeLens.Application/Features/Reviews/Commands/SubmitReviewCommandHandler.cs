using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CodeLens.Application.Common.Exceptions;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.DTOs;
using CodeLens.Domain.Entities;
using CodeLens.Domain.Interfaces;

namespace CodeLens.Application.Features.Reviews.Commands;

public sealed class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, CodeReviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiReviewService _aiReviewService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<SubmitReviewCommandHandler> _logger;

    public SubmitReviewCommandHandler(
        IUnitOfWork unitOfWork,
        IAiReviewService aiReviewService,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<SubmitReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _aiReviewService = aiReviewService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CodeReviewDto> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenException("You must be authenticated to submit a review.");

        var review = CodeReview.Create(userId, request.Title, request.SourceCode, request.Language);

        try
        {
            var aiResult = await _aiReviewService.AnalyseAsync(
                request.SourceCode, request.Language, cancellationToken);

            var issues = aiResult.Issues
                .Select(i => ReviewIssue.Create(
                    review.Id, i.Severity, i.Title, i.Description,
                    i.LineStart, i.LineEnd, i.SuggestedFix, i.Category))
                .ToList();

            review.Complete(aiResult.Summary, aiResult.OverallScore, aiResult.Model, issues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI review failed for {ReviewId}", review.Id);
            review.Fail($"AI analysis failed: {ex.Message}");
        }

        await _unitOfWork.CodeReviews.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CodeReviewDto>(review);
    }
}
