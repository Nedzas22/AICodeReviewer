using FluentValidation;
using MediatR;
using CodeLens.Application.DTOs;
using CodeLens.Domain.Enums;

namespace CodeLens.Application.Features.Reviews.Commands;

/// <summary>
/// Submits source code for AI-powered review.
/// The authenticated user's ID is resolved inside the handler via <c>ICurrentUserService</c>;
/// it is never trusted from the client payload.
/// </summary>
/// <param name="Title">A short descriptive name for this review session (max 100 chars).</param>
/// <param name="SourceCode">The code to review (max 50,000 characters).</param>
/// <param name="Language">The programming language of the submitted code.</param>
public sealed record SubmitReviewCommand(
    string Title,
    string SourceCode,
    ProgrammingLanguage Language) : IRequest<CodeReviewDto>, IAiRequest;

/// <summary>Validates <see cref="SubmitReviewCommand"/> before the handler executes.</summary>
public sealed class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    /// <summary>Defines all validation rules for review submission.</summary>
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.SourceCode)
            .NotEmpty().WithMessage("Source code is required.")
            .MinimumLength(10).WithMessage("Source code is too short to review meaningfully.")
            .MaximumLength(50_000).WithMessage("Source code must not exceed 50,000 characters.");

        RuleFor(x => x.Language)
            .IsInEnum().WithMessage("A valid programming language must be selected.");
    }
}
