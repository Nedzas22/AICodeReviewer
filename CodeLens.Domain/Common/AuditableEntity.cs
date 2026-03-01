namespace CodeLens.Domain.Common;

/// <summary>
/// Extends <see cref="BaseEntity"/> with audit trail fields.
/// All entities that need creation/update tracking should inherit from this.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>Gets or sets the UTC timestamp when this entity was first created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp of the last update, or <c>null</c> if never updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Gets or sets the ID of the user who created this entity.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>Gets or sets the ID of the user who last updated this entity.</summary>
    public string? UpdatedBy { get; set; }
}
