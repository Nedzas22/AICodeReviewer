using CodeLens.Domain.Interfaces;
using CodeLens.Infrastructure.Persistence;
using CodeLens.Infrastructure.Persistence.Repositories;

namespace CodeLens.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>.
/// All repositories share the same <see cref="AppDbContext"/> instance within a request scope,
/// ensuring all changes are committed in a single transaction.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private ICodeReviewRepository? _codeReviews;

    /// <summary>Initialises the unit of work with the scoped database context.</summary>
    public UnitOfWork(AppDbContext context) => _context = context;

    /// <inheritdoc />
    public ICodeReviewRepository CodeReviews =>
        _codeReviews ??= new CodeReviewRepository(_context);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public void Dispose() => _context.Dispose();
}
