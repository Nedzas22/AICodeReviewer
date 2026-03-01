namespace CodeLens.Application.Common.Interfaces;

/// <summary>
/// Provides the identity of the currently authenticated HTTP request principal.
/// Populated from the JWT claims in the API layer and injected into command/query handlers.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Gets the Identity user ID of the authenticated user, or <c>null</c> if unauthenticated.</summary>
    string? UserId { get; }

    /// <summary>Gets the email address of the authenticated user, or <c>null</c> if unauthenticated.</summary>
    string? Email { get; }

    /// <summary>Gets a value indicating whether the current request has an authenticated user.</summary>
    bool IsAuthenticated { get; }
}
