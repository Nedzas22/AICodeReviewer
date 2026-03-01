using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Domain.Interfaces;
using CodeLens.Infrastructure.Persistence;
using CodeLens.Infrastructure.Persistence.Repositories;
using CodeLens.Infrastructure.Services;
using CodeLens.Infrastructure.Settings;

namespace CodeLens.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services with the DI container.
/// Call <see cref="AddInfrastructure"/> from <c>Program.cs</c> or the API's service registration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers EF Core (<see cref="AppDbContext"/>), ASP.NET Core Identity,
    /// repository implementations, and all application services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration (from appsettings.json / env vars).</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ── Identity ─────────────────────────────────────────────────────────
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // ── Settings ─────────────────────────────────────────────────────────
        services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateOnStart();

        services.Configure<OpenAiSettings>(
            configuration.GetSection(OpenAiSettings.SectionName));

        // ── Repositories & Unit of Work ───────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Application Services ─────────────────────────────────────────────
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAiReviewService, OpenAiReviewService>();

        return services;
    }
}
