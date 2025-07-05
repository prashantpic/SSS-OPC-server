# Specification

# 1. Files

- **Path:** src/Services/AI/AI.sln  
**Description:** The solution file for the AI microservice, grouping all related projects.  
**Template:** .NET Solution  
**Dependency Level:** 0  
**Name:** AI  
**Type:** Solution  
**Relative Path:**   
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the projects that constitute the AI service solution.  
**Logic Description:** This file is managed by Visual Studio or the 'dotnet sln' command and lists all projects (API, Application, Domain, Infrastructure).  
**Documentation:**
    
    - **Summary:** Visual Studio Solution File.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services/AI/AI.API/AI.API.csproj  
**Description:** The project file for the AI.API layer, defining its dependencies on other projects and NuGet packages.  
**Template:** .NET Project  
**Dependency Level:** 1  
**Name:** AI.API  
**Type:** Project  
**Relative Path:** AI.API  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Specifies the build settings and dependencies for the API project, including ASP.NET Core, gRPC, and references to Application and Infrastructure projects.  
**Logic Description:** This XML file includes package references for ASP.NET Core, gRPC, Swashbuckle for OpenAPI, and project references to AI.Application and AI.Infrastructure to enable dependency injection.  
**Documentation:**
    
    - **Summary:** .NET C# Project file.
    
**Namespace:** Opc.System.Services.AI.API  
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services/AI/AI.API/Program.cs  
**Description:** The main entry point for the AI microservice. This file configures and launches the ASP.NET Core host.  
**Template:** C# Program  
**Dependency Level:** 2  
**Name:** Program  
**Type:** EntryPoint  
**Relative Path:** AI.API  
**Repository Id:** REPO-SAP-005  
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
    
**Requirement Ids:**
    
    
**Purpose:** Initializes the web application, configures the dependency injection container, and sets up the HTTP request processing pipeline including routing, authentication, and gRPC services.  
**Logic Description:** Configures the WebApplicationBuilder by registering services from the Application and Infrastructure layers using their respective extension methods. Sets up the HTTP pipeline, mapping controllers, gRPC services, health checks, and Swagger/OpenAPI endpoints. Finally, runs the application.  
**Documentation:**
    
    - **Summary:** Bootstraps and runs the AI microservice.
    
**Namespace:** Opc.System.Services.AI.API  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/AI/AI.API/appsettings.json  
**Description:** Configuration file for the AI service, containing settings for external services, connection strings, logging, and feature toggles.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:** AI.API  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides a centralized location for application configuration that can be overridden by environment-specific files (e.g., appsettings.Development.json) or environment variables.  
**Logic Description:** Contains key-value pairs for Logging levels, allowed hosts, connection strings for the Data Service, endpoints for MLOps platforms, API keys for NLP services, and configurations for message queues and model storage.  
**Documentation:**
    
    - **Summary:** Primary application configuration file.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Services/AI/AI.API/Controllers/ModelManagementController.cs  
**Description:** REST API controller for managing the lifecycle of AI models.  
**Template:** C# Controller  
**Dependency Level:** 2  
**Name:** ModelManagementController  
**Type:** Controller  
**Relative Path:** AI.API/Controllers  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** UploadModel  
**Parameters:**
    
    - IFormFile file
    - string modelType
    - string version
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** DeployModel  
**Parameters:**
    
    - Guid modelId
    - string environment
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetModelDetails  
**Parameters:**
    
    - Guid modelId
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetModelPerformance  
**Parameters:**
    
    - Guid modelId
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - ModelUpload
    - ModelDeployment
    - ModelMonitoring
    
**Requirement Ids:**
    
    - REQ-7-004
    - REQ-7-005
    - REQ-7-010
    
**Purpose:** Exposes endpoints for uploading, deploying, and monitoring AI models.  
**Logic Description:** Receives HTTP requests for model management operations. Validates input and creates corresponding commands (e.g., UploadModelCommand, DeployModelCommand) or queries. Sends the command/query to the MediatR pipeline for processing by the application layer and returns an appropriate HTTP response.  
**Documentation:**
    
    - **Summary:** Manages AI model lifecycle via REST API.
    
**Namespace:** Opc.System.Services.AI.API.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/AI/AI.API/Controllers/AnalysisController.cs  
**Description:** REST API controller for running AI analysis like predictive maintenance and anomaly detection.  
**Template:** C# Controller  
**Dependency Level:** 2  
**Name:** AnalysisController  
**Type:** Controller  
**Relative Path:** AI.API/Controllers  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetMaintenancePrediction  
**Parameters:**
    
    - PredictionRequestDto request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** DetectAnomalies  
**Parameters:**
    
    - AnomalyDetectionRequestDto request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - PredictiveMaintenanceExecution
    - AnomalyDetectionExecution
    
**Requirement Ids:**
    
    - REQ-7-001
    - REQ-7-008
    
**Purpose:** Exposes endpoints for triggering AI-driven analysis of operational data.  
**Logic Description:** Accepts HTTP requests containing data for analysis. Creates and dispatches commands like `RunPredictionCommand` or `DetectAnomaliesCommand` to the application layer via MediatR. Returns the analysis results (e.g., prediction, detected anomalies) as an HTTP response.  
**Documentation:**
    
    - **Summary:** Handles AI analysis requests.
    
**Namespace:** Opc.System.Services.AI.API.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/AI/AI.API/Controllers/NlqController.cs  
**Description:** REST API controller for processing Natural Language Queries.  
**Template:** C# Controller  
**Dependency Level:** 2  
**Name:** NlqController  
**Type:** Controller  
**Relative Path:** AI.API/Controllers  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ProcessQuery  
**Parameters:**
    
    - NlqRequestDto request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** CreateAlias  
**Parameters:**
    
    - CreateNlqAliasDto request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - NaturalLanguageQueryProcessing
    - NLQ Alias Management
    
**Requirement Ids:**
    
    - REQ-7-013
    - REQ-7-015
    
**Purpose:** Provides endpoints for users to submit natural language queries and manage tag aliases.  
**Logic Description:** Receives text-based queries via HTTP. Creates a `ProcessNlqCommand` and sends it for processing. Also handles requests to create or update aliases for OPC tags to improve NLQ accuracy. Returns the query result or confirmation of alias creation.  
**Documentation:**
    
    - **Summary:** Processes natural language queries via REST API.
    
**Namespace:** Opc.System.Services.AI.API.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/AI/AI.API/Controllers/FeedbackController.cs  
**Description:** REST API controller for submitting user feedback on AI results.  
**Template:** C# Controller  
**Dependency Level:** 2  
**Name:** FeedbackController  
**Type:** Controller  
**Relative Path:** AI.API/Controllers  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SubmitPredictionFeedback  
**Parameters:**
    
    - PredictionFeedbackDto feedback
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** LabelAnomaly  
**Parameters:**
    
    - AnomalyLabelDto label
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - AI Feedback Collection
    
**Requirement Ids:**
    
    - REQ-7-005
    - REQ-7-011
    
**Purpose:** Exposes endpoints for users to provide feedback on predictions and label detected anomalies, which is crucial for model retraining.  
**Logic Description:** Accepts user feedback via HTTP POST requests. Packages the feedback into a `SubmitFeedbackCommand` and dispatches it to the application layer. This feedback is then stored and used to evaluate model performance and for future retraining datasets.  
**Documentation:**
    
    - **Summary:** Handles user feedback on AI model outputs.
    
**Namespace:** Opc.System.Services.AI.API.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/AI/AI.Application/DependencyInjection.cs  
**Description:** Extension method for IServiceCollection to register Application layer services.  
**Template:** C# Extension Method  
**Dependency Level:** 1  
**Name:** DependencyInjection  
**Type:** Configuration  
**Relative Path:** AI.Application  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddApplicationServices  
**Parameters:**
    
    - this IServiceCollection services
    
**Return Type:** IServiceCollection  
**Attributes:** public|static  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Encapsulates the registration of all Application layer dependencies, such as MediatR, FluentValidation, and AutoMapper.  
**Logic Description:** This static class provides an extension method that scans the assembly for MediatR handlers and FluentValidation validators and registers them with the DI container. It also registers any AutoMapper profiles.  
**Documentation:**
    
    - **Summary:** Sets up DI for the Application layer.
    
**Namespace:** Opc.System.Services.AI.Application  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Services/AI/AI.Application/Features/Nlq/ProcessNlqCommandHandler.cs  
**Description:** Handles the logic for processing a natural language query.  
**Template:** C# Handler  
**Dependency Level:** 2  
**Name:** ProcessNlqCommandHandler  
**Type:** Handler  
**Relative Path:** AI.Application/Features/Nlq  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _nlpServiceFactory  
**Type:** INlpServiceFactory  
**Attributes:** private|readonly  
    - **Name:** _aliasRepository  
**Type:** INlqAliasRepository  
**Attributes:** private|readonly  
    - **Name:** _dataServiceClient  
**Type:** IDataServiceClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - ProcessNlqCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<NlqResultDto>  
**Attributes:** public  
    
**Implemented Features:**
    
    - NaturalLanguageQueryProcessing
    
**Requirement Ids:**
    
    - REQ-7-013
    - REQ-7-014
    - REQ-7-015
    - REQ-7-016
    
**Purpose:** Orchestrates the process of interpreting and executing a natural language query.  
**Logic Description:** Receives a ProcessNlqCommand. Uses the NlpServiceFactory to get the configured NLP provider. Sends the query text to the provider to extract intent and entities. Uses the NlqAliasRepository to resolve any user-defined aliases. Based on the intent (e.g., 'get value', 'show trend'), it calls the DataServiceClient to fetch the required data and formats the result.  
**Documentation:**
    
    - **Summary:** Orchestrates NLQ processing.
    
**Namespace:** Opc.System.Services.AI.Application.Features.Nlq  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/AI/AI.Application/Features/PredictiveMaintenance/RunPredictionCommandHandler.cs  
**Description:** Handles the command to run a predictive maintenance analysis.  
**Template:** C# Handler  
**Dependency Level:** 2  
**Name:** RunPredictionCommandHandler  
**Type:** Handler  
**Relative Path:** AI.Application/Features/PredictiveMaintenance  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _modelRepository  
**Type:** IAiModelRepository  
**Attributes:** private|readonly  
    - **Name:** _modelRunner  
**Type:** IModelRunner  
**Attributes:** private|readonly  
    - **Name:** _dataServiceClient  
**Type:** IDataServiceClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - RunPredictionCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<PredictionResultDto>  
**Attributes:** public  
    
**Implemented Features:**
    
    - PredictiveMaintenanceExecution
    
**Requirement Ids:**
    
    - REQ-7-001
    - REQ-7-002
    - REQ-7-003
    
**Purpose:** Orchestrates the execution of a predictive maintenance model.  
**Logic Description:** Receives a RunPredictionCommand. Fetches the required historical data for the specified asset using the DataServiceClient. Retrieves the appropriate deployed AI model metadata from the IAiModelRepository. Loads the model artifact and runs inference using the IModelRunner service. Returns the prediction result.  
**Documentation:**
    
    - **Summary:** Handles a predictive maintenance analysis request.
    
**Namespace:** Opc.System.Services.AI.Application.Features.PredictiveMaintenance  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/AI/AI.Application/Features/AnomalyDetection/DetectAnomaliesCommandHandler.cs  
**Description:** Handles the command to run an anomaly detection analysis on a stream of data.  
**Template:** C# Handler  
**Dependency Level:** 2  
**Name:** DetectAnomaliesCommandHandler  
**Type:** Handler  
**Relative Path:** AI.Application/Features/AnomalyDetection  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _modelRepository  
**Type:** IAiModelRepository  
**Attributes:** private|readonly  
    - **Name:** _modelRunner  
**Type:** IModelRunner  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - DetectAnomaliesCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<List<AnomalyDto>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - AnomalyDetectionExecution
    
**Requirement Ids:**
    
    - REQ-7-008
    - REQ-7-009
    
**Purpose:** Orchestrates the execution of an anomaly detection model on real-time or historical data.  
**Logic Description:** Receives a DetectAnomaliesCommand containing a dataset. Retrieves the appropriate deployed anomaly detection model from the repository. Validates the input data against the model's requirements. Executes the model using the IModelRunner. If anomalies are detected, it creates AnomalyDto objects and returns them. It may also publish an `AnomalyDetectedEvent` to a message bus.  
**Documentation:**
    
    - **Summary:** Handles an anomaly detection request.
    
**Namespace:** Opc.System.Services.AI.Application.Features.AnomalyDetection  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/AI/AI.Application/Interfaces/IModelStore.cs  
**Description:** Defines the contract for storing and retrieving AI model binary artifacts.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IModelStore  
**Type:** Interface  
**Relative Path:** AI.Application/Interfaces  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SaveModelAsync  
**Parameters:**
    
    - string modelId
    - string version
    - Stream modelStream
    
**Return Type:** Task<string>  
**Attributes:** public  
    - **Name:** GetModelStreamAsync  
**Parameters:**
    
    - string modelId
    - string version
    
**Return Type:** Task<Stream>  
**Attributes:** public  
    - **Name:** DeleteModelAsync  
**Parameters:**
    
    - string modelId
    - string version
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-006
    
**Purpose:** Provides an abstraction for the physical storage of AI model files (e.g., ONNX files), decoupling the application logic from the specific storage technology (e.g., blob storage, file system).  
**Logic Description:** This interface declares methods for saving a model from a stream, retrieving a model as a stream, and deleting a model artifact. It uses identifiers to manage different models and their versions.  
**Documentation:**
    
    - **Summary:** Interface for AI model binary storage.
    
**Namespace:** Opc.System.Services.AI.Application.Interfaces  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Services/AI/AI.Domain/Aggregates/AiModel.cs  
**Description:** The AiModel aggregate root, representing a machine learning model and its lifecycle.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** AiModel  
**Type:** AggregateRoot  
**Relative Path:** AI.Domain/Aggregates  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - DomainDrivenDesign
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Name  
**Type:** string  
**Attributes:** public  
    - **Name:** CurrentVersion  
**Type:** ModelVersion  
**Attributes:** public  
    - **Name:** ModelType  
**Type:** ModelType  
**Attributes:** public  
    - **Name:** DeploymentStatus  
**Type:** DeploymentStatus  
**Attributes:** public  
    - **Name:** PerformanceMetrics  
**Type:** ModelPerformanceMetrics  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** Deploy  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** AddNewVersion  
**Parameters:**
    
    - string versionTag
    - string checksum
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** UpdatePerformance  
**Parameters:**
    
    - ModelPerformanceMetrics newMetrics
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Model Lifecycle Management
    
**Requirement Ids:**
    
    - REQ-7-004
    - REQ-7-010
    
**Purpose:** Encapsulates the state and behavior of an AI model, ensuring business rules (invariants) around its lifecycle are maintained.  
**Logic Description:** Contains methods to manage the state transitions of a model, such as deploying a new version or updating its performance metrics. These methods contain the business logic, like preventing a retired model from being deployed. It may raise domain events like `ModelDeployedEvent`.  
**Documentation:**
    
    - **Summary:** Represents an AI model as a DDD aggregate root.
    
**Namespace:** Opc.System.Services.AI.Domain.Aggregates  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/AI/AI.Domain/Interfaces/IAiModelRepository.cs  
**Description:** Interface defining the contract for data persistence operations for the AiModel aggregate.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IAiModelRepository  
**Type:** RepositoryInterface  
**Relative Path:** AI.Domain/Interfaces  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<AiModel>  
**Attributes:** public  
    - **Name:** AddAsync  
**Parameters:**
    
    - AiModel model
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** UpdateAsync  
**Parameters:**
    
    - AiModel model
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-004
    - REQ-7-010
    
**Purpose:** Abstracts the data storage mechanism for AiModel metadata, allowing the domain to remain persistence-ignorant.  
**Logic Description:** Defines the standard repository methods for finding, adding, and updating an AiModel aggregate. The implementation will reside in the Infrastructure layer and could interact with a relational database or a NoSQL document store.  
**Documentation:**
    
    - **Summary:** Contract for AiModel persistence.
    
**Namespace:** Opc.System.Services.AI.Domain.Interfaces  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Services/AI/AI.Infrastructure/DependencyInjection.cs  
**Description:** Extension method for IServiceCollection to register Infrastructure layer services.  
**Template:** C# Extension Method  
**Dependency Level:** 2  
**Name:** DependencyInjection  
**Type:** Configuration  
**Relative Path:** AI.Infrastructure  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddInfrastructureServices  
**Parameters:**
    
    - this IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public|static  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Encapsulates the registration of all Infrastructure layer dependencies, such as repository implementations, external service clients, and the ONNX model runner.  
**Logic Description:** This static class's extension method registers concrete implementations from the Infrastructure layer against the interfaces defined in the Application and Domain layers. For example, it registers `BlobModelStore` as `IModelStore` and `AiModelRepository` as `IAiModelRepository`. It also configures clients for NLP services and the Data Service gRPC client based on `appsettings.json`.  
**Documentation:**
    
    - **Summary:** Sets up DI for the Infrastructure layer.
    
**Namespace:** Opc.System.Services.AI.Infrastructure  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Services/AI/AI.Infrastructure/Stores/BlobModelStore.cs  
**Description:** Implements the IModelStore interface using Azure Blob Storage.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** BlobModelStore  
**Type:** Repository  
**Relative Path:** AI.Infrastructure/Stores  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    - **Name:** _blobServiceClient  
**Type:** BlobServiceClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SaveModelAsync  
**Parameters:**
    
    - string modelId
    - string version
    - Stream modelStream
    
**Return Type:** Task<string>  
**Attributes:** public  
    - **Name:** GetModelStreamAsync  
**Parameters:**
    
    - string modelId
    - string version
    
**Return Type:** Task<Stream>  
**Attributes:** public  
    
**Implemented Features:**
    
    - AI Model Artifact Storage
    
**Requirement Ids:**
    
    - REQ-7-006
    
**Purpose:** Provides a concrete implementation for storing and retrieving AI model files (e.g., .onnx files) in a cloud blob storage service.  
**Logic Description:** Uses the Azure Blob Storage SDK to interact with a specific container. The `SaveModelAsync` method uploads the provided stream to a blob named according to the model ID and version. `GetModelStreamAsync` downloads the corresponding blob.  
**Documentation:**
    
    - **Summary:** Implements IModelStore using Azure Blob Storage.
    
**Namespace:** Opc.System.Services.AI.Infrastructure.Stores  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Services/AI/AI.Infrastructure/Nlp/AzureNlpServiceProvider.cs  
**Description:** Implements the NLP service provider contract using Azure Cognitive Service for Language.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** AzureNlpServiceProvider  
**Type:** Service  
**Relative Path:** AI.Infrastructure/Nlp  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - AdapterPattern
    
**Members:**
    
    - **Name:** _client  
**Type:** QuestionAnsweringClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ExtractIntentAndEntitiesAsync  
**Parameters:**
    
    - string queryText
    
**Return Type:** Task<NlpResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - NLQ Intent and Entity Extraction
    
**Requirement Ids:**
    
    - REQ-7-014
    
**Purpose:** Adapts requests from the application layer to the specific API of the Azure Cognitive Service for Language.  
**Logic Description:** Uses the `Azure.AI.Language.QuestionAnswering` SDK to send the user's query text to the configured Azure service. It then parses the response from Azure to extract the identified intent and entities, mapping them to a common `NlpResult` model used by the application layer. This class will be instantiated with Polly policies for retries and circuit breaking.  
**Documentation:**
    
    - **Summary:** Integration with Azure's NLP service.
    
**Namespace:** Opc.System.Services.AI.Infrastructure.Nlp  
**Metadata:**
    
    - **Category:** Integration
    
- **Path:** src/Services/AI/AI.Infrastructure/Services/OnnxModelRunner.cs  
**Description:** A service responsible for executing ONNX models.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** OnnxModelRunner  
**Type:** Service  
**Relative Path:** AI.Infrastructure/Services  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _modelStore  
**Type:** IModelStore  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** RunPredictionAsync  
**Parameters:**
    
    - Guid modelId
    - string version
    - ModelInputData input
    
**Return Type:** Task<ModelOutputData>  
**Attributes:** public  
    
**Implemented Features:**
    
    - ONNX Model Execution
    
**Requirement Ids:**
    
    - REQ-7-001
    - REQ-7-003
    - REQ-7-008
    
**Purpose:** Provides the core functionality for loading an ONNX model from storage and running inference on it.  
**Logic Description:** Fetches the model stream from the IModelStore. Uses the `Microsoft.ML.OnnxRuntime` library to create an `InferenceSession` from the model stream. Prepares the input tensors from the provided `ModelInputData`. Runs the session and parses the output tensors into a common `ModelOutputData` format.  
**Documentation:**
    
    - **Summary:** Loads and executes ONNX models for inference.
    
**Namespace:** Opc.System.Services.AI.Infrastructure.Services  
**Metadata:**
    
    - **Category:** BusinessLogic
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - UseGoogleNlp
  - EnableModelExplainability
  - EnableCustomModelFineTuning
  
- **Database Configs:**
  
  - ConnectionStrings:DataServiceGrpc
  - Azure:CognitiveServices:Endpoint
  - Azure:CognitiveServices:ApiKey
  - Azure:Storage:ConnectionString
  - Azure:Storage:ModelContainerName
  - MlOps:Mlflow:TrackingUri
  


---

