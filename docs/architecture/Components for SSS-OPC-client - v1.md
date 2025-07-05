# Architecture Design Specification

# 1. Components

- **Components:**
  
  ### .1. OPC DA Client
  Handles all communication with OPC Data Access (DA) servers, including connecting, reading, writing, and browsing.

  #### .1.4. Type
  Adapter

  #### .1.5. Dependencies
  
  - CoreOpcClientService_ClientServerCommunicator
  - CoreOpcClientService_ClientWriteAuditor
  - CoreOpcClientService_ClientWriteController
  
  #### .1.6. Properties
  
  - **Supported Da Versions:** 2.05a, 3.0
  
  #### .1.7. Interfaces
  
  - IOpcDaOperations
  
  #### .1.8. Technology
  .NET 8, OPC Foundation Libraries (or equivalent DA SDK)

  #### .1.9. Resources
  
  - **Cpu:** Low-Medium (depends on tag count and update rate)
  - **Memory:** Medium (depends on tag count)
  - **Storage:** Low (for logs, potential cache)
  - **Network:** Medium (OPC-DA traffic)
  
  #### .1.10. Configuration
  
  - **Default Timeout Ms:** 5000
  - **Retry Attempts:** 3
  
  #### .1.11. Health Check
  None

  #### .1.12. Responsible Features
  
  - REQ-CSVC-001
  - REQ-CSVC-002
  - REQ-CSVC-003
  - REQ-CSVC-004
  - REQ-CSVC-005
  - REQ-CSVC-006
  - REQ-DLP-010
  
  #### .1.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .2. OPC UA Client
  Handles all communication with OPC Unified Architecture (UA) servers, including connecting, security, reading, writing, browsing, and managing subscriptions.

  #### .2.4. Type
  Adapter

  #### .2.5. Dependencies
  
  - CoreOpcClientService_ClientSubscriptionManager
  - CoreOpcClientService_ClientServerCommunicator
  - CoreOpcClientService_ClientWriteAuditor
  - CoreOpcClientService_ClientWriteController
  - CoreOpcClientService_SecurityCertificateManager
  
  #### .2.6. Properties
  
  - **Supported Ua Version:** 1.04+
  - **Supported Transports:** OPC UA TCP, WebSockets (PubSub JSON/UADP)
  
  #### .2.7. Interfaces
  
  - IOpcUaOperations
  - IOpcSubscriptionCreator
  
  #### .2.8. Technology
  .NET 8, OPCFoundation.NetStandard.Opc.Ua

  #### .2.9. Resources
  
  - **Cpu:** Medium (depends on tag count, subscriptions, encryption)
  - **Memory:** Medium-High (depends on tag count, subscriptions)
  - **Storage:** Low (for logs, certificates, potential cache)
  - **Network:** Medium (OPC-UA traffic)
  
  #### .2.10. Configuration
  
  - **Session Timeout Ms:** 60000
  - **Secure Channel Lifetime Ms:** 3600000
  - **Ua Application Uri:** urn:example:opc-client
  - **Ua Product Uri:** urn:example:opc-client-product
  
  #### .2.11. Health Check
  None

  #### .2.12. Responsible Features
  
  - REQ-CSVC-001
  - REQ-CSVC-002
  - REQ-CSVC-003
  - REQ-CSVC-004
  - REQ-CSVC-005
  - REQ-CSVC-006
  - REQ-CSVC-023
  - REQ-CSVC-024
  - REQ-CSVC-025
  - REQ-CSVC-026
  - REQ-CSVC-028
  - REQ-3-001
  - REQ-DLP-010
  
  #### .2.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .3. OPC XML-DA Client
  Handles communication with OPC XML-DA servers.

  #### .3.4. Type
  Adapter

  #### .3.5. Dependencies
  
  - CoreOpcClientService_ClientServerCommunicator
  - CoreOpcClientService_ClientWriteAuditor
  - CoreOpcClientService_ClientWriteController
  
  #### .3.6. Properties
  
  - **Supported Xml Da Version:** 1.01
  
  #### .3.7. Interfaces
  
  - IOpcXmlDaOperations
  
  #### .3.8. Technology
  .NET 8, SOAP/XML Libraries

  #### .3.9. Resources
  
  - **Cpu:** Low-Medium
  - **Memory:** Medium
  - **Storage:** Low
  - **Network:** Medium (XML traffic)
  
  #### .3.10. Configuration
  
  - **Endpoint Timeout Ms:** 10000
  
  #### .3.11. Health Check
  None

  #### .3.12. Responsible Features
  
  - REQ-CSVC-001
  - REQ-CSVC-003
  - REQ-CSVC-004
  - REQ-CSVC-005
  - REQ-CSVC-006
  - REQ-DLP-010
  
  #### .3.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .4. OPC HDA Client
  Handles retrieval of historical data from OPC HDA servers, including aggregated data.

  #### .4.4. Type
  Adapter

  #### .4.5. Dependencies
  
  - CoreOpcClientService_ClientServerCommunicator
  
  #### .4.6. Properties
  
  - **Supported Hda Version:** 1.20 (typically)
  
  #### .4.7. Interfaces
  
  - IOpcHdaOperations
  
  #### .4.8. Technology
  .NET 8, OPC HDA SDK (if available) or custom COM interop

  #### .4.9. Resources
  
  - **Cpu:** Low-Medium (query dependent)
  - **Memory:** Medium (query result size dependent)
  - **Storage:** Low
  - **Network:** Medium (HDA traffic)
  
  #### .4.10. Configuration
  
  - **Query Timeout Ms:** 30000
  
  #### .4.11. Health Check
  None

  #### .4.12. Responsible Features
  
  - REQ-CSVC-011
  - REQ-CSVC-012
  - REQ-CSVC-014
  - REQ-CSVC-015
  
  #### .4.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .5. OPC A&C Client
  Handles receiving and managing alarms and events from OPC A&C servers, including acknowledgements.

  #### .5.4. Type
  Adapter

  #### .5.5. Dependencies
  
  - CoreOpcClientService_ClientServerCommunicator
  
  #### .5.6. Properties
  
  - **Supported Ac Version:** 1.10 (typically)
  
  #### .5.7. Interfaces
  
  - IOpcAcOperations
  
  #### .5.8. Technology
  .NET 8, OPC A&C SDK (if available) or custom COM interop

  #### .5.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Medium
  - **Storage:** Low
  - **Network:** Low-Medium (A&C traffic)
  
  #### .5.10. Configuration
  
  - **Event Subscription Keep Alive Ms:** 10000
  
  #### .5.11. Health Check
  None

  #### .5.12. Responsible Features
  
  - REQ-CSVC-017
  - REQ-CSVC-018
  - REQ-CSVC-019
  - REQ-CSVC-021
  
  #### .5.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .6. Client Tag Configuration Manager
  Manages importing and accessing tag configurations (tag lists, server addresses, item IDs, scaling parameters) for the OPC client.

  #### .6.4. Type
  Manager

  #### .6.5. Dependencies
  
  
  #### .6.6. Properties
  
  
  #### .6.7. Interfaces
  
  - ITagConfigurationProvider
  - ITagConfigurationImporter
  
  #### .6.8. Technology
  .NET 8

  #### .6.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low-Medium (depends on number of configurations)
  - **Storage:** Low (if configurations are cached locally)
  - **Network:** Low (if fetching from central store)
  
  #### .6.10. Configuration
  
  - **Default Import Path:** /etc/opc_client/tags/
  - **Supported Import Formats:** CSV,XML
  
  #### .6.11. Health Check
  None

  #### .6.12. Responsible Features
  
  - REQ-CSVC-008
  - REQ-CSVC-027
  - REQ-SAP-009
  
  #### .6.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .7. Client Write Auditor
  Logs write operations to designated critical control system tags, including user credentials, timestamp, tag ID, old/new values.

  #### .7.4. Type
  Utility

  #### .7.5. Dependencies
  
  - CoreOpcClientService_ClientServerCommunicator
  
  #### .7.6. Properties
  
  
  #### .7.7. Interfaces
  
  - IWriteOperationLogger
  
  #### .7.8. Technology
  .NET 8

  #### .7.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low
  - **Storage:** Low (local buffer before sending to server)
  - **Network:** Low (to server for centralized logging)
  
  #### .7.10. Configuration
  
  - **Audit Queue Name:** opc.client.audit.writes
  - **Critical Tag Identifier Pattern:** ^CRITICAL_.*
  
  #### .7.11. Health Check
  None

  #### .7.12. Responsible Features
  
  - REQ-CSVC-009
  
  #### .7.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .8. Client Write Controller
  Enforces configurable limits on data write operations (rate limits, value change thresholds) and performs client-side data validation.

  #### .8.4. Type
  Controller

  #### .8.5. Dependencies
  
  
  #### .8.6. Properties
  
  
  #### .8.7. Interfaces
  
  - IWriteOperationValidator
  - IWriteOperationLimiter
  
  #### .8.8. Technology
  .NET 8

  #### .8.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low
  - **Storage:** None
  - **Network:** None
  
  #### .8.10. Configuration
  
  - **Default Rate Limit Per Tag Per Second:** 1
  - **Default Confirmation Threshold Percentage:** 10
  
  #### .8.11. Health Check
  None

  #### .8.12. Responsible Features
  
  - REQ-CSVC-007
  - REQ-CSVC-010
  
  #### .8.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .9. Client Subscription Manager
  Manages OPC UA subscriptions, including creation, parameter configuration, monitoring, automatic re-establishment, and client-side buffering.

  #### .9.4. Type
  Manager

  #### .9.5. Dependencies
  
  - CoreOpcClientService_ClientDataBuffer
  - CoreOpcClientService_ClientServerCommunicator
  
  #### .9.6. Properties
  
  
  #### .9.7. Interfaces
  
  - ISubscriptionManager
  
  #### .9.8. Technology
  .NET 8, OPCFoundation.NetStandard.Opc.Ua

  #### .9.9. Resources
  
  - **Cpu:** Low-Medium
  - **Memory:** Medium (depends on number of subscriptions and buffered data)
  - **Storage:** Low (for buffering if to disk)
  - **Network:** Low (for keep-alives, status to server)
  
  #### .9.10. Configuration
  
  - **Max Reconnect Attempts:** 5
  - **Reconnect Interval Ms:** 10000
  - **Default Publishing Interval:** 1000
  - **Default Sampling Interval:** 500
  - **Default Queue Size:** 10
  - **Buffer Duration Secs:** 300
  
  #### .9.11. Health Check
  None

  #### .9.12. Responsible Features
  
  - REQ-CSVC-023
  - REQ-CSVC-024
  - REQ-CSVC-026
  - REQ-CSVC-028
  
  #### .9.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .10. Client Data Buffer
  Provides client-side buffering for critical subscription data during short interruptions to prevent data loss.

  #### .10.4. Type
  Utility

  #### .10.5. Dependencies
  
  
  #### .10.6. Properties
  
  
  #### .10.7. Interfaces
  
  - IDataBuffer
  
  #### .10.8. Technology
  .NET 8

  #### .10.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Medium (depends on buffer size and duration)
  - **Storage:** Low-Medium (if disk-backed buffer)
  
  #### .10.10. Configuration
  
  - **Max Buffer Size Mb:** 100
  - **Buffer Storage Type:** Memory
  
  #### .10.11. Health Check
  None

  #### .10.12. Responsible Features
  
  - REQ-CSVC-026
  
  #### .10.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .11. Client-Server Communicator
  Handles communication between the Core OPC Client Service and the Server-Side Application using gRPC for synchronous and Message Queues for asynchronous operations.

  #### .11.4. Type
  Gateway

  #### .11.5. Dependencies
  
  
  #### .11.6. Properties
  
  
  #### .11.7. Interfaces
  
  - IServerboundSynchronousComms
  - IServerboundAsynchronousComms
  
  #### .11.8. Technology
  .NET 8, Grpc.Net.Client, RabbitMQ.Client / Confluent.Kafka

  #### .11.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low
  - **Storage:** None
  - **Network:** Low-Medium (to server application)
  
  #### .11.10. Configuration
  
  - **Server Grpc Endpoint:** https://server-app:5001
  - **Message Queue Host:** rabbitmq-server
  - **Realtime Data Topic:** opc.client.data.realtime
  - **Alarm Event Topic:** opc.client.events.alarms
  
  #### .11.11. Health Check
  None

  #### .11.12. Responsible Features
  
  - REQ-SAP-003
  - REQ-SAP-004
  
  #### .11.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - ClientServiceRole
    
  
  ### .12. Edge AI Executor
  Executes lightweight AI models (e.g., ONNX) on the edge device for local data processing and predictions.

  #### .12.4. Type
  Processor

  #### .12.5. Dependencies
  
  
  #### .12.6. Properties
  
  
  #### .12.7. Interfaces
  
  - IEdgeAiModelRunner
  
  #### .12.8. Technology
  .NET 8, ONNX Runtime

  #### .12.9. Resources
  
  - **Cpu:** Medium-High (during model execution)
  - **Memory:** Medium-High (model dependent)
  - **Storage:** Low (for model files)
  - **Network:** Low (for model updates)
  
  #### .12.10. Configuration
  
  - **Model Base Path:** /opt/opc_client/models/
  - **Default Inference Timeout Ms:** 100
  
  #### .12.11. Health Check
  None

  #### .12.12. Responsible Features
  
  - REQ-7-001
  - REQ-8-001
  
  #### .12.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .13. Security Certificate Manager (Client)
  Manages OPC UA client application certificates and trust lists for secure communication.

  #### .13.4. Type
  Manager

  #### .13.5. Dependencies
  
  
  #### .13.6. Properties
  
  
  #### .13.7. Interfaces
  
  - ICertificateProvider
  - ITrustListManager
  
  #### .13.8. Technology
  .NET 8, OPCFoundation.NetStandard.Opc.Ua

  #### .13.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low
  - **Storage:** Low (certificate store)
  - **Network:** Low (for CRL/OCSP if configured)
  
  #### .13.10. Configuration
  
  - **Certificate Store Path:** /var/lib/opc_client/pki/
  - **Auto Accept Server Certificates:** false
  
  #### .13.11. Health Check
  None

  #### .13.12. Responsible Features
  
  - REQ-3-001
  
  #### .13.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .14. API Gateway
  Acts as a single entry point for all client requests to the server-side microservices. Handles request routing, authentication, authorization, rate limiting, and response aggregation.

  #### .14.4. Type
  Gateway

  #### .14.5. Dependencies
  
  - ServerSide_AuthService_Api
  - ServerSide_ManagementService_Api
  - ServerSide_AiService_Api
  - ServerSide_ReportingService_Api
  - ServerSide_DataService_Api
  - ServerSide_ExternalIntegrationService_Api
  
  #### .14.6. Properties
  
  
  #### .14.7. Interfaces
  
  - IExternalApiEndpoint
  
  #### .14.8. Technology
  ASP.NET Core (.NET 8) with YARP or Ocelot, or dedicated API Gateway (e.g., Kong, Tyk)

  #### .14.9. Resources
  
  - **Cpu:** Medium
  - **Memory:** Medium
  - **Storage:** Low (for logs, cache)
  - **Network:** High (entry point for all traffic)
  
  #### .14.10. Configuration
  
  - **Jwt Validation Authority:** https://auth-service/identity
  - **Default Rate Limit Per Ip Per Second:** 100
  - **Cors Allowed Origins:** *
  
  #### .14.11. Health Check
  
  - **Path:** /health
  - **Interval:** 30
  - **Timeout:** 5
  
  #### .14.12. Responsible Features
  
  - REQ-SAP-002
  - REQ-3-010
  - REQ-SAP-004
  
  #### .14.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - User
    - Administrator
    - Engineer
    - Operator
    - Viewer
    - ApiClient
    
  
  ### .15. Web Application Host
  Hosts and serves the Blazor WebAssembly based User Interface and related static assets.

  #### .15.4. Type
  Service

  #### .15.5. Dependencies
  
  - ServerSide_ApiGateway
  
  #### .15.6. Properties
  
  
  #### .15.7. Interfaces
  
  
  #### .15.8. Technology
  ASP.NET Core (.NET 8)

  #### .15.9. Resources
  
  - **Cpu:** Low-Medium
  - **Memory:** Medium
  - **Storage:** Low (static assets)
  - **Network:** Medium (serving UI assets)
  
  #### .15.10. Configuration
  
  - **Content Root Path:** /app/wwwroot
  
  #### .15.11. Health Check
  
  - **Path:** /healthz
  - **Interval:** 30
  - **Timeout:** 5
  
  #### .15.12. Responsible Features
  
  - REQ-UIX-001
  - REQ-UIX-002
  - REQ-SAP-002
  
  #### .15.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .16. Management Service API
  Microservice providing RESTful/gRPC APIs for centralized management of OPC client instances, configurations, and monitoring.

  #### .16.4. Type
  Service

  #### .16.5. Dependencies
  
  - ServerSide_DataService_RelationalDbRepository
  - ServerSide_Messaging_MessageBusGateway
  
  #### .16.6. Properties
  
  
  #### .16.7. Interfaces
  
  - IManagementApi
  
  #### .16.8. Technology
  ASP.NET Core (.NET 8), gRPC, REST

  #### .16.9. Resources
  
  - **Cpu:** Medium
  - **Memory:** Medium
  - **Storage:** Low (logs)
  - **Network:** Medium (internal traffic)
  
  #### .16.10. Configuration
  
  - **Default Client Heartbeat Timeout Sec:** 120
  
  #### .16.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 15
  - **Timeout:** 3
  
  #### .16.12. Responsible Features
  
  - REQ-SAP-002
  - REQ-6-001
  - REQ-6-002
  - REQ-SAP-009
  - REQ-UIX-012
  - REQ-UIX-022
  - REQ-9-004
  - REQ-9-005
  
  #### .16.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - Administrator
    - Engineer
    
  
  ### .17. AI Processing Service API
  Microservice providing RESTful/gRPC APIs for backend AI processing, including predictive maintenance, anomaly detection, NLQ, and AI model management.

  #### .17.4. Type
  Service

  #### .17.5. Dependencies
  
  - ServerSide_DataService_TimeSeriesDbRepository
  - ServerSide_DataService_BlobStorageRepository
  - ServerSide_NlpServiceProvider
  
  #### .17.6. Properties
  
  
  #### .17.7. Interfaces
  
  - IAiApi
  
  #### .17.8. Technology
  ASP.NET Core (.NET 8), ML.NET, ONNX Runtime, Python (optional interop)

  #### .17.9. Resources
  
  - **Cpu:** High (during model inference/training)
  - **Memory:** High (model dependent)
  - **Storage:** Medium (models, datasets)
  - **Network:** Medium (internal traffic, to NLP services)
  
  #### .17.10. Configuration
  
  - **Ml Ops Platform Endpoint:** http://mlflow-server:5000
  - **Default Model Cache Duration Sec:** 3600
  
  #### .17.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 15
  - **Timeout:** 3
  
  #### .17.12. Responsible Features
  
  - REQ-SAP-002
  - REQ-7-001
  - REQ-7-002
  - REQ-7-003
  - REQ-7-004
  - REQ-7-005
  - REQ-7-006
  - REQ-7-008
  - REQ-7-009
  - REQ-7-010
  - REQ-7-011
  - REQ-7-012
  - REQ-7-013
  - REQ-7-014
  - REQ-7-015
  - REQ-7-016
  - REQ-UIX-013
  - REQ-UIX-014
  
  #### .17.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - Administrator
    - Engineer
    - User
    
  
  ### .18. NLP Service Provider
  Adapter component to integrate with external NLP services (e.g., Azure Cognitive Services, Google Cloud NL) or on-premise solutions (e.g., spaCy) for intent recognition and entity extraction.

  #### .18.4. Type
  Adapter

  #### .18.5. Dependencies
  
  
  #### .18.6. Properties
  
  
  #### .18.7. Interfaces
  
  - INlpProcessor
  
  #### .18.8. Technology
  .NET 8, SDKs for Azure/Google NLP, spaCy (if Python interop)

  #### .18.9. Resources
  
  - **Cpu:** Low (mostly network bound)
  - **Memory:** Low
  - **Storage:** None
  - **Network:** Medium (to external NLP services)
  
  #### .18.10. Configuration
  
  - **Nlp Service Type:** AzureCognitiveServices
  - **Nlp Service Endpoint:** YOUR_AZURE_ENDPOINT
  - **Nlp Service Api Key:** YOUR_AZURE_KEY
  
  #### .18.11. Health Check
  None

  #### .18.12. Responsible Features
  
  - REQ-7-014
  - REQ-7-016
  
  #### .18.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .19. Reporting Service API
  Microservice providing RESTful/gRPC APIs for automated and on-demand report generation, customization, and distribution.

  #### .19.4. Type
  Service

  #### .19.5. Dependencies
  
  - ServerSide_DataService_TimeSeriesDbRepository
  - ServerSide_AiService_Api
  
  #### .19.6. Properties
  
  
  #### .19.7. Interfaces
  
  - IReportingApi
  
  #### .19.8. Technology
  ASP.NET Core (.NET 8), QuestPDF, ClosedXML

  #### .19.9. Resources
  
  - **Cpu:** Medium (during report generation)
  - **Memory:** Medium (report data dependent)
  - **Storage:** Medium (generated reports, templates)
  - **Network:** Medium (internal traffic, report distribution)
  
  #### .19.10. Configuration
  
  - **Report Template Path:** /app/reports/templates/
  - **Generated Report Path:** /app/reports/generated/
  - **Smtp Server:** smtp.example.com
  
  #### .19.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 15
  - **Timeout:** 3
  
  #### .19.12. Responsible Features
  
  - REQ-SAP-002
  - REQ-7-018
  - REQ-7-019
  - REQ-7-020
  - REQ-7-021
  - REQ-7-022
  - REQ-UIX-017
  
  #### .19.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - Administrator
    - Engineer
    - User
    
  
  ### .20. Relational DB Repository
  Manages persistence and retrieval of structured data like user configurations, roles, permissions, audit logs, and OPC server/tag metadata in a relational database.

  #### .20.4. Type
  Repository

  #### .20.5. Dependencies
  
  
  #### .20.6. Properties
  
  
  #### .20.7. Interfaces
  
  - IUserRepository
  - IRoleRepository
  - IPermissionRepository
  - IAuditLogRepository
  - IOpcMetadataRepository
  - IConfigurationRepository
  
  #### .20.8. Technology
  .NET 8, Entity Framework Core, PostgreSQL/SQL Server

  #### .20.9. Resources
  
  - **Cpu:** Low-Medium (DB server dependent)
  - **Memory:** Low-Medium (DB server dependent)
  - **Storage:** Medium-High (DB server dependent)
  - **Network:** Low (to DB server)
  
  #### .20.10. Configuration
  
  - **Connection String Name:** DefaultConnection
  - **Db Provider:** PostgreSQL
  
  #### .20.11. Health Check
  None

  #### .20.12. Responsible Features
  
  - REQ-DLP-008
  - REQ-3-003
  - REQ-3-005
  - REQ-3-012
  - REQ-CSVC-008
  - REQ-9-001
  - REQ-9-002
  - REQ-9-003
  
  #### .20.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .21. Time-Series DB Repository
  Manages persistence and retrieval of time-series data, including historical process data and alarm/event data, in a specialized time-series database.

  #### .21.4. Type
  Repository

  #### .21.5. Dependencies
  
  
  #### .21.6. Properties
  
  
  #### .21.7. Interfaces
  
  - IHistoricalDataRepository
  - IAlarmEventRepository
  
  #### .21.8. Technology
  .NET 8, InfluxDB Client / TimescaleDB Client

  #### .21.9. Resources
  
  - **Cpu:** Low-Medium (DB server dependent)
  - **Memory:** Low-Medium (DB server dependent)
  - **Storage:** High (DB server dependent)
  - **Network:** Low (to DB server)
  
  #### .21.10. Configuration
  
  - **Time Series Db Endpoint:** http://influxdb:8086
  - **Time Series Db Token:** YOUR_INFLUXDB_TOKEN
  - **Time Series Db Organization:** your-org
  - **Time Series Db Bucket:** opc_data
  
  #### .21.11. Health Check
  None

  #### .21.12. Responsible Features
  
  - REQ-DLP-001
  - REQ-DLP-002
  - REQ-DLP-003
  - REQ-DLP-005
  - REQ-DLP-006
  - REQ-CSVC-013
  - REQ-CSVC-019
  
  #### .21.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .22. Blob Storage Repository
  Manages storage and retrieval of unstructured data such as AI model artifacts, large datasets, and potentially report archives in blob storage.

  #### .22.4. Type
  Repository

  #### .22.5. Dependencies
  
  
  #### .22.6. Properties
  
  
  #### .22.7. Interfaces
  
  - IAiModelArtifactRepository
  - IUnstructuredDataRepository
  
  #### .22.8. Technology
  .NET 8, Azure Blob Storage SDK / AWS S3 SDK / MinIO SDK

  #### .22.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low
  - **Storage:** High (cloud/object storage dependent)
  - **Network:** Low (to blob storage)
  
  #### .22.10. Configuration
  
  - **Blob Storage Connection String:** YOUR_AZURE_CONNECTION_STRING
  - **Blob Container Name Models:** ai-models
  - **Blob Container Name Reports:** generated-reports
  
  #### .22.11. Health Check
  None

  #### .22.12. Responsible Features
  
  - REQ-DLP-024
  - REQ-7-006
  - REQ-DLP-018
  
  #### .22.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .23. Data Ingestion Processor
  Consumes data from message queues (real-time data, alarms, audit logs from clients) and routes it to the appropriate repositories for storage. Handles validation and transformation.

  #### .23.4. Type
  Processor

  #### .23.5. Dependencies
  
  - ServerSide_Messaging_MessageBusGateway
  - ServerSide_DataService_TimeSeriesDbRepository
  - ServerSide_DataService_RelationalDbRepository
  
  #### .23.6. Properties
  
  
  #### .23.7. Interfaces
  
  - IMessageConsumer
  
  #### .23.8. Technology
  .NET 8, RabbitMQ.Client / Confluent.Kafka

  #### .23.9. Resources
  
  - **Cpu:** Medium
  - **Memory:** Medium
  - **Storage:** Low (logs)
  - **Network:** Medium (to message queue and databases)
  
  #### .23.10. Configuration
  
  - **Realtime Data Queue Name:** opc.client.data.realtime.ingress
  - **Alarm Event Queue Name:** opc.client.events.alarms.ingress
  - **Write Audit Queue Name:** opc.client.audit.writes.ingress
  - **Ingestion Batch Size:** 1000
  
  #### .23.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 15
  - **Timeout:** 3
  
  #### .23.12. Responsible Features
  
  - REQ-DLP-002
  - REQ-DLP-006
  - REQ-CSVC-009
  - REQ-CSVC-013
  - REQ-CSVC-019
  - REQ-DLP-008
  
  #### .23.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .24. Data Retention Manager
  Implements and enforces configurable data retention policies for various data types, including archiving to cost-effective storage and secure purging.

  #### .24.4. Type
  Manager

  #### .24.5. Dependencies
  
  - ServerSide_DataService_RelationalDbRepository
  - ServerSide_DataService_TimeSeriesDbRepository
  - ServerSide_DataService_BlobStorageRepository
  
  #### .24.6. Properties
  
  
  #### .24.7. Interfaces
  
  - IDataLifecycleManager
  
  #### .24.8. Technology
  .NET 8, Hangfire (or similar for scheduled jobs)

  #### .24.9. Resources
  
  - **Cpu:** Low (runs periodically)
  - **Memory:** Low-Medium
  - **Storage:** Low (logs)
  - **Network:** Low (to databases/storage for archive/purge)
  
  #### .24.10. Configuration
  
  - **Retention Check Interval Hours:** 24
  - **Default Archive Storage Tier:** AzureCoolBlob
  
  #### .24.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 15
  - **Timeout:** 3
  
  #### .24.12. Responsible Features
  
  - REQ-DLP-017
  - REQ-DLP-018
  - REQ-DLP-019
  - REQ-DLP-029
  - REQ-DLP-030
  
  #### .24.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - Administrator
    - SystemJob
    
  
  ### .25. Data Migration Manager
  Provides tools and processes for migrating historical data, alarm/event logs, and configurations from legacy systems.

  #### .25.4. Type
  Manager

  #### .25.5. Dependencies
  
  - ServerSide_DataService_RelationalDbRepository
  - ServerSide_DataService_TimeSeriesDbRepository
  
  #### .25.6. Properties
  
  
  #### .25.7. Interfaces
  
  - IDataMigrationOrchestrator
  
  #### .25.8. Technology
  .NET 8

  #### .25.9. Resources
  
  - **Cpu:** Medium-High (during migration)
  - **Memory:** Medium-High (during migration)
  - **Storage:** Low (migration scripts, logs)
  - **Network:** Medium (to source and target systems)
  
  #### .25.10. Configuration
  
  - **Migration Script Path:** /app/migrations/
  - **Default Batch Size:** 10000
  
  #### .25.11. Health Check
  None

  #### .25.12. Responsible Features
  
  - REQ-DLP-004
  - REQ-DLP-007
  - REQ-DLP-022
  - REQ-DLP-023
  - REQ-CSVC-016
  - REQ-CSVC-022
  - REQ-SAP-009
  
  #### .25.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - Administrator
    
  
  ### .26. Authentication & Authorization Service API
  Microservice providing RESTful/gRPC APIs for user authentication (internal or IdP), Role-Based Access Control (RBAC), and security token management.

  #### .26.4. Type
  Service

  #### .26.5. Dependencies
  
  - ServerSide_DataService_RelationalDbRepository
  - ServerSide_IdpConnector
  
  #### .26.6. Properties
  
  
  #### .26.7. Interfaces
  
  - IAuthApi
  
  #### .26.8. Technology
  ASP.NET Core (.NET 8), ASP.NET Core Identity, OpenID Connect/OAuth2 libraries

  #### .26.9. Resources
  
  - **Cpu:** Medium
  - **Memory:** Medium
  - **Storage:** Low (logs)
  - **Network:** Medium (internal traffic, to IdP)
  
  #### .26.10. Configuration
  
  - **Jwt Issuer:** https://auth-service/identity
  - **Jwt Audience:** api://my-app
  - **Token Lifetime Minutes:** 60
  - **Password Min Length:** 12
  - **Account Lockout Threshold:** 5
  
  #### .26.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 15
  - **Timeout:** 3
  
  #### .26.12. Responsible Features
  
  - REQ-3-003
  - REQ-3-004
  - REQ-3-005
  - REQ-3-010
  - REQ-3-012
  - REQ-3-016
  - REQ-9-001
  - REQ-9-002
  - REQ-9-003
  - REQ-9-018
  
  #### .26.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .27. Identity Provider Connector
  Handles communication with external Identity Providers (e.g., Keycloak, Azure AD, Okta) using OAuth 2.0/OpenID Connect protocols.

  #### .27.4. Type
  Adapter

  #### .27.5. Dependencies
  
  
  #### .27.6. Properties
  
  
  #### .27.7. Interfaces
  
  - IExternalIdpAuthenticator
  
  #### .27.8. Technology
  .NET 8, OpenID Connect Client Libraries

  #### .27.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low
  - **Storage:** None
  - **Network:** Medium (to external IdP)
  
  #### .27.10. Configuration
  
  - **Idp Authority Url:** https://keycloak.example.com/auth/realms/myrealm
  - **Idp Client Id:** opc-system-client
  - **Idp Client Secret:** YOUR_CLIENT_SECRET
  - **Idp Scopes:** openid profile email roles
  
  #### .27.11. Health Check
  None

  #### .27.12. Responsible Features
  
  - REQ-3-004
  
  #### .27.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .28. External Integration Service API
  Microservice providing RESTful/gRPC APIs for connecting to external systems like IoT platforms, AR devices, Blockchain, and Digital Twins.

  #### .28.4. Type
  Service

  #### .28.5. Dependencies
  
  - ServerSide_DataService_TimeSeriesDbRepository
  - ServerSide_BlockchainConnector_Adapter
  - ServerSide_Messaging_MessageBusGateway
  
  #### .28.6. Properties
  
  
  #### .28.7. Interfaces
  
  - IExternalIntegrationApi
  
  #### .28.8. Technology
  ASP.NET Core (.NET 8), MQTT/AMQP client libraries, WebSocket libraries

  #### .28.9. Resources
  
  - **Cpu:** Medium
  - **Memory:** Medium
  - **Storage:** Low (logs, queue buffers)
  - **Network:** High (to various external systems)
  
  #### .28.10. Configuration
  
  - **Default Iot Platform:** AzureIoTHub
  - **Digital Twin Data Format:** DTDL
  
  #### .28.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 15
  - **Timeout:** 3
  
  #### .28.12. Responsible Features
  
  - REQ-8-001
  - REQ-8-002
  - REQ-8-003
  - REQ-8-004
  - REQ-8-005
  - REQ-8-006
  - REQ-8-007
  - REQ-8-008
  - REQ-8-009
  - REQ-8-010
  - REQ-8-011
  
  #### .28.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - Administrator
    - Engineer
    - SystemIntegrationRole
    
  
  ### .29. Blockchain Connector Adapter
  Handles interaction with the configured private permissioned blockchain (e.g., Hyperledger Fabric, Ethereum private network) for logging critical data exchanges.

  #### .29.4. Type
  Adapter

  #### .29.5. Dependencies
  
  - ServerSide_DataService_RelationalDbRepository
  
  #### .29.6. Properties
  
  
  #### .29.7. Interfaces
  
  - IBlockchainLogger
  
  #### .29.8. Technology
  .NET 8, Nethereum / Fabric SDK for .NET

  #### .29.9. Resources
  
  - **Cpu:** Low-Medium (during transaction submission)
  - **Memory:** Medium
  - **Storage:** Low (transaction logs)
  - **Network:** Medium (to blockchain nodes)
  
  #### .29.10. Configuration
  
  - **Blockchain Type:** HyperledgerFabric
  - **Blockchain Network Endpoint:** grpc://fabric-peer0:7051
  - **Chaincode Name:** critical_data_log
  - **Channel Name:** mychannel
  - **Wallet Path:** /app/wallet/
  
  #### .29.11. Health Check
  None

  #### .29.12. Responsible Features
  
  - REQ-8-007
  - REQ-8-008
  - REQ-DLP-025
  - REQ-DLP-026
  
  #### .29.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .30. Message Bus Gateway
  Provides an abstraction layer for publishing messages to and consuming messages from the message broker (RabbitMQ/Kafka). Used by various services for asynchronous communication.

  #### .30.4. Type
  Gateway

  #### .30.5. Dependencies
  
  
  #### .30.6. Properties
  
  
  #### .30.7. Interfaces
  
  - IMessagePublisher
  - IMessageSubscriber
  
  #### .30.8. Technology
  .NET 8, RabbitMQ.Client / Confluent.Kafka (wrapper libraries)

  #### .30.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low-Medium (connection pools, internal buffers)
  - **Storage:** None
  - **Network:** Medium (to message broker)
  
  #### .30.10. Configuration
  
  - **Message Broker Host:** rabbitmq-server
  - **Message Broker Port:** 5672
  - **Message Broker Username:** user
  - **Message Broker Password:** password
  - **Default Exchange Name:** system_events
  
  #### .30.11. Health Check
  None

  #### .30.12. Responsible Features
  
  - REQ-SAP-003
  
  #### .30.13. Security
  
  - **Requires Authentication:** False
  - **Requires Authorization:** False
  - **Allowed Roles:**
    
    
  
  ### .31. Structured Logging Utility
  A shared library providing consistent, structured logging capabilities (e.g., using Serilog) for all .NET components.

  #### .31.4. Type
  Utility

  #### .31.5. Dependencies
  
  
  #### .31.6. Properties
  
  
  #### .31.7. Interfaces
  
  - ILoggerAdapter
  
  #### .31.8. Technology
  .NET 8, Serilog

  #### .31.9. Resources
  
  - **Cpu:** Minimal
  - **Memory:** Minimal
  - **Storage:** None (delegates to sinks)
  - **Network:** Minimal (delegates to sinks)
  
  #### .31.10. Configuration
  
  - **Log Level Default:** Information
  - **Log Output Template:** {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}
  - **Elk Sink Url:** http://elk-stack:9200
  
  #### .31.11. Health Check
  None

  #### .31.12. Responsible Features
  
  - REQ-6-004
  - REQ-6-005
  
  #### .31.13. Security
  None

  ### .32. Metrics Collector Utility
  A shared library for collecting and exporting application metrics using OpenTelemetry standards.

  #### .32.4. Type
  Utility

  #### .32.5. Dependencies
  
  
  #### .32.6. Properties
  
  
  #### .32.7. Interfaces
  
  - IMetricsEmitter
  
  #### .32.8. Technology
  .NET 8, OpenTelemetry SDK

  #### .32.9. Resources
  
  - **Cpu:** Minimal
  - **Memory:** Minimal
  - **Storage:** None
  - **Network:** Minimal (to metrics backend)
  
  #### .32.10. Configuration
  
  - **Otel Exporter Endpoint:** http://prometheus:9090/api/v1/write
  - **Service Name:** OpcSystem
  
  #### .32.11. Health Check
  None

  #### .32.12. Responsible Features
  
  - REQ-6-004
  - REQ-6-005
  - REQ-6-006
  
  #### .32.13. Security
  None

  ### .33. Licensing Service
  Manages software license activation, validation, and enforcement. Supports tiered features and various licensing models.

  #### .33.4. Type
  Service

  #### .33.5. Dependencies
  
  - ServerSide_DataService_RelationalDbRepository
  
  #### .33.6. Properties
  
  
  #### .33.7. Interfaces
  
  - ILicenseValidator
  - ILicenseManagerApi
  
  #### .33.8. Technology
  ASP.NET Core (.NET 8), Custom or COTS licensing library

  #### .33.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low-Medium
  - **Storage:** Low (license keys, activation records)
  - **Network:** Low (for online activation/validation)
  
  #### .33.10. Configuration
  
  - **License Server Url:** https://licenses.example.com/api
  - **Grace Period Days:** 7
  - **Offline Activation Key Path:** /app/licenses/offline_keys/
  
  #### .33.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 60
  - **Timeout:** 10
  
  #### .33.12. Responsible Features
  
  - REQ-9-006
  - REQ-9-007
  - REQ-9-008
  
  #### .33.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - Administrator
    - SystemInternal
    
  
  ### .34. Notification Service
  Microservice responsible for dispatching notifications (e.g., email, SMS) based on system events, such as critical alarms or system health alerts.

  #### .34.4. Type
  Service

  #### .34.5. Dependencies
  
  - ServerSide_Messaging_MessageBusGateway
  - ServerSide_DataService_RelationalDbRepository
  
  #### .34.6. Properties
  
  
  #### .34.7. Interfaces
  
  - IMessageConsumer
  
  #### .34.8. Technology
  ASP.NET Core (.NET 8), SMTP client libraries, SMS gateway client libraries

  #### .34.9. Resources
  
  - **Cpu:** Low
  - **Memory:** Low
  - **Storage:** Low (logs, templates)
  - **Network:** Low-Medium (to SMTP/SMS gateways)
  
  #### .34.10. Configuration
  
  - **Smtp Server:** smtp.example.com
  - **Smtp Port:** 587
  - **Smtp User:** notify_user
  - **Smtp Password:** notify_password
  - **Sms Gateway Api Url:** https://api.smsgateway.example.com/send
  - **Sms Gateway Api Key:** YOUR_SMS_API_KEY
  - **Default Notification Channels:** Email,SMS
  - **Alarm Escalation Topic:** system.alarms.escalated
  - **System Alert Topic:** system.alerts.dispatch
  
  #### .34.11. Health Check
  
  - **Path:** /health/live
  - **Interval:** 30
  - **Timeout:** 5
  
  #### .34.12. Responsible Features
  
  - REQ-CSVC-020
  - REQ-6-005
  
  #### .34.13. Security
  
  - **Requires Authentication:** True
  - **Requires Authorization:** True
  - **Allowed Roles:**
    
    - SystemInternal
    
  
  
- **Configuration:**
  
  - **Global Log Level:** Information
  - **Enable Distributed Tracing:** true
  - **Default Api Version:** v1
  - **Message Queue System:** RabbitMQ
  - **Primary Database Provider:** PostgreSQL
  - **Time Series Database Provider:** InfluxDB
  - **Blob Storage Provider:** AzureBlobStorage
  - **Identity Provider:** Internal
  - **Deployment Environment:** Production
  - **Cross Origin Resource Sharing_Default Policy:** AllowSpecificOrigins
  - **System Base Url:** https://my-opc-system.example.com
  


---

# 2. Component_Relations

- **Relationships:**
  
  ### .1. rel_layer_CoreOpcClientService_to_ServerSideApplication_Presentation_01
  Core OPC Client Service communicates with the Server-Side Application's API Gateway (part of Presentation Layer) for synchronous requests (e.g., configuration). The dependency in CoreOpcClientService lists 'ServerSideApplication_ApiGateway', which is interpreted as the API gateway functionality within the Presentation Layer or the ServerSide_ApiGateway component.

  #### .1.2. Source Id
  CoreOpcClientService

  #### .1.3. Target Id
  ServerSideApplication_Presentation

  #### .1.4. Type
  Dependency

  #### .1.6. Properties
  
  - Synchronous
  - Secure
  - gRPC/REST
  
  #### .1.7. Configuration
  
  - **Protocol:** gRPC, REST
  - **Authentication:** JWT/Certificate
  
  ### .2. rel_layer_CoreOpcClientService_to_ServerSideApplication_Messaging_02
  Core OPC Client Service publishes real-time data and events to the Messaging Infrastructure.

  #### .2.2. Source Id
  CoreOpcClientService

  #### .2.3. Target Id
  ServerSideApplication_Messaging

  #### .2.4. Type
  Dependency

  #### .2.6. Properties
  
  - Asynchronous
  - Reliable
  - AMQP/Kafka
  
  #### .2.7. Configuration
  
  - **Protocol:** AMQP/Kafka
  - **Topics:** opc.data, opc.events
  
  ### .3. rel_layer_Presentation_to_ManagementService_03
  Presentation Layer (UI/API Gateway) calls Management Service for client management operations.

  #### .3.2. Source Id
  ServerSideApplication_Presentation

  #### .3.3. Target Id
  ServerSideApplication_ManagementService

  #### .3.4. Type
  Dependency

  #### .3.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .3.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .4. rel_layer_Presentation_to_AiService_04
  Presentation Layer calls AI Service for AI-related operations and data.

  #### .4.2. Source Id
  ServerSideApplication_Presentation

  #### .4.3. Target Id
  ServerSideApplication_AiService

  #### .4.4. Type
  Dependency

  #### .4.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .4.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .5. rel_layer_Presentation_to_ReportingService_05
  Presentation Layer calls Reporting Service for report generation and management.

  #### .5.2. Source Id
  ServerSideApplication_Presentation

  #### .5.3. Target Id
  ServerSideApplication_ReportingService

  #### .5.4. Type
  Dependency

  #### .5.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .5.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .6. rel_layer_Presentation_to_DataService_06
  Presentation Layer calls Data Service for direct data queries and streaming for the UI.

  #### .6.2. Source Id
  ServerSideApplication_Presentation

  #### .6.3. Target Id
  ServerSideApplication_DataService

  #### .6.4. Type
  Dependency

  #### .6.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .6.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .7. rel_layer_Presentation_to_AuthService_07
  Presentation Layer integrates with Authentication & Authorization Service.

  #### .7.2. Source Id
  ServerSideApplication_Presentation

  #### .7.3. Target Id
  ServerSideApplication_AuthService

  #### .7.4. Type
  Dependency

  #### .7.6. Properties
  
  - Internal API
  - HTTP/JSON or gRPC
  
  #### .7.7. Configuration
  
  - **Protocol:** HTTP/JSON or gRPC
  
  ### .8. rel_layer_ManagementService_to_DataService_08
  Management Service uses Data Service to store/retrieve client configurations.

  #### .8.2. Source Id
  ServerSideApplication_ManagementService

  #### .8.3. Target Id
  ServerSideApplication_DataService

  #### .8.4. Type
  Dependency

  #### .8.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .8.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .9. rel_layer_ManagementService_to_Messaging_09
  Management Service (optionally) uses Messaging Infrastructure for asynchronous operations.

  #### .9.2. Source Id
  ServerSideApplication_ManagementService

  #### .9.3. Target Id
  ServerSideApplication_Messaging

  #### .9.4. Type
  Dependency

  #### .9.6. Properties
  
  - Asynchronous
  - Optional
  - AMQP/Kafka
  
  #### .9.7. Configuration
  
  - **Protocol:** AMQP/Kafka
  
  ### .10. rel_layer_AiService_to_DataService_10
  AI Service uses Data Service to fetch data for models and store model artifacts.

  #### .10.2. Source Id
  ServerSideApplication_AiService

  #### .10.3. Target Id
  ServerSideApplication_DataService

  #### .10.4. Type
  Dependency

  #### .10.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .10.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .11. rel_layer_AiService_to_Messaging_11
  AI Service (optionally) uses Messaging Infrastructure for data streams or events.

  #### .11.2. Source Id
  ServerSideApplication_AiService

  #### .11.3. Target Id
  ServerSideApplication_Messaging

  #### .11.4. Type
  Dependency

  #### .11.6. Properties
  
  - Asynchronous
  - Optional
  - AMQP/Kafka
  
  #### .11.7. Configuration
  
  - **Protocol:** AMQP/Kafka
  
  ### .12. rel_layer_ReportingService_to_DataService_12
  Reporting Service uses Data Service to fetch data for reports.

  #### .12.2. Source Id
  ServerSideApplication_ReportingService

  #### .12.3. Target Id
  ServerSideApplication_DataService

  #### .12.4. Type
  Dependency

  #### .12.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .12.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .13. rel_layer_ReportingService_to_AiService_13
  Reporting Service (optionally) queries AI Service for insights.

  #### .13.2. Source Id
  ServerSideApplication_ReportingService

  #### .13.3. Target Id
  ServerSideApplication_AiService

  #### .13.4. Type
  Dependency

  #### .13.6. Properties
  
  - Internal API
  - Optional
  - gRPC/REST
  
  #### .13.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .14. rel_layer_AuthService_to_DataService_14
  Auth Service uses Data Service to store user and role information.

  #### .14.2. Source Id
  ServerSideApplication_AuthService

  #### .14.3. Target Id
  ServerSideApplication_DataService

  #### .14.4. Type
  Dependency

  #### .14.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .14.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .15. rel_layer_ExternalIntegrationService_to_DataService_15
  External Integration Service uses Data Service for integration-related data.

  #### .15.2. Source Id
  ServerSideApplication_ExternalIntegrationService

  #### .15.3. Target Id
  ServerSideApplication_DataService

  #### .15.4. Type
  Dependency

  #### .15.6. Properties
  
  - Internal API
  - gRPC/REST
  
  #### .15.7. Configuration
  
  - **Protocol:** gRPC/REST
  
  ### .16. rel_layer_ExternalIntegrationService_to_Messaging_16
  External Integration Service (optionally) uses Messaging for asynchronous external communication.

  #### .16.2. Source Id
  ServerSideApplication_ExternalIntegrationService

  #### .16.3. Target Id
  ServerSideApplication_Messaging

  #### .16.4. Type
  Dependency

  #### .16.6. Properties
  
  - Asynchronous
  - Optional
  - AMQP/Kafka
  
  #### .16.7. Configuration
  
  - **Protocol:** AMQP/Kafka
  
  ### .17. rel_layer_Service_to_CrossCutting_Mgmt_17
  Management Service utilizes shared Cross-Cutting libraries (logging, monitoring, config).

  #### .17.2. Source Id
  ServerSideApplication_ManagementService

  #### .17.3. Target Id
  ServerSideApplication_CrossCutting

  #### .17.4. Type
  Dependency

  #### .17.6. Properties
  
  - LibraryLinkage
  - SharedFunctionality
  
  #### .17.7. Configuration
  
  
  ### .18. rel_layer_Service_to_CrossCutting_Ai_18
  AI Service utilizes shared Cross-Cutting libraries.

  #### .18.2. Source Id
  ServerSideApplication_AiService

  #### .18.3. Target Id
  ServerSideApplication_CrossCutting

  #### .18.4. Type
  Dependency

  #### .18.6. Properties
  
  - LibraryLinkage
  - SharedFunctionality
  
  #### .18.7. Configuration
  
  
  ### .19. rel_layer_Service_to_CrossCutting_Reporting_19
  Reporting Service utilizes shared Cross-Cutting libraries.

  #### .19.2. Source Id
  ServerSideApplication_ReportingService

  #### .19.3. Target Id
  ServerSideApplication_CrossCutting

  #### .19.4. Type
  Dependency

  #### .19.6. Properties
  
  - LibraryLinkage
  - SharedFunctionality
  
  #### .19.7. Configuration
  
  
  ### .20. rel_layer_Service_to_CrossCutting_Data_20
  Data Service utilizes shared Cross-Cutting libraries.

  #### .20.2. Source Id
  ServerSideApplication_DataService

  #### .20.3. Target Id
  ServerSideApplication_CrossCutting

  #### .20.4. Type
  Dependency

  #### .20.6. Properties
  
  - LibraryLinkage
  - SharedFunctionality
  
  #### .20.7. Configuration
  
  
  ### .21. rel_layer_Service_to_CrossCutting_Auth_21
  Auth Service utilizes shared Cross-Cutting libraries.

  #### .21.2. Source Id
  ServerSideApplication_AuthService

  #### .21.3. Target Id
  ServerSideApplication_CrossCutting

  #### .21.4. Type
  Dependency

  #### .21.6. Properties
  
  - LibraryLinkage
  - SharedFunctionality
  
  #### .21.7. Configuration
  
  
  ### .22. rel_layer_Service_to_CrossCutting_ExtInt_22
  External Integration Service utilizes shared Cross-Cutting libraries.

  #### .22.2. Source Id
  ServerSideApplication_ExternalIntegrationService

  #### .22.3. Target Id
  ServerSideApplication_CrossCutting

  #### .22.4. Type
  Dependency

  #### .22.6. Properties
  
  - LibraryLinkage
  - SharedFunctionality
  
  #### .22.7. Configuration
  
  
  ### .23. rel_layer_Service_to_CrossCutting_Pres_23
  Presentation Layer utilizes shared Cross-Cutting libraries.

  #### .23.2. Source Id
  ServerSideApplication_Presentation

  #### .23.3. Target Id
  ServerSideApplication_CrossCutting

  #### .23.4. Type
  Dependency

  #### .23.6. Properties
  
  - LibraryLinkage
  - SharedFunctionality
  
  #### .23.7. Configuration
  
  
  ### .24. rel_layer_CoreOpcClient_to_ReusableLibs_24
  Core OPC Client Service utilizes Reusable Domain & Utility Libraries.

  #### .24.2. Source Id
  CoreOpcClientService

  #### .24.3. Target Id
  ReusableLibraries

  #### .24.4. Type
  Dependency

  #### .24.6. Properties
  
  - LibraryLinkage
  - DomainLogic
  
  #### .24.7. Configuration
  
  
  ### .25. rel_comp_OpcDaClient_to_ClientServerCommunicator_01
  OPC DA Client uses Client-Server Communicator to send data/status to the server-side application.

  #### .25.2. Source Id
  CoreOpcClientService_OpcDaClient

  #### .25.3. Target Id
  CoreOpcClientService_ClientServerCommunicator

  #### .25.4. Type
  Dependency

  #### .25.6. Properties
  
  - InternalCall
  - Synchronous/Asynchronous
  
  #### .25.7. Configuration
  
  - **Interface:** IServerboundSynchronousComms, IServerboundAsynchronousComms
  
  ### .26. rel_comp_OpcDaClient_to_ClientWriteAuditor_02
  OPC DA Client uses Client Write Auditor to log critical write operations.

  #### .26.2. Source Id
  CoreOpcClientService_OpcDaClient

  #### .26.3. Target Id
  CoreOpcClientService_ClientWriteAuditor

  #### .26.4. Type
  Dependency

  #### .26.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .26.7. Configuration
  
  - **Interface:** IWriteOperationLogger
  
  ### .27. rel_comp_OpcDaClient_to_ClientWriteController_03
  OPC DA Client uses Client Write Controller to enforce write limits and validate data.

  #### .27.2. Source Id
  CoreOpcClientService_OpcDaClient

  #### .27.3. Target Id
  CoreOpcClientService_ClientWriteController

  #### .27.4. Type
  Dependency

  #### .27.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .27.7. Configuration
  
  - **Interface:** IWriteOperationValidator, IWriteOperationLimiter
  
  ### .28. rel_comp_OpcUaClient_to_ClientSubscriptionManager_04
  OPC UA Client uses Client Subscription Manager to handle UA subscriptions.

  #### .28.2. Source Id
  CoreOpcClientService_OpcUaClient

  #### .28.3. Target Id
  CoreOpcClientService_ClientSubscriptionManager

  #### .28.4. Type
  Dependency

  #### .28.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .28.7. Configuration
  
  - **Interface:** ISubscriptionManager
  
  ### .29. rel_comp_OpcUaClient_to_ClientServerCommunicator_05
  OPC UA Client uses Client-Server Communicator.

  #### .29.2. Source Id
  CoreOpcClientService_OpcUaClient

  #### .29.3. Target Id
  CoreOpcClientService_ClientServerCommunicator

  #### .29.4. Type
  Dependency

  #### .29.6. Properties
  
  - InternalCall
  - Synchronous/Asynchronous
  
  #### .29.7. Configuration
  
  - **Interface:** IServerboundSynchronousComms, IServerboundAsynchronousComms
  
  ### .30. rel_comp_OpcUaClient_to_ClientWriteAuditor_06
  OPC UA Client uses Client Write Auditor.

  #### .30.2. Source Id
  CoreOpcClientService_OpcUaClient

  #### .30.3. Target Id
  CoreOpcClientService_ClientWriteAuditor

  #### .30.4. Type
  Dependency

  #### .30.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .30.7. Configuration
  
  - **Interface:** IWriteOperationLogger
  
  ### .31. rel_comp_OpcUaClient_to_ClientWriteController_07
  OPC UA Client uses Client Write Controller.

  #### .31.2. Source Id
  CoreOpcClientService_OpcUaClient

  #### .31.3. Target Id
  CoreOpcClientService_ClientWriteController

  #### .31.4. Type
  Dependency

  #### .31.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .31.7. Configuration
  
  - **Interface:** IWriteOperationValidator, IWriteOperationLimiter
  
  ### .32. rel_comp_OpcUaClient_to_SecurityCertificateManager_08
  OPC UA Client uses Security Certificate Manager for handling UA security certificates.

  #### .32.2. Source Id
  CoreOpcClientService_OpcUaClient

  #### .32.3. Target Id
  CoreOpcClientService_SecurityCertificateManager

  #### .32.4. Type
  Dependency

  #### .32.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .32.7. Configuration
  
  - **Interface:** ICertificateProvider, ITrustListManager
  
  ### .33. rel_comp_OpcXmlDaClient_to_ClientServerCommunicator_09
  OPC XML-DA Client uses Client-Server Communicator.

  #### .33.2. Source Id
  CoreOpcClientService_OpcXmlDaClient

  #### .33.3. Target Id
  CoreOpcClientService_ClientServerCommunicator

  #### .33.4. Type
  Dependency

  #### .33.6. Properties
  
  - InternalCall
  - Synchronous/Asynchronous
  
  #### .33.7. Configuration
  
  - **Interface:** IServerboundSynchronousComms, IServerboundAsynchronousComms
  
  ### .34. rel_comp_OpcXmlDaClient_to_ClientWriteAuditor_10
  OPC XML-DA Client uses Client Write Auditor.

  #### .34.2. Source Id
  CoreOpcClientService_OpcXmlDaClient

  #### .34.3. Target Id
  CoreOpcClientService_ClientWriteAuditor

  #### .34.4. Type
  Dependency

  #### .34.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .34.7. Configuration
  
  - **Interface:** IWriteOperationLogger
  
  ### .35. rel_comp_OpcXmlDaClient_to_ClientWriteController_11
  OPC XML-DA Client uses Client Write Controller.

  #### .35.2. Source Id
  CoreOpcClientService_OpcXmlDaClient

  #### .35.3. Target Id
  CoreOpcClientService_ClientWriteController

  #### .35.4. Type
  Dependency

  #### .35.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .35.7. Configuration
  
  - **Interface:** IWriteOperationValidator, IWriteOperationLimiter
  
  ### .36. rel_comp_OpcHdaClient_to_ClientServerCommunicator_12
  OPC HDA Client uses Client-Server Communicator to send retrieved historical data.

  #### .36.2. Source Id
  CoreOpcClientService_OpcHdaClient

  #### .36.3. Target Id
  CoreOpcClientService_ClientServerCommunicator

  #### .36.4. Type
  Dependency

  #### .36.6. Properties
  
  - InternalCall
  - Asynchronous
  
  #### .36.7. Configuration
  
  - **Interface:** IServerboundAsynchronousComms
  
  ### .37. rel_comp_OpcAcClient_to_ClientServerCommunicator_13
  OPC A&C Client uses Client-Server Communicator to send alarm/event data.

  #### .37.2. Source Id
  CoreOpcClientService_OpcAcClient

  #### .37.3. Target Id
  CoreOpcClientService_ClientServerCommunicator

  #### .37.4. Type
  Dependency

  #### .37.6. Properties
  
  - InternalCall
  - Asynchronous
  
  #### .37.7. Configuration
  
  - **Interface:** IServerboundAsynchronousComms
  
  ### .38. rel_comp_ClientWriteAuditor_to_ClientServerCommunicator_14
  Client Write Auditor uses Client-Server Communicator to send audit logs to the server.

  #### .38.2. Source Id
  CoreOpcClientService_ClientWriteAuditor

  #### .38.3. Target Id
  CoreOpcClientService_ClientServerCommunicator

  #### .38.4. Type
  Dependency

  #### .38.6. Properties
  
  - InternalCall
  - Asynchronous
  
  #### .38.7. Configuration
  
  - **Interface:** IServerboundAsynchronousComms
  
  ### .39. rel_comp_ClientSubscriptionManager_to_ClientDataBuffer_15
  Client Subscription Manager uses Client Data Buffer for temporary data storage during outages.

  #### .39.2. Source Id
  CoreOpcClientService_ClientSubscriptionManager

  #### .39.3. Target Id
  CoreOpcClientService_ClientDataBuffer

  #### .39.4. Type
  Dependency

  #### .39.6. Properties
  
  - InternalCall
  - Synchronous
  
  #### .39.7. Configuration
  
  - **Interface:** IDataBuffer
  
  ### .40. rel_comp_ClientSubscriptionManager_to_ClientServerCommunicator_16
  Client Subscription Manager uses Client-Server Communicator to send subscription data and status.

  #### .40.2. Source Id
  CoreOpcClientService_ClientSubscriptionManager

  #### .40.3. Target Id
  CoreOpcClientService_ClientServerCommunicator

  #### .40.4. Type
  Dependency

  #### .40.6. Properties
  
  - InternalCall
  - Asynchronous
  
  #### .40.7. Configuration
  
  - **Interface:** IServerboundAsynchronousComms
  
  ### .41. rel_comp_ApiGateway_to_AuthServiceApi_17
  API Gateway routes authentication/authorization requests and validates tokens using Auth Service API.

  #### .41.2. Source Id
  ServerSide_ApiGateway

  #### .41.3. Target Id
  ServerSide_AuthService_Api

  #### .41.4. Type
  Dependency

  #### .41.6. Properties
  
  - NetworkCall
  - Synchronous
  - Secure
  
  #### .41.7. Configuration
  
  - **Protocol:** gRPC/REST
  - **Interface:** IAuthApi
  
  ### .42. rel_comp_ApiGateway_to_ManagementServiceApi_18
  API Gateway routes requests to Management Service API.

  #### .42.2. Source Id
  ServerSide_ApiGateway

  #### .42.3. Target Id
  ServerSide_ManagementService_Api

  #### .42.4. Type
  Dependency

  #### .42.6. Properties
  
  - NetworkCall
  - Synchronous
  - Secure
  
  #### .42.7. Configuration
  
  - **Protocol:** gRPC/REST
  - **Interface:** IManagementApi
  
  ### .43. rel_comp_ApiGateway_to_AiServiceApi_19
  API Gateway routes requests to AI Processing Service API.

  #### .43.2. Source Id
  ServerSide_ApiGateway

  #### .43.3. Target Id
  ServerSide_AiService_Api

  #### .43.4. Type
  Dependency

  #### .43.6. Properties
  
  - NetworkCall
  - Synchronous
  - Secure
  
  #### .43.7. Configuration
  
  - **Protocol:** gRPC/REST
  - **Interface:** IAiApi
  
  ### .44. rel_comp_ApiGateway_to_ReportingServiceApi_20
  API Gateway routes requests to Reporting Service API.

  #### .44.2. Source Id
  ServerSide_ApiGateway

  #### .44.3. Target Id
  ServerSide_ReportingService_Api

  #### .44.4. Type
  Dependency

  #### .44.6. Properties
  
  - NetworkCall
  - Synchronous
  - Secure
  
  #### .44.7. Configuration
  
  - **Protocol:** gRPC/REST
  - **Interface:** IReportingApi
  
  ### .45. rel_comp_ApiGateway_to_DataQueryEndpoints_21
  API Gateway routes data query requests to Data Service Query Endpoints. This fulfills the conceptual 'ServerSide_DataService_Api' dependency.

  #### .45.2. Source Id
  ServerSide_ApiGateway

  #### .45.3. Target Id
  ServerSide_DataService_DataQueryEndpoints

  #### .45.4. Type
  Dependency

  #### .45.6. Properties
  
  - NetworkCall
  - Synchronous
  - Secure
  
  #### .45.7. Configuration
  
  - **Protocol:** gRPC/REST
  - **Interface:** IDataAccessServiceApi (subset)
  
  ### .46. rel_comp_ApiGateway_to_ExternalIntegrationServiceApi_22
  API Gateway routes requests to External Integration Service API.

  #### .46.2. Source Id
  ServerSide_ApiGateway

  #### .46.3. Target Id
  ServerSide_ExternalIntegrationService_Api

  #### .46.4. Type
  Dependency

  #### .46.6. Properties
  
  - NetworkCall
  - Synchronous
  - Secure
  
  #### .46.7. Configuration
  
  - **Protocol:** gRPC/REST
  - **Interface:** IExternalIntegrationApi
  
  ### .47. rel_comp_WebAppHost_to_ApiGateway_23
  Web Application Host (serving Blazor WASM) interacts with backend services via the API Gateway.

  #### .47.2. Source Id
  ServerSide_WebAppHost

  #### .47.3. Target Id
  ServerSide_ApiGateway

  #### .47.4. Type
  Dependency

  #### .47.6. Properties
  
  - NetworkCall
  - Synchronous
  - Secure
  
  #### .47.7. Configuration
  
  - **Protocol:** HTTP/JSON, WebSockets
  
  ### .48. rel_comp_ManagementServiceApi_to_RelationalDbRepo_24
  Management Service API uses Relational DB Repository for storing and retrieving configurations and metadata.

  #### .48.2. Source Id
  ServerSide_ManagementService_Api

  #### .48.3. Target Id
  ServerSide_DataService_RelationalDbRepository

  #### .48.4. Type
  Dependency

  #### .48.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .48.7. Configuration
  
  - **Interface:** IConfigurationRepository, IOpcMetadataRepository
  
  ### .49. rel_comp_ManagementServiceApi_to_MessageBusGateway_25
  Management Service API uses Message Bus Gateway for asynchronous communication.

  #### .49.2. Source Id
  ServerSide_ManagementService_Api

  #### .49.3. Target Id
  ServerSide_Messaging_MessageBusGateway

  #### .49.4. Type
  Dependency

  #### .49.6. Properties
  
  - InternalCall/NetworkCall
  - Asynchronous
  
  #### .49.7. Configuration
  
  - **Interface:** IMessagePublisher, IMessageSubscriber
  
  ### .50. rel_comp_AiServiceApi_to_TimeSeriesDbRepo_26
  AI Service API uses Time-Series DB Repository for historical data.

  #### .50.2. Source Id
  ServerSide_AiService_Api

  #### .50.3. Target Id
  ServerSide_DataService_TimeSeriesDbRepository

  #### .50.4. Type
  Dependency

  #### .50.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .50.7. Configuration
  
  - **Interface:** IHistoricalDataRepository
  
  ### .51. rel_comp_AiServiceApi_to_BlobStorageRepo_27
  AI Service API uses Blob Storage Repository for AI model artifacts.

  #### .51.2. Source Id
  ServerSide_AiService_Api

  #### .51.3. Target Id
  ServerSide_DataService_BlobStorageRepository

  #### .51.4. Type
  Dependency

  #### .51.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .51.7. Configuration
  
  - **Interface:** IAiModelArtifactRepository
  
  ### .52. rel_comp_AiServiceApi_to_NlpServiceProvider_28
  AI Service API uses NLP Service Provider for natural language query processing.

  #### .52.2. Source Id
  ServerSide_AiService_Api

  #### .52.3. Target Id
  ServerSide_NlpServiceProvider

  #### .52.4. Type
  Dependency

  #### .52.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .52.7. Configuration
  
  - **Interface:** INlpProcessor
  
  ### .53. rel_comp_ReportingServiceApi_to_TimeSeriesDbRepo_29
  Reporting Service API uses Time-Series DB Repository for report data.

  #### .53.2. Source Id
  ServerSide_ReportingService_Api

  #### .53.3. Target Id
  ServerSide_DataService_TimeSeriesDbRepository

  #### .53.4. Type
  Dependency

  #### .53.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .53.7. Configuration
  
  - **Interface:** IHistoricalDataRepository
  
  ### .54. rel_comp_ReportingServiceApi_to_AiServiceApi_30
  Reporting Service API queries AI Service API for AI-driven insights in reports.

  #### .54.2. Source Id
  ServerSide_ReportingService_Api

  #### .54.3. Target Id
  ServerSide_AiService_Api

  #### .54.4. Type
  Dependency

  #### .54.6. Properties
  
  - NetworkCall
  - Synchronous
  
  #### .54.7. Configuration
  
  - **Interface:** IAiApi
  
  ### .55. rel_comp_DataIngestionProcessor_to_MessageBusGateway_31
  Data Ingestion Processor consumes messages from the Message Bus Gateway.

  #### .55.2. Source Id
  ServerSide_DataService_DataIngestionProcessor

  #### .55.3. Target Id
  ServerSide_Messaging_MessageBusGateway

  #### .55.4. Type
  Dependency

  #### .55.6. Properties
  
  - InternalCall/NetworkCall
  - Asynchronous
  
  #### .55.7. Configuration
  
  - **Interface:** IMessageSubscriber
  
  ### .56. rel_comp_DataIngestionProcessor_to_TimeSeriesDbRepo_32
  Data Ingestion Processor stores data in Time-Series DB Repository.

  #### .56.2. Source Id
  ServerSide_DataService_DataIngestionProcessor

  #### .56.3. Target Id
  ServerSide_DataService_TimeSeriesDbRepository

  #### .56.4. Type
  Dependency

  #### .56.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .56.7. Configuration
  
  - **Interface:** IHistoricalDataRepository, IAlarmEventRepository
  
  ### .57. rel_comp_DataIngestionProcessor_to_RelationalDbRepo_33
  Data Ingestion Processor stores audit logs or structured data in Relational DB Repository.

  #### .57.2. Source Id
  ServerSide_DataService_DataIngestionProcessor

  #### .57.3. Target Id
  ServerSide_DataService_RelationalDbRepository

  #### .57.4. Type
  Dependency

  #### .57.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .57.7. Configuration
  
  - **Interface:** IAuditLogRepository
  
  ### .58. rel_comp_DataRetentionManager_to_RelationalDbRepo_34
  Data Retention Manager interacts with Relational DB Repository for retention policies.

  #### .58.2. Source Id
  ServerSide_DataService_DataRetentionManager

  #### .58.3. Target Id
  ServerSide_DataService_RelationalDbRepository

  #### .58.4. Type
  Dependency

  #### .58.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .58.7. Configuration
  
  
  ### .59. rel_comp_DataRetentionManager_to_TimeSeriesDbRepo_35
  Data Retention Manager interacts with Time-Series DB Repository for retention policies.

  #### .59.2. Source Id
  ServerSide_DataService_DataRetentionManager

  #### .59.3. Target Id
  ServerSide_DataService_TimeSeriesDbRepository

  #### .59.4. Type
  Dependency

  #### .59.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .59.7. Configuration
  
  
  ### .60. rel_comp_DataRetentionManager_to_BlobStorageRepo_36
  Data Retention Manager interacts with Blob Storage Repository for archiving.

  #### .60.2. Source Id
  ServerSide_DataService_DataRetentionManager

  #### .60.3. Target Id
  ServerSide_DataService_BlobStorageRepository

  #### .60.4. Type
  Dependency

  #### .60.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .60.7. Configuration
  
  
  ### .61. rel_comp_DataMigrationManager_to_RelationalDbRepo_37
  Data Migration Manager uses Relational DB Repository for migrating structured data.

  #### .61.2. Source Id
  ServerSide_DataService_DataMigrationManager

  #### .61.3. Target Id
  ServerSide_DataService_RelationalDbRepository

  #### .61.4. Type
  Dependency

  #### .61.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .61.7. Configuration
  
  
  ### .62. rel_comp_DataMigrationManager_to_TimeSeriesDbRepo_38
  Data Migration Manager uses Time-Series DB Repository for migrating time-series data.

  #### .62.2. Source Id
  ServerSide_DataService_DataMigrationManager

  #### .62.3. Target Id
  ServerSide_DataService_TimeSeriesDbRepository

  #### .62.4. Type
  Dependency

  #### .62.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .62.7. Configuration
  
  
  ### .63. rel_comp_AuthServiceApi_to_RelationalDbRepo_39
  Auth Service API uses Relational DB Repository for user store, roles, and permissions.

  #### .63.2. Source Id
  ServerSide_AuthService_Api

  #### .63.3. Target Id
  ServerSide_DataService_RelationalDbRepository

  #### .63.4. Type
  Dependency

  #### .63.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .63.7. Configuration
  
  - **Interface:** IUserRepository, IRoleRepository, IPermissionRepository
  
  ### .64. rel_comp_AuthServiceApi_to_IdpConnector_40
  Auth Service API uses Identity Provider Connector for external IdP authentication.

  #### .64.2. Source Id
  ServerSide_AuthService_Api

  #### .64.3. Target Id
  ServerSide_IdpConnector

  #### .64.4. Type
  Dependency

  #### .64.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .64.7. Configuration
  
  - **Interface:** IExternalIdpAuthenticator
  
  ### .65. rel_comp_ExternalIntegrationServiceApi_to_TimeSeriesDbRepo_41
  External Integration Service API uses Time-Series DB Repository for data exchange.

  #### .65.2. Source Id
  ServerSide_ExternalIntegrationService_Api

  #### .65.3. Target Id
  ServerSide_DataService_TimeSeriesDbRepository

  #### .65.4. Type
  Dependency

  #### .65.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .65.7. Configuration
  
  - **Interface:** IHistoricalDataRepository
  
  ### .66. rel_comp_ExternalIntegrationServiceApi_to_BlockchainConnectorAdapter_42
  External Integration Service API uses Blockchain Connector Adapter for logging to blockchain.

  #### .66.2. Source Id
  ServerSide_ExternalIntegrationService_Api

  #### .66.3. Target Id
  ServerSide_BlockchainConnector_Adapter

  #### .66.4. Type
  Dependency

  #### .66.6. Properties
  
  - InternalCall/NetworkCall
  - Asynchronous
  
  #### .66.7. Configuration
  
  - **Interface:** IBlockchainLogger
  
  ### .67. rel_comp_ExternalIntegrationServiceApi_to_MessageBusGateway_43
  External Integration Service API uses Message Bus Gateway for asynchronous external communication.

  #### .67.2. Source Id
  ServerSide_ExternalIntegrationService_Api

  #### .67.3. Target Id
  ServerSide_Messaging_MessageBusGateway

  #### .67.4. Type
  Dependency

  #### .67.6. Properties
  
  - InternalCall/NetworkCall
  - Asynchronous
  
  #### .67.7. Configuration
  
  - **Interface:** IMessagePublisher, IMessageSubscriber
  
  ### .68. rel_comp_BlockchainConnectorAdapter_to_RelationalDbRepo_44
  Blockchain Connector Adapter may use Relational DB Repository for storing transaction metadata or logs.

  #### .68.2. Source Id
  ServerSide_BlockchainConnector_Adapter

  #### .68.3. Target Id
  ServerSide_DataService_RelationalDbRepository

  #### .68.4. Type
  Dependency

  #### .68.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .68.7. Configuration
  
  
  ### .69. rel_comp_LicensingService_to_RelationalDbRepo_45
  Licensing Service uses Relational DB Repository to store and validate license information.

  #### .69.2. Source Id
  ServerSide_LicensingService

  #### .69.3. Target Id
  ServerSide_DataService_RelationalDbRepository

  #### .69.4. Type
  Dependency

  #### .69.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .69.7. Configuration
  
  
  ### .70. rel_comp_NotificationService_to_MessageBusGateway_46
  Notification Service consumes events from Message Bus Gateway to trigger notifications.

  #### .70.2. Source Id
  ServerSide_NotificationService

  #### .70.3. Target Id
  ServerSide_Messaging_MessageBusGateway

  #### .70.4. Type
  Dependency

  #### .70.6. Properties
  
  - InternalCall/NetworkCall
  - Asynchronous
  
  #### .70.7. Configuration
  
  - **Interface:** IMessageSubscriber
  
  ### .71. rel_comp_NotificationService_to_RelationalDbRepo_47
  Notification Service may use Relational DB Repository for notification templates or user contact preferences.

  #### .71.2. Source Id
  ServerSide_NotificationService

  #### .71.3. Target Id
  ServerSide_DataService_RelationalDbRepository

  #### .71.4. Type
  Dependency

  #### .71.6. Properties
  
  - InternalCall/NetworkCall
  - Synchronous
  
  #### .71.7. Configuration
  
  
  ### .72. rel_comp_ServiceApi_to_StructuredLogger_Mgmt_48
  Management Service API uses Structured Logging Utility for logging.

  #### .72.2. Source Id
  ServerSide_ManagementService_Api

  #### .72.3. Target Id
  SharedLib_StructuredLogger

  #### .72.4. Type
  Dependency

  #### .72.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .72.7. Configuration
  
  - **Interface:** ILoggerAdapter
  
  ### .73. rel_comp_ServiceApi_to_MetricsCollector_Mgmt_49
  Management Service API uses Metrics Collector Utility for telemetry.

  #### .73.2. Source Id
  ServerSide_ManagementService_Api

  #### .73.3. Target Id
  SharedLib_MetricsCollector

  #### .73.4. Type
  Dependency

  #### .73.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .73.7. Configuration
  
  - **Interface:** IMetricsEmitter
  
  ### .74. rel_comp_ServiceApi_to_StructuredLogger_Ai_50
  AI Service API uses Structured Logging Utility.

  #### .74.2. Source Id
  ServerSide_AiService_Api

  #### .74.3. Target Id
  SharedLib_StructuredLogger

  #### .74.4. Type
  Dependency

  #### .74.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .74.7. Configuration
  
  - **Interface:** ILoggerAdapter
  
  ### .75. rel_comp_ServiceApi_to_MetricsCollector_Ai_51
  AI Service API uses Metrics Collector Utility.

  #### .75.2. Source Id
  ServerSide_AiService_Api

  #### .75.3. Target Id
  SharedLib_MetricsCollector

  #### .75.4. Type
  Dependency

  #### .75.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .75.7. Configuration
  
  - **Interface:** IMetricsEmitter
  
  ### .76. rel_comp_ServiceApi_to_StructuredLogger_Reporting_52
  Reporting Service API uses Structured Logging Utility.

  #### .76.2. Source Id
  ServerSide_ReportingService_Api

  #### .76.3. Target Id
  SharedLib_StructuredLogger

  #### .76.4. Type
  Dependency

  #### .76.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .76.7. Configuration
  
  - **Interface:** ILoggerAdapter
  
  ### .77. rel_comp_ServiceApi_to_MetricsCollector_Reporting_53
  Reporting Service API uses Metrics Collector Utility.

  #### .77.2. Source Id
  ServerSide_ReportingService_Api

  #### .77.3. Target Id
  SharedLib_MetricsCollector

  #### .77.4. Type
  Dependency

  #### .77.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .77.7. Configuration
  
  - **Interface:** IMetricsEmitter
  
  ### .78. rel_comp_DataRepo_to_StructuredLogger_Relational_54
  Relational DB Repository uses Structured Logging Utility.

  #### .78.2. Source Id
  ServerSide_DataService_RelationalDbRepository

  #### .78.3. Target Id
  SharedLib_StructuredLogger

  #### .78.4. Type
  Dependency

  #### .78.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .78.7. Configuration
  
  - **Interface:** ILoggerAdapter
  
  ### .79. rel_comp_DataRepo_to_MetricsCollector_Relational_55
  Relational DB Repository uses Metrics Collector Utility.

  #### .79.2. Source Id
  ServerSide_DataService_RelationalDbRepository

  #### .79.3. Target Id
  SharedLib_MetricsCollector

  #### .79.4. Type
  Dependency

  #### .79.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .79.7. Configuration
  
  - **Interface:** IMetricsEmitter
  
  ### .80. rel_comp_DataRepo_to_StructuredLogger_TimeSeries_56
  Time-Series DB Repository uses Structured Logging Utility.

  #### .80.2. Source Id
  ServerSide_DataService_TimeSeriesDbRepository

  #### .80.3. Target Id
  SharedLib_StructuredLogger

  #### .80.4. Type
  Dependency

  #### .80.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .80.7. Configuration
  
  - **Interface:** ILoggerAdapter
  
  ### .81. rel_comp_DataRepo_to_MetricsCollector_TimeSeries_57
  Time-Series DB Repository uses Metrics Collector Utility.

  #### .81.2. Source Id
  ServerSide_DataService_TimeSeriesDbRepository

  #### .81.3. Target Id
  SharedLib_MetricsCollector

  #### .81.4. Type
  Dependency

  #### .81.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .81.7. Configuration
  
  - **Interface:** IMetricsEmitter
  
  ### .82. rel_comp_ServiceApi_to_StructuredLogger_Auth_58
  Auth Service API uses Structured Logging Utility.

  #### .82.2. Source Id
  ServerSide_AuthService_Api

  #### .82.3. Target Id
  SharedLib_StructuredLogger

  #### .82.4. Type
  Dependency

  #### .82.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .82.7. Configuration
  
  - **Interface:** ILoggerAdapter
  
  ### .83. rel_comp_ServiceApi_to_MetricsCollector_Auth_59
  Auth Service API uses Metrics Collector Utility.

  #### .83.2. Source Id
  ServerSide_AuthService_Api

  #### .83.3. Target Id
  SharedLib_MetricsCollector

  #### .83.4. Type
  Dependency

  #### .83.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .83.7. Configuration
  
  - **Interface:** IMetricsEmitter
  
  ### .84. rel_comp_ServiceApi_to_StructuredLogger_ExtInt_60
  External Integration Service API uses Structured Logging Utility.

  #### .84.2. Source Id
  ServerSide_ExternalIntegrationService_Api

  #### .84.3. Target Id
  SharedLib_StructuredLogger

  #### .84.4. Type
  Dependency

  #### .84.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .84.7. Configuration
  
  - **Interface:** ILoggerAdapter
  
  ### .85. rel_comp_ServiceApi_to_MetricsCollector_ExtInt_61
  External Integration Service API uses Metrics Collector Utility.

  #### .85.2. Source Id
  ServerSide_ExternalIntegrationService_Api

  #### .85.3. Target Id
  SharedLib_MetricsCollector

  #### .85.4. Type
  Dependency

  #### .85.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .85.7. Configuration
  
  - **Interface:** IMetricsEmitter
  
  ### .86. rel_comp_CoreOpcClient_to_StructuredLogger_62
  Core OPC Client Service components use Structured Logging Utility (Serilog mentioned in tech stack).

  #### .86.2. Source Id
  CoreOpcClientService

  #### .86.3. Target Id
  SharedLib_StructuredLogger

  #### .86.4. Type
  Dependency

  #### .86.6. Properties
  
  - LibraryLinkage
  - Utility
  
  #### .86.7. Configuration
  
  - **Interface:** ILoggerAdapter
  
  ### .87. rel_comp_CoreOpcClient_to_MetricsCollector_63
  Core OPC Client Service components may use Metrics Collector Utility if OpenTelemetry is adopted client-side.

  #### .87.2. Source Id
  CoreOpcClientService

  #### .87.3. Target Id
  SharedLib_MetricsCollector

  #### .87.4. Type
  Dependency

  #### .87.6. Properties
  
  - LibraryLinkage
  - Utility
  - Optional
  
  #### .87.7. Configuration
  
  - **Interface:** IMetricsEmitter
  
  
- **Configuration:**
  
  - **Description:** Global relationship configuration. Defines conventions for inter-component communication.
  - **Default Communication Timeout Ms:** 5000
  - **Default Retry Attempts:** 3
  


---

