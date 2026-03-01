using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CodeLens.Domain.Entities;
using CodeLens.Infrastructure.Persistence;

namespace CodeLens.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent EF Core configuration for the <see cref="CodeReview"/> aggregate root.
/// </summary>
internal sealed class CodeReviewConfiguration : IEntityTypeConfiguration<CodeReview>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CodeReview> builder)
    {
        builder.HasKey(r => r.Id);

        // We generate GUIDs client-side via BaseEntity; tell EF not to auto-generate
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450); // Identity uses NVARCHAR(450) for user IDs

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.SourceCode)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.Language).IsRequired();
        builder.Property(r => r.Status).IsRequired();

        builder.Property(r => r.AiModel).HasMaxLength(50);
        builder.Property(r => r.Summary).HasMaxLength(4000);
        builder.Property(r => r.ErrorMessage).HasMaxLength(1000);

        // FK to ApplicationUser (string-keyed Identity user) — no navigation on the user side
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many to ReviewIssue with backing field access
        builder.HasMany(r => r.Issues)
            .WithOne()
            .HasForeignKey(i => i.CodeReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Issues)
            .HasField("_issues")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CreatedAt);
    }
}
