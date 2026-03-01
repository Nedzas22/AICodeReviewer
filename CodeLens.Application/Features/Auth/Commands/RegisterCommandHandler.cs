using MediatR;
using CodeLens.Application.Common.Exceptions;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.DTOs;

namespace CodeLens.Application.Features.Auth.Commands;

/// <summary>
/// Handles <see cref="RegisterCommand"/>: creates an Identity user and issues a JWT.
/// </summary>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;

    /// <summary>Initialises the handler with its required services.</summary>
    public RegisterCommandHandler(IIdentityService identityService, IJwtService jwtService)
    {
        _identityService = identityService;
        _jwtService = jwtService;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"An account with email '{request.Email}' already exists.");

        var (success, userId, errors) = await _identityService
            .RegisterAsync(request.Email, request.Password, request.DisplayName, cancellationToken);

        if (!success || userId is null)
            throw new ValidationException("Registration", errors);

        var (token, expiresAt) = _jwtService.GenerateToken(userId, request.Email, request.DisplayName);

        return new AuthResponse(
            Token: token,
            UserId: userId,
            Email: request.Email,
            DisplayName: request.DisplayName,
            ExpiresAt: expiresAt);
    }
}
