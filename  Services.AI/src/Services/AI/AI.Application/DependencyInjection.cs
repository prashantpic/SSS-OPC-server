using FluentValidation;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Encapsulates the registration of all Application layer dependencies,
/// such as MediatR, FluentValidation, and AutoMapper.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers services from the Application layer into the DI container.
    /// </summary>
    /// <param name="services">The IServiceCollection instance.</param>
    /// <returns>The configured IServiceCollection instance.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register AutoMapper profiles
        services.AddAutoMapper(assembly);

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register MediatR handlers, behaviors, etc.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Optionally add pipeline behaviors for validation, logging, etc.
            // cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
}