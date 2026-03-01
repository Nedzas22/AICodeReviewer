using System.Linq.Expressions;
using CodeLens.Domain.Common;

namespace CodeLens.Domain.Interfaces;

/// <summary>
/// Generic repository contract for basic CRUD operations over a domain entity.
/// Implementations live in the Infrastructure layer.
/// </summary>
/// <typeparam name="T">Any domain entity that extends <see cref="BaseEntity"/>.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Retrieves an entity by its unique identifier.</summary>
    /// <param name="id">The entity GUID.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>The entity, or <c>null</c> if not found.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all entities matching <paramref name="predicate"/>.</summary>
    /// <param name="predicate">A filter expression — always required to prevent accidental full-table scans.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<T>> GetWhereAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Stages a new entity for insertion in the current unit of work.</summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Marks an entity as modified so EF tracks its changes.</summary>
    void Update(T entity);

    /// <summary>Stages an entity for deletion in the current unit of work.</summary>
    void Remove(T entity);
}
