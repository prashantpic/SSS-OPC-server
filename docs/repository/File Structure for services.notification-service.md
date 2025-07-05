# Specification

# 1. Files

- **Path:** src/services.notification-service.csproj  
**Description:** The C# project file defining project properties, dependencies (ASP.NET Core, MailKit, Twilio, Serilog, MassTransit/RabbitMQ.Client), and other build settings.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** services.notification-service  
**Type:** Project  
**Relative Path:** .  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - ProjectDependencies
    
**Requirement Ids:**
    
    
**Purpose:** Defines the .NET project, its target framework, and all required third-party library dependencies for the Notification Service.  
**Logic Description:** This file will contain ItemGroup sections for PackageReference entries for libraries such as MailKit for email, Twilio for SMS, MassTransit.RabbitMQ for message queue consumption, Serilog for structured logging, and the base ASP.NET Core framework packages.  
**Documentation:**
    
    - **Summary:** Specifies the project's metadata and dependencies required for compilation and execution of the Notification Service.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Program.cs  
**Description:** The main entry point for the Notification Service application. Configures and launches the ASP.NET Core host, sets up dependency injection, logging, message queue listeners, and other essential services.  
**Template:** C# Main Entry Point  
**Dependency Level:** 4  
**Name:** Program  
**Type:** ApplicationHost  
**Relative Path:** .  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - ServiceBootstrap
    - DependencyInjectionSetup
    - ConfigurationLoading
    
**Requirement Ids:**
    
    - REQ-CSVC-020
    - REQ-6-005
    
**Purpose:** Initializes and configures all components of the Notification Service, including registering services, channels, providers, and message consumers with the DI container.  
**Logic Description:** The file will create a WebApplicationBuilder, configure Serilog for logging, and read settings from appsettings.json. It will register application services (INotificationService), channel implementations (EmailChannel, SmsChannel), provider adapters (MailKit, Twilio), and the message bus (MassTransit/RabbitMQ). It will configure endpoints for message consumers and build and run the application.  
**Documentation:**
    
    - **Summary:** Bootstraps the entire Notification Service, composing all its parts and starting the execution loop.
    
**Namespace:** SSS.Services.Notification  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Presentation/Consumers/AlarmEventConsumer.cs  
**Description:** A message queue consumer that listens for critical alarm events from the system. It processes these events and triggers the notification dispatch process.  
**Template:** C# MassTransit Consumer  
**Dependency Level:** 3  
**Name:** AlarmEventConsumer  
**Type:** Consumer  
**Relative Path:** Presentation/Consumers  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - EventDrivenArchitecture
    
**Members:**
    
    - **Name:** _logger  
**Type:** ILogger<AlarmEventConsumer>  
**Attributes:** private|readonly  
    - **Name:** _notificationService  
**Type:** Application.Abstractions.INotificationService  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Consume  
**Parameters:**
    
    - ConsumeContext<Shared.Events.AlarmTriggeredEvent> context
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - AlarmNotificationTrigger
    
**Requirement Ids:**
    
    - REQ-CSVC-020
    
**Purpose:** Acts as the entry point for alarm-based notifications, decoupling the notification logic from the alarm generation source.  
**Logic Description:** This class implements the MassTransit IConsumer interface for a specific event type (e.g., AlarmTriggeredEvent). In the Consume method, it logs the received event, maps the event data to a NotificationRequest DTO, and invokes the INotificationService to orchestrate the sending of the notification.  
**Documentation:**
    
    - **Summary:** Handles incoming `AlarmTriggeredEvent` messages from a queue, initiating the process to notify users about the alarm.
    
**Namespace:** SSS.Services.Notification.Presentation.Consumers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Presentation/Consumers/HealthAlertConsumer.cs  
**Description:** A message queue consumer that listens for system health alerts (e.g., KPI thresholds breached). It processes these alerts to notify system administrators.  
**Template:** C# MassTransit Consumer  
**Dependency Level:** 3  
**Name:** HealthAlertConsumer  
**Type:** Consumer  
**Relative Path:** Presentation/Consumers  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - EventDrivenArchitecture
    
**Members:**
    
    - **Name:** _logger  
**Type:** ILogger<HealthAlertConsumer>  
**Attributes:** private|readonly  
    - **Name:** _notificationService  
**Type:** Application.Abstractions.INotificationService  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Consume  
**Parameters:**
    
    - ConsumeContext<Shared.Events.HealthAlertEvent> context
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - HealthAlertNotificationTrigger
    
**Requirement Ids:**
    
    - REQ-6-005
    
**Purpose:** Acts as the entry point for system health-based notifications, intended for administrators.  
**Logic Description:** Similar to the AlarmEventConsumer, this class implements IConsumer for a `HealthAlertEvent`. It extracts relevant information from the event, constructs a NotificationRequest, and calls the INotificationService to handle the dispatch to the appropriate administrative channels.  
**Documentation:**
    
    - **Summary:** Handles incoming `HealthAlertEvent` messages from a queue, initiating the process to notify administrators of system health issues.
    
**Namespace:** SSS.Services.Notification.Presentation.Consumers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Application/Abstractions/INotificationService.cs  
**Description:** Defines the primary contract for the notification orchestration logic, abstracting the process of sending a notification from the entry points (consumers).  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** INotificationService  
**Type:** Interface  
**Relative Path:** Application/Abstractions  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SendNotificationAsync  
**Parameters:**
    
    - Application.DTOs.NotificationRequest request
    - CancellationToken cancellationToken
    
**Return Type:** Task<bool>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides a clean interface for the core application logic, decoupling it from the presentation layer.  
**Logic Description:** This interface will contain a single primary method, `SendNotificationAsync`, which accepts a request object containing all necessary information to send a notification, such as recipient details, content data, and template identifiers.  
**Documentation:**
    
    - **Summary:** Declares the main entry point into the application's core logic for sending notifications.
    
**Namespace:** SSS.Services.Notification.Application.Abstractions  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Application/Services/NotificationService.cs  
**Description:** Implements the INotificationService. It orchestrates the notification sending process: selecting the channel, rendering the template, and dispatching via the chosen channel provider.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** NotificationService  
**Type:** Service  
**Relative Path:** Application/Services  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    - **Name:** _channels  
**Type:** IEnumerable<Infrastructure.Channels.Abstractions.INotificationChannel>  
**Attributes:** private|readonly  
    - **Name:** _templateService  
**Type:** Infrastructure.Templates.Abstractions.ITemplateService  
**Attributes:** private|readonly  
    - **Name:** _logger  
**Type:** ILogger<NotificationService>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SendNotificationAsync  
**Parameters:**
    
    - Application.DTOs.NotificationRequest request
    - CancellationToken cancellationToken
    
**Return Type:** Task<bool>  
**Attributes:** public  
    
**Implemented Features:**
    
    - NotificationOrchestration
    - ChannelSelection
    
**Requirement Ids:**
    
    - REQ-CSVC-020
    - REQ-6-005
    
**Purpose:** Centralizes the business logic for dispatching notifications, acting as a coordinator for various infrastructure components.  
**Logic Description:** The constructor will inject a collection of all registered INotificationChannel implementations. The `SendNotificationAsync` method will determine the target channels from the request or configuration. For each channel, it will render the appropriate template using the ITemplateService, then find the correct INotificationChannel from the injected collection and invoke its `SendAsync` method. It will handle errors and log the outcome of each dispatch attempt.  
**Documentation:**
    
    - **Summary:** The core orchestration service that receives a notification request, resolves the appropriate channels and templates, and manages the dispatch process.
    
**Namespace:** SSS.Services.Notification.Application.Services  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Application/DTOs/NotificationRequest.cs  
**Description:** Data Transfer Object representing a request to send a notification. Used to pass data from consumers to the application service.  
**Template:** C# DTO  
**Dependency Level:** 0  
**Name:** NotificationRequest  
**Type:** DTO  
**Relative Path:** Application/DTOs  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** Recipients  
**Type:** List<string>  
**Attributes:** public  
    - **Name:** TargetChannels  
**Type:** List<Domain.Enums.NotificationChannel>  
**Attributes:** public  
    - **Name:** TemplateId  
**Type:** string  
**Attributes:** public  
    - **Name:** TemplateData  
**Type:** Dictionary<string, object>  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides a structured, well-defined contract for initiating a notification, decoupling the internal domain from the message bus events.  
**Logic Description:** This is a plain C# record or class with properties for all the information needed to send a notification. This includes a list of recipients (e.g., email addresses or phone numbers), a list of channels to try (e.g., Email, SMS), the identifier for the template to be used, and a dictionary of key-value pairs for personalizing the template content.  
**Documentation:**
    
    - **Summary:** A data structure that encapsulates all information required to process and send a single notification.
    
**Namespace:** SSS.Services.Notification.Application.DTOs  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Domain/Enums/NotificationChannel.cs  
**Description:** An enumeration defining the supported notification channels.  
**Template:** C# Enum  
**Dependency Level:** 0  
**Name:** NotificationChannel  
**Type:** Enum  
**Relative Path:** Domain/Enums  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** Email  
**Type:** enum  
**Attributes:**   
    - **Name:** Sms  
**Type:** enum  
**Attributes:**   
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides a type-safe way to represent and select notification channels throughout the application.  
**Logic Description:** A simple C# enum with values for Email and SMS. This allows for strong typing and avoids the use of magic strings when identifying channels.  
**Documentation:**
    
    - **Summary:** Defines the distinct types of notification channels supported by the service.
    
**Namespace:** SSS.Services.Notification.Domain.Enums  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Infrastructure/Channels/Abstractions/INotificationChannel.cs  
**Description:** Defines the contract for a notification channel. This is the key interface for the Strategy pattern, allowing the application to treat all channels uniformly.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** INotificationChannel  
**Type:** Interface  
**Relative Path:** Infrastructure/Channels/Abstractions  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    - **Name:** ChannelType  
**Type:** Domain.Enums.NotificationChannel  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** SendAsync  
**Parameters:**
    
    - string recipient
    - string subject
    - string body
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Decouples the core application logic from the specific implementation details of sending notifications via different methods like email or SMS.  
**Logic Description:** This interface will define a `ChannelType` property to identify which channel it represents (e.g., Email, SMS). It will also define a `SendAsync` method that takes the generic, rendered content (recipient, subject, body) and is responsible for dispatching it.  
**Documentation:**
    
    - **Summary:** A common contract that all notification channel implementations must adhere to.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Channels.Abstractions  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Infrastructure/Channels/Email/EmailChannel.cs  
**Description:** Implementation of INotificationChannel for sending emails. It uses an IEmailProvider adapter to interact with the actual email sending service (e.g., MailKit).  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** EmailChannel  
**Type:** Service  
**Relative Path:** Infrastructure/Channels/Email  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - StrategyPattern
    - AdapterPattern
    
**Members:**
    
    - **Name:** ChannelType  
**Type:** Domain.Enums.NotificationChannel  
**Attributes:** public  
    - **Name:** _emailProvider  
**Type:** Infrastructure.Channels.Email.Abstractions.IEmailProvider  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SendAsync  
**Parameters:**
    
    - string recipient
    - string subject
    - string body
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - EmailDispatch
    
**Requirement Ids:**
    
    
**Purpose:** Encapsulates all logic related to sending an email notification, delegating the low-level communication to a provider adapter.  
**Logic Description:** This class implements `INotificationChannel`. Its `ChannelType` property returns `NotificationChannel.Email`. The `SendAsync` method simply calls the `SendEmailAsync` method on the injected `IEmailProvider` adapter, passing through the recipient, subject, and body.  
**Documentation:**
    
    - **Summary:** The strategy implementation for handling email notifications.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Channels.Email  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Infrastructure/Channels/Email/Abstractions/IEmailProvider.cs  
**Description:** Defines the contract for an email provider adapter, decoupling the EmailChannel from a specific service like MailKit or SendGrid.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IEmailProvider  
**Type:** Interface  
**Relative Path:** Infrastructure/Channels/Email/Abstractions  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - AdapterPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SendEmailAsync  
**Parameters:**
    
    - string to
    - string subject
    - string htmlBody
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides an abstraction layer over third-party email services, enabling provider-agnostic email sending logic.  
**Logic Description:** A simple interface with a single method, `SendEmailAsync`, that defines the parameters needed to send an email.  
**Documentation:**
    
    - **Summary:** A contract for any service that can send emails.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Channels.Email.Abstractions  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Infrastructure/Channels/Email/Adapters/MailKitSmtpAdapter.cs  
**Description:** An adapter that implements IEmailProvider using the MailKit library for communicating with an SMTP server.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** MailKitSmtpAdapter  
**Type:** Adapter  
**Relative Path:** Infrastructure/Channels/Email/Adapters  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - AdapterPattern
    
**Members:**
    
    - **Name:** _settings  
**Type:** IOptions<Configuration.EmailSettings>  
**Attributes:** private|readonly  
    - **Name:** _logger  
**Type:** ILogger<MailKitSmtpAdapter>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SendEmailAsync  
**Parameters:**
    
    - string to
    - string subject
    - string htmlBody
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - SmtpEmailSending
    
**Requirement Ids:**
    
    
**Purpose:** Encapsulates the specific implementation details of using MailKit to send emails, isolating this third-party dependency.  
**Logic Description:** This class implements `IEmailProvider`. The constructor injects email settings via `IOptions<EmailSettings>`. The `SendEmailAsync` method will use MailKit's `SmtpClient` to connect to the configured SMTP server, authenticate, construct a `MimeMessage` with the provided details, and send it. It will include robust error handling and logging for the SMTP communication.  
**Documentation:**
    
    - **Summary:** An adapter for sending emails via an SMTP server using the MailKit library.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Channels.Email.Adapters  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Infrastructure/Channels/Sms/SmsChannel.cs  
**Description:** Implementation of INotificationChannel for sending SMS messages. It uses an ISmsProvider adapter to interact with the actual SMS gateway (e.g., Twilio).  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** SmsChannel  
**Type:** Service  
**Relative Path:** Infrastructure/Channels/Sms  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - StrategyPattern
    - AdapterPattern
    
**Members:**
    
    - **Name:** ChannelType  
**Type:** Domain.Enums.NotificationChannel  
**Attributes:** public  
    - **Name:** _smsProvider  
**Type:** Infrastructure.Channels.Sms.Abstractions.ISmsProvider  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SendAsync  
**Parameters:**
    
    - string recipient
    - string subject
    - string body
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - SmsDispatch
    
**Requirement Ids:**
    
    
**Purpose:** Encapsulates all logic related to sending an SMS notification, delegating the low-level communication to a provider adapter.  
**Logic Description:** This class implements `INotificationChannel`. Its `ChannelType` property returns `NotificationChannel.Sms`. The `SendAsync` method calls the `SendSmsAsync` method on the injected `ISmsProvider` adapter, passing the recipient's phone number and the message body. The `subject` parameter is ignored for SMS.  
**Documentation:**
    
    - **Summary:** The strategy implementation for handling SMS notifications.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Channels.Sms  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Infrastructure/Channels/Sms/Abstractions/ISmsProvider.cs  
**Description:** Defines the contract for an SMS provider adapter, decoupling the SmsChannel from a specific gateway like Twilio.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** ISmsProvider  
**Type:** Interface  
**Relative Path:** Infrastructure/Channels/Sms/Abstractions  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - AdapterPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SendSmsAsync  
**Parameters:**
    
    - string to
    - string body
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides an abstraction layer over third-party SMS gateways, enabling provider-agnostic SMS sending logic.  
**Logic Description:** A simple interface with a single method, `SendSmsAsync`, that defines the parameters needed to send a text message.  
**Documentation:**
    
    - **Summary:** A contract for any service that can send SMS messages.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Channels.Sms.Abstractions  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Infrastructure/Channels/Sms/Adapters/TwilioSmsAdapter.cs  
**Description:** An adapter that implements ISmsProvider using the Twilio SDK to send SMS messages.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** TwilioSmsAdapter  
**Type:** Adapter  
**Relative Path:** Infrastructure/Channels/Sms/Adapters  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - AdapterPattern
    
**Members:**
    
    - **Name:** _settings  
**Type:** IOptions<Configuration.SmsSettings>  
**Attributes:** private|readonly  
    - **Name:** _logger  
**Type:** ILogger<TwilioSmsAdapter>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SendSmsAsync  
**Parameters:**
    
    - string to
    - string body
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - TwilioSmsSending
    
**Requirement Ids:**
    
    
**Purpose:** Encapsulates the specific implementation details of using the Twilio SDK, isolating this third-party dependency.  
**Logic Description:** This class implements `ISmsProvider`. It initializes the Twilio client using credentials from injected `SmsSettings`. The `SendSmsAsync` method uses the Twilio SDK to create and send a new message, specifying the recipient, the message body, and the configured 'from' number. It includes error handling and logging for the API calls to Twilio.  
**Documentation:**
    
    - **Summary:** An adapter for sending SMS messages via the Twilio platform.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Channels.Sms.Adapters  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Infrastructure/Templates/Abstractions/ITemplateService.cs  
**Description:** Defines the contract for a service that can render notification content from a template.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** ITemplateService  
**Type:** Interface  
**Relative Path:** Infrastructure/Templates/Abstractions  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** RenderTemplateAsync  
**Parameters:**
    
    - Domain.Enums.NotificationChannel channel
    - string templateId
    - object data
    
**Return Type:** Task<(string Subject, string Body)>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Decouples the logic of message creation from the core notification service, allowing template rendering to be a distinct, swappable component.  
**Logic Description:** This interface defines a single method, `RenderTemplateAsync`, which takes the channel type (to find the right template variant, e.g., HTML for email, text for SMS), a template identifier, and a data object/dictionary for personalization. It returns the rendered subject and body as a tuple.  
**Documentation:**
    
    - **Summary:** A contract for a service that can process a template and data to produce final notification content.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Templates.Abstractions  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Infrastructure/Templates/Services/FileSystemTemplateService.cs  
**Description:** An implementation of ITemplateService that reads template files from the local file system and renders them using a templating engine like Handlebars.Net or Scriban.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** FileSystemTemplateService  
**Type:** Service  
**Relative Path:** Infrastructure/Templates/Services  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _templateBasePath  
**Type:** string  
**Attributes:** private|readonly  
    - **Name:** _logger  
**Type:** ILogger<FileSystemTemplateService>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** RenderTemplateAsync  
**Parameters:**
    
    - Domain.Enums.NotificationChannel channel
    - string templateId
    - object data
    
**Return Type:** Task<(string Subject, string Body)>  
**Attributes:** public  
    
**Implemented Features:**
    
    - TemplateRendering
    
**Requirement Ids:**
    
    
**Purpose:** Manages the loading and processing of physical template files, separating content from code.  
**Logic Description:** This service determines the correct file path based on the channel and templateId (e.g., `Templates/Email/AlarmEvent.html`). It reads the file content, potentially caching it for performance. It then uses a templating library to compile the template and render it with the provided data object. It may also parse the template file for a subject line (e.g., from a comment or a front-matter section).  
**Documentation:**
    
    - **Summary:** Renders notification content by loading template files from the file system and applying a data model.
    
**Namespace:** SSS.Services.Notification.Infrastructure.Templates.Services  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Configuration/NotificationSettings.cs  
**Description:** A collection of strongly-typed configuration classes used to bind settings from appsettings.json via the .NET Options pattern.  
**Template:** C# POCO  
**Dependency Level:** 0  
**Name:** NotificationSettings  
**Type:** Configuration  
**Relative Path:** Configuration  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    - OptionsPattern
    
**Members:**
    
    - **Name:** EmailSettings  
**Type:** EmailSettings  
**Attributes:** public  
    - **Name:** SmsSettings  
**Type:** SmsSettings  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides a type-safe and structured way to access application configuration for various notification channels.  
**Logic Description:** This file will contain multiple classes. `EmailSettings` will have properties for SmtpServer, Port, Username, Password, and FromAddress. `SmsSettings` will have properties for TwilioAccountSid, AuthToken, and FromPhoneNumber. These classes will map directly to sections in the appsettings.json file.  
**Documentation:**
    
    - **Summary:** Defines the C# object model for the service's configuration settings.
    
**Namespace:** SSS.Services.Notification.Configuration  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/appsettings.json  
**Description:** JSON configuration file for the Notification Service. Contains settings for different environments, such as SMTP server details, Twilio credentials, and message queue connection strings.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:** .  
**Repository Id:** REPO-SAP-010  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - ExternalizedConfiguration
    
**Requirement Ids:**
    
    
**Purpose:** Allows environment-specific configuration of the service without requiring code changes.  
**Logic Description:** This file will have a JSON structure with top-level keys like `Serilog`, `ConnectionStrings`, and `NotificationSettings`. The `NotificationSettings` object will contain nested objects for `EmailSettings` (with SMTP details) and `SmsSettings` (with Twilio SID, Token, etc.). Sensitive values should be managed via user secrets or environment variables in production.  
**Documentation:**
    
    - **Summary:** Provides the primary configuration values for the Notification Service application.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableEmailChannel
  - EnableSmsChannel
  
- **Database Configs:**
  
  


---

