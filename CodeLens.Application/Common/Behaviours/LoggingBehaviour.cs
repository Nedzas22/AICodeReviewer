using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using CodeLens.Application.Common.Interfaces;

namespace CodeLens.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that logs the name of every incoming request and the
/// elapsed time after the handler completes. Runs before <see cref="ValidationBehaviour{TRequest,TResponse}"/>.
/// Emits a <see cref="LogLevel.Warning"/> when a request exceeds its slow-request threshold:
/// <list type="bullet">
///   <item><description>3 000 ms for requests that implement <see cref="IAiRequest"/> (external AI calls).</description></item>
///   <item><description>500 ms for all other requests.</description></item>
/// </list>
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Slow-request threshold for AI-backed commands (ms).</summary>
    private const int AiSlowThresholdMs = 3_000;

    /// <summary>Slow-request threshold for all other requests (ms).</summary>
    private const int DefaultSlowThresholdMs = 500;

    /// <summary>Resolved once per closed generic type; avoids per-call reflection.</summary>
    private static readonly bool IsAiRequest =
        typeof(IAiRequest).IsAssignableFrom(typeof(TRequest));

    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    /// <summary>Initialises the behaviour with the application logger.</summary>
    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        => _logger = logger;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("CodeLens → Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        var elapsedMs = sw.ElapsedMilliseconds;
        var threshold = IsAiRequest ? AiSlowThresholdMs : DefaultSlowThresholdMs;

        _logger.LogInformation(
            "CodeLens ← Handled  {RequestName} in {ElapsedMs}ms",
            requestName, elapsedMs);

        if (elapsedMs > threshold)
            _logger.LogWarning(
                "CodeLens ⚠ SLOW    {RequestName} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                requestName, elapsedMs, threshold);

        return response;
    }
}

