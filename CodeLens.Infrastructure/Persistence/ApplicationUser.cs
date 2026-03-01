using Microsoft.AspNetCore.Identity;

namespace CodeLens.Infrastructure.Persistence;

/// <summary>
/// Extends ASP.NET Core Identity's <see cref="IdentityUser"/> with application-specific
/// profile fields. This entity is the database owner of the <c>AspNetUsers</c> table.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>Gets or sets the name shown in the UI and stored in JWT claims.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when this account was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
