using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using CodeLens.Application.Common.Behaviours;

namespace CodeLens.Application;

/// <summary>
/// Extension methods for registering Application layer services with the DI container.
/// Call <see cref="AddApplication"/> from <c>Program.cs</c> or the API's service registration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR (with pipeline behaviours), FluentValidation validators,
    /// and AutoMapper profiles from the Application assembly.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Order matters: Logging runs first (outermost), Validation runs second
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);

        return services;
    }
}
