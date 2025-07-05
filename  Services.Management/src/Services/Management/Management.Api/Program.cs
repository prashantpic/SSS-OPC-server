using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Opc.System.Services.Management.Application;
using Opc.System.Services.Management.Application.Migration.Commands.ImportConfiguration;
using Opc.System.Services.Management.Domain.Repositories;
using Opc.System.Services.Management.Infrastructure.Persistence;
using Opc.System.Services.Management.Infrastructure.Persistence.Repositories;
using Serilog;
using System;

// Placeholder class for assembly scanning
namespace Opc.System.Services.Management.Application
{
    public class AssemblyReference { }
}

// Placeholder classes for shared results and exception handling
namespace Opc.System.Services.Management.Application.Shared
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }
        protected Result(bool isSuccess, string error) { IsSuccess = isSuccess; Error = error; }
        public static Result Success() => new(true, string.Empty);
        public static Result Failure(string error) => new(false, error);
    }

    public class Result<T> : Result
    {
        public T Value { get; }
        protected Result(T value, bool isSuccess, string error) : base(isSuccess, error) { Value = value; }
        public static Result<T> Success(T value) => new(value, true, string.Empty);
        public static new Result<T> Failure(string error) => new(default, false, error);
    }
}

// Placeholder for GetClientDetails use case
namespace Opc.System.Services.Management.Application.ClientInstances.Queries.GetClientDetails
{
    using MediatR;
    using Opc.System.Services.Management.Application.ClientInstances.Commands.UpdateClientConfiguration;
    public record ClientDetailsDto; // Placeholder
    public record GetClientDetailsQuery(Guid Id) : IRequest<ClientDetailsDto>;
    public class GetClientDetailsQueryHandler : IRequestHandler<GetClientDetailsQuery, ClientDetailsDto> { public Task<ClientDetailsDto> Handle(GetClientDetailsQuery request, CancellationToken cancellationToken) => Task.FromResult<ClientDetailsDto>(null); }
}

namespace Opc.System.Services.Management.Api
{
    /// <summary>
    /// The bootstrap file for the Management service. It wires up all dependencies, configurations, and middleware before starting the web host.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configure Logging
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            // 2. Configure Dependency Injection
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // 3. Configure HTTP pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            // app.UseExceptionHandler("/error"); // Add custom exception handler middleware
            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add MediatR for CQRS
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly));

            // Add Infrastructure Layer dependencies
            services.AddDbContext<ManagementDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("ManagementDb")));
            
            services.AddScoped<IOpcClientInstanceRepository, OpcClientInstanceRepository>();
            // Add other repositories here
            // services.AddScoped<IMigrationStrategyRepository, MigrationStrategyRepository>();

            // Add Application Layer dependencies (e.g., factories, validators)
            // services.AddScoped<IConfigurationParserFactory, ConfigurationParserFactory>(); // Example
            // services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly); // FluentValidation

            // Add Presentation Layer services
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Management Service API",
                    Version = "v1"
                });
            });
        }
    }
}