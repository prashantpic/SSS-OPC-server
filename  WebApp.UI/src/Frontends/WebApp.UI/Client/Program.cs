using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudBlazor;
using Opc.System.UI.Client.Shared.Services.Notification;
using Microsoft.AspNetCore.Components.Authorization;

// These are placeholder using statements for services that will be fully implemented in other files/iterations.
// They are necessary for this file to compile as it registers them for DI.
using Opc.System.UI.Client.Shared.Services.Api;
using Opc.System.UI.Client.Shared.Services.Authentication;
using Opc.System.UI.Client.Shared.Services.State;

namespace Opc.System.UI.Client
{
    /// <summary>
    /// The client-side application entry point. Configures services for dependency injection, 
    /// such as HTTP clients for API communication, state management services, localization, 
    /// and UI notification services.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Bootstraps the Blazor WebAssembly application, setting up all necessary client-side services
        /// and attaching the root component to the DOM.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            // 1. HTTP Client for API Gateway
            // As per SDS 5.1, this registers a typed HttpClient for communicating with the backend.
            // The AuthorizationMessageHandler will automatically attach the JWT Bearer token to outgoing requests.
            builder.Services.AddHttpClient("GatewayApi", client =>
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<AuthorizationMessageHandler>();

            // 2. Register API Client Services
            // Provides a typed client for interacting with the API Gateway, abstracting HTTP calls.
            builder.Services.AddScoped<IGatewayApiClient, GatewayApiClient>();

            // 3. UI Services from MudBlazor
            // As per SDS 3, MudBlazor is our component library. This registers its required services.
            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            // 4. Localization Services
            // As per SDS 5.4, this configures the standard .NET localization framework.
            // It's set up to find resource files in the specified path.
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            // Further configuration to load JSON files from wwwroot/i18n would be added here
            // using a custom provider or configuration extension if needed.

            // 5. Application State & Notification Services
            // These services manage cross-cutting concerns as described in the SDS.
            builder.Services.AddScoped<AuthorizationMessageHandler>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<AppState>(); // Simple singleton for shared, non-auth state.
            builder.Services.AddScoped<INotificationService, NotificationService>(); // Facade for MudSnackbar.

            // 6. Authentication
            // Standard Blazor services for handling authentication and authorization.
            // AppAuthenticationStateProvider is our custom implementation to manage JWT-based auth state.
            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, AppAuthenticationStateProvider>();

            await builder.Build().RunAsync();
        }
    }
}