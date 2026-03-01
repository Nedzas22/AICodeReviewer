using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using CodeLens.Application.Common.Exceptions;

namespace CodeLens.Api.Middleware;

/// <summary>
/// ASP.NET Core middleware that catches all unhandled exceptions thrown by the request pipeline
/// and converts them into RFC-7807-compliant <c>application/problem+json</c> responses.
/// Prevents stack traces leaking to clients in production.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>Initialises the middleware with the next delegate and a logger.</summary>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the middleware, wrapping exceptions in structured JSON responses.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteValidationProblemAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not Found", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Forbidden", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.");
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private async Task WriteValidationProblemAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        // Produces a payload compatible with RFC 7807 + ASP.NET Core's ValidationProblemDetails
        var problem = new
        {
            type = RfcLink(400),
            title = "Validation Error",
            status = 400,
            detail = ex.Message,
            errors = ex.Errors,
            traceId = TraceId(context)
        };

        await context.Response.WriteAsJsonAsync(problem, SerializerOptions);
    }

    private static async Task WriteProblemAsync(
        HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = RfcLink(statusCode),
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = TraceId(context);

        await context.Response.WriteAsJsonAsync(problem, SerializerOptions);
    }

    private static string TraceId(HttpContext context) =>
        Activity.Current?.Id ?? context.TraceIdentifier;

    private static string RfcLink(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        _   => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
