# Specification

# 1. Alerts

## 1.1. OPCUASubscriptionFailure
OPC UA subscription failure detected, potential data loss.

### 1.1.3. Condition
opc_ua_subscription_status == Failed

### 1.1.4. Threshold
1 or more failed subscriptions for 5 minutes

### 1.1.5. Severity
High

### 1.1.6. Channels

- PagerDuty
- Email

## 1.2. OPCClientConnectionFailure
Core OPC Client service lost connection to an OPC server.

### 1.2.3. Condition
opc_client_connection_status == Disconnected

### 1.2.4. Threshold
Disconnected for 5 minutes

### 1.2.5. Severity
High

### 1.2.6. Channels

- PagerDuty
- Email

## 1.3. HighOPCDAReadWriteLatency
OPC DA synchronous read/write latency exceeding threshold (REQ-CSVC-005).

### 1.3.3. Condition
opc_da_read_write_latency_p99 > 50

### 1.3.4. Threshold
P99 latency > 50ms for 2 minutes

### 1.3.5. Severity
Medium

### 1.3.6. Channels

- Email

## 1.4. HighOPCUASubscriptionLatency
OPC UA subscription update latency exceeding threshold (REQ-CSVC-024).

### 1.4.3. Condition
opc_ua_subscription_latency_p99 > 100

### 1.4.4. Threshold
P99 latency > 100ms for 2 minutes

### 1.4.5. Severity
Medium

### 1.4.6. Channels

- Email

## 1.5. HighHDAQueryTime
OPC HDA query time exceeding threshold (REQ-CSVC-014).

### 1.5.3. Condition
opc_hda_query_time_p95 > 2000

### 1.5.4. Threshold
P95 query time > 2 seconds for 1 day/100 tags for 5 minutes

### 1.5.5. Severity
Medium

### 1.5.6. Channels

- Email

## 1.6. APIGatewayHighLatency
API Gateway latency exceeding acceptable threshold.

### 1.6.3. Condition
api_gateway_latency_p99 > 200

### 1.6.4. Threshold
P99 latency > 200ms sustained for 5 minutes

### 1.6.5. Severity
High

### 1.6.6. Channels

- PagerDuty
- Email

## 1.7. APIGatewayHighErrorRate
API Gateway experiencing high error rate.

### 1.7.3. Condition
api_gateway_error_rate > 1

### 1.7.4. Threshold
Error rate > 1% sustained for 5 minutes

### 1.7.5. Severity
High

### 1.7.6. Channels

- PagerDuty
- Email

## 1.8. CriticalWriteFailure
Failure to log critical write operations (REQ-CSVC-009).

### 1.8.3. Condition
critical_write_log_failures > 0

### 1.8.4. Threshold
Any failure

### 1.8.5. Severity
Critical

### 1.8.6. Channels

- PagerDuty
- SMS

## 1.9. DatabaseConnectivityIssue
Issue connecting to a critical database (Relational or Time-Series).

### 1.9.3. Condition
database_connection_status == Failed

### 1.9.4. Threshold
Failed connection for 2 minutes

### 1.9.5. Severity
Critical

### 1.9.6. Channels

- PagerDuty
- SMS

## 1.10. MessageQueueConnectivityIssue
Core OPC Client or server-side service cannot connect to message queue (REQ-SAP-003).

### 1.10.3. Condition
message_queue_connection_status == Failed

### 1.10.4. Threshold
Failed connection for 2 minutes

### 1.10.5. Severity
Critical

### 1.10.6. Channels

- PagerDuty
- SMS

## 1.11. UIDashboardHighLatency
UI dashboard load time or interaction response time exceeding threshold (REQ-UIX-010).

### 1.11.3. Condition
ui_dashboard_load_time_p95 > 3000 OR ui_interaction_response_time_p95 > 200

### 1.11.4. Threshold
Load time > 3s or response time > 200ms sustained for 3 minutes

### 1.11.5. Severity
Medium

### 1.11.6. Channels

- Email

## 1.12. ExternalServiceUnavailable
External service (NLP provider, IoT platform, Blockchain, Digital Twin) unavailable (REQ-7-016).

### 1.12.3. Condition
external_service_status == Unavailable

### 1.12.4. Threshold
Unavailable for 10 minutes (after circuit breaker trips)

### 1.12.5. Severity
High

### 1.12.6. Channels

- PagerDuty
- Email

## 1.13. LicenseActivationFailure
Failure to activate or validate software license (REQ-9-006).

### 1.13.3. Condition
license_status == Invalid

### 1.13.4. Threshold
Invalid license state for 1 hour

### 1.13.5. Severity
Critical

### 1.13.6. Channels

- PagerDuty
- SMS
- Email



---

