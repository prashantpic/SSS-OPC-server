# Software Design Specification (SDS): Notification Service

## 1. Introduction

### 1.1. Purpose

This document provides a detailed software design for the **Notification Service**. This service is a dedicated microservice responsible for dispatching system-level notifications, such as critical alarm escalations and system health alerts, to users and administrators via multiple channels. It acts as a decoupled, centralized hub for all outgoing notifications, consuming events from a message queue and interacting with third-party providers for email and SMS delivery.

### 1.2. Scope

The scope of this document is limited to the design and implementation of the `services.notification-service` repository. This includes:
- Consuming messages from a message bus (RabbitMQ via MassTransit).
- Orchestrating the notification process based on event data.
- Rendering message content from templates.
- Dispatching notifications via Email (SMTP) and SMS (Twilio).
- Configuration management and structured logging.

Interactions with other services (e.g., the services that produce the events) are defined by the event contracts consumed by this service.

## 2. System Architecture & Design

### 2.1. Architectural Style

The Notification Service is a C#/.NET 8 microservice designed to run as a background worker process. It follows a **Layered Architecture** (often called Clean or Onion Architecture) to ensure a high degree of separation of concerns, maintainability, and testability.

- **Presentation Layer:** Contains the message queue consumers, which act as the entry point into the service.
- **Application Layer:** Contains the core business logic, orchestrating the steps required to send a notification. It is independent of any specific infrastructure.
- **Domain Layer:** Contains core business entities and enums (e.g., `NotificationChannel`).
- **Infrastructure Layer:** Contains all implementations that interact with external systems, such as email servers, SMS gateways, and the file system for templates.

### 2.2. Design Patterns

- **Event-Driven Consumer:** The service is entirely reactive, triggered by `AlarmTriggeredEvent` and `HealthAlertEvent` messages received from a RabbitMQ message bus. This decouples the notification logic from the event producers.
- **Dependency Injection (DI):** Heavily utilized by the .NET Host to manage the lifecycle of services and promote loose coupling. All services and adapters are registered in `Program.cs`.
- **Strategy Pattern:** `INotificationChannel` serves as the strategy interface, with `EmailChannel` and `SmsChannel` as concrete strategies. This allows the `NotificationService` to select a delivery method at runtime without changing its core logic.
- **Adapter Pattern:** `IEmailProvider` and `ISmsProvider` act as contracts for adapters that wrap third-party libraries (`MailKit`, `Twilio SDK`). This isolates the application from specific third-party implementations, making them swappable.
- **Options Pattern:** `IOptions<T>` is used to inject strongly-typed configuration objects (`EmailSettings`, `SmsSettings`) into services, sourced from `appsettings.json`.

## 3. Shared Contracts (Events)

The service will consume events from a shared library (`Shared.Events`). The structure of these events is as follows:

csharp
// In a shared library, e.g., SSS.Shared.Events
public record AlarmTriggeredEvent(
    Guid CorrelationId,
    string AlarmMessage,
    int Severity,
    string SourceNode,
    DateTimeOffset Timestamp,
    List<RecipientInfo> Recipients
);

public record HealthAlertEvent(
    Guid CorrelationId,
    string AlertMessage,
    string Component,
    DateTimeOffset Timestamp
);

public record RecipientInfo(
    string? EmailAddress,
    string? PhoneNumber
);

**Note:** For `HealthAlertEvent`, recipients are not included in the event. The service will fetch a pre-configured list of administrator contacts from its own configuration.

## 4. Detailed Component Specification

This section details the design of each C# file within the project.

### 4.1. Project File (`src/services.notification-service.csproj`)

- **Purpose:** Defines the project's framework, dependencies, and build settings.
- **Framework:** `net8.0`
- **Package References:**
  - `Microsoft.Extensions.Hosting`
  - `Serilog.AspNetCore`
  - `Serilog.Sinks.Console`
  - `MassTransit.RabbitMQ`
  - `MailKit`
  - `Twilio`
  - `Scriban` (for templating)

### 4.2. Host & Configuration

#### 4.2.1. `src/Program.cs`

- **Responsibilities:**
  - Configure and build the .NET generic host.
  - Set up Serilog for structured logging.
  - Load configuration from `appsettings.json` and environment variables.
  - Register all services, channels, providers, and consumers for dependency injection.
  - Configure and start the MassTransit message bus listener.
- **Logic:**
  1. Create a `Host.CreateDefaultBuilder()`.
  2. Configure Serilog to write to the console in JSON format.
  3. Configure services (`services.AddOptions<T>`, `services.AddSingleton<T>`, `services.AddScoped<T>`).
  4. Register all `INotificationChannel` implementations.
  5. Register `IEmailProvider` -> `MailKitSmtpAdapter` and `ISmsProvider` -> `TwilioSmsAdapter`.
  6. Register `ITemplateService` -> `FileSystemTemplateService`.
  7. Register `INotificationService` -> `NotificationService`.
  8. Configure MassTransit using `services.AddMassTransit()`:
     - Set up the RabbitMQ transport, reading the connection string from configuration.
     - Automatically discover and register consumers (`AlarmEventConsumer`, `HealthAlertConsumer`) from the assembly.
     - Configure retry policies for consumers (e.g., exponential backoff, 3 retries).
  9. Build and run the host.

#### 4.2.2. `src/appsettings.json`

- **Purpose:** Externalize configuration.
- **Structure:**
  json
  {
    "Serilog": { /* ... */ },
    "ConnectionStrings": {
      "MessageBus": "amqp://guest:guest@localhost:5672"
    },
    "NotificationSettings": {
      "AdminRecipients": {
        "Emails": ["admin1@example.com", "admin2@example.com"],
        "PhoneNumbers": ["+15551234567"]
      },
      "EmailSettings": {
        "SmtpServer": "smtp.example.com",
        "Port": 587,
        "UseSsl": true,
        "Username": "user",
        "Password": "YOUR_PASSWORD_HERE", // Use user secrets
        "FromAddress": "noreply@sss-opc.com",
        "FromName": "SSS System Alerts"
      },
      "SmsSettings": {
        "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxx", // Use user secrets
        "AuthToken": "YOUR_AUTH_TOKEN_HERE",  // Use user secrets
        "FromPhoneNumber": "+15557654321"
      }
    }
  }
  
- **Security Note:** Sensitive values (`Password`, `AccountSid`, `AuthToken`) **MUST** be managed using .NET User Secrets in development and environment variables or a secure vault (e.g., Azure Key Vault) in production.

#### 4.2.3. `src/Configuration/NotificationSettings.cs`

- **Purpose:** Strongly-typed classes to map the `appsettings.json` `NotificationSettings` section.
- **Classes:**
  - `public class EmailSettings { ... }` with properties for `SmtpServer`, `Port`, `UseSsl`, `Username`, `Password`, etc.
  - `public class SmsSettings { ... }` with properties for `AccountSid`, `AuthToken`, `FromPhoneNumber`.
  - `public class AdminRecipients { ... }` with `List<string> Emails` and `List<string> PhoneNumbers`.

### 4.3. Presentation Layer (Message Consumers)

#### 4.3.1. `src/Presentation/Consumers/AlarmEventConsumer.cs`

- **Purpose:** To consume `AlarmTriggeredEvent` messages and initiate the notification process.
- **Dependencies:** `ILogger<AlarmEventConsumer>`, `Application.Abstractions.INotificationService`.
- **Method: `public Task Consume(ConsumeContext<AlarmTriggeredEvent> context)`**
  - **Logic:**
    1. Log the reception of the event with its `CorrelationId`.
    2. Create a new `NotificationRequest` DTO.
    3. Map the recipients from `context.Message.Recipients` to the `NotificationRequest.Recipients` list. This requires iterating and extracting non-null email addresses and phone numbers.
    4. Set `TemplateId` to a constant, e.g., `"CriticalAlarm"`.
    5. Set `TemplateData` with key information from the event: `AlarmMessage`, `Severity`, `SourceNode`, `Timestamp`.
    6. Set `TargetChannels` to `[NotificationChannel.Email, NotificationChannel.Sms]`.
    7. Await `_notificationService.SendNotificationAsync(request)`.
    8. Log success or failure of the service call.

#### 4.3.2. `src/Presentation/Consumers/HealthAlertConsumer.cs`

- **Purpose:** To consume `HealthAlertEvent` messages and notify administrators.
- **Dependencies:** `ILogger<HealthAlertConsumer>`, `Application.Abstractions.INotificationService`, `IOptions<Configuration.NotificationSettings>`.
- **Method: `public Task Consume(ConsumeContext<HealthAlertEvent> context)`**
  - **Logic:**
    1. Log the reception of the health alert.
    2. Create a new `NotificationRequest`.
    3. Populate `Recipients` with the administrator contacts from the injected `NotificationSettings.AdminRecipients`.
    4. Set `TemplateId` to a constant, e.g., `"HealthAlert"`.
    5. Set `TemplateData` with `AlertMessage`, `Component`, and `Timestamp` from the event.
    6. Set `TargetChannels` to `[NotificationChannel.Email, NotificationChannel.Sms]`.
    7. Await `_notificationService.SendNotificationAsync(request)`.

### 4.4. Application Layer

#### 4.4.1. `src/Application/Abstractions/INotificationService.cs`

- **Purpose:** Define the contract for the core application logic.
- **Interface Definition:**
  csharp
  public interface INotificationService
  {
      Task<bool> SendNotificationAsync(NotificationRequest request, CancellationToken cancellationToken);
  }
  

#### 4.4.2. `src/Application/Services/NotificationService.cs`

- **Purpose:** Implements `INotificationService` to orchestrate template rendering and channel dispatching.
- **Dependencies:** `IEnumerable<Infrastructure.Channels.Abstractions.INotificationChannel>`, `Infrastructure.Templates.Abstractions.ITemplateService`, `ILogger<NotificationService>`.
- **Method: `public async Task<bool> SendNotificationAsync(...)`**
  - **Logic:**
    1. Log the start of the notification process.
    2. Iterate through each `channelType` in `request.TargetChannels`.
    3. Inside the loop, find the corresponding `INotificationChannel` implementation from the injected `_channels` collection (`_channels.FirstOrDefault(c => c.ChannelType == channelType)`).
    4. If no channel implementation is found, log a warning and continue.
    5. Call `_templateService.RenderTemplateAsync(channelType, request.TemplateId, request.TemplateData)` to get the rendered subject and body.
    6. Iterate through each `recipient` in `request.Recipients`.
    7. Inside the recipient loop, in a `try-catch` block:
       - Await `channel.SendAsync(recipient, rendered.Subject, rendered.Body, cancellationToken)`.
       - Log success for the recipient and channel.
    8. `catch (Exception ex)`:
       - Log the error, including recipient, channel, and exception details.
       - Do not re-throw; continue to the next recipient/channel to ensure one failure doesn't stop all notifications.
    9. Return `true` if at least one notification was sent successfully, otherwise `false`.

### 4.5. Infrastructure Layer

#### 4.5.1. Templates (`src/Infrastructure/Templates/...`)

- **`ITemplateService.cs`:**
  - Defines the interface `public interface ITemplateService { Task<(string Subject, string Body)> RenderTemplateAsync(...); }`
- **`FileSystemTemplateService.cs`:**
  - **Dependencies:** `ILogger<FileSystemTemplateService>`, `IHostEnvironment`.
  - **Logic for `RenderTemplateAsync`:**
    1. Determine the file extension based on channel (`.html` for Email, `.txt` for Sms).
    2. Construct the file path: `Path.Combine(_hostEnvironment.ContentRootPath, "Templates", channel.ToString(), $"{templateId}{extension}")`.
    3. Read the template content from the file (implement file content caching for performance).
    4. The template file will be structured with a subject line and body, separated by `---`. Example:
       
       Subject: Critical Alarm Notification - {{ source_node }}
       ---
       A critical alarm has been triggered.
       Message: {{ alarm_message }}
       Severity: {{ severity }}
       
    5. Parse the file to extract the subject template and the body template.
    6. Use the `Scriban` library to render both the subject and body templates with the provided `data` object.
    7. Return the rendered subject and body.
    8. Handle `FileNotFoundException` gracefully by logging an error and returning an empty result.

#### 4.5.2. Channels & Providers (`src/Infrastructure/Channels/...`)

- **`INotificationChannel.cs` (Strategy Interface):**
  - Defines `NotificationChannel ChannelType { get; }` and `Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken);`
- **`IEmailProvider.cs` / `ISmsProvider.cs` (Adapter Interfaces):**
  - Define simple contracts for sending messages, e.g., `Task SendEmailAsync(string to, string subject, string htmlBody, ...)`
- **`EmailChannel.cs` / `SmsChannel.cs` (Concrete Strategies):**
  - Implement `INotificationChannel`.
  - `ChannelType` property returns `NotificationChannel.Email` or `NotificationChannel.Sms`.
  - `SendAsync` method simply calls the corresponding method on the injected provider (`_emailProvider.SendEmailAsync` or `_smsProvider.SendSmsAsync`). It adapts the generic `SendAsync` parameters to the provider-specific ones. For SMS, the `subject` is ignored.
- **`MailKitSmtpAdapter.cs` (Concrete Adapter):**
  - **Dependencies:** `IOptions<Configuration.EmailSettings>`, `ILogger<MailKitSmtpAdapter>`.
  - **Logic for `SendEmailAsync`:**
    1. Create a `MimeMessage`.
    2. Set `From`, `To`, `Subject`.
    3. Create a `BodyBuilder` and set `HtmlBody`.
    4. `using var client = new MailKit.Net.Smtp.SmtpClient();`
    5. `await client.ConnectAsync(...)` using settings from `_settings.Value`.
    6. `await client.AuthenticateAsync(...)` if credentials are provided.
    7. `await client.SendAsync(message, cancellationToken)`.
    8. `await client.DisconnectAsync(true, cancellationToken)`.
    9. Log connection, authentication, and send events.
- **`TwilioSmsAdapter.cs` (Concrete Adapter):**
  - **Dependencies:** `IOptions<Configuration.SmsSettings>`, `ILogger<TwilioSmsAdapter>`.
  - **Initialization:** `TwilioClient.Init(_settings.Value.AccountSid, _settings.Value.AuthToken)` should be called once at startup (e.g., in `Program.cs` or the constructor).
  - **Logic for `SendSmsAsync`:**
    1. `await MessageResource.CreateAsync(to: new Twilio.Types.PhoneNumber(to), from: new Twilio.Types.PhoneNumber(_settings.Value.FromPhoneNumber), body: body);`
    2. The API call is wrapped in a `try-catch` to handle Twilio-specific exceptions and log them appropriately.

## 5. Logging & Error Handling

- **Logging:** All logs will be structured (JSON) via Serilog. Every log entry should, where possible, include a `CorrelationId` propagated from the initial event.
- **Consumer Error Handling:** MassTransit will be configured with a retry policy. If an event fails processing after all retries, it will be moved to a dedicated `_error` queue for manual inspection. This prevents poison messages from blocking the main queue.
- **Provider Error Handling:** Network errors, authentication failures, or API errors from MailKit/Twilio will be caught within the respective adapter. The exception will be logged with details, but it will not be re-thrown up to the consumer, allowing the `NotificationService` to attempt other channels or recipients. The `SendNotificationAsync` method's return value will indicate overall success/failure.