using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Services;
using IntegrationService.Adapters.IoT;
using IntegrationService.Adapters.Blockchain;
using IntegrationService.Adapters.DigitalTwin;
using IntegrationService.Resiliency;
using IntegrationService.Validation;
using IntegrationService.Messaging;
using IntegrationService.BackgroundServices;
using Microsoft.Extensions.Options; // Required for IOptions

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        // Load and Bind Configuration
        builder.Services.Configure<IntegrationSettings>(builder.Configuration.GetSection("IntegrationSettings"));
        builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("IntegrationSettings:MessageBroker"));
        builder.Services.Configure<IoTPlatformSettings>(builder.Configuration.GetSection("IntegrationSettings:IoTPlatformSettings"));
        builder.Services.Configure<BlockchainSettings>(builder.Configuration.GetSection("IntegrationSettings:BlockchainSettings"));
        builder.Services.Configure<DigitalTwinSettings>(builder.Configuration.GetSection("IntegrationSettings:DigitalTwinSettings"));
        builder.Services.Configure<ResiliencySettings>(builder.Configuration.GetSection("IntegrationSettings:ResiliencySettings"));
        builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("IntegrationSettings:SecuritySettings"));
        builder.Services.Configure<IntegrationServiceMiscSettings>(builder.Configuration.GetSection("IntegrationSettings:IntegrationServiceMiscSettings"));


        // Add services to the container.
        builder.Services.AddControllers()
            .AddFluentValidation(fv =>
            {
                fv.ImplicitlyValidateChildProperties = true;
                fv.ImplicitlyValidateRootCollectionElements = true;
                fv.RegisterValidatorsFromAssemblyContaining<IncomingIoTDataValidator>();
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Integration Service API", Version = "v1" });
        });

        // Register HttpClientFactory
        builder.Services.AddHttpClient();


        // Resiliency Factories
        builder.Services.AddSingleton<RetryPolicyFactory>();
        builder.Services.AddSingleton<CircuitBreakerPolicyFactory>();

        // Core Services
        builder.Services.AddSingleton<IDataMapper, DataMappingService>();
        builder.Services.AddSingleton<ICredentialManager, SecureCredentialService>();

        // Integration Services (Scoped as they might involve request-specific logic or dependencies)
        builder.Services.AddScoped<IoTIntegrationService>();
        builder.Services.AddScoped<BlockchainIntegrationService>();
        builder.Services.AddScoped<DigitalTwinIntegrationService>();

        // Adapters (Singleton if they manage long-lived connections or are stateless, otherwise Scoped/Transient)
        // For IoT, adaptors manage connections, so Singleton seems appropriate.
        // If multiple instances of the same type of adaptor are needed (e.g. two MQTT brokers), a factory might be better.
        // For now, assuming services will select from IEnumerable<IIoTPlatformAdaptor> or similar.

        // Register all IIoTPlatformAdaptor implementations
        builder.Services.AddSingleton<MqttAdaptor>();
        builder.Services.AddSingleton<AmqpAdaptor>();
        builder.Services.AddSingleton<HttpIoTAdaptor>();

        // This allows IoTIntegrationService to inject IEnumerable<IIoTPlatformAdaptor>
        // and then select the correct one based on IoTPlatformConfig.Type.
        // We need to ensure these concrete types are also registered as their interface for specific injection if needed,
        // or the service consuming them is prepared to filter from IEnumerable.
        // A common pattern is to have a factory or a way for the consuming service to identify the correct adaptor.
        // For simplicity, the service can iterate and check type or a configured ID.
        // Alternative: Use a factory that takes IoTPlatformConfig and returns the correct adaptor.
        builder.Services.AddSingleton<IIoTPlatformAdaptor, MqttAdaptor>(sp => sp.GetRequiredService<MqttAdaptor>());
        builder.Services.AddSingleton<IIoTPlatformAdaptor, AmqpAdaptor>(sp => sp.GetRequiredService<AmqpAdaptor>());
        builder.Services.AddSingleton<IIoTPlatformAdaptor, HttpIoTAdaptor>(sp => sp.GetRequiredService<HttpIoTAdaptor>());


        builder.Services.AddSingleton<IBlockchainAdaptor, NethereumBlockchainAdaptor>();
        
        // Assuming one HttpDigitalTwinAdaptor is sufficient or it can handle multiple configurations.
        // If distinct instances per DigitalTwinConfig are needed, a factory or more complex registration is required.
        builder.Services.AddSingleton<HttpDigitalTwinAdaptor>();
        builder.Services.AddSingleton<IDigitalTwinAdaptor, HttpDigitalTwinAdaptor>(sp => sp.GetRequiredService<HttpDigitalTwinAdaptor>());


        // Validators (already registered with AddFluentValidation)
        // builder.Services.AddScoped<IValidator<IoTDataMessage>, IncomingIoTDataValidator>(); // Example if not using assembly scanning

        // Hosted Services
        builder.Services.AddHostedService<OpcDataConsumer>();
        builder.Services.AddHostedService<DigitalTwinSyncService>();

        // Configuration for HTTP Client Policies (Polly with HttpClientFactory)
        // Example for HttpIoTAdaptor and HttpDigitalTwinAdaptor if they use IHttpClientFactory
        // and want specific policies.
        // builder.Services.AddHttpClient<HttpIoTAdaptor>()
        //    .AddPolicyHandler((sp, request) => sp.GetRequiredService<RetryPolicyFactory>().CreateAsyncRetryPolicyForHttp())
        //    .AddPolicyHandler((sp, request) => sp.GetRequiredService<CircuitBreakerPolicyFactory>().CreateAsyncCircuitBreakerPolicyForHttp());

        // Configure OpenTelemetry
        var otelServiceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "IntegrationService";
        var otlpEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:Exporter:Otlp:Endpoint");

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: otelServiceName))
            .WithTracing(tracingProviderBuilder =>
            {
                tracingProviderBuilder
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSource(otelServiceName) // For custom activities
                    .AddSource("RabbitMQ.Client"); // If RabbitMQ.Client instrumentation is added

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracingProviderBuilder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(otlpEndpoint);
                    });
                }
                else
                {
                    // Fallback to console exporter if OTLP endpoint is not configured
                    tracingProviderBuilder.AddConsoleExporter();
                }
            });
            // .WithMetrics(metricsProviderBuilder => // Add metrics later if needed
            // {
            //     metricsProviderBuilder
            //         .AddAspNetCoreInstrumentation()
            //         .AddHttpClientInstrumentation()
            //         .AddProcessInstrumentation()  // For .NET 8 Process
            //         .AddRuntimeInstrumentation(); // For .NET Runtime
            //     if (!string.IsNullOrEmpty(otlpEndpoint))
            //     {
            //         metricsProviderBuilder.AddOtlpExporter(otlpOptions =>
            //         {
            //             otlpOptions.Endpoint = new Uri(otlpEndpoint);
            //         });
            //     }
            // });


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Integration Service API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at apps root
            });
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // Add custom error handling middleware for production
            // app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseSerilogRequestLogging(); // Log all HTTP requests

        // app.UseAuthentication(); // If JWT or other auth is directly handled by this service
        app.UseAuthorization();

        app.MapControllers();

        Log.Information("Starting Integration Service host");
        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Integration Service host terminated unexpectedly");
        }
        finally
        {
            Log.Information("Stopping Integration Service host");
            await Log.CloseAndFlushAsync();
        }
    }
}