# Specification

# 1. Standard Metrics

## 1.1. cpu_utilization
### 1.1.2. Type
gauge

### 1.1.3. Unit
percent

### 1.1.4. Collection

- **Interval:** 15s
- **Method:** node_exporter

## 1.2. memory_usage
### 1.2.2. Type
gauge

### 1.2.3. Unit
bytes

### 1.2.4. Collection

- **Interval:** 30s
- **Method:** node_exporter

## 1.3. disk_space_available
### 1.3.2. Type
gauge

### 1.3.3. Unit
bytes

### 1.3.4. Collection

- **Interval:** 1m
- **Method:** node_exporter

## 1.4. network_bandwidth_used
### 1.4.2. Type
gauge

### 1.4.3. Unit
bytes_per_second

### 1.4.4. Collection

- **Interval:** 1m
- **Method:** node_exporter

## 1.5. http_request_duration
### 1.5.2. Type
histogram

### 1.5.3. Unit
milliseconds

### 1.5.4. Collection

- **Interval:** 1s
- **Method:** OpenTelemetry

## 1.6. grpc_request_duration
### 1.6.2. Type
histogram

### 1.6.3. Unit
milliseconds

### 1.6.4. Collection

- **Interval:** 1s
- **Method:** OpenTelemetry

## 1.7. dotnet_gc_collections
### 1.7.2. Type
counter

### 1.7.3. Unit
collections

### 1.7.4. Collection

- **Interval:** 1m
- **Method:** dotnet_micrometer

## 1.8. message_queue_length
### 1.8.2. Type
gauge

### 1.8.3. Unit
messages

### 1.8.4. Collection

- **Interval:** 15s
- **Method:** rabbitmq_exporter



---

# 2. Custom Metrics

## 2.1. opc_client_connection_status
### 2.1.2. Type
gauge

### 2.1.3. Unit
status

### 2.1.4. Collection

- **Interval:** 10s
- **Method:** CoreOpcClientService

## 2.2. opc_subscription_status
### 2.2.2. Type
gauge

### 2.2.3. Unit
status

### 2.2.4. Collection

- **Interval:** 10s
- **Method:** CoreOpcClientService

## 2.3. opc_data_throughput
### 2.3.2. Type
counter

### 2.3.3. Unit
datapoints_per_second

### 2.3.4. Collection

- **Interval:** 1m
- **Method:** CoreOpcClientService

## 2.4. ai_model_inference_duration
### 2.4.2. Type
histogram

### 2.4.3. Unit
milliseconds

### 2.4.4. Collection

- **Interval:** 1s
- **Method:** OpenTelemetry

## 2.5. report_generation_duration
### 2.5.2. Type
histogram

### 2.5.3. Unit
milliseconds

### 2.5.4. Collection

- **Interval:** 1s
- **Method:** OpenTelemetry

## 2.6. license_validation_failures
### 2.6.2. Type
counter

### 2.6.3. Unit
failures

### 2.6.4. Collection

- **Interval:** 1h
- **Method:** LicensingService



---

