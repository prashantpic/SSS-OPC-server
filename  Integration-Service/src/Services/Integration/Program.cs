using IntegrationService.Configuration;
using IntegrationService.BackgroundServices;
using IntegrationService.Interfaces;
using IntegrationService.Messaging;
using IntegrationService.Resiliency;
using IntegrationService.Services;
using IntegrationService.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Polly;
using IntegrationService.Adapters.Blockchain;
using IntegrationService.Adapters.IoT;
using IntegrationService.Adapters.DigitalTwin;

namespace IntegrationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    // Integrate with shared logging from REPO-SHARED-UTILS
                    // Example: logging.AddRepoSharedUtilitiesLogging(Configuration); // Assumes an extension method
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // appsettings.json and environment-specific appsettings are loaded by default
                    // Add environment variables explicitly to ensure they override others
                    config.AddEnvironmentVariables();

                    // Placeholder for advanced configuration sources, e.g., Azure Key Vault
                    // This would typically read from SecurityConfigs to determine if enabled.
                    // var builtConfig = config.Build(); // Build temporary config to read vault settings
                    // var securityConfigs = builtConfig.GetSection("SecurityConfigs").Get<SecurityConfigs>();
                    // if (securityConfigs?.CredentialManagerType == "AzureKeyVault" && !string.IsNullOrEmpty(securityConfigs.VaultUri))
                    // {
                    //    config.AddAzureKeyVault(new Uri(securityConfigs.VaultUri), new DefaultAzureCredential());
                    // }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure strongly typed settings
            services.Configure<IntegrationSettings>(Configuration.GetSection(IntegrationSettings.SectionName));
            services.Configure<FeatureFlags>(Configuration.GetSection("FeatureFlags"));
            services.Configure<ResiliencyConfigs>(Configuration.GetSection("ResiliencyConfigs"));
            services.Configure<SecurityConfigs>(Configuration.GetSection("SecurityConfigs"));

            // Add framework services
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Integration Service API", Version = "v1" });
                // Optional: Include XML comments for API documentation
                // var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
                // if (System.IO.File.Exists(xmlPath))
                // {
                //     c.IncludeXmlComments(xmlPath);
                // }
            });
            services.AddHttpClient(); // For general HttpClientFactory usage

            // Add application services and interfaces
            services.AddSingleton<IDataMapper, DataMappingService>();
            services.AddSingleton<ICredentialManager, SecureCredentialService>();

            services.AddSingleton<RetryPolicyFactory>();
            services.AddSingleton<CircuitBreakerPolicyFactory>();

            services.AddSingleton<IncomingIoTDataValidator>();

            // Register Adaptors. These are typically managed by their respective Integration Services.
            // If adaptors need to be resolved independently or have complex lifecycles not tied to an Integration Service,
            // they could be registered here (e.g., as Scoped or Transient if they use HttpClient directly without factory,
            // or Singleton if they manage long-lived connections and are thread-safe).
            // For this design, IntegrationServices will instantiate them.
            // Exception: IBlockchainAdaptor is used by BlockchainIntegrationService.
            services.AddSingleton<IBlockchainAdaptor, NethereumBlockchainAdaptor>();
            // IIoTPlatformAdaptor and IDigitalTwinAdaptor are not registered directly as they are lists/factories within services.

            // Register Integration Services - Singleton as they are used by HostedServices.
            services.AddSingleton<IoTIntegrationService>();
            services.AddSingleton<BlockchainIntegrationService>();
            services.AddSingleton<DigitalTwinIntegrationService>();

            // Register Hosted Services (Message Consumers, Background Sync Tasks)
            services.AddHostedService<OpcDataConsumer>();
            services.AddHostedService<DigitalTwinSyncService>();

            // Placeholder for Message Queue Client (specific to technology chosen)
            // E.g., services.AddSingleton<IMessageQueueClient, MyRabbitMQClient>();

            // Placeholder for REPO-DATA-SERVICE client interfaces if controllers/services need them for configuration persistence
            // E.g., services.AddScoped<IPlatformConfigRepository, PlatformConfigRepositoryClient>();

            // Optional: Add FluentValidation validators if used more broadly
            // services.AddFluentValidationAutoValidation();
            // services.AddFluentValidationClientsideAdapters();
            // services.AddValidatorsFromAssemblyContaining<IncomingIoTDataValidator>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Integration Service API v1"));
                logger.LogInformation("Swagger UI enabled at /swagger");
            }
            else
            {
                app.UseExceptionHandler("/Error"); // Create an ErrorController or Razor page for this
                app.UseHsts();
            }

            app.UseHttpsRedirection(); // Enforce HTTPS

            app.UseRouting();

            // Add authentication and authorization middleware if needed
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // Optional: Map health checks endpoints
                // endpoints.MapHealthChecks("/health");
            });

            logger.LogInformation("Integration Service application started successfully in {EnvironmentName} mode.", env.EnvironmentName);

            // Optionally, trigger initial connections or setup for services on startup
            // This ensures services are ready to process data immediately.
            // Be mindful of startup time impact.
            // var serviceProvider = app.ApplicationServices;
            // _ = serviceProvider.GetRequiredService<IoTIntegrationService>().ConnectAllAdaptorsAsync();
            // _ = serviceProvider.GetRequiredService<DigitalTwinIntegrationService>().ConnectAllAdaptorsAsync();
            // The Blockchain adaptor connection is already initiated in its constructor.
        }
    }

    // These configuration classes are typically defined in their own files within a Configuration namespace.
    // For Program.cs to reference them as IOptions<T>, they need to be defined.
    // Assuming they are properly defined in the Configuration/ folder.
    // If FeatureFlags is not a separate class but part of IntegrationSettings, adjust DI registration.

    // public class FeatureFlags { ... } // Defined in Configuration/IntegrationSettings.cs or similar
    // public class ResiliencyConfigs { ... } // Defined in Configuration/IntegrationSettings.cs or similar
    // public class SecurityConfigs { ... } // Defined in Configuration/IntegrationSettings.cs or similar
}