using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CodeLens.Domain.Common;
using CodeLens.Domain.Entities;
using CodeLens.Infrastructure.Persistence;

namespace CodeLens.Infrastructure.Persistence;

/// <summary>
/// The Entity Framework Core database context for CodeLens.
/// Extends <see cref="IdentityDbContext{TUser}"/> to include all Identity tables
/// alongside the application's own <see cref="CodeReview"/> and <see cref="ReviewIssue"/> tables.
/// </summary>
public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>Initialises the context with the provided options.</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Gets the <see cref="CodeReview"/> entity set.</summary>
    public DbSet<CodeReview> CodeReviews => Set<CodeReview>();

    /// <summary>Gets the <see cref="ReviewIssue"/> entity set.</summary>
    public DbSet<ReviewIssue> ReviewIssues => Set<ReviewIssue>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Discover and apply all IEntityTypeConfiguration<T> implementations in this assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
