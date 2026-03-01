using MediatR;
using CodeLens.Application.Common.Exceptions;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.DTOs;

namespace CodeLens.Application.Features.Auth.Commands;

/// <summary>
/// Handles <see cref="LoginCommand"/>: verifies credentials and issues a JWT.
/// Error messages are intentionally generic to prevent user enumeration.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;

    /// <summary>Initialises the handler with its required services.</summary>
    public LoginCommandHandler(IIdentityService identityService, IJwtService jwtService)
    {
        _identityService = identityService;
        _jwtService = jwtService;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (success, userId, email, displayName) = await _identityService
            .LoginAsync(request.Email, request.Password, cancellationToken);

        if (!success || userId is null || email is null || displayName is null)
            throw new ValidationException("Credentials", ["Invalid email or password."]);

        var (token, expiresAt) = _jwtService.GenerateToken(userId, email, displayName);

        return new AuthResponse(
            Token: token,
            UserId: userId,
            Email: email,
            DisplayName: displayName,
            ExpiresAt: expiresAt);
    }
}
