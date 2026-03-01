using FluentValidation;
using FluentValidation.Results;
using MediatR;
using CodeLens.Application.Common.Exceptions;
using ValidationException = CodeLens.Application.Common.Exceptions.ValidationException;

namespace CodeLens.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that runs all registered <see cref="IValidator{T}"/> instances
/// for a request before the handler executes. Throws <see cref="ValidationException"/>
/// if any rules fail, preventing the handler from ever running with invalid input.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initialises the behaviour with all validators registered for <typeparamref name="TRequest"/>.
    /// </summary>
    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
