# Software Design Specification (SDS) for ui.blazor-web-app

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the `ui.blazor-web-app` repository. This repository contains the Blazor WebAssembly (WASM) Single Page Application (SPA), which serves as the primary user interface for the entire industrial data platform. It is responsible for providing a cohesive, responsive, and accessible user experience for all system features, including centralized management, data visualization, alarm monitoring, and configuration.

### 1.2. Scope
The scope of this document is limited to the `ui.blazor-web-app` project. This application is a pure client-side application that runs in the user's browser. All communication with the backend is performed exclusively through a designated API Gateway via secure HTTP/WebSocket protocols. The design covers application structure, component design, state management, API communication, and UI/UX patterns.

### 1.3. Technologies and Libraries
- **Framework:** .NET 8, Blazor WebAssembly
- **Language:** C#, HTML, CSS, JavaScript
- **UI Component Library:** MudBlazor
- **Charting Libraries:** Chart.js, Plotly.js (via `ChartJs.Blazor` and `Plotly.Blazor` wrappers and JS Interop)
- **State Management:** Custom C# Services with `CascadingValue` and `INotifyPropertyChanged` patterns.

## 2. Architectural Design

### 2.1. Overall Architecture
The application follows a **Single Page Application (SPA)** architecture using Blazor WebAssembly. It is organized into a modular structure based on application features.

- **Component-Based:** The UI is built as a tree of reusable Razor components.
- **Client-Side Rendering:** All UI rendering and logic execution happen in the user's browser after the initial download.
- **Decoupled Backend:** The frontend is completely decoupled from the backend services, communicating only with the API Gateway. This enforces a clear separation of concerns and allows the frontend and backend to be developed and deployed independently.

### 2.2. State Management Strategy
Given the complexity of a centralized management UI, a robust state management strategy is essential.
- **Global State Services:** For application-wide state (e.g., authenticated user, theme settings, localization), singleton C# services will be used. These services will be registered in `Program.cs`.
- **Scoped Feature State:** For state that is confined to a specific feature area (e.g., the current list of alarms in the console), scoped services or page-level view models will be used.
- **State Change Notification:** State services will implement `INotifyPropertyChanged` or use `Action` events to notify components of state changes. Components will subscribe to these events in `OnInitialized` and unsubscribe in `Dispose` to trigger re-rendering via `StateHasChanged()`.
- **Cascading Values:** Global services like the current theme or user information can be provided to the entire component tree using `<CascadingValue>`.

### 2.3. API Communication Strategy
- **Typed HttpClients:** For every major backend feature area (e.g., Management, Alarms, Data, Authentication), a dedicated typed `HttpClient` service will be created (e.g., `IManagementApiClient`). This provides a strongly-typed, testable, and reusable way to interact with the backend API.
- **Centralized Registration:** All API clients will be registered in `Program.cs`, configured with the base address of the API Gateway.
- **Authentication:** A custom `AuthorizationMessageHandler` will be created. This handler will be attached to the `HttpClient` pipeline. It will intercept every outgoing request, retrieve the JWT from the `ITokenManagerService`, and add it to the `Authorization` header.
- **DTOs (Data Transfer Objects):** A shared `Models` or `Contracts` project (or folder) will define all DTOs used for API communication, ensuring consistency between the client and the API gateway contract.

### 2.4. Error Handling Strategy
- **Global Error Boundary:** The root `App.razor` component will be wrapped in Blazor's `<ErrorBoundary>` component to catch any unhandled exceptions during the component lifecycle and display a user-friendly error UI instead of a blank page.
- **API Error Handling:** API client services will be responsible for handling non-successful HTTP status codes. They will either throw specific exceptions (e.g., `ApiException`) or return a result object containing the success status and error message.
- **User Notifications:** A singleton `INotificationService` will be implemented, leveraging MudBlazor's `ISnackbar` service. This service will be used by components and services to display user-friendly "toast" notifications for success, error, warning, and info messages, as required by `REQ-UIX-001`.

### 2.5. Accessibility (WCAG 2.1 AA)
Accessibility (`REQ-UIX-001`) will be a primary design consideration.
- **Semantic HTML:** Where custom HTML is used, it will be semantic.
- **MudBlazor:** The `MudBlazor` library has strong built-in accessibility support. Components will be used according to their documentation to leverage this.
- **ARIA Attributes:** `aria-label`, `aria-describedby`, and other ARIA attributes will be used to provide context for non-textual controls and complex components.
- **Keyboard Navigation:** All interactive elements must be reachable and operable using the keyboard alone. Tab order will be logical.
- **Focus Management:** Focus will be managed programmatically where necessary, especially in dialogs and dynamic content.

## 3. Core Application Implementation

### 3.1. `Program.cs` - Application Entry Point
This file will configure and bootstrap the application.
- **Services Registration:**
  - `AddMudServices()`: Registers all MudBlazor services (Snackbar, Dialogs, etc.).
  - **API Clients:** Registers all typed `HttpClient` services (e.g., `builder.Services.AddHttpClient<IManagementApiClient, ManagementApiClient>(...)`). Each client will be configured with the API Gateway's base address and the `AuthorizationMessageHandler`.
  - **Authentication:**
    - `AddAuthorizationCore()`: Adds core authorization services.
    - `AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>()`: Registers the custom auth state provider.
    - `AddScoped<ITokenManagerService, TokenManagerService>()`: Registers the service for managing the JWT in browser storage.
  - **State and UI Services:**
    - `AddSingleton<INotificationService, NotificationService>()`: For user feedback.
    - `AddLocalization()`: Registers localization services. The `LocalizationService` will be registered as a singleton.
- **Host Building:** The `Main` method will configure the `WebAssemblyHostBuilder` and build the application host.

### 3.2. Authentication (`Authentication/`)
- **`ITokenManagerService.cs`:**
  - `Task<string> GetTokenAsync()`: Retrieves the token from secure browser storage.
  - `Task SetTokenAsync(string token)`: Saves the token.
  - `Task RemoveTokenAsync()`: Deletes the token.
- **`ApiAuthenticationStateProvider.cs`:**
  - Overrides `GetAuthenticationStateAsync()` to read the token via `ITokenManagerService`, parse its claims, and construct a `ClaimsPrincipal`.
  - Exposes `NotifyUserAuthentication(string token)` and `NotifyUserLogout()` methods. These methods will update the token and call `NotifyAuthenticationStateChanged()` to update the entire application's UI.
- **`AuthorizationMessageHandler.cs`:**
  - A `DelegatingHandler`.
  - Overrides `SendAsync()` to check if a token exists using `ITokenManagerService` and, if so, adds the `Authorization: Bearer <token>` header to the request before sending it.

### 3.3. Layout (`Layout/`)
- **`MainLayout.razor`:**
  - Uses `<MudLayout>`, `<MudAppBar>`, `<MudDrawer>`, and `<MudMainContent>`.
  - The `<MudAppBar>` will contain the drawer toggle button, application title, and potentially a user profile/logout menu and a language selector.
  - The `<MudDrawer>` will host the `NavMenu` component.
  - The `<MudMainContent>` will render the `@Body`.
- **`NavMenu.razor`:**
  - Uses `<MudNavMenu>` and `<MudNavLink>`.
  - Navigation links will be grouped logically (e.g., "Dashboards", "Alarms", "Management", "Configuration", "Admin").
  - The visibility of certain links will be controlled by wrapping them in the `<AuthorizeView>` component with specific roles.

## 4. Feature Modules Design

This section outlines the design for the major features of the application, corresponding to the `Features/` directory.

### 4.1. Centralized Management (`Features/Management/`)
- **Purpose:** To monitor and configure all connected OPC client instances (`REQ-UIX-022`, `REQ-9-004`).
- **Pages:**
  - **`ClientInstancesPage.razor`:**
    - **Route:** `@page "/management/clients"`
    - **UI:** A `<MudDataGrid>` displaying all registered client instances. Columns will include Name, Site, Status (Online/Offline), Health KPIs (CPU/Mem), and an actions button/menu.
    - **Logic:** In `OnInitializedAsync`, it will call `ManagementApiClient.GetClientInstancesAsync()` to populate the grid. It may use a timer to periodically refresh the status.
- **Components:**
  - **`ClientDetailsPanel.razor`:** A component (potentially in a dialog or a separate page) that shows detailed information and configuration options for a single client instance. This will host other specialized configuration components like the `TagConfigurationEditor`.

### 4.2. Tag Configuration (`Features/Configuration/`)
- **Purpose:** To provide a UI for browsing OPC server namespaces and configuring tags (`REQ-UIX-002`, `REQ-UIX-004`).
- **Components:**
  - **`TagConfigurationEditor.razor`:**
    - **UI:** A two-panel layout.
      - **Left Panel:** A `<MudTreeView>` to represent the OPC server's namespace.
      - **Right Panel:** A `<MudDataGrid>` or `<MudList>` showing the tags already configured for the selected client.
    - **Logic:**
      - The tree view will load the root nodes initially. On node expansion (`OnNodeExpanded`), it will call `ManagementApiClient.BrowseNamespaceAsync(clientId, nodeId)` to fetch child nodes dynamically.
      - Each tree node will be a `<MudTreeViewItem>` wrapped in a `<MudDragAndDropItem>`.
      - The right panel will be a `<MudDropZone>`.
      - When a user drags a node from the tree and drops it on the right panel, the `ItemDropped` event will fire. The handler will create a new tag configuration DTO and add it to the local list.
      - A "Save" button will call `ManagementApiClient.UpdateClientConfigurationAsync()` to persist the changes.

### 4.3. Dashboards (`Features/Dashboards/`)
- **Purpose:** To visualize real-time and historical data in user-configurable layouts (`REQ-UIX-005`, `REQ-UIX-008`).
- **Pages:**
  - **`DashboardPage.razor`:**
    - **Route:** `@page "/dashboards/{DashboardId}"`
    - **Logic:** Fetches the dashboard layout (a JSON structure defining widgets, their positions, and configurations) from the backend. It will render a grid (e.g., using CSS Grid) and dynamically render the appropriate widget component for each item in the layout configuration.
- **Components:**
  - **`TrendChartWidget.razor`:**
    - **Parameters:** `[Parameter] public List<string> TagIds { get; set; }`, `[Parameter] public TimeRange TimeRange { get; set; }`.
    - **Logic:** In `OnParametersSetAsync`, it calls `DataApiClient.GetHistoricalDataAsync()`. On success, it formats the data into a structure suitable for Chart.js/Plotly.js and calls the `chartInterop.js` function via `IJSRuntime` to render the chart. It will display a `LoadingSpinner` while data is being fetched.
  - **`GaugeWidget.razor`:** Similar to the trend chart but for a single real-time value.
  - **`SingleValueWidget.razor`:** Displays a single tag's value and timestamp.

### 4.4. Alarms (`Features/Alarms/`)
- **Purpose:** To provide a console for monitoring and acknowledging alarms (`REQ-UIX-009`).
- **Pages:**
  - **`AlarmConsolePage.razor`:**
    - **Route:** `@page "/alarms"`
    - **UI:** A `<MudDataGrid>` configured for server-side data, filtering, and sorting. Columns: Severity (with color-coding), Timestamp, Source, Message, Status (Active/Acknowledged).
    - **Logic:**
      - The grid will be bound to a `ServerData` function. This function will call `AlarmApiClient.GetAlarmsAsync(gridState)` with the current paging, sorting, and filtering options.
      - A real-time connection (WebSocket preferred, polling fallback) will be established to receive new alarms and push them to the top of the grid or show a "New Alarms" notification.
      - Each row will have an "Acknowledge" button. Clicking it will call `AlarmApiClient.AcknowledgeAlarmAsync()`, and on success, the grid will be refreshed.

## 5. API Client Services (`ApiClient/`)

A set of interfaces and classes that abstract all backend communication.

- **`IAuthApiClient.cs`:**
  - `Task<LoginResponseDto> LoginAsync(LoginRequestDto request);`
- **`IManagementApiClient.cs`:**
  - `Task<List<ClientInstanceDto>> GetClientInstancesAsync();`
  - `Task<ClientConfigurationDto> GetClientConfigurationAsync(Guid clientId);`
  - `Task<List<OpcNodeDto>> BrowseNamespaceAsync(Guid clientId, string nodeId);`
  - `Task UpdateClientConfigurationAsync(Guid clientId, ClientConfigurationDto config);`
- **`IDataApiClient.cs`:**
  - `Task<List<HistoricalDataPointDto>> GetHistoricalDataAsync(HistoricalDataRequestDto request);`
- **`IAlarmApiClient.cs`:**
  - `Task<PagedResult<AlarmDto>> GetAlarmsAsync(GridState gridState);`
  - `Task AcknowledgeAlarmAsync(AcknowledgeAlarmDto request);`

## 6. JavaScript Interop (`wwwroot/js/`)

- **`chartInterop.js`:**
  - `createChart(elementId, chartType, data, options)`: Creates a new Chart.js or Plotly.js chart instance.
  - `updateChart(elementId, newData)`: Updates an existing chart with new data.
  - `destroyChart(elementId)`: Properly disposes of a chart instance to prevent memory leaks.

## 7. Localization (`wwwroot/i18n/`)

- **`en-US.json`, `de-DE.json`, `es-ES.json`, `zh-CN.json`:**
  - These files will contain key-value pairs for all UI strings.
  - Keys will follow a convention, e.g., `Page.Alarms.Title`, `Button.Acknowledge`.
- **`LocalizationService.cs`:**
  - Injected with `IStringLocalizer<App>`.
  - Provides a simple `this[string key]` indexer to retrieve localized strings.
  - Can be used in both `.razor` files (`@inject ILocalizationService L`) and `.cs` files.