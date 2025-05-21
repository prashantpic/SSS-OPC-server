using ManagementService.Application.Behaviors;
using ManagementService.Application.Features.ConfigurationMigrations.Services;
// using ManagementService.Application.Mappings; // AutoMapper profiles are discovered by assembly
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ManagementService.Application;

/// <summary>
/// Centralizes the registration of application layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            // Add MediatR pipeline behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            // Add other behaviors like logging, performance monitoring, etc. here if needed
        });

        // Add AutoMapper
        // AutoMapper profiles are automatically discovered from the assembly passed to AddAutoMapper.
        // Ensure ManagementProfile (or any other profile) is in the Application assembly.
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Add FluentValidation
        // Registers all validators derived from AbstractValidator<> in the specified assembly.
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        // Note: ValidationBehavior (which uses these validators) is registered in MediatR configuration above.

        // Register Application Services
        services.AddScoped<ConfigurationMigrationOrchestrator>();

        return services;
    }
}