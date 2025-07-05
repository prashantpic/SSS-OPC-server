using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using ui.webapp.Services;
using ui.webapp.Authentication;
using ui.webapp.ApiClient;
using ui.webapp.Models; // Assuming DTOs are in this namespace

namespace ui.webapp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // --- UI & Utility Services Registration ---
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.PreventDuplicates = false;
        });
        
        builder.Services.AddLocalization();
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();

        // --- Authentication Services Registration ---
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<ITokenManagerService, TokenManagerService>();
        builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
        builder.Services.AddTransient<AuthorizationMessageHandler>();

        // --- API Client Services Registration ---
        var apiGatewayBaseUrl = builder.Configuration["ApiGatewayBaseUrl"] 
                                ?? throw new InvalidOperationException("ApiGatewayBaseUrl not configured.");

        builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiGatewayBaseUrl);
        });

        builder.Services.AddHttpClient<IManagementApiClient, ManagementApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiGatewayBaseUrl);
        }).AddHttpMessageHandler<AuthorizationMessageHandler>();

        builder.Services.AddHttpClient<IDataApiClient, DataApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiGatewayBaseUrl);
        }).AddHttpMessageHandler<AuthorizationMessageHandler>();

        builder.Services.AddHttpClient<IAlarmApiClient, AlarmApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiGatewayBaseUrl);
        }).AddHttpMessageHandler<AuthorizationMessageHandler>();


        await builder.Build().RunAsync();
    }
}