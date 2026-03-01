using Microsoft.EntityFrameworkCore;
using CodeLens.Domain.Entities;
using CodeLens.Domain.Interfaces;
using CodeLens.Infrastructure.Persistence;

namespace CodeLens.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ICodeReviewRepository"/>.
/// </summary>
internal sealed class CodeReviewRepository : GenericRepository<CodeReview>, ICodeReviewRepository
{
    /// <summary>Initialises the repository with the injected context.</summary>
    public CodeReviewRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<CodeReview?> GetWithIssuesAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(r => r.Issues)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CodeReview>> GetPagedByUserIdAsync(
        string userId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<int> CountByUserIdAsync(
        string userId, CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(r => r.UserId == userId, cancellationToken);

    /// <inheritdoc />
    public async Task ReplaceIssuesAsync(
        Guid reviewId,
        IEnumerable<ReviewIssue> newIssues,
        CancellationToken cancellationToken = default)
    {
        // Delete all existing issues for this review in one round trip
        await Context.ReviewIssues
            .Where(i => i.CodeReviewId == reviewId)
            .ExecuteDeleteAsync(cancellationToken);

        // Stage new issues for insertion
        await Context.ReviewIssues.AddRangeAsync(newIssues, cancellationToken);
    }
}
