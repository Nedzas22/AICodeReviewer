using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CodeLens.Api.Controllers;
using CodeLens.Api.Extensions;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Infrastructure.Settings;

namespace CodeLens.Api.Extensions;

/// <summary>
/// Extension methods for registering API-layer services — controllers, authentication,
/// authorisation, CORS, and Swagger — with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all API-layer services: controllers (with JSON enum support), JWT Bearer auth,
    /// CORS policy, <see cref="ICurrentUserService"/>, and Swagger/OpenAPI.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Controllers ───────────────────────────────────────────────────────
        services.AddControllers()
            .AddJsonOptions(opts =>
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // ── Current user (reads JWT claims from IHttpContextAccessor) ─────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── JWT Authentication ────────────────────────────────────────────────
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"'{JwtSettings.SectionName}' configuration section is missing.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Keep raw claim names (sub, email) instead of mapping to MS claim URIs
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer   = true,
                    ValidIssuer      = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience    = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew        = TimeSpan.Zero,
                    NameClaimType    = "sub"
                };
            });

        services.AddAuthorization();

        // ── CORS ──────────────────────────────────────────────────────────────
        var allowedOrigins = configuration
            .GetValue<string>("Cors:AllowedOrigins")
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        services.AddCors(opts =>
            opts.AddPolicy("AllowBlazorOrigin", policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()));

        // ── Swagger / OpenAPI ─────────────────────────────────────────────────
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opts =>
        {
            opts.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "CodeLens API",
                Version     = "v1",
                Description = "AI-powered code review — submit code, get structured feedback.",
                Contact     = new OpenApiContact { Name = "CodeLens" }
            });

            // Include XML doc comments generated from /// summaries
            var xmlPath = Path.Combine(
                AppContext.BaseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

            if (File.Exists(xmlPath))
                opts.IncludeXmlComments(xmlPath);

            // JWT Bearer token UI in Swagger
            opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = SecuritySchemeType.Http,
                Scheme      = "bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "Paste your JWT token here (without the 'Bearer' prefix)."
            });

            opts.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // ── Rate limiting ─────────────────────────────────────────────────────
        // Fixed window: 10 requests per minute per IP, applied only to auth endpoints
        // via [EnableRateLimiting(AuthController.RateLimitPolicy)].
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthController.RateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window      = TimeSpan.FromMinutes(1),
                        QueueLimit  = 0
                    }));
        });

        return services;
    }
}
