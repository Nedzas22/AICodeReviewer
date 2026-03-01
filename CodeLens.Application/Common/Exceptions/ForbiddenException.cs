namespace CodeLens.Application.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated user attempts an action they are not authorised to perform.
/// Maps to HTTP 403 in the API layer.
/// </summary>
public sealed class ForbiddenException : Exception
{
    /// <summary>Initialises a <see cref="ForbiddenException"/> with a reason message.</summary>
    public ForbiddenException(string message) : base(message) { }
}
