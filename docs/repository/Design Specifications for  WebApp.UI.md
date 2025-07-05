# Software Design Specification (SDS) for WebApp.UI

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the `Opc.System.UI` repository (`REPO-SAP-003`). This repository contains the Blazor WebAssembly (WASM) Single Page Application (SPA) which serves as the primary user interface for the entire OPC System. It is responsible for all user-facing interactions, including centralized management, data visualization, alarm monitoring, configuration, and reporting.

### 1.2. Scope
The scope of this document is limited to the `WebApp.UI` project. This includes:
- The Blazor WASM client application (`Opc.System.UI.Client`).
- The ASP.NET Core server application for hosting the client (`Opc.System.UI.Server`).
- All related components, services, and assets required to fulfill the UI/UX requirements.

This application communicates exclusively with the backend services via a dedicated API Gateway and does not have direct access to databases or OPC servers.

## 2. General Design Principles

*   **Single Page Application (SPA):** The UI will be built as a SPA using Blazor WebAssembly, providing a rich, responsive, and app-like user experience within the browser.
*   **Component-Based Architecture:** The UI will be composed of reusable, modular, and maintainable Razor components, organized by feature.
*   **API-Driven Communication:** All data and business logic will be fetched from and submitted to the backend through a well-defined set of RESTful APIs exposed by the API Gateway. The UI will not contain any business logic that is the responsibility of the backend.
*   **Centralized State Management:** A lightweight, centralized state management pattern will be used to manage shared application state (e.g., authentication status, user preferences, selected client instance) to ensure UI consistency and reduce component coupling.
*   **Responsive & Accessible Design:** The UI will be designed to be responsive across various screen sizes (desktop, tablet) and must adhere to WCAG 2.1 Level AA accessibility standards (`REQ-UIX-001`).

## 3. Technology Stack & Libraries

*   **Framework:** .NET 8, ASP.NET Core 8 (for hosting), Blazor WebAssembly 8.
*   **Language:** C#, HTML5, CSS3.
*   **UI Component Library:** **MudBlazor**. This library will be used for all standard UI components, including grids, forms, dialogs, navigation, and layout, to ensure a consistent look and feel and leverage its built-in accessibility features.
*   **Charting/Visualization:** **Plotly.Blazor**. This library will be used for rendering complex, interactive historical and real-time data charts and dashboards, as required by `REQ-UIX-010`.
*   **Localization:** **Microsoft.Extensions.Localization**. The standard .NET localization framework will be used to support multiple languages (`REQ-UIX-006`).
*   **Build & Deployment:** The application will be built using the .NET SDK and configured for Ahead-of-Time (AOT) compilation in release mode to improve runtime performance.

## 4. Project Structure

The solution will consist of two primary projects within the `src/Frontends/WebApp.UI` directory:

*   **`Opc.System.UI.Client`:** The Blazor WASM project containing all UI components, pages, client-side services, and static assets.
*   **`Opc.System.UI.Server`:** The ASP.NET Core project responsible for hosting the compiled Blazor WASM application.

The `Client` project will be organized using a feature-based folder structure:


Opc.System.UI.Client/
├── Features/
│   ├── AnomalyDetection/
│   ├── Configuration/
│   ├── DataVisualization/
│   ├── PredictiveMaintenance/
│   ├── Reporting/
│   └── UserManagement/
├── Shared/
│   ├── Components/
│   ├── Layout/
│   └── Services/
├── wwwroot/
│   ├── css/
│   └── i18n/
└── Program.cs


## 5. Core Services & Configuration

### 5.1. Client-Side Service Registration (`Client/Program.cs`)

The client's main entry point will configure the application's services for Dependency Injection (DI).

csharp
// In Opc.System.UI.Client/Program.cs
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);

    // 1. HTTP Client for API Gateway
    builder.Services.AddHttpClient("GatewayApi", client => 
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
        .AddHttpMessageHandler<AuthorizationMessageHandler>(); // Adds Bearer token

    // 2. Register API Client Services
    builder.Services.AddScoped<IGatewayApiClient, GatewayApiClient>();

    // 3. UI Services from MudBlazor
    builder.Services.AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
        // ... other configurations
    });

    // 4. Localization Services
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    // Configuration to load i18n JSON files

    // 5. Application State & Notification Services
    builder.Services.AddScoped<AuthorizationMessageHandler>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddSingleton<AppState>(); // Simple state management
    builder.Services.AddScoped<INotificationService, NotificationService>();

    // 6. Authentication
    builder.Services.AddAuthorizationCore();
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<AuthenticationStateProvider, AppAuthenticationStateProvider>();
    
    await builder.Build().RunAsync();
}


### 5.2. API Communication (`GatewayApiClient.cs`)

A typed `HttpClient` service will be created to abstract all communication with the backend API Gateway.

*   **`GatewayApiClient.cs`:**
    *   Will be injected with `IHttpClientFactory`.
    *   Will contain methods for each API endpoint group (e.g., `GetClientInstancesAsync()`, `UpdateTagConfigurationAsync(string clientId, TagConfigDto config)`, `GetHistoricalDataAsync(HistoricalQueryDto query)`).
    *   Methods will handle JSON serialization/deserialization and error handling, converting HTTP status codes into meaningful application exceptions or results.

### 5.3. Authentication and State Management

*   **`AppAuthenticationStateProvider.cs`:** A custom `AuthenticationStateProvider` will manage the user's auth state. It will read the JWT from secure storage (browser local storage), parse it to extract claims, and notify Blazor of the user's authentication status.
*   **`AuthenticationService.cs`:** Will handle the login/logout process by calling the API gateway's auth endpoints and storing/removing the JWT.
*   **`AuthorizationMessageHandler.cs`:** An `HttpMessageHandler` that automatically attaches the JWT as a `Bearer` token to all outgoing API requests.
*   **`AppState.cs`:** A singleton service for simple, non-auth-related shared state.
    *   `public event Action OnChange;`
    *   `private void NotifyStateChanged() => OnChange?.Invoke();`
    *   Will hold properties like `CurrentTheme`, `SelectedClientInstance`, etc., and call `NotifyStateChanged()` when they are modified. Components will subscribe to the `OnChange` event to re-render.

### 5.4. Localization (`REQ-UIX-006`)

*   Localization will be provided via JSON files in `wwwroot/i18n/` (e.g., `en-US.json`, `de-DE.json`).
*   In `Program.cs`, the localization service will be configured to fetch these files.
*   Components will inject `IStringLocalizer<T>` to access localized strings (e.g., `@L["Dashboard.Title"]`).
*   A `LanguageSelector.razor` component will be created to allow users to switch the current culture, which will be persisted in browser local storage.

## 6. Feature-Specific Implementation Details

### 6.1. Layout & Navigation (`Shared/`)

*   **`MainLayout.razor`:** Will use `MudLayout`, `MudAppBar`, `MudDrawer`, and `MudMainContent`. It will also host the `MudSnackbarProvider` for notifications.
*   **`NavMenu.razor`:** Will contain a `MudNavMenu` with `MudNavLink` components pointing to the application's main routes. Links will be permission-based, rendered only if the user has the required role/claim. A link to the documentation portal will be included (`REQ-UIX-024`).

### 6.2. Data Visualization (`Features/DataVisualization/`)

*   **`DataExplorerPage.razor`:** The main page for dashboards. It will allow users to select, create, or edit a dashboard layout.
*   **`DashboardView.razor`:** Renders a dashboard from a configuration object. It will use a `MudGrid` to position widgets.
*   **`HistoricalDataChart.razor` (`REQ-UIX-010`):**
    *   **Parameters:** `List<string> TagIds`, `DateTimeRange TimeRange`.
    *   **Logic:**
        1.  On parameters set, it will show a `MudProgressCircular` loading indicator.
        2.  It will call `GatewayApiClient.GetHistoricalDataAsync()` with the parameters. The API is expected to return down-sampled data if the time range is large.
        3.  The component will use the `Plotly.Blazor` library to render a time-series line chart.
        4.  It will handle error states by displaying a user-friendly message from the `NotificationService`.
        5.  The entire component will be wrapped in an `ErrorBoundary` component.

### 6.3. Configuration Management (`Features/Configuration/`)

*   **`TagConfiguration.razor` (`REQ-UIX-004`):**
    *   Will use a `MudDataGrid` to display configured tags for a selected client instance.
    *   The grid will have buttons for "Add", "Edit", and "Delete" which open a `MudDialog`.
    *   Will include a button to "Browse Server", which launches the `NamespaceBrowser` component in a dialog.
*   **`NamespaceBrowser.razor` (`REQ-UIX-002`, `REQ-UIX-004`):**
    *   Will use a `MudTreeView` component.
    *   When a tree node is expanded, it will call `GatewayApiClient.BrowseNamespaceAsync(ServerId, NodeId)` to fetch child nodes.
    -   Users can select tags from the tree, which are then added to the `TagConfiguration` component.

### 6.4. AI Feedback (`Features/PredictiveMaintenance/`, `Features/AnomalyDetection/`)

*   **`PredictionFeedbackForm.razor` (`REQ-UIX-013`):**
    *   A simple form with `MudRadioGroup` for validation (Correct/Incorrect) and a `MudTextField` for comments.
    *   A submit button will call `GatewayApiClient.SubmitPredictionFeedbackAsync()` and use `INotificationService` to show success/error.
*   **`AnomalyReviewCard.razor` (`REQ-UIX-014`):**
    *   Displays anomaly details (tag, time, value).
    *   Includes a small `HistoricalDataChart` to show the data context around the anomaly.
    *   Uses a `MudSelect` to allow users to label the anomaly (e.g., True Positive, False Positive).
    *   Saving the label calls `GatewayApiClient.LabelAnomalyAsync()`.

### 6.5. Reporting (`Features/Reporting/`)

*   **`ReportTemplateEditor.razor` (`REQ-UIX-017`):**
    *   A complex component allowing users to build a report definition.
    *   Uses `MudTabs` to separate sections like "Data Sources", "Visualizations", "Scheduling", and "Branding".
    *   Users can select tags, define aggregations, and choose chart types.
    *   Provides inputs for scheduling (e.g., cron expression) and distribution (email list).
    *   Saving the template calls `GatewayApiClient.SaveReportTemplateAsync()`.

## 7. Cross-Cutting Concerns

### 7.1. Error Handling and User Feedback

*   A global `ErrorBoundary` will be configured in `App.razor` to catch unhandled exceptions.
*   The `GatewayApiClient` will be responsible for catching `HttpRequestException` and returning a structured `Result` object (e.g., `{ bool Succeeded, string ErrorMessage }`).
*   UI components will check the `Result` object and use the injected `INotificationService` to display user-friendly snackbar messages (`REQ-UIX-001`).

### 7.2. Performance (`REQ-UIX-010`)

*   **AOT Compilation:** The `Opc.System.UI.Client.csproj` will be configured to use AOT compilation for release builds. `<RunAOTCompilation>true</RunAOTCompilation>`.
*   **Virtualization:** Long lists of data (e.g., client instances, alarms, tags) will use the `MudDataGrid` component, which has built-in virtualization and server-side data loading capabilities.
*   **Lazy Loading:** For assemblies not required on the initial load, Blazor's lazy loading feature will be utilized.
*   **Data Fetching:** Components will fetch only the data they need to display. The backend API is responsible for efficient data querying and aggregation.

### 7.3. Accessibility (`REQ-UIX-001`)

*   **Semantic HTML:** Developers must use correct HTML5 elements.
*   **ARIA Attributes:** Where custom components or complex interactions are built, appropriate ARIA roles and attributes must be used.
*   **Keyboard Navigation:** All interactive `MudBlazor` components are keyboard-accessible. Any custom interactive components must be tested for full keyboard operability (tabbing, enter/space to activate).
*   **Color Contrast:** The application's theme, defined in `MainLayout.razor` using `MudThemeProvider`, will be configured with colors that meet WCAG 2.1 AA contrast ratios.

## 8. Data Models (ViewModels)

The UI will use a set of local ViewModel classes to represent data. These are distinct from backend DTOs to allow for UI-specific properties and state (e.g., `IsSelected`, `IsLoading`).

*   `ClientInstanceViewModel { Guid Id, string Name, string Status, double CpuUsage, ... }`
*   `TagViewModel { Guid Id, string NodeId, string DataType, bool IsSelected, ... }`
*   `DashboardViewModel { Guid Id, string Name, List<WidgetViewModel> Widgets }`
*   `WidgetViewModel { Guid Id, WidgetType Type, GridPosition Position, ... }`
*   `AnomalyViewModel { Guid Id, DateTime Timestamp, string TagName, double Value, string Label, ... }`
*   `ReportTemplateViewModel { Guid Id, string Name, ReportConfigData Config, ... }`