# Specification

# 1. Files

- **Path:** src/Infrastructure/Data/Infrastructure.Data.csproj  
**Description:** The .NET 8 project file for the data access library. It lists all dependencies, such as Entity Framework Core, Npgsql for PostgreSQL, InfluxDB.Client, Azure.Storage.Blobs, and project references to the domain/shared kernel layer.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** Infrastructure.Data  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-001
    - REQ-DLP-002
    - REQ-DLP-003
    - REQ-DLP-005
    - REQ-DLP-008
    - REQ-DLP-009
    - REQ-DLP-024
    - REQ-DLP-025
    - REQ-CSVC-013
    - REQ-CSVC-019
    
**Purpose:** Defines the project structure, framework target, and all necessary NuGet and project dependencies for the data access layer.  
**Logic Description:** This file will be configured to target .NET 8. It will include PackageReference items for Npgsql.EntityFrameworkCore.PostgreSQL, InfluxDB.Client, Azure.Storage.Blobs, and potentially a ProjectReference to a shared domain model library.  
**Documentation:**
    
    - **Summary:** Specifies the build and dependency information for the Infrastructure.Data library, which provides a comprehensive data access solution for the entire server-side application.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Infrastructure/Data/DependencyInjection.cs  
**Description:** Provides an IServiceCollection extension method to register all data access services, repositories, and DbContexts with the application's dependency injection container.  
**Template:** C# Extension Method  
**Dependency Level:** 3  
**Name:** DependencyInjection  
**Type:** ServiceRegistration  
**Relative Path:** DependencyInjection  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddInfrastructureData  
**Parameters:**
    
    - this IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Service Registration
    
**Requirement Ids:**
    
    
**Purpose:** Simplifies the setup of the data access layer in the main application by providing a single point of registration for all its components.  
**Logic Description:** The AddInfrastructureData method will read connection strings and other settings from the IConfiguration object. It will register AppDbContext using AddDbContext, configure clients for Time-Series and Blob storage as singletons, and register all repository interfaces with their concrete implementations (e.g., services.AddScoped<IHistoricalDataRepository, HistoricalDataRepository>()).  
**Documentation:**
    
    - **Summary:** Defines extension methods for IServiceCollection to encapsulate the registration of all data access components, making the layer self-contained and easy to integrate.
    
**Namespace:** Opc.System.Infrastructure.Data  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Infrastructure/Data/Abstractions/IUnitOfWork.cs  
**Description:** Defines a contract for the Unit of Work pattern, allowing for atomic operations across multiple repositories within the relational database context.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IUnitOfWork  
**Type:** Interface  
**Relative Path:** Abstractions  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - UnitOfWork
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SaveChangesAsync  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task<int>  
**Attributes:**   
    
**Implemented Features:**
    
    - Transactional Integrity
    
**Requirement Ids:**
    
    - REQ-DLP-008
    
**Purpose:** To ensure data consistency by grouping multiple database operations into a single transaction.  
**Logic Description:** This interface provides a method to commit all changes made within a single business transaction. The implementation will wrap the DbContext.SaveChangesAsync method.  
**Documentation:**
    
    - **Summary:** A contract for managing transactions and persisting changes to the underlying relational data store atomically.
    
**Namespace:** Opc.System.Infrastructure.Data.Abstractions  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/Abstractions/IHistoricalDataRepository.cs  
**Description:** Interface defining operations for persisting and retrieving historical time-series data.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IHistoricalDataRepository  
**Type:** Interface  
**Relative Path:** Abstractions  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** IngestDataBatchAsync  
**Parameters:**
    
    - IEnumerable<HistoricalDataPoint> dataBatch
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** QueryDataAsync  
**Parameters:**
    
    - HistoricalDataQuery query
    
**Return Type:** Task<IEnumerable<HistoricalDataPoint>>  
**Attributes:**   
    
**Implemented Features:**
    
    - Historical Data Ingestion
    - Historical Data Querying
    
**Requirement Ids:**
    
    - REQ-DLP-001
    - REQ-DLP-002
    - REQ-DLP-003
    - REQ-CSVC-013
    
**Purpose:** Provides a contract for interacting with the time-series database for historical process values, abstracting away the specific database technology.  
**Logic Description:** Defines methods for batch ingestion of data points to optimize write performance and a flexible query method that takes a query object to support various filtering and aggregation scenarios.  
**Documentation:**
    
    - **Summary:** A contract for managing the lifecycle of historical data, including efficient ingestion and powerful querying capabilities.
    
**Namespace:** Opc.System.Infrastructure.Data.Abstractions  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/Abstractions/IAlarmEventRepository.cs  
**Description:** Interface defining operations for persisting and retrieving alarm and event time-series data.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IAlarmEventRepository  
**Type:** Interface  
**Relative Path:** Abstractions  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** IngestAlarmEventsAsync  
**Parameters:**
    
    - IEnumerable<AlarmEventPoint> alarmEvents
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** QueryAlarmsAsync  
**Parameters:**
    
    - AlarmEventQuery query
    
**Return Type:** Task<IEnumerable<AlarmEventPoint>>  
**Attributes:**   
    
**Implemented Features:**
    
    - Alarm Data Ingestion
    - Alarm Data Querying
    
**Requirement Ids:**
    
    - REQ-DLP-005
    - REQ-CSVC-019
    
**Purpose:** Provides a contract for interacting with the time-series database for alarm and event logs.  
**Logic Description:** Defines methods for writing alarm and event data and a query method to retrieve them based on time ranges, severity, or acknowledgment state.  
**Documentation:**
    
    - **Summary:** A contract for managing the storage and retrieval of alarm and event logs, supporting analysis and monitoring use cases.
    
**Namespace:** Opc.System.Infrastructure.Data.Abstractions  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/Abstractions/IAiArtifactRepository.cs  
**Description:** Interface defining operations for storing and retrieving unstructured AI artifacts, such as models and datasets.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IAiArtifactRepository  
**Type:** Interface  
**Relative Path:** Abstractions  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** UploadArtifactAsync  
**Parameters:**
    
    - string artifactName
    - Stream content
    
**Return Type:** Task<Uri>  
**Attributes:**   
    - **Name:** DownloadArtifactAsync  
**Parameters:**
    
    - string artifactName
    
**Return Type:** Task<Stream>  
**Attributes:**   
    - **Name:** DeleteArtifactAsync  
**Parameters:**
    
    - string artifactName
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    - AI Artifact Storage
    
**Requirement Ids:**
    
    - REQ-DLP-024
    
**Purpose:** Provides a contract for managing the lifecycle of AI model files and other large binary objects in a blob storage system.  
**Logic Description:** Defines basic blob operations: upload, download, and delete. The implementation will map these to the appropriate cloud storage SDK calls (e.g., Azure Blob Storage or AWS S3).  
**Documentation:**
    
    - **Summary:** A contract for interacting with a blob storage solution to manage large, unstructured AI-related files.
    
**Namespace:** Opc.System.Infrastructure.Data.Abstractions  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/Abstractions/IBlockchainLogRepository.cs  
**Description:** Interface defining operations for persisting metadata related to blockchain transactions.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IBlockchainLogRepository  
**Type:** Interface  
**Relative Path:** Abstractions  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** LogTransactionAsync  
**Parameters:**
    
    - BlockchainTransaction logEntry
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** GetTransactionByHashAsync  
**Parameters:**
    
    - string dataHash
    
**Return Type:** Task<BlockchainTransaction>  
**Attributes:**   
    
**Implemented Features:**
    
    - Blockchain Transaction Logging
    
**Requirement Ids:**
    
    - REQ-DLP-025
    
**Purpose:** Provides a contract for storing and retrieving metadata about data that has been immutably recorded on a blockchain.  
**Logic Description:** Defines methods to save a new transaction record and retrieve an existing one by its unique data hash. The implementation will use the relational database.  
**Documentation:**
    
    - **Summary:** A contract for managing the off-chain metadata associated with on-chain data integrity records.
    
**Namespace:** Opc.System.Infrastructure.Data.Abstractions  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/Relational/AppDbContext.cs  
**Description:** The Entity Framework Core DbContext for the application's relational database (PostgreSQL). Defines all DbSets and configures entity relationships.  
**Template:** C# DbContext  
**Dependency Level:** 2  
**Name:** AppDbContext  
**Type:** DbContext  
**Relative Path:** Relational  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - UnitOfWork
    - DataMapper
    
**Members:**
    
    - **Name:** Users  
**Type:** DbSet<User>  
**Attributes:** public  
    - **Name:** Roles  
**Type:** DbSet<Role>  
**Attributes:** public  
    - **Name:** OpcServers  
**Type:** DbSet<OpcServer>  
**Attributes:** public  
    - **Name:** OpcTags  
**Type:** DbSet<OpcTag>  
**Attributes:** public  
    - **Name:** BlockchainTransactions  
**Type:** DbSet<BlockchainTransaction>  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** OnModelCreating  
**Parameters:**
    
    - ModelBuilder modelBuilder
    
**Return Type:** void  
**Attributes:** protected override  
    
**Implemented Features:**
    
    - Relational Data Persistence
    - Entity Configuration
    
**Requirement Ids:**
    
    - REQ-DLP-008
    - REQ-DLP-025
    
**Purpose:** Acts as the primary API for interacting with the relational database, managing entity tracking, and saving changes.  
**Logic Description:** This class will inherit from DbContext. It will define DbSet properties for each entity in the relational model. The OnModelCreating method will be used to configure primary keys, foreign keys, indexes, unique constraints, and potentially value converters for features like data encryption on specific fields as required by REQ-DLP-008.  
**Documentation:**
    
    - **Summary:** Represents a session with the PostgreSQL database, allowing for querying and saving of relational data entities using Entity Framework Core.
    
**Namespace:** Opc.System.Infrastructure.Data.Relational  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/Relational/Repositories/BlockchainLogRepository.cs  
**Description:** Implements the IBlockchainLogRepository interface using Entity Framework Core for data persistence.  
**Template:** C# Repository  
**Dependency Level:** 3  
**Name:** BlockchainLogRepository  
**Type:** Repository  
**Relative Path:** Relational/Repositories  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    - **Name:** _context  
**Type:** AppDbContext  
**Attributes:** private readonly  
    
**Methods:**
    
    - **Name:** LogTransactionAsync  
**Parameters:**
    
    - BlockchainTransaction logEntry
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** GetTransactionByHashAsync  
**Parameters:**
    
    - string dataHash
    
**Return Type:** Task<BlockchainTransaction>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Blockchain Transaction Persistence
    
**Requirement Ids:**
    
    - REQ-DLP-025
    
**Purpose:** To provide concrete persistence logic for storing and retrieving blockchain transaction metadata in the relational database.  
**Logic Description:** The constructor will take an AppDbContext via dependency injection. LogTransactionAsync will add the new entry to the corresponding DbSet and the caller will use IUnitOfWork to save. GetTransactionByHashAsync will query the DbSet using a Where clause on the dataHash property.  
**Documentation:**
    
    - **Summary:** Handles the storage and retrieval of blockchain transaction metadata from the PostgreSQL database.
    
**Namespace:** Opc.System.Infrastructure.Data.Relational.Repositories  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/TimeSeries/PersistenceModels/HistoricalDataPoint.cs  
**Description:** A persistence-specific model representing a single historical data point, decorated with attributes for the InfluxDB client.  
**Template:** C# POCO  
**Dependency Level:** 0  
**Name:** HistoricalDataPoint  
**Type:** Model  
**Relative Path:** TimeSeries/PersistenceModels  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** TagId  
**Type:** string  
**Attributes:** public  
    - **Name:** Value  
**Type:** object  
**Attributes:** public  
    - **Name:** Quality  
**Type:** string  
**Attributes:** public  
    - **Name:** Time  
**Type:** DateTime  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-001
    
**Purpose:** To define the schema for data points being written to and read from the InfluxDB time-series database.  
**Logic Description:** This is a Plain Old Csharp Object (POCO). It will be decorated with InfluxDB.Client attributes like [Measurement], [Column(IsTag=true)], [Column], and [Column(IsTimestamp=true)] to map its properties to the InfluxDB data structure.  
**Documentation:**
    
    - **Summary:** Represents the data transfer object for historical data points, tailored for interaction with the InfluxDB time-series database.
    
**Namespace:** Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/TimeSeries/PersistenceModels/AlarmEventPoint.cs  
**Description:** A persistence-specific model representing a single alarm or event, decorated with attributes for the InfluxDB client.  
**Template:** C# POCO  
**Dependency Level:** 0  
**Name:** AlarmEventPoint  
**Type:** Model  
**Relative Path:** TimeSeries/PersistenceModels  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** SourceNode  
**Type:** string  
**Attributes:** public  
    - **Name:** EventType  
**Type:** string  
**Attributes:** public  
    - **Name:** Severity  
**Type:** int  
**Attributes:** public  
    - **Name:** Message  
**Type:** string  
**Attributes:** public  
    - **Name:** OccurrenceTime  
**Type:** DateTime  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-005
    
**Purpose:** To define the schema for alarm and event records being written to and read from the InfluxDB time-series database.  
**Logic Description:** Similar to HistoricalDataPoint, this POCO will be decorated with InfluxDB.Client attributes to map its properties to the InfluxDB measurement schema for alarms and events.  
**Documentation:**
    
    - **Summary:** Represents the data transfer object for alarm and event records, tailored for interaction with the InfluxDB time-series database.
    
**Namespace:** Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/TimeSeries/Repositories/HistoricalDataRepository.cs  
**Description:** Implements the IHistoricalDataRepository interface using the InfluxDB.Client library.  
**Template:** C# Repository  
**Dependency Level:** 3  
**Name:** HistoricalDataRepository  
**Type:** Repository  
**Relative Path:** TimeSeries/Repositories  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    - **Name:** _influxDbClient  
**Type:** InfluxDBClient  
**Attributes:** private readonly  
    - **Name:** _options  
**Type:** TimeSeriesDbOptions  
**Attributes:** private readonly  
    
**Methods:**
    
    - **Name:** IngestDataBatchAsync  
**Parameters:**
    
    - IEnumerable<HistoricalDataPoint> dataBatch
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** QueryDataAsync  
**Parameters:**
    
    - HistoricalDataQuery query
    
**Return Type:** Task<IEnumerable<HistoricalDataPoint>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Historical Data Persistence
    - Time-Series Querying
    
**Requirement Ids:**
    
    - REQ-DLP-001
    - REQ-DLP-002
    - REQ-DLP-003
    - REQ-CSVC-013
    
**Purpose:** Provides the concrete implementation for writing and reading historical data to/from an InfluxDB or compatible time-series database.  
**Logic Description:** The constructor receives the InfluxDBClient and configuration options. IngestDataBatchAsync will use the client's WriteApiAsync to perform an optimized batch write. Before writing, it will perform validation as per REQ-DLP-002. QueryDataAsync will construct a Flux query string based on the input query object, execute it using QueryApi, and handle potential errors, returning informative feedback as per REQ-DLP-003.  
**Documentation:**
    
    - **Summary:** Handles the concrete logic for interacting with a time-series database to store and retrieve historical process data.
    
**Namespace:** Opc.System.Infrastructure.Data.TimeSeries.Repositories  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/BlobStorage/Repositories/AiArtifactRepository.cs  
**Description:** Implements the IAiArtifactRepository interface using the Azure Blob Storage SDK.  
**Template:** C# Repository  
**Dependency Level:** 3  
**Name:** AiArtifactRepository  
**Type:** Repository  
**Relative Path:** BlobStorage/Repositories  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    - **Name:** _blobServiceClient  
**Type:** BlobServiceClient  
**Attributes:** private readonly  
    - **Name:** _containerName  
**Type:** string  
**Attributes:** private readonly  
    
**Methods:**
    
    - **Name:** UploadArtifactAsync  
**Parameters:**
    
    - string artifactName
    - Stream content
    
**Return Type:** Task<Uri>  
**Attributes:** public  
    - **Name:** DownloadArtifactAsync  
**Parameters:**
    
    - string artifactName
    
**Return Type:** Task<Stream>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Blob Storage Interaction
    
**Requirement Ids:**
    
    - REQ-DLP-024
    
**Purpose:** Manages the persistence of large binary AI artifacts like ONNX models in cloud blob storage.  
**Logic Description:** The constructor will receive a BlobServiceClient. Methods will get a BlobContainerClient for the configured container. UploadArtifactAsync will get a BlobClient and call UploadAsync. DownloadArtifactAsync will use OpenReadAsync. Error handling for blob not found or access issues will be implemented.  
**Documentation:**
    
    - **Summary:** Provides a concrete implementation for storing and retrieving AI artifacts from a configured Azure Blob Storage container.
    
**Namespace:** Opc.System.Infrastructure.Data.BlobStorage.Repositories  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Data/Services/DataMaskingService.cs  
**Description:** Provides services for masking or anonymizing sensitive data.  
**Template:** C# Service  
**Dependency Level:** 1  
**Name:** DataMaskingService  
**Type:** Service  
**Relative Path:** Services  
**Repository Id:** REPO-SAP-009  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** MaskUserData  
**Parameters:**
    
    - User user
    
**Return Type:** User  
**Attributes:** public  
    - **Name:** AnonymizeHistoricalData  
**Parameters:**
    
    - IEnumerable<HistoricalDataPoint> data
    
**Return Type:** IEnumerable<HistoricalDataPoint>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Data Anonymization
    - Data Masking
    
**Requirement Ids:**
    
    - REQ-DLP-009
    
**Purpose:** To provide reusable logic for de-identifying data for use in non-production environments like testing or development.  
**Logic Description:** This service will contain methods that take data objects and return new objects with sensitive fields replaced. For example, MaskUserData might replace email with a dummy value and name with 'Test User'. AnonymizeHistoricalData might apply a random jitter to numerical values to obscure the exact figures while retaining the general trend.  
**Documentation:**
    
    - **Summary:** A service that implements data masking and anonymization techniques to protect sensitive information in non-production contexts.
    
**Namespace:** Opc.System.Infrastructure.Data.Services  
**Metadata:**
    
    - **Category:** DataAccess
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - UseInfluxDB
  - UseTimescaleDB
  - EnableDataMasking
  
- **Database Configs:**
  
  - ConnectionStrings:PostgresConnection
  - ConnectionStrings:InfluxDBConnection
  - ConnectionStrings:AzureBlobStorage
  - TimeSeriesDbOptions:BucketName
  - TimeSeriesDbOptions:Organization
  - BlobStorageOptions:ContainerName
  


---

