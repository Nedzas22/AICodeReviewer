namespace CodeLens.Application.Common.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with the current state of the system
/// (e.g., duplicate registration). Maps to HTTP 409 in the API layer.
/// </summary>
public sealed class ConflictException : Exception
{
    /// <summary>Initialises a <see cref="ConflictException"/> with a reason message.</summary>
    public ConflictException(string message) : base(message) { }
}
