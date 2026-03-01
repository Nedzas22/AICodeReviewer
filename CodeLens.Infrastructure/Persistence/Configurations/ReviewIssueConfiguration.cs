using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CodeLens.Domain.Entities;

namespace CodeLens.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent EF Core configuration for <see cref="ReviewIssue"/>.
/// </summary>
internal sealed class ReviewIssueConfiguration : IEntityTypeConfiguration<ReviewIssue>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ReviewIssue> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.CodeReviewId).IsRequired();
        builder.Property(i => i.Severity).IsRequired();

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(i => i.SuggestedFix).HasColumnType("nvarchar(max)");
        builder.Property(i => i.Category).HasMaxLength(50);

        builder.HasIndex(i => i.CodeReviewId);
        builder.HasIndex(i => i.Severity);
    }
}
