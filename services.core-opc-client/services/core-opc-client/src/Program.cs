using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using services.core_opc_client.Application.HostedServices;
using services.core_opc_client.Application.Orchestration;
using services.opc.client.Domain.Abstractions;
using services.opc.client.Infrastructure.EdgeProcessing;
using services.opc.client.Infrastructure.OpcProtocolClients.Ac;
using services.opc.client.Infrastructure.OpcProtocolClients.Hda;
using services.opc.client.Infrastructure.OpcProtocolClients.Ua;

namespace services.opc.client;

/// <summary>
/// The main entry point for the Core OPC Client Service.
/// </summary>
public static class Program
{
    /// <summary>
    /// Configures and runs the application host.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    /// <summary>
    /// Creates and configures the application host builder.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>An initialized IHostBuilder.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext())
            .ConfigureServices((hostContext, services) =>
            {
                // NOTE: The concrete implementations for IMessageBusPublisher (RabbitMqPublisher)
                // and IDataBuffer (FileBasedDataBuffer) are expected to be provided in a future step.
                // The registration is commented out to allow the current code to compile.
                // Once implemented, these lines should be uncommented.
                // services.AddSingleton<IMessageBusPublisher, RabbitMqPublisher>();
                // services.AddSingleton<IDataBuffer, FileBasedDataBuffer>();
                // services.AddHostedService(provider => provider.GetRequiredService<IDataBuffer>() as FileBasedDataBuffer);

                // Registering a stub for IMessageBusPublisher to allow the application to run without the full implementation.
                services.AddSingleton<IMessageBusPublisher, StubMessageBusPublisher>();

                services.AddSingleton<IEdgeAiRuntime, OnnxAiRuntime>();

                // Register protocol clients. The factory in ConnectionManager will select the correct one.
                services.AddTransient<OpcUaProtocolClient>();
                services.AddTransient<OpcHdaProtocolClient>();
                services.AddTransient<OpcAcProtocolClient>();

                // Register Application Layer services
                services.AddSingleton<ConnectionManager>();
                
                // The DataFlowOrchestrator will be created in a later step.
                // For now, ConnectionManager will log data directly.
                // services.AddSingleton<DataFlowOrchestrator>();

                // Register the main background service
                services.AddHostedService<OpcClientHost>();
            });
}

/// <summary>
/// A temporary stub implementation of IMessageBusPublisher to allow for dependency injection setup.
/// This should be replaced by the full RabbitMqPublisher implementation.
/// </summary>
file class StubMessageBusPublisher : IMessageBusPublisher
{
    private readonly ILogger<StubMessageBusPublisher> _logger;

    public StubMessageBusPublisher(ILogger<StubMessageBusPublisher> logger)
    {
        _logger = logger;
        _logger.LogWarning("Using STUB implementation for {Service}. No messages will be published.", nameof(IMessageBusPublisher));
    }

    public Task PublishDataPointAsync(Domain.Models.DataPoint dataPoint)
    {
        _logger.LogInformation("Stubbed Data Point Publish: {DataPoint}", dataPoint);
        return Task.CompletedTask;
    }
    
    public Task PublishDataPointsAsync(IEnumerable<Domain.Models.DataPoint> dataPoints)
    {
        foreach (var dataPoint in dataPoints)
        {
            _logger.LogInformation("Stubbed Data Point Publish: {DataPoint}", dataPoint);
        }
        return Task.CompletedTask;
    }

    public Task PublishAlarmAsync(Domain.Models.AlarmEvent alarmEvent)
    {
        _logger.LogInformation("Stubbed Alarm Publish: {AlarmEvent}", alarmEvent);
        return Task.CompletedTask;
    }
}