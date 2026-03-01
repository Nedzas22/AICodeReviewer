using FluentValidation;
using FluentValidation.Results;

namespace CodeLens.Application.Common.Exceptions;

/// <summary>
/// Thrown when one or more FluentValidation rules fail.
/// Maps to HTTP 400 in the API layer.
/// Errors are grouped by property name for structured API responses.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors grouped by property name.
    /// Key = property name, Value = array of error messages for that property.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initialises a <see cref="ValidationException"/> from a collection of
    /// <see cref="ValidationFailure"/> objects produced by FluentValidation.
    /// </summary>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    /// <summary>
    /// Initialises a <see cref="ValidationException"/> from plain string error messages
    /// (e.g., from the Identity layer).
    /// </summary>
    /// <param name="propertyName">The logical property the errors relate to.</param>
    /// <param name="errors">The error messages.</param>
    public ValidationException(string propertyName, IEnumerable<string> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>
        {
            [propertyName] = errors.ToArray()
        };
    }
}
