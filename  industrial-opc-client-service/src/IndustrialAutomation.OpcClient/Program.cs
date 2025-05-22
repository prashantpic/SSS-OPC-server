using IndustrialAutomation.OpcClient;
using IndustrialAutomation.OpcClient.CrossCuttingConcerns.Extensions;
using IndustrialAutomation.OpcClient.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                // Assuming SerilogSetup.Configure will be implemented later
                // based on the file structure design.
                // For now, providing a basic console logger setup.
                // SerilogSetup.Configure(hostingContext, services, loggerConfiguration);
                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Assuming ServiceCollectionExtensions.AddOpcClientServices will be implemented later
                // based on the file structure design.
                services.AddOpcClientServices(hostContext.Configuration);
                services.AddHostedService<OpcClientHostedService>();
            })
            .Build();

        await host.RunAsync();
    }
}