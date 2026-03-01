namespace CodeLens.Web.Models.Common;

/// <summary>Wraps a service call result, separating success data from error information.</summary>
public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Data { get; private init; }
    public string? ErrorMessage { get; private init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; private init; }

    public static ServiceResult<T> Success(T data) =>
        new() { IsSuccess = true, Data = data };

    public static ServiceResult<T> Failure(
        string error,
        IReadOnlyDictionary<string, string[]>? validationErrors = null) =>
        new() { IsSuccess = false, ErrorMessage = error, ValidationErrors = validationErrors };
}
