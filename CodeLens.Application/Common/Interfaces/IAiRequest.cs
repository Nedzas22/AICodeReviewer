namespace CodeLens.Application.Common.Interfaces;

/// <summary>
/// Marker interface for MediatR requests that invoke an external AI service.
/// <see cref="CodeLens.Application.Common.Behaviours.LoggingBehaviour{TRequest,TResponse}"/>
/// uses this to apply a higher slow-request warning threshold (default 3 000 ms)
/// instead of the standard threshold (default 500 ms).
/// </summary>
public interface IAiRequest { }
