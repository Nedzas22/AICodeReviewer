using AutoMapper;
using MediatR;
using CodeLens.Application.Common.Exceptions;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.DTOs;
using CodeLens.Domain.Entities;
using CodeLens.Domain.Interfaces;

namespace CodeLens.Application.Features.Reviews.Queries;

/// <summary>Handles <see cref="GetReviewByIdQuery"/>.</summary>
public sealed class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, CodeReviewDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    /// <summary>Initialises the handler with its required dependencies.</summary>
    public GetReviewByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<CodeReviewDetailDto> Handle(
        GetReviewByIdQuery request,
        CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.CodeReviews.GetWithIssuesAsync(request.ReviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(CodeReview), request.ReviewId);

        if (review.UserId != _currentUserService.UserId)
            throw new ForbiddenException("You do not have permission to view this review.");

        return _mapper.Map<CodeReviewDetailDto>(review);
    }
}
