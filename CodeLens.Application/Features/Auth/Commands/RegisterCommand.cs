using FluentValidation;
using MediatR;
using CodeLens.Application.DTOs;

namespace CodeLens.Application.Features.Auth.Commands;

/// <summary>Creates a new user account and returns a JWT on success.</summary>
/// <param name="Email">The user's email address — used as the login username.</param>
/// <param name="Password">The plain-text password (min 8 chars, must contain digit and uppercase).</param>
/// <param name="ConfirmPassword">Must match <paramref name="Password"/>.</param>
/// <param name="DisplayName">The name shown in the UI (2–50 characters).</param>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string DisplayName) : IRequest<AuthResponse>;

/// <summary>Validates <see cref="RegisterCommand"/> before the handler executes.</summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>Defines all validation rules for user registration.</summary>
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Display name must not exceed 50 characters.");
    }
}
