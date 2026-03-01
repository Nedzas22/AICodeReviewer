using CodeLens.Domain.Interfaces;

namespace CodeLens.Domain.Interfaces;

/// <summary>
/// Coordinates multiple repositories within a single database transaction.
/// Call <see cref="SaveChangesAsync"/> once to atomically persist all staged changes.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Gets the repository for <see cref="Entities.CodeReview"/> aggregates.</summary>
    ICodeReviewRepository CodeReviews { get; }

    /// <summary>
    /// Persists all staged changes to the database atomically.
    /// </summary>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
