using FluentValidation;
using MediatR;
using CodeLens.Application.DTOs;


namespace CodeLens.Application.Features.Auth.Commands;

/// <summary>Authenticates an existing user and returns a JWT on success.</summary>
/// <param name="Email">The registered email address.</param>
/// <param name="Password">The plain-text password to verify.</param>
public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

/// <summary>Validates <see cref="LoginCommand"/> before the handler executes.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Defines validation rules for login.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
