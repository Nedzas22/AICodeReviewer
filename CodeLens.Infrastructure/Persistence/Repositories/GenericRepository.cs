using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CodeLens.Domain.Common;
using CodeLens.Domain.Interfaces;
using CodeLens.Infrastructure.Persistence;

namespace CodeLens.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core repository providing standard CRUD operations.
/// Domain-specific repositories inherit from this class.
/// </summary>
/// <typeparam name="T">Any domain entity that extends <see cref="BaseEntity"/>.</typeparam>
internal class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>The underlying database context.</summary>
    protected readonly AppDbContext Context;

    /// <summary>The typed <see cref="DbSet{TEntity}"/> for <typeparamref name="T"/>.</summary>
    protected readonly DbSet<T> DbSet;

    /// <summary>Initialises the repository with the injected context.</summary>
    protected GenericRepository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync([id], cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetWhereAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    /// <inheritdoc />
    public void Update(T entity) => Context.Entry(entity).State = EntityState.Modified;

    /// <inheritdoc />
    public void Remove(T entity) => DbSet.Remove(entity);
}
