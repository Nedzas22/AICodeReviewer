using Microsoft.AspNetCore.Identity;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.DTOs;
using CodeLens.Infrastructure.Persistence;

namespace CodeLens.Infrastructure.Services;

internal sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Success, string? UserId, IReadOnlyList<string> Errors)> RegisterAsync(
        string email, string password, string displayName,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName
        };

        var result = await _userManager.CreateAsync(user, password);

        return result.Succeeded
            ? (true, user.Id, [])
            : (false, null, result.Errors.Select(e => e.Description).ToList());
    }

    public async Task<(bool Success, string? UserId, string? Email, string? DisplayName)> LoginAsync(
        string email, string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return (false, null, null, null);

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, password, lockoutOnFailure: true);

        return result.Succeeded
            ? (true, user.Id, user.Email, user.DisplayName)
            : (false, null, null, null);
    }

    public async Task<UserDto?> GetUserByIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : new UserDto(user.Id, user.Email!, user.DisplayName);
    }

    public async Task<UserDto?> GetUserByEmailAsync(
        string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : new UserDto(user.Id, user.Email!, user.DisplayName);
    }
}
