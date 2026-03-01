using AutoMapper;
using MediatR;
using CodeLens.Application.Common.Exceptions;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.Common.Models;
using CodeLens.Application.DTOs;
using CodeLens.Domain.Interfaces;

namespace CodeLens.Application.Features.Reviews.Queries;

/// <summary>Handles <see cref="GetReviewHistoryQuery"/>.</summary>
public sealed class GetReviewHistoryQueryHandler
    : IRequestHandler<GetReviewHistoryQuery, PagedResult<CodeReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    /// <summary>Initialises the handler with its required dependencies.</summary>
    public GetReviewHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<PagedResult<CodeReviewDto>> Handle(
        GetReviewHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenException("You must be authenticated to view review history.");

        var totalCount = await _unitOfWork.CodeReviews
            .CountByUserIdAsync(userId, cancellationToken);

        var reviews = await _unitOfWork.CodeReviews
            .GetPagedByUserIdAsync(userId, request.Page, request.PageSize, cancellationToken);

        var items = _mapper.Map<IReadOnlyList<CodeReviewDto>>(reviews);

        return new PagedResult<CodeReviewDto>(items, totalCount, request.Page, request.PageSize);
    }
}
