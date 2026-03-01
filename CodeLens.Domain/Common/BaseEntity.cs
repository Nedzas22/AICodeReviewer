namespace CodeLens.Domain.Common;

/// <summary>
/// Root base class for all domain entities. Provides a strongly-typed
/// <see cref="Guid"/> identifier and equality semantics based on that identity.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Gets the unique identifier for this entity.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>Equality operator based on entity identity.</summary>
    public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
        left?.Equals(right) ?? right is null;

    /// <summary>Inequality operator based on entity identity.</summary>
    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
}
