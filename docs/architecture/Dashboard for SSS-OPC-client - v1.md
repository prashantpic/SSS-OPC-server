# Specification

# 1. Dashboards

## 1.1. OPC Client Fleet Overview
Monitors the health, performance, and data acquisition status of all deployed Core OPC Client instances.

### 1.1.3. Type
operational

### 1.1.4. Panels

- **Title:** OPC Client Connection Status  
**Type:** status_matrix  
**Metrics:**
    
    - opc_client_connection_status
    
- **Title:** OPC UA Subscription Status  
**Type:** status_matrix  
**Metrics:**
    
    - opc_subscription_status
    
- **Title:** OPC Data Throughput (Datapoints/Sec)  
**Type:** counter  
**Metrics:**
    
    - opc_data_throughput
    
- **Title:** OPC DA Read/Write Latency (P99)  
**Type:** line_chart  
**Metrics:**
    
    - opc_da_read_write_latency_p99
    
- **Title:** OPC UA Subscription Latency (P99)  
**Type:** line_chart  
**Metrics:**
    
    - opc_ua_subscription_latency_p99
    
- **Title:** OPC HDA Query Time (P95)  
**Type:** line_chart  
**Metrics:**
    
    - opc_hda_query_time_p95
    
- **Title:** Critical Write Logging Failures  
**Type:** stat  
**Metrics:**
    
    - critical_write_log_failures
    

## 1.2. Server-Side Application Overview
Provides a summary of the health and performance of the central microservices and their key dependencies.

### 1.2.3. Type
operational

### 1.2.4. Panels

- **Title:** API Gateway P99 Latency (ms)  
**Type:** stat  
**Metrics:**
    
    - api_gateway_latency_p99
    
- **Title:** API Gateway Error Rate (%)  
**Type:** stat  
**Metrics:**
    
    - api_gateway_error_rate
    
- **Title:** Service Health Status  
**Type:** status_matrix  
**Metrics:**
    
    - ServerSide_ManagementService_Api_Health
    - ServerSide_AiService_Api_Health
    - ServerSide_ReportingService_Api_Health
    - ServerSide_DataService_DataIngestionProcessor_Health
    - ServerSide_AuthService_Api_Health
    - ServerSide_ExternalIntegrationService_Api_Health
    - ServerSide_LicensingService_Health
    - ServerSide_NotificationService_Health
    
- **Title:** Database Connectivity Status  
**Type:** status_matrix  
**Metrics:**
    
    - database_connection_status
    
- **Title:** Message Queue Connectivity Status  
**Type:** status_matrix  
**Metrics:**
    
    - message_queue_connection_status
    
- **Title:** Message Queue Length  
**Type:** gauge  
**Metrics:**
    
    - message_queue_length
    
- **Title:** AI Model Inference Duration (Histogram)  
**Type:** histogram  
**Metrics:**
    
    - ai_model_inference_duration
    
- **Title:** Report Generation Duration (Histogram)  
**Type:** histogram  
**Metrics:**
    
    - report_generation_duration
    
- **Title:** External Integration Status  
**Type:** status_matrix  
**Metrics:**
    
    - external_service_status
    
- **Title:** License Validation Failures  
**Type:** stat  
**Metrics:**
    
    - license_validation_failures
    
- **Title:** UI P95 Load Time (ms)  
**Type:** stat  
**Metrics:**
    
    - ui_dashboard_load_time_p95
    
- **Title:** UI P95 Interaction Response Time (ms)  
**Type:** stat  
**Metrics:**
    
    - ui_interaction_response_time_p95
    

## 1.3. Infrastructure Resource Usage
Monitors CPU, Memory, Disk, and Network usage for the hosting environment.

### 1.3.3. Type
infrastructure

### 1.3.4. Panels

- **Title:** Server CPU Utilization (Aggregated %)  
**Type:** line_chart  
**Metrics:**
    
    - cpu_utilization
    
- **Title:** Server Memory Usage (Aggregated %)  
**Type:** line_chart  
**Metrics:**
    
    - memory_usage
    
- **Title:** Server Network Traffic (Aggregated Bytes/Sec)  
**Type:** line_chart  
**Metrics:**
    
    - network_bandwidth_used
    
- **Title:** Server Disk Space Available (GB)  
**Type:** stat  
**Metrics:**
    
    - disk_space_available
    
- **Title:** .NET GC Collections (Rate/Min)  
**Type:** counter  
**Metrics:**
    
    - dotnet_gc_collections
    
- **Title:** Message Broker Queue Length  
**Type:** gauge  
**Metrics:**
    
    - message_queue_length
    



---

