using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CodeLens.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef migrations add</c> when run from the CLI
/// (where the full DI container is not available).
/// Uses a local SQL Server instance; the real connection string comes from <c>appsettings.json</c>
/// at runtime.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=CodeLens_Dev;Trusted_Connection=True;",
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
