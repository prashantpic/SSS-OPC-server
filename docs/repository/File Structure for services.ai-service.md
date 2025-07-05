# Specification

# 1. Files

- **Path:** src/services/ai-service/AiService.sln  
**Description:** Visual Studio solution file for the AI Service and its class libraries.  
**Template:** C# Solution  
**Dependency Level:** 0  
**Name:** AiService  
**Type:** Solution  
**Relative Path:**   
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the projects and their dependencies for the AI microservice.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** The top-level solution file that groups all related C# projects for the AI microservice.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/services/ai-service/Core/AiService.Domain/AiService.Domain.csproj  
**Description:** Project file for the Domain layer, containing core business logic and types. It has no dependencies on other layers.  
**Template:** C# Class Library  
**Dependency Level:** 1  
**Name:** AiService.Domain  
**Type:** Project  
**Relative Path:** Core/AiService.Domain  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - DomainModel
    - Aggregate
    - Entity
    - ValueObject
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To define the core, technology-agnostic business logic, rules, and models of the AI domain.  
**Logic Description:** This project contains the pure domain model. It defines aggregates like AiModel, entities, value objects, and repository interfaces. It must not reference any infrastructure-specific packages like Entity Framework or specific cloud SDKs.  
**Documentation:**
    
    - **Summary:** Defines the heart of the AI service, its entities, aggregates, and the contracts (interfaces) for data persistence and domain services.
    
**Namespace:** AiService.Domain  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/services/ai-service/Core/AiService.Domain/Aggregates/AiModel/AiModel.cs  
**Description:** The aggregate root for an AI Model, encapsulating its state, version, and lifecycle.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** AiModel  
**Type:** AggregateRoot  
**Relative Path:** Core/AiService.Domain/Aggregates/AiModel  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - Aggregate
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Name  
**Type:** string  
**Attributes:** public  
    - **Name:** Version  
**Type:** string  
**Attributes:** public  
    - **Name:** ModelType  
**Type:** ModelType  
**Attributes:** public  
    - **Name:** Framework  
**Type:** string  
**Attributes:** public  
    - **Name:** Status  
**Type:** ModelStatus  
**Attributes:** public  
    - **Name:** Artifacts  
**Type:** IReadOnlyCollection<ModelArtifact>  
**Attributes:** private|readonly  
    - **Name:** PerformanceHistory  
**Type:** IReadOnlyCollection<ModelPerformanceLog>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Register  
**Parameters:**
    
    - string name
    - string version
    - ModelType modelType
    - string framework
    
**Return Type:** AiModel  
**Attributes:** public|static  
    - **Name:** Deploy  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** LogPerformance  
**Parameters:**
    
    - DateTime timestamp
    - decimal score
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** CheckForPerformanceDrift  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - AI Model Lifecycle Management
    
**Requirement Ids:**
    
    - REQ-7-004
    
**Purpose:** Represents a single AI model and its associated metadata and business rules for deployment and versioning.  
**Logic Description:** This class is the root of the AiModel aggregate. It enforces invariants, such as a model must have a version and a type. It contains methods to change the model's state, like deploying it or logging its performance, which might raise domain events.  
**Documentation:**
    
    - **Summary:** Manages the lifecycle of an AI model, including registration, deployment, and performance tracking. Acts as a consistency boundary for all model-related operations.
    
**Namespace:** AiService.Domain.Aggregates.AiModel  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/services/ai-service/Core/AiService.Domain/Aggregates/AiModel/ModelArtifact.cs  
**Description:** Represents a physical model file artifact, like an ONNX file, associated with an AiModel.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** ModelArtifact  
**Type:** Entity  
**Relative Path:** Core/AiService.Domain/Aggregates/AiModel  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - Entity
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** StoragePath  
**Type:** string  
**Attributes:** public  
    - **Name:** Checksum  
**Type:** string  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-001
    
**Purpose:** To track the location and integrity of a trained model's file.  
**Logic Description:** A simple entity that holds information about a model's stored file, including its path in blob storage and a checksum to verify its integrity.  
**Documentation:**
    
    - **Summary:** An entity within the AiModel aggregate that represents a specific file artifact of the model.
    
**Namespace:** AiService.Domain.Aggregates.AiModel  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/services/ai-service/Core/AiService.Domain/Enums/ModelType.cs  
**Description:** Enumeration for the different types of AI models supported by the service.  
**Template:** C# Enum  
**Dependency Level:** 2  
**Name:** ModelType  
**Type:** Enum  
**Relative Path:** Core/AiService.Domain/Enums  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** PredictiveMaintenance  
**Type:** enum  
**Attributes:**   
    - **Name:** AnomalyDetection  
**Type:** enum  
**Attributes:**   
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-001
    - REQ-7-008
    
**Purpose:** To provide a strongly-typed classification for AI models.  
**Logic Description:** Defines the supported categories of AI models, such as PredictiveMaintenance and AnomalyDetection.  
**Documentation:**
    
    - **Summary:** A standard C# enum to classify AI models.
    
**Namespace:** AiService.Domain.Enums  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/services/ai-service/Core/AiService.Domain/Interfaces/Repositories/IAiModelRepository.cs  
**Description:** Interface defining the contract for data access operations for the AiModel aggregate.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IAiModelRepository  
**Type:** RepositoryInterface  
**Relative Path:** Core/AiService.Domain/Interfaces/Repositories  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - RepositoryPattern
    - DependencyInversion
    
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
    
**Purpose:** To abstract the persistence mechanism for AI models from the domain logic.  
**Logic Description:** Defines the standard CRUD-like operations needed by the application layer to manage the AiModel aggregate. The implementation will be in the Infrastructure layer.  
**Documentation:**
    
    - **Summary:** Contract for storing and retrieving AiModel aggregates.
    
**Namespace:** AiService.Domain.Interfaces.Repositories  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/services/ai-service/Core/AiService.Application/AiService.Application.csproj  
**Description:** Project file for the Application layer, orchestrating domain logic and use cases.  
**Template:** C# Class Library  
**Dependency Level:** 2  
**Name:** AiService.Application  
**Type:** Project  
**Relative Path:** Core/AiService.Application  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    - DTO
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To contain the application-specific business logic, defining and handling commands and queries.  
**Logic Description:** This project depends on the Domain layer. It uses a mediator pattern (like MediatR) to handle commands and queries, orchestrating calls to domain entities and repository interfaces. It defines DTOs for data transfer.  
**Documentation:**
    
    - **Summary:** The application layer that orchestrates use cases and connects the API layer with the domain model.
    
**Namespace:** AiService.Application  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Core/AiService.Application/Features/PredictiveMaintenance/Queries/GetMaintenancePrediction/GetMaintenancePredictionQuery.cs  
**Description:** A CQRS query to request a maintenance prediction for a given set of input data.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** GetMaintenancePredictionQuery  
**Type:** Query  
**Relative Path:** Core/AiService.Application/Features/PredictiveMaintenance/Queries/GetMaintenancePrediction  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** ModelId  
**Type:** Guid  
**Attributes:** public  
    - **Name:** InputData  
**Type:** Dictionary<string, float>  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Predictive Maintenance Execution
    
**Requirement Ids:**
    
    - REQ-7-001
    
**Purpose:** Represents a request to execute a predictive maintenance model and get a forecast.  
**Logic Description:** A simple data-carrying class that represents a query. It will be handled by a corresponding QueryHandler. It implements IRequest from MediatR, specifying the response type (PredictionResultDto).  
**Documentation:**
    
    - **Summary:** Defines the input parameters needed to generate a maintenance prediction.
    
**Namespace:** AiService.Application.Features.PredictiveMaintenance.Queries.GetMaintenancePrediction  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Core/AiService.Application/Features/PredictiveMaintenance/Queries/GetMaintenancePrediction/GetMaintenancePredictionQueryHandler.cs  
**Description:** Handles the GetMaintenancePredictionQuery, orchestrating model execution.  
**Template:** C# Class  
**Dependency Level:** 4  
**Name:** GetMaintenancePredictionQueryHandler  
**Type:** QueryHandler  
**Relative Path:** Core/AiService.Application/Features/PredictiveMaintenance/Queries/GetMaintenancePrediction  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _predictionEngine  
**Type:** IPredictionEngine  
**Attributes:** private|readonly  
    - **Name:** _modelRepository  
**Type:** IAiModelRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - GetMaintenancePredictionQuery request
    - CancellationToken cancellationToken
    
**Return Type:** Task<PredictionResultDto>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Predictive Maintenance Execution
    
**Requirement Ids:**
    
    - REQ-7-001
    
**Purpose:** To process prediction requests by fetching model details and invoking the ML engine.  
**Logic Description:** This handler fetches the specified AI model via the repository, validates its type and status, retrieves the model artifact path, invokes the prediction engine (implemented in Infrastructure) with the input data, and returns the result as a DTO.  
**Documentation:**
    
    - **Summary:** The handler responsible for orchestrating the execution of a predictive maintenance model.
    
**Namespace:** AiService.Application.Features.PredictiveMaintenance.Queries.GetMaintenancePrediction  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Core/AiService.Application/Features/AnomalyDetection/Commands/DetectAnomalies/DetectAnomaliesCommand.cs  
**Description:** A command to process a data stream or batch for anomaly detection.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** DetectAnomaliesCommand  
**Type:** Command  
**Relative Path:** Core/AiService.Application/Features/AnomalyDetection/Commands/DetectAnomalies  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** ModelId  
**Type:** Guid  
**Attributes:** public  
    - **Name:** RealTimeDataPoints  
**Type:** IEnumerable<DataPointDto>  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Anomaly Detection
    
**Requirement Ids:**
    
    - REQ-7-008
    
**Purpose:** Represents a request to run an anomaly detection model on a set of data.  
**Logic Description:** A data-carrying class for an anomaly detection request. It contains the model to use and the data to be analyzed.  
**Documentation:**
    
    - **Summary:** Defines the input data for an anomaly detection task.
    
**Namespace:** AiService.Application.Features.AnomalyDetection.Commands.DetectAnomalies  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Core/AiService.Application/Features/AnomalyDetection/Commands/DetectAnomalies/DetectAnomaliesCommandHandler.cs  
**Description:** Handles the anomaly detection command.  
**Template:** C# Class  
**Dependency Level:** 4  
**Name:** DetectAnomaliesCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Core/AiService.Application/Features/AnomalyDetection/Commands/DetectAnomalies  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _anomalyDetectionEngine  
**Type:** IAnomalyDetectionEngine  
**Attributes:** private|readonly  
    - **Name:** _mediator  
**Type:** IMediator  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - DetectAnomaliesCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<AnomalyDto>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Anomaly Detection
    
**Requirement Ids:**
    
    - REQ-7-008
    
**Purpose:** To orchestrate the anomaly detection process and publish events for found anomalies.  
**Logic Description:** This handler invokes the anomaly detection engine (from Infrastructure) to process the data. If any anomalies are found, it will publish an `AnomalyDetectedEvent` via the mediator so other parts of the system can react (e.g., send notifications).  
**Documentation:**
    
    - **Summary:** Processes data to find anomalies and notifies the system if any are detected.
    
**Namespace:** AiService.Application.Features.AnomalyDetection.Commands.DetectAnomalies  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Core/AiService.Application/Features/Nlq/Queries/ProcessNlq/ProcessNlqQuery.cs  
**Description:** Represents a user's natural language query to be processed.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** ProcessNlqQuery  
**Type:** Query  
**Relative Path:** Core/AiService.Application/Features/Nlq/Queries/ProcessNlq  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** QueryText  
**Type:** string  
**Attributes:** public  
    - **Name:** UserId  
**Type:** Guid  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Natural Language Querying
    
**Requirement Ids:**
    
    - REQ-7-013
    
**Purpose:** To encapsulate a natural language query for processing.  
**Logic Description:** A simple DTO-like class that holds the text of the user's query.  
**Documentation:**
    
    - **Summary:** Input for the NLQ processing feature.
    
**Namespace:** AiService.Application.Features.Nlq.Queries.ProcessNlq  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Core/AiService.Application/Features/Nlq/Queries/ProcessNlq/ProcessNlqQueryHandler.cs  
**Description:** Handles the processing of natural language queries.  
**Template:** C# Class  
**Dependency Level:** 4  
**Name:** ProcessNlqQueryHandler  
**Type:** QueryHandler  
**Relative Path:** Core/AiService.Application/Features/Nlq/Queries/ProcessNlq  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _nlpServiceProvider  
**Type:** INlpServiceProvider  
**Attributes:** private|readonly  
    - **Name:** _dataServiceClient  
**Type:** IDataServiceClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - ProcessNlqQuery request
    - CancellationToken cancellationToken
    
**Return Type:** Task<NlqResultDto>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Natural Language Querying
    
**Requirement Ids:**
    
    - REQ-7-013
    
**Purpose:** To translate natural language into structured data queries.  
**Logic Description:** This handler sends the query text to an external NLP service (via the INlpServiceProvider interface). It receives back extracted intents and entities. It then uses this structured information to formulate a query against the Data Service (via IDataServiceClient) to fetch the actual data the user requested.  
**Documentation:**
    
    - **Summary:** Orchestrates NLQ processing by integrating with NLP and Data services.
    
**Namespace:** AiService.Application.Features.Nlq.Queries.ProcessNlq  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Core/AiService.Application/Interfaces/Infrastructure/IModelArtifactStorage.cs  
**Description:** Interface for storing and retrieving AI model file artifacts.  
**Template:** C# Interface  
**Dependency Level:** 3  
**Name:** IModelArtifactStorage  
**Type:** Interface  
**Relative Path:** Core/AiService.Application/Interfaces/Infrastructure  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** UploadModelAsync  
**Parameters:**
    
    - Stream modelStream
    - string fileName
    
**Return Type:** Task<string>  
**Attributes:** public  
    - **Name:** GetModelStreamAsync  
**Parameters:**
    
    - string storagePath
    
**Return Type:** Task<Stream>  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-004
    
**Purpose:** To abstract the underlying storage mechanism (e.g., local disk, cloud blob storage) for model files.  
**Logic Description:** Defines methods to upload a model file from a stream and retrieve a model file as a stream, using a storage path identifier. The implementation will handle the specifics of interacting with Azure Blob Storage, AWS S3, or a local file system.  
**Documentation:**
    
    - **Summary:** Provides a contract for interacting with a blob/file storage system for AI model artifacts.
    
**Namespace:** AiService.Application.Interfaces.Infrastructure  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Core/AiService.Application/Interfaces/Infrastructure/IMlopsPlatformClient.cs  
**Description:** Interface for interacting with an MLOps platform like MLflow or Azure ML.  
**Template:** C# Interface  
**Dependency Level:** 3  
**Name:** IMlopsPlatformClient  
**Type:** Interface  
**Relative Path:** Core/AiService.Application/Interfaces/Infrastructure  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** LogModelPerformanceAsync  
**Parameters:**
    
    - string modelName
    - stringmodelVersion
    - Dictionary<string, double> metrics
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** TriggerRetrainingPipelineAsync  
**Parameters:**
    
    - string modelName
    
**Return Type:** Task<string>  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-004
    
**Purpose:** To abstract the communication with external MLOps platforms for model tracking and lifecycle management.  
**Logic Description:** Defines methods to log model performance metrics and trigger retraining pipelines. The implementation in the Infrastructure layer will contain the specific SDK calls for the chosen MLOps platform.  
**Documentation:**
    
    - **Summary:** Contract for managing the model lifecycle through an external MLOps platform.
    
**Namespace:** AiService.Application.Interfaces.Infrastructure  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/services/ai-service/Infrastructure/AiService.Infrastructure/AiService.Infrastructure.csproj  
**Description:** Project file for the Infrastructure layer, containing concrete implementations of interfaces defined in the Application layer.  
**Template:** C# Class Library  
**Dependency Level:** 3  
**Name:** AiService.Infrastructure  
**Type:** Project  
**Relative Path:** Infrastructure/AiService.Infrastructure  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - RepositoryPattern
    - Adapter
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To provide concrete implementations for data persistence, external service clients, and other infrastructure concerns.  
**Logic Description:** This project depends on the Application layer. It implements repository and service client interfaces using technologies like Entity Framework Core, gRPC clients, cloud SDKs, and ML runtimes like ONNX Runtime.  
**Documentation:**
    
    - **Summary:** Implements data access, machine learning execution, and external service communication.
    
**Namespace:** AiService.Infrastructure  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/services/ai-service/Infrastructure/AiService.Infrastructure/MachineLearning/Onnx/OnnxPredictionEngine.cs  
**Description:** Implements a prediction engine using the ONNX Runtime for executing models.  
**Template:** C# Class  
**Dependency Level:** 4  
**Name:** OnnxPredictionEngine  
**Type:** Service  
**Relative Path:** Infrastructure/AiService.Infrastructure/MachineLearning/Onnx  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _modelCache  
**Type:** MemoryCache  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** RunPredictionAsync  
**Parameters:**
    
    - Stream modelStream
    - Dictionary<string, float> inputData
    
**Return Type:** Task<PredictionResultDto>  
**Attributes:** public  
    
**Implemented Features:**
    
    - AI Model Execution
    
**Requirement Ids:**
    
    - REQ-7-001
    
**Purpose:** To provide a concrete implementation for running inference on ONNX models.  
**Logic Description:** This class will use the Microsoft.ML.OnnxRuntime library. It will load a model from a stream, prepare the input tensor from the provided data dictionary, run the inference session, and parse the output tensor into a result DTO. It will include caching for loaded models to improve performance.  
**Documentation:**
    
    - **Summary:** Concrete implementation for model inference using ONNX Runtime.
    
**Namespace:** AiService.Infrastructure.MachineLearning.Onnx  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/services/ai-service/Infrastructure/AiService.Infrastructure/ExternalServices/DataService/DataServiceClient.cs  
**Description:** A gRPC client for communicating with the external Data Service.  
**Template:** C# Class  
**Dependency Level:** 4  
**Name:** DataServiceClient  
**Type:** ServiceClient  
**Relative Path:** Infrastructure/AiService.Infrastructure/ExternalServices/DataService  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - Client
    
**Members:**
    
    - **Name:** _grpcClient  
**Type:** DataService.DataServiceClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetHistoricalDataForModelAsync  
**Parameters:**
    
    - Guid modelId
    - DateTime start
    - DateTime end
    
**Return Type:** Task<IEnumerable<DataPointDto>>  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To abstract the communication required to fetch historical data needed for model training or inference.  
**Logic Description:** This class implements the IDataServiceClient interface defined in the Application layer. It uses a generated gRPC client to make calls to the Data Service's gRPC endpoint, requesting the necessary time-series data for a given model.  
**Documentation:**
    
    - **Summary:** Handles communication with the Data Service to retrieve historical data.
    
**Namespace:** AiService.Infrastructure.ExternalServices.DataService  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/services/ai-service/Presentation/AiService.Api/AiService.Api.csproj  
**Description:** The main executable project for the AI Service, hosting the ASP.NET Core application.  
**Template:** C# Web API  
**Dependency Level:** 4  
**Name:** AiService.Api  
**Type:** Project  
**Relative Path:** Presentation/AiService.Api  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - RESTful API
    - gRPC Service
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To expose the service's functionality via REST and gRPC endpoints and to host the application.  
**Logic Description:** This project depends on the Application and Infrastructure layers. It sets up the ASP.NET Core pipeline, configures dependency injection, registers controllers and gRPC services, and runs the web host.  
**Documentation:**
    
    - **Summary:** The entry point and API host for the AI microservice.
    
**Namespace:** AiService.Api  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/services/ai-service/Presentation/AiService.Api/Controllers/PredictionsController.cs  
**Description:** REST API controller for handling predictive maintenance requests.  
**Template:** C# Controller  
**Dependency Level:** 5  
**Name:** PredictionsController  
**Type:** Controller  
**Relative Path:** Presentation/AiService.Api/Controllers  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - RESTful API
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetPrediction  
**Parameters:**
    
    - Guid modelId
    - PredictionRequestDto requestDto
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Predictive Maintenance API
    
**Requirement Ids:**
    
    - REQ-7-001
    
**Purpose:** To provide a public HTTP endpoint for requesting maintenance predictions.  
**Logic Description:** This controller receives HTTP POST requests. It validates the incoming DTO, creates a GetMaintenancePredictionQuery, and sends it to the mediator. It then formats the handler's response (either a result or an error) into an appropriate HTTP response.  
**Documentation:**
    
    - **Summary:** Exposes the predictive maintenance feature via a RESTful API.
    
**Namespace:** AiService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/services/ai-service/Presentation/AiService.Api/Controllers/NlqController.cs  
**Description:** REST API controller for handling Natural Language Query requests.  
**Template:** C# Controller  
**Dependency Level:** 5  
**Name:** NlqController  
**Type:** Controller  
**Relative Path:** Presentation/AiService.Api/Controllers  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - RESTful API
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ProcessQuery  
**Parameters:**
    
    - NlqRequestDto requestDto
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - NLQ API
    
**Requirement Ids:**
    
    - REQ-7-013
    
**Purpose:** To provide a public HTTP endpoint for submitting natural language queries.  
**Logic Description:** Receives a query string, creates a ProcessNlqQuery, sends it via the mediator, and returns the structured result or clarification question from the handler.  
**Documentation:**
    
    - **Summary:** Exposes the Natural Language Query feature via a RESTful API.
    
**Namespace:** AiService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/services/ai-service/Presentation/AiService.Api/Extensions/ServiceCollectionExtensions.cs  
**Description:** Extension methods for IServiceCollection to set up dependency injection for all layers.  
**Template:** C# Class  
**Dependency Level:** 5  
**Name:** ServiceCollectionExtensions  
**Type:** Configuration  
**Relative Path:** Presentation/AiService.Api/Extensions  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddApplicationServices  
**Parameters:**
    
    - IServiceCollection services
    
**Return Type:** IServiceCollection  
**Attributes:** public|static  
    - **Name:** AddInfrastructureServices  
**Parameters:**
    
    - IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public|static  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To centralize and organize the registration of services in the DI container.  
**Logic Description:** Contains extension methods to register all the services, repositories, and clients from the Application and Infrastructure layers. This keeps the Program.cs file clean and separates DI configuration by layer.  
**Documentation:**
    
    - **Summary:** Sets up all dependency injection registrations for the AI service.
    
**Namespace:** AiService.Api.Extensions  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/services/ai-service/Presentation/AiService.Api/Program.cs  
**Description:** The main entry point for the AI Service application.  
**Template:** C# Program  
**Dependency Level:** 6  
**Name:** Program  
**Type:** EntryPoint  
**Relative Path:** Presentation/AiService.Api  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public|static  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To build and run the ASP.NET Core web host.  
**Logic Description:** This file creates the WebApplicationBuilder, configures services by calling the extension methods in ServiceCollectionExtensions, configures the HTTP request pipeline (e.g., routing, authentication, gRPC, exception handling middleware), and runs the application.  
**Documentation:**
    
    - **Summary:** Bootstraps and runs the AI microservice application.
    
**Namespace:** AiService.Api  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/services/ai-service/Presentation/AiService.Api/appsettings.json  
**Description:** Configuration file for the AI service.  
**Template:** JSON  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:** Presentation/AiService.Api  
**Repository Id:** REPO-SAP-005  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To provide environment-independent configuration settings for the service.  
**Logic Description:** Contains settings for logging levels, allowed hosts, connection strings for databases, endpoint URLs for external services (Data Service, MLOps, NLP providers), and other application settings. Sensitive values should be overridden by user secrets or environment variables.  
**Documentation:**
    
    - **Summary:** JSON configuration file for the AI Service.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnablePredictiveMaintenance
  - EnableAnomalyDetection
  - EnableNlqProcessing
  - UseAzureNlpService
  - UseGoogleNlpService
  
- **Database Configs:**
  
  - ConnectionStrings:AiServiceDb
  - Storage:BlobStorageConnectionString
  - ExternalServices:DataServiceUrl
  - ExternalServices:MlopsPlatformUrl
  - NlpProviders:Azure:Endpoint
  - NlpProviders:Azure:ApiKey
  - NlpProviders:Google:CredentialsJson
  


---

