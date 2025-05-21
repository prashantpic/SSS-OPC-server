```csharp
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
using System.IO;
using FluentValidation.AspNetCore;
using IntegrationService.Adapters.Blockchain; // For IBlockchainAdaptor concrete type
using IntegrationService.Adapters.IoT; // For IIoTPlatformAdaptor concrete types (though not directly registered)
using IntegrationService.Adapters.DigitalTwin; // For IDigitalTwinAdaptor concrete types (though not directly registered)


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
                    // logging.AddDebug(); // Optionally add debug logger
                    // TODO: Integrate with shared logging from REPO-SHARED-UTILS
                    // logging.AddSharedLoggingProvider(configuration.GetSection("SharedLoggingConfig"));
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Default configuration sources are already added by CreateDefaultBuilder:
                    // appsettings.json, appsettings.{Environment}.json, Environment Variables, Command-line arguments.
                    // Optionally add other sources like Azure Key Vault:
                    /*
                    var builtConfig = config.Build(); // Build temporary config to read vault URI
                    var keyVaultUri = builtConfig["SecurityConfigs:VaultUri"];
                    if (!string.IsNullOrEmpty(keyVaultUri) && builtConfig["SecurityConfigs:CredentialManagerType"] == "AzureKeyVault")
                    {
                        // Requires Azure.Extensions.AspNetCore.Configuration.Secrets and Azure.Identity
                        // config.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
                    }
                    */
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
            _logger.LogInformation("IntegrationService Startup configuration loaded.");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogInformation("Configuring services...");

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
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });
            services.AddHttpClient(); // For HttpIoTAdaptor and HttpDigitalTwinAdaptor

            // Add application services and interfaces
            services.AddSingleton<IDataMapper, DataMappingService>();
            services.AddSingleton<ICredentialManager, SecureCredentialService>();

            services.AddSingleton<RetryPolicyFactory>();
            services.AddSingleton<CircuitBreakerPolicyFactory>();

            services.AddSingleton<IncomingIoTDataValidator>();

            // Register specific Blockchain Adaptor implementation
            services.AddSingleton<IBlockchainAdaptor, NethereumBlockchainAdaptor>();


            // Register Integration Services - Singleton as they manage long-lived adaptors and are used by HostedServices.
            services.AddSingleton<IoTIntegrationService>();
            services.AddSingleton<BlockchainIntegrationService>();
            services.AddSingleton<DigitalTwinIntegrationService>();

            // Register Hosted Services
            services.AddHostedService<OpcDataConsumer>();
            services.AddHostedService<DigitalTwinSyncService>();

            // Add placeholder for Message Queue Client (specific to technology chosen)
            // e.g., services.AddSingleton<IMessageQueueProducer, RabbitMqProducer>();
            // e.g., services.AddSingleton<IMessageQueueConsumer, RabbitMqConsumer>(); // OpcDataConsumer would then use this

            // Add placeholder for REPO-DATA-SERVICE client interfaces if controllers/services need them
            // e.g., services.AddScoped<IConfigurationPersistenceService, DataServiceConfigurationClient>();

            // Add FluentValidation
            services.AddFluentValidationAutoValidation(); // Automatically registers validators and enables ASP.NET Core integration
            services.AddFluentValidationClientsideAdapters(); // For client-side validation (if using MVC/Razor Pages)
            // Note: IncomingIoTDataValidator itself is not registered as an IValidator<T> here,
            // but is injected directly. If using FluentValidation's ASP.NET Core integration,
            // you'd typically register validators like:
            // services.AddValidatorsFromAssemblyContaining<IncomingIoTDataValidator>();

            _logger.LogInformation("Service configuration completed.");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Configuring application pipeline for environment: {EnvironmentName}", env.EnvironmentName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Integration Service API v1"));
                logger.LogInformation("Swagger UI enabled for development.");
            }
            else
            {
                app.UseExceptionHandler("/Error"); // Generic error handler
                app.UseHsts(); // Adds HSTS headers
            }

            app.UseHttpsRedirection(); // Redirect HTTP to HTTPS

            app.UseRouting();

            // app.UseAuthentication(); // If authentication is configured
            // app.UseAuthorization();  // If authorization is configured

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // Add health check endpoint
                endpoints.MapHealthChecks("/health"); // Requires services.AddHealthChecks() in ConfigureServices
            });

            logger.LogInformation("Application pipeline configured.");

            // Optional: Trigger initial connection for critical adaptors on startup
            // This ensures they are ready. HostedServices usually manage their own startup.
            var serviceProvider = app.ApplicationServices;
            var iotService = serviceProvider.GetService<IoTIntegrationService>();
            var blockchainService = serviceProvider.GetService<BlockchainIntegrationService>(); // BlockchainIntegrationService constructor already connects
            var dtService = serviceProvider.GetService<DigitalTwinIntegrationService>();

            // Asynchronously connect adaptors without blocking startup.
            // IoTIntegrationService and DigitalTwinIntegrationService should ideally handle
            // connecting adaptors gracefully during their initialization or first use.
            // If explicit startup connection is desired:
            /*
            Task.Run(async () => {
                logger.LogInformation("Initiating startup connection for IoT adaptors...");
                await iotService?.ConnectAllAdaptorsAsync();
                logger.LogInformation("Startup connection for IoT adaptors attempt finished.");

                logger.LogInformation("Initiating startup connection for Digital Twin adaptors...");
                await dtService?.ConnectAllAdaptorsAsync();
                logger.LogInformation("Startup connection for Digital Twin adaptors attempt finished.");
            });
            */
             logger.LogInformation("Application started. Integration services are initializing their adaptors if applicable.");
        }
    }

    // These classes are defined here for simplicity as they are used directly by IOptions<T> in Startup.
    // In a larger application, they would be in their own files under the Configuration namespace.
    public class FeatureFlags
    {
        public bool EnableMqttIntegration { get; set; } = true;
        public bool EnableAmqpIntegration { get; set; } = false;
        public bool EnableHttpIoTIntegration { get; set; } = true;
        public bool EnableBlockchainLogging { get; set; } = true;
        public bool EnableDigitalTwinSync { get; set; } = true;
        public bool EnableIoTRuleBasedMapping { get; set; } = true;
        public bool EnableBiDirectionalIoT { get; set; } = true;
    }

    public class ResiliencyConfigs
    {
        public int DefaultRetryAttempts { get; set; } = 3;
        public int DefaultRetryDelaySeconds { get; set; } = 2;
        public int DefaultCircuitBreakerThresholdExceptionsAllowed { get; set; } = 5;
        public int DefaultCircuitBreakDurationSeconds { get; set; } = 30;
    }

    public class SecurityConfigs
    {
        public string CredentialManagerType { get; set; } = "Configuration"; // Default to reading from appsettings
        public string VaultUri { get; set; } = string.Empty;
        public string DefaultTenantIdForCloudServices { get; set; } = string.Empty;
    }
}
```