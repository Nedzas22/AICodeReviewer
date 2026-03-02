using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using CodeLens.Application.Common.Interfaces;

namespace CodeLens.Application.Common.Behaviours;

public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int AiSlowThresholdMs = 3_000;
    private const int DefaultSlowThresholdMs = 500;

    private static readonly bool IsAiRequest =
        typeof(IAiRequest).IsAssignableFrom(typeof(TRequest));

    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        => _logger = logger;
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

