using CodeLens.Domain.Entities;

namespace CodeLens.Domain.Interfaces;

/// <summary>
/// Extends <see cref="IRepository{T}"/> with <see cref="CodeReview"/>-specific query operations.
/// </summary>
public interface ICodeReviewRepository : IRepository<CodeReview>
{
    /// <summary>
    /// Returns a review with its <see cref="CodeReview.Issues"/> collection eagerly loaded.
    /// </summary>
    /// <param name="id">Review GUID.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task<CodeReview?> GetWithIssuesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paged slice of a user's review history with issues loaded.
    /// </summary>
    /// <param name="userId">The Identity user ID.</param>
    /// <param name="page">1-based page index.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task<IReadOnlyList<CodeReview>> GetPagedByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the total number of reviews belonging to a user.</summary>
    Task<int> CountByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically deletes all existing <see cref="ReviewIssue"/> rows for the given review
    /// and stages <paramref name="newIssues"/> for insertion.
    /// Call <c>SaveChangesAsync</c> on the unit of work afterwards to commit.
    /// </summary>
    /// <param name="reviewId">The parent review's ID.</param>
    /// <param name="newIssues">The replacement set of issues to insert.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task ReplaceIssuesAsync(
        Guid reviewId,
        IEnumerable<ReviewIssue> newIssues,
        CancellationToken cancellationToken = default);
}
