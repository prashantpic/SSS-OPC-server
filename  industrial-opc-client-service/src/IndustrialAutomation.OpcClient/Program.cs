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
        // Serilog initial "bootstrap" logger for startup issues
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting IndustrialAutomation.OpcClient service host.");
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "IndustrialAutomation.OpcClient service host terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, loggerConfiguration) =>
            {
                SerilogSetup.Configure(context, services, loggerConfiguration);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddOpcClientServices(hostContext.Configuration);
                services.AddHostedService<OpcClientHostedService>();
            });
}