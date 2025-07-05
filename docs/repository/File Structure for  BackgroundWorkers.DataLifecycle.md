# Specification

# 1. Files

- **Path:** src/BackgroundWorkers/DataLifecycle/BackgroundWorkers.DataLifecycle.csproj  
**Description:** The C# project file for the Data Lifecycle background worker. Defines the .NET 8 target framework, project dependencies like Quartz.NET, Serilog, and cloud storage SDKs (AWS S3, Azure Blob Storage), and project properties.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** BackgroundWorkers.DataLifecycle  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - ProjectConfiguration
    
**Requirement Ids:**
    
    
**Purpose:** To define all project-level configurations, dependencies, and build settings for the Data Lifecycle worker service.  
**Logic Description:** This file will contain PackageReference nodes for Quartz, Quartz.Extensions.Hosting, Serilog, Serilog.Sinks.Console, Microsoft.Extensions.Hosting, and relevant cloud SDKs (e.g., AWSSDK.S3, Azure.Storage.Blobs). It will specify the target framework as net8.0.  
**Documentation:**
    
    - **Summary:** Defines the build artifacts and dependencies for the background worker service responsible for data retention and archival.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Program.cs  
**Description:** The main entry point for the Data Lifecycle background worker application. Configures and builds the .NET Generic Host, setting up dependency injection, configuration, logging, and the Quartz.NET scheduler.  
**Template:** C# Program.cs Template  
**Dependency Level:** 2  
**Name:** Program  
**Type:** ApplicationEntry  
**Relative Path:**   
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public static  
    - **Name:** CreateHostBuilder  
**Parameters:**
    
    - string[] args
    
**Return Type:** IHostBuilder  
**Attributes:** public static  
    
**Implemented Features:**
    
    - ServiceHostConfiguration
    - DependencyInjectionSetup
    - SchedulerIntegration
    
**Requirement Ids:**
    
    - REQ-DLP-017
    - REQ-DLP-018
    
**Purpose:** Initializes and runs the background worker service, registering all necessary components and starting the job scheduler.  
**Logic Description:** The Main method will call CreateHostBuilder. The host builder will configure appsettings.json, set up Serilog for logging, and configure services. It will register all application services (IPolicyProvider, IArchiver, IPurger, IAuditLogger, strategies) and configure Quartz.NET using AddQuartz and AddQuartzHostedService, setting up the jobs and triggers defined in QuartzStartup.  
**Documentation:**
    
    - **Summary:** Bootstraps the Data Lifecycle service, wiring up all dependencies and kicking off the scheduled job processing.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/appsettings.json  
**Description:** Configuration file for the Data Lifecycle service. Contains connection strings for accessing policy and data databases, settings for archive storage (e.g., S3 bucket, Azure container), and the CRON schedule for the data lifecycle job.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:**   
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - ExternalizedConfiguration
    
**Requirement Ids:**
    
    - REQ-DLP-017
    - REQ-DLP-018
    
**Purpose:** To provide runtime configuration for the service, allowing settings to be changed without recompiling the application.  
**Logic Description:** This JSON file will have top-level keys like 'ConnectionStrings', 'ArchiveStorage' (with sub-keys for 'Type', 'BucketName', 'Endpoint'), and 'Scheduler' (with a 'CronExpression' for the lifecycle job). It allows environment-specific overrides via appsettings.Development.json.  
**Documentation:**
    
    - **Summary:** Stores all external configuration values needed by the Data Lifecycle worker, including database connections and job schedules.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Scheduling/QuartzStartup.cs  
**Description:** Configures the Quartz.NET scheduler, defining jobs and their triggers. Encapsulates the logic for adding the DataLifecycleJob and setting its schedule based on the CRON expression from appsettings.json.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** QuartzStartup  
**Type:** Configuration  
**Relative Path:** Scheduling  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Configure  
**Parameters:**
    
    - IServiceCollectionQuartzConfigurator q
    - IConfiguration configuration
    
**Return Type:** void  
**Attributes:** public static  
    
**Implemented Features:**
    
    - JobScheduling
    
**Requirement Ids:**
    
    - REQ-DLP-017
    - REQ-DLP-018
    
**Purpose:** To centralize the setup of all scheduled tasks within the application, making it easy to manage job definitions and schedules.  
**Logic Description:** This static class will have a Configure method called during DI setup in Program.cs. It will read the CRON expression from IConfiguration, create a JobKey for DataLifecycleJob, and set up a trigger for that job using the CRON schedule. This ensures the job is automatically scheduled when the service starts.  
**Documentation:**
    
    - **Summary:** Handles the registration and scheduling of all Quartz.NET jobs for the Data Lifecycle worker.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Scheduling  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Jobs/DataLifecycleJob.cs  
**Description:** The core Quartz.NET job that executes the data lifecycle management process. It is triggered by the scheduler and orchestrates the application of retention policies.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** DataLifecycleJob  
**Type:** Job  
**Relative Path:** Jobs  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    - **Name:** _policyProvider  
**Type:** IPolicyProvider  
**Attributes:** private|readonly  
    - **Name:** _strategyFactory  
**Type:** IDataLifecycleStrategyFactory  
**Attributes:** private|readonly  
    - **Name:** _logger  
**Type:** ILogger<DataLifecycleJob>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Execute  
**Parameters:**
    
    - IJobExecutionContext context
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - PolicyEnforcementOrchestration
    
**Requirement Ids:**
    
    - REQ-DLP-017
    - REQ-DLP-018
    - REQ-DLP-019
    
**Purpose:** To serve as the main execution logic for the periodic data lifecycle check, iterating through all policies and delegating work.  
**Logic Description:** The Execute method will be called by Quartz.NET on schedule. It will first call _policyProvider.GetActivePoliciesAsync(). It will then iterate through each policy, use the _strategyFactory to get the correct IDataLifecycleStrategy based on the policy's DataType, and then call ExecuteAsync on that strategy, passing the policy. It will include comprehensive logging for the start, end, and any errors during the job run.  
**Documentation:**
    
    - **Summary:** Implements the IJob interface for Quartz.NET. Fetches active data retention policies and delegates their execution to the appropriate strategy.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Jobs  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Application/Interfaces/IPolicyProvider.cs  
**Description:** Defines the contract for a service that retrieves data retention policies from a persistent store.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IPolicyProvider  
**Type:** Interface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetActivePoliciesAsync  
**Parameters:**
    
    
**Return Type:** Task<IEnumerable<DataRetentionPolicy>>  
**Attributes:**   
    
**Implemented Features:**
    
    - PolicyRetrieval
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** To abstract the source of data retention policies, decoupling the core job logic from the data access implementation.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Provides an interface for retrieving all currently active data retention policies that the lifecycle job needs to enforce.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Infrastructure/PolicyProvider.cs  
**Description:** Implements the IPolicyProvider interface. It interacts with a repository to fetch data retention policy configurations from the database.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** PolicyProvider  
**Type:** Service  
**Relative Path:** Infrastructure  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _policyRepository  
**Type:** IDataRetentionPolicyRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetActivePoliciesAsync  
**Parameters:**
    
    
**Return Type:** Task<IEnumerable<DataRetentionPolicy>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - PolicyRetrieval
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** To provide a concrete implementation for fetching retention policies, acting as a bridge between the application layer and data access layer.  
**Logic Description:** This class will be injected with an IDataRetentionPolicyRepository. The GetActivePoliciesAsync method will simply call the corresponding method on the repository to retrieve the policy data from the database.  
**Documentation:**
    
    - **Summary:** Fetches active data retention policies from the persistence layer using the data retention policy repository.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Application/Interfaces/IDataLifecycleStrategy.cs  
**Description:** Defines the contract for a data lifecycle strategy, allowing for different implementations for various data types (e.g., historical, alarm, audit).  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IDataLifecycleStrategy  
**Type:** Interface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    - **Name:** DataType  
**Type:** DataType  
**Attributes:** get;  
    
**Methods:**
    
    - **Name:** ExecuteAsync  
**Parameters:**
    
    - DataRetentionPolicy policy
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    - LifecyclePolicyExecution
    
**Requirement Ids:**
    
    - REQ-DLP-017
    - REQ-DLP-018
    - REQ-DLP-019
    
**Purpose:** To create a common interface for handling the lifecycle of different data types, promoting code reuse and extensibility.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Represents a strategy for executing a data retention policy for a specific type of data.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Application/Factories/IDataLifecycleStrategyFactory.cs  
**Description:** Defines the contract for a factory that creates or resolves the correct data lifecycle strategy based on data type.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IDataLifecycleStrategyFactory  
**Type:** Interface  
**Relative Path:** Application/Factories  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - FactoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetStrategy  
**Parameters:**
    
    - DataType dataType
    
**Return Type:** IDataLifecycleStrategy  
**Attributes:**   
    
**Implemented Features:**
    
    - StrategyResolution
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** To decouple the job from the concrete strategy implementations, allowing for easy registration and retrieval of strategies.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Provides an interface to get the appropriate IDataLifecycleStrategy implementation for a given data type.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Application.Factories  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Application/Factories/DataLifecycleStrategyFactory.cs  
**Description:** Implements the strategy factory. It holds a collection of all registered IDataLifecycleStrategy instances and returns the correct one on request.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** DataLifecycleStrategyFactory  
**Type:** Factory  
**Relative Path:** Application/Factories  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - FactoryPattern
    
**Members:**
    
    - **Name:** _strategies  
**Type:** IReadOnlyDictionary<DataType, IDataLifecycleStrategy>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** DataLifecycleStrategyFactory  
**Parameters:**
    
    - IEnumerable<IDataLifecycleStrategy> strategies
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** GetStrategy  
**Parameters:**
    
    - DataType dataType
    
**Return Type:** IDataLifecycleStrategy  
**Attributes:** public  
    
**Implemented Features:**
    
    - StrategyResolution
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** To provide a concrete factory implementation that leverages dependency injection to resolve the correct strategy.  
**Logic Description:** The constructor will be injected with an IEnumerable of all registered strategies. It will build a dictionary mapping the DataType property of each strategy to the strategy instance itself. The GetStrategy method will then perform a lookup in this dictionary.  
**Documentation:**
    
    - **Summary:** Resolves and returns a specific data lifecycle strategy based on the provided data type.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Application.Factories  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Application/Strategies/HistoricalDataLifecycleStrategy.cs  
**Description:** The specific strategy implementation for managing the lifecycle of historical process data. Handles archiving and purging based on the policy.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** HistoricalDataLifecycleStrategy  
**Type:** Service  
**Relative Path:** Application/Strategies  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    - **Name:** DataType  
**Type:** DataType  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** ExecuteAsync  
**Parameters:**
    
    - DataRetentionPolicy policy
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - HistoricalDataArchival
    - HistoricalDataPurging
    
**Requirement Ids:**
    
    - REQ-DLP-017
    - REQ-DLP-018
    - REQ-DLP-019
    
**Purpose:** To encapsulate all business logic related to managing the lifecycle of historical data.  
**Logic Description:** The ExecuteAsync method will first check if archiving is configured in the policy. If so, it will query for data older than the retention period, call the IArchiver service to move it to blob storage, and then call the IPurger service to delete the archived data from the source. If archiving is not configured, it will directly call the IPurger. All actions (start, success, failure, number of records affected) will be logged via the IAuditLogger.  
**Documentation:**
    
    - **Summary:** Implements the lifecycle logic for historical data, including archiving to long-term storage and purging from the primary time-series database.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Application.Strategies  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Application/Interfaces/IArchiver.cs  
**Description:** Defines the contract for a service that archives data from a source to a destination.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IArchiver  
**Type:** Interface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** ArchiveAsync  
**Parameters:**
    
    - DataType dataType
    - DateTimeOffset threshold
    - string archiveLocation
    
**Return Type:** Task<ArchiveResult>  
**Attributes:**   
    
**Implemented Features:**
    
    - DataArchival
    
**Requirement Ids:**
    
    - REQ-DLP-018
    
**Purpose:** To abstract the mechanism of archiving data, allowing different implementations for different storage backends (e.g., S3, Azure Blob, local file system).  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Provides an interface for archiving data of a specific type that is older than a given threshold to a specified location.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Infrastructure/Archiving/BlobStorageArchiver.cs  
**Description:** Implements the IArchiver interface using a generic cloud blob storage client (e.g., AWS S3 or Azure Blob Storage).  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** BlobStorageArchiver  
**Type:** Service  
**Relative Path:** Infrastructure/Archiving  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _storageClient  
**Type:** IStorageClient  
**Attributes:** private|readonly  
    - **Name:** _sourceDataClientFactory  
**Type:** ISourceDataClientFactory  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ArchiveAsync  
**Parameters:**
    
    - DataType dataType
    - DateTimeOffset threshold
    - string archiveLocation
    
**Return Type:** Task<ArchiveResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - CloudDataArchival
    
**Requirement Ids:**
    
    - REQ-DLP-018
    
**Purpose:** To provide a concrete implementation for archiving data to cost-effective cloud storage.  
**Logic Description:** This class will use a factory to get the correct source data client (e.g., IHistoricalDataClient). It will query the source for data to be archived in batches. Each batch will be serialized (e.g., to CSV or Parquet format) and uploaded to the specified blob storage location (bucket/container) using the injected IStorageClient. It will return a result object with the status and count of archived records.  
**Documentation:**
    
    - **Summary:** Handles the process of fetching data from a source database, serializing it, and uploading it to a cloud blob storage service.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Archiving  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Application/Interfaces/IPurger.cs  
**Description:** Defines the contract for a service that securely purges data from a source based on a time threshold.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IPurger  
**Type:** Interface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** PurgeAsync  
**Parameters:**
    
    - DataType dataType
    - DateTimeOffset threshold
    
**Return Type:** Task<PurgeResult>  
**Attributes:**   
    
**Implemented Features:**
    
    - DataPurging
    
**Requirement Ids:**
    
    - REQ-DLP-018
    
**Purpose:** To abstract the mechanism of data deletion, allowing for different implementations depending on the data source.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Provides an interface for securely and permanently deleting data of a specific type that is older than a given threshold.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Infrastructure/Purging/DatabasePurger.cs  
**Description:** Implements the IPurger interface for deleting records from a database.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** DatabasePurger  
**Type:** Service  
**Relative Path:** Infrastructure/Purging  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _sourceDataClientFactory  
**Type:** ISourceDataClientFactory  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** PurgeAsync  
**Parameters:**
    
    - DataType dataType
    - DateTimeOffset threshold
    
**Return Type:** Task<PurgeResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - DatabaseRecordPurging
    
**Requirement Ids:**
    
    - REQ-DLP-018
    
**Purpose:** To provide a concrete implementation for purging data from relational or time-series databases.  
**Logic Description:** This class will use a factory to get the correct source data client. It will then execute a bulk delete command against the source database to remove all records of the specified type older than the time threshold. It will return a result object with the status and count of deleted records.  
**Documentation:**
    
    - **Summary:** Handles the secure deletion of records from a source database based on a retention threshold.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Purging  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Application/Interfaces/IAuditLogger.cs  
**Description:** Defines the contract for a service that logs auditable events.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IAuditLogger  
**Type:** Interface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** LogDataLifecycleEventAsync  
**Parameters:**
    
    - string action
    - DataType dataType
    - bool success
    - long recordsAffected
    - string details
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    - AuditLogging
    
**Requirement Ids:**
    
    - REQ-DLP-019
    
**Purpose:** To abstract the audit logging mechanism, decoupling the application logic from how and where audit trails are stored.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Provides an interface for logging significant data management actions, such as archiving and purging, to a central audit trail.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Infrastructure/Persistence/IDataRetentionPolicyRepository.cs  
**Description:** Defines the data access contract for DataRetentionPolicy entities.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IDataRetentionPolicyRepository  
**Type:** RepositoryInterface  
**Relative Path:** Infrastructure/Persistence  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetActivePoliciesAsync  
**Parameters:**
    
    
**Return Type:** Task<IEnumerable<DataRetentionPolicy>>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** To abstract the database operations for retrieving data retention policy configurations.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Interface for repository handling database operations for DataRetentionPolicy entities.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Persistence  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Domain/Entities/DataRetentionPolicy.cs  
**Description:** Represents a data retention policy, defining the lifecycle rules for a specific type of data.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** DataRetentionPolicy  
**Type:** Entity  
**Relative Path:** Domain/Entities  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** PolicyId  
**Type:** Guid  
**Attributes:** public|get; set;  
    - **Name:** DataType  
**Type:** DataType  
**Attributes:** public|get; set;  
    - **Name:** RetentionPeriodDays  
**Type:** int  
**Attributes:** public|get; set;  
    - **Name:** ArchiveLocation  
**Type:** string?  
**Attributes:** public|get; set;  
    - **Name:** IsActive  
**Type:** bool  
**Attributes:** public|get; set;  
    
**Methods:**
    
    
**Implemented Features:**
    
    - DataRetentionConfiguration
    
**Requirement Ids:**
    
    - REQ-DLP-017
    - REQ-DLP-018
    
**Purpose:** To model the configuration for a single data retention rule within the system.  
**Logic Description:** This is a Plain Old C# Object (POCO) that maps directly to the DataRetentionPolicy table in the database. It will be used to transport policy information between the data access layer and the application layer.  
**Documentation:**
    
    - **Summary:** A domain entity representing the settings for a data retention policy, including data type, retention period, and archival information.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/BackgroundWorkers/DataLifecycle/Domain/Enums/DataType.cs  
**Description:** An enumeration of the different data types managed by the lifecycle service.  
**Template:** C# Enum  
**Dependency Level:** 0  
**Name:** DataType  
**Type:** Enum  
**Relative Path:** Domain/Enums  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** Historical  
**Type:** enum  
**Attributes:**   
    - **Name:** Alarm  
**Type:** enum  
**Attributes:**   
    - **Name:** Audit  
**Type:** enum  
**Attributes:**   
    - **Name:** AI  
**Type:** enum  
**Attributes:**   
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** To provide a strongly-typed identifier for different data categories subject to retention policies.  
**Logic Description:** A simple public enum to be used throughout the application to identify the type of data being processed.  
**Documentation:**
    
    - **Summary:** Defines the categories of data that can have retention policies applied to them.
    
**Namespace:** Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums  
**Metadata:**
    
    - **Category:** Domain
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableArchivingHistoricalData
  - EnablePurgingHistoricalData
  - EnableArchivingAlarmData
  - EnablePurgingAlarmData
  
- **Database Configs:**
  
  - ConnectionStrings:ApplicationDb
  - ArchiveStorage:Type
  - ArchiveStorage:ConnectionString
  - ArchiveStorage:BucketName
  - Scheduler:CronExpression
  


---

