namespace CodeLens.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found in the data store.
/// Maps to HTTP 404 in the API layer.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Initialises a <see cref="NotFoundException"/> for a named entity and key.
    /// </summary>
    /// <param name="entityName">The name of the entity type (e.g., nameof(CodeReview)).</param>
    /// <param name="key">The key that was searched for.</param>
    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.") { }
}
