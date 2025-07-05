# Specification

# 1. Database Design

## 1.1. User
System users with authentication credentials and role assignments

### 1.1.3. Attributes

### 1.1.3.1. userId
#### 1.1.3.1.2. Type
UUID

#### 1.1.3.1.3. Is Required
True

#### 1.1.3.1.4. Is Primary Key
True

### 1.1.3.2. username
#### 1.1.3.2.2. Type
VARCHAR

#### 1.1.3.2.3. Is Required
True

#### 1.1.3.2.4. Size
50

#### 1.1.3.2.5. Is Unique
True

### 1.1.3.3. email
#### 1.1.3.3.2. Type
VARCHAR

#### 1.1.3.3.3. Is Required
True

#### 1.1.3.3.4. Size
255

#### 1.1.3.3.5. Is Unique
True

### 1.1.3.4. passwordHash
#### 1.1.3.4.2. Type
VARCHAR

#### 1.1.3.4.3. Is Required
True

#### 1.1.3.4.4. Size
255

### 1.1.3.5. lastLogin
#### 1.1.3.5.2. Type
DateTimeOffset

#### 1.1.3.5.3. Is Required
False

### 1.1.3.6. isActive
#### 1.1.3.6.2. Type
BOOLEAN

#### 1.1.3.6.3. Is Required
True

#### 1.1.3.6.4. Default Value
true

### 1.1.3.7. createdAt
#### 1.1.3.7.2. Type
DateTimeOffset

#### 1.1.3.7.3. Is Required
True

### 1.1.3.8. updatedAt
#### 1.1.3.8.2. Type
DateTimeOffset

#### 1.1.3.8.3. Is Required
True

### 1.1.3.9. externalIdpId
#### 1.1.3.9.2. Type
VARCHAR

#### 1.1.3.9.3. Is Required
False

#### 1.1.3.9.4. Size
255


### 1.1.4. Primary Keys

- userId

### 1.1.5. Unique Constraints

### 1.1.5.1. uq_user_username
#### 1.1.5.1.2. Columns

- username

### 1.1.5.2. uq_user_email
#### 1.1.5.2.2. Columns

- email


### 1.1.6. Indexes


### 1.1.7. Caching Suggestions

### 1.1.7.1. UserPermissions_Cache
Contributes to the application-level cache for resolved user permissions. Invalidate the cache upon changes to users, roles, or permissions.

#### 1.1.7.1.3. Scope
ApplicationLevel


## 1.2. Role
Predefined or custom roles with specific system permissions

### 1.2.3. Attributes

### 1.2.3.1. roleId
#### 1.2.3.1.2. Type
UUID

#### 1.2.3.1.3. Is Required
True

#### 1.2.3.1.4. Is Primary Key
True

### 1.2.3.2. name
#### 1.2.3.2.2. Type
VARCHAR

#### 1.2.3.2.3. Is Required
True

#### 1.2.3.2.4. Size
50

#### 1.2.3.2.5. Is Unique
True

### 1.2.3.3. description
#### 1.2.3.3.2. Type
VARCHAR

#### 1.2.3.3.3. Is Required
False

#### 1.2.3.3.4. Size
255

### 1.2.3.4. isSystemRole
#### 1.2.3.4.2. Type
BOOLEAN

#### 1.2.3.4.3. Is Required
True

#### 1.2.3.4.4. Default Value
false


### 1.2.4. Primary Keys

- roleId

### 1.2.5. Unique Constraints

### 1.2.5.1. uq_role_name
#### 1.2.5.1.2. Columns

- name


### 1.2.6. Indexes


### 1.2.7. Caching Suggestions

### 1.2.7.1. UserPermissions_Cache
Contributes to the application-level cache for resolved user permissions. Invalidate the cache upon changes to users, roles, or permissions.

#### 1.2.7.1.3. Scope
ApplicationLevel

### 1.2.7.2. SystemConfiguration_Cache
Cache Role definitions as they are relatively static configuration data. Invalidate on configuration changes.

#### 1.2.7.2.3. Scope
ApplicationLevel


## 1.3. Permission
Atomic system permissions that can be assigned to roles

### 1.3.3. Attributes

### 1.3.3.1. permissionId
#### 1.3.3.1.2. Type
UUID

#### 1.3.3.1.3. Is Required
True

#### 1.3.3.1.4. Is Primary Key
True

### 1.3.3.2. code
#### 1.3.3.2.2. Type
VARCHAR

#### 1.3.3.2.3. Is Required
True

#### 1.3.3.2.4. Size
50

#### 1.3.3.2.5. Is Unique
True

### 1.3.3.3. description
#### 1.3.3.3.2. Type
VARCHAR

#### 1.3.3.3.3. Is Required
False

#### 1.3.3.3.4. Size
255


### 1.3.4. Primary Keys

- permissionId

### 1.3.5. Unique Constraints

### 1.3.5.1. uq_permission_code
#### 1.3.5.1.2. Columns

- code


### 1.3.6. Indexes


### 1.3.7. Caching Suggestions

### 1.3.7.1. UserPermissions_Cache
Contributes to the application-level cache for resolved user permissions. Invalidate the cache upon changes to users, roles, or permissions.

#### 1.3.7.1.3. Scope
ApplicationLevel

### 1.3.7.2. SystemConfiguration_Cache
Cache Permission definitions as they are relatively static configuration data. Invalidate on configuration changes.

#### 1.3.7.2.3. Scope
ApplicationLevel


## 1.4. OpcServer
Configured OPC servers of various specifications

### 1.4.3. Attributes

### 1.4.3.1. serverId
#### 1.4.3.1.2. Type
UUID

#### 1.4.3.1.3. Is Required
True

#### 1.4.3.1.4. Is Primary Key
True

### 1.4.3.2. name
#### 1.4.3.2.2. Type
VARCHAR

#### 1.4.3.2.3. Is Required
True

#### 1.4.3.2.4. Size
100

### 1.4.3.3. type
#### 1.4.3.3.2. Type
VARCHAR

#### 1.4.3.3.3. Is Required
True

#### 1.4.3.3.4. Size
20

#### 1.4.3.3.5. Constraints

- CHECK (type IN ('DA', 'UA', 'XML-DA', 'HDA', 'A&C'))

### 1.4.3.4. version
#### 1.4.3.4.2. Type
VARCHAR

#### 1.4.3.4.3. Is Required
True

#### 1.4.3.4.4. Size
10

### 1.4.3.5. endpointUrl
#### 1.4.3.5.2. Type
VARCHAR

#### 1.4.3.5.3. Is Required
True

#### 1.4.3.5.4. Size
255

### 1.4.3.6. securityPolicy
#### 1.4.3.6.2. Type
VARCHAR

#### 1.4.3.6.3. Is Required
False

#### 1.4.3.6.4. Size
50

### 1.4.3.7. certificate
#### 1.4.3.7.2. Type
TEXT

#### 1.4.3.7.3. Is Required
False

### 1.4.3.8. isActive
#### 1.4.3.8.2. Type
BOOLEAN

#### 1.4.3.8.3. Is Required
True

#### 1.4.3.8.4. Default Value
true


### 1.4.4. Primary Keys

- serverId

### 1.4.5. Unique Constraints


### 1.4.6. Indexes


### 1.4.7. Caching Suggestions

### 1.4.7.1. SystemConfiguration_Cache
Cache OpcServer configurations as they are relatively static. Invalidate on configuration changes.

#### 1.4.7.1.3. Scope
ApplicationLevel


## 1.5. OpcTag
Configured data points from OPC servers

### 1.5.3. Attributes

### 1.5.3.1. tagId
#### 1.5.3.1.2. Type
UUID

#### 1.5.3.1.3. Is Required
True

#### 1.5.3.1.4. Is Primary Key
True

### 1.5.3.2. serverId
#### 1.5.3.2.2. Type
UUID

#### 1.5.3.2.3. Is Required
True

#### 1.5.3.2.4. Is Foreign Key
True

### 1.5.3.3. nodeId
#### 1.5.3.3.2. Type
VARCHAR

#### 1.5.3.3.3. Is Required
True

#### 1.5.3.3.4. Size
255

### 1.5.3.4. dataType
#### 1.5.3.4.2. Type
VARCHAR

#### 1.5.3.4.3. Is Required
True

#### 1.5.3.4.4. Size
50

### 1.5.3.5. description
#### 1.5.3.5.2. Type
VARCHAR

#### 1.5.3.5.3. Is Required
False

#### 1.5.3.5.4. Size
255

### 1.5.3.6. isWritable
#### 1.5.3.6.2. Type
BOOLEAN

#### 1.5.3.6.3. Is Required
True

### 1.5.3.7. validationRules
#### 1.5.3.7.2. Type
JSON

#### 1.5.3.7.3. Is Required
False


### 1.5.4. Primary Keys

- tagId

### 1.5.5. Unique Constraints

### 1.5.5.1. uq_opctag_serverid_nodeid
#### 1.5.5.1.2. Columns

- serverId
- nodeId


### 1.5.6. Indexes


### 1.5.7. Caching Suggestions

### 1.5.7.1. SystemConfiguration_Cache
Cache OpcTag metadata as it is relatively static. Invalidate on configuration changes.

#### 1.5.7.1.3. Scope
ApplicationLevel


## 1.6. Subscription
Active OPC UA subscriptions with configuration parameters

### 1.6.3. Attributes

### 1.6.3.1. subscriptionId
#### 1.6.3.1.2. Type
UUID

#### 1.6.3.1.3. Is Required
True

#### 1.6.3.1.4. Is Primary Key
True

### 1.6.3.2. serverId
#### 1.6.3.2.2. Type
UUID

#### 1.6.3.2.3. Is Required
True

#### 1.6.3.2.4. Is Foreign Key
True

### 1.6.3.3. publishingInterval
#### 1.6.3.3.2. Type
INT

#### 1.6.3.3.3. Is Required
True

### 1.6.3.4. samplingInterval
#### 1.6.3.4.2. Type
INT

#### 1.6.3.4.3. Is Required
True

### 1.6.3.5. queueSize
#### 1.6.3.5.2. Type
INT

#### 1.6.3.5.3. Is Required
True

### 1.6.3.6. deadbandType
#### 1.6.3.6.2. Type
VARCHAR

#### 1.6.3.6.3. Is Required
False

#### 1.6.3.6.4. Size
20

### 1.6.3.7. deadbandValue
#### 1.6.3.7.2. Type
DECIMAL

#### 1.6.3.7.3. Is Required
False

#### 1.6.3.7.4. Precision
18

#### 1.6.3.7.5. Scale
4


### 1.6.4. Primary Keys

- subscriptionId

### 1.6.5. Unique Constraints


### 1.6.6. Indexes


## 1.7. DataLog
Audit trail for write operations to critical tags

### 1.7.3. Attributes

### 1.7.3.1. logId
#### 1.7.3.1.2. Type
UUID

#### 1.7.3.1.3. Is Required
True

#### 1.7.3.1.4. Is Primary Key
True

### 1.7.3.2. tagId
#### 1.7.3.2.2. Type
UUID

#### 1.7.3.2.3. Is Required
True

#### 1.7.3.2.4. Is Foreign Key
True

### 1.7.3.3. userId
#### 1.7.3.3.2. Type
UUID

#### 1.7.3.3.3. Is Required
False

#### 1.7.3.3.4. Is Foreign Key
True

### 1.7.3.4. oldValue
#### 1.7.3.4.2. Type
VARCHAR

#### 1.7.3.4.3. Is Required
False

#### 1.7.3.4.4. Size
255

### 1.7.3.5. newValue
#### 1.7.3.5.2. Type
VARCHAR

#### 1.7.3.5.3. Is Required
True

#### 1.7.3.5.4. Size
255

### 1.7.3.6. timestamp
#### 1.7.3.6.2. Type
DateTimeOffset

#### 1.7.3.6.3. Is Required
True

### 1.7.3.7. operationStatus
#### 1.7.3.7.2. Type
VARCHAR

#### 1.7.3.7.3. Is Required
True

#### 1.7.3.7.4. Size
20


### 1.7.4. Primary Keys

- logId

### 1.7.5. Unique Constraints


### 1.7.6. Indexes

### 1.7.6.1. idx_datalog_tagid_timestamp
#### 1.7.6.1.2. Columns

- tagId
- timestamp

#### 1.7.6.1.3. Type
BTree


### 1.7.7. Partitioning Info

- **Type:** RANGE
- **Columns:**
  
  - timestamp
  
- **Strategy Details:** e.g., monthly, if data volume is very large. Aids in managing large log volumes and query performance.

## 1.8. HistoricalData
Time-series data from OPC HDA servers

### 1.8.3. Attributes

### 1.8.3.1. dataId
#### 1.8.3.1.2. Type
UUID

#### 1.8.3.1.3. Is Required
True

#### 1.8.3.1.4. Is Primary Key
True

### 1.8.3.2. tagId
#### 1.8.3.2.2. Type
UUID

#### 1.8.3.2.3. Is Required
True

#### 1.8.3.2.4. Is Foreign Key
True

### 1.8.3.3. timestamp
#### 1.8.3.3.2. Type
DateTimeOffset

#### 1.8.3.3.3. Is Required
True

### 1.8.3.4. value
#### 1.8.3.4.2. Type
VARCHAR

#### 1.8.3.4.3. Is Required
True

#### 1.8.3.4.4. Size
255

### 1.8.3.5. quality
#### 1.8.3.5.2. Type
VARCHAR

#### 1.8.3.5.3. Is Required
True

#### 1.8.3.5.4. Size
50

### 1.8.3.6. aggregationType
#### 1.8.3.6.2. Type
VARCHAR

#### 1.8.3.6.3. Is Required
False

#### 1.8.3.6.4. Size
50


### 1.8.4. Primary Keys

- dataId

### 1.8.5. Unique Constraints


### 1.8.6. Indexes

### 1.8.6.1. idx_historicaldata_tagid_timestamp
#### 1.8.6.1.2. Columns

- tagId
- timestamp

#### 1.8.6.1.3. Type
BTree


### 1.8.7. Partitioning Info

- **Type:** RANGE
- **Columns:**
  
  - timestamp
  
- **Strategy Details:** e.g., monthly or quarterly. Improves query performance on recent data and simplifies data lifecycle management.

### 1.8.8. Caching Suggestions

### 1.8.8.1. RecentTimeSeries_DashboardCache
Cache recent HistoricalData (e.g., last 15-60 minutes) frequently accessed by dashboards. Use TTL-based or sliding window eviction.

#### 1.8.8.1.3. Scope
ApplicationLevel


### 1.8.9. Materialized View Suggestions

### 1.8.9.1. mv_aggregated_historical_data
Create materialized views for frequently requested aggregations (e.g., hourly/daily averages, min/max for key tags). Refresh periodically or based on triggers. Supports faster reporting and dashboard visualizations.

#### 1.8.9.1.3. Base Table
HistoricalData

#### 1.8.9.1.4. Columns

- tagId
- timestamp
- value

#### 1.8.9.1.5. Aggregations

- AVG(value)
- MIN(value)
- MAX(value)
- COUNT(value)


## 1.9. AlarmEvent
Alarms and conditions from OPC A&C servers

### 1.9.3. Attributes

### 1.9.3.1. alarmId
#### 1.9.3.1.2. Type
UUID

#### 1.9.3.1.3. Is Required
True

#### 1.9.3.1.4. Is Primary Key
True

### 1.9.3.2. tagId
#### 1.9.3.2.2. Type
UUID

#### 1.9.3.2.3. Is Required
True

#### 1.9.3.2.4. Is Foreign Key
True

### 1.9.3.3. eventType
#### 1.9.3.3.2. Type
VARCHAR

#### 1.9.3.3.3. Is Required
True

#### 1.9.3.3.4. Size
50

### 1.9.3.4. severity
#### 1.9.3.4.2. Type
INT

#### 1.9.3.4.3. Is Required
True

### 1.9.3.5. message
#### 1.9.3.5.2. Type
VARCHAR

#### 1.9.3.5.3. Is Required
True

#### 1.9.3.5.4. Size
500

### 1.9.3.6. occurrenceTime
#### 1.9.3.6.2. Type
DateTimeOffset

#### 1.9.3.6.3. Is Required
True

### 1.9.3.7. acknowledgedTime
#### 1.9.3.7.2. Type
DateTimeOffset

#### 1.9.3.7.3. Is Required
False


### 1.9.4. Primary Keys

- alarmId

### 1.9.5. Unique Constraints


### 1.9.6. Indexes

### 1.9.6.1. idx_alarmevent_tagid_occurrencetime
#### 1.9.6.1.2. Columns

- tagId
- occurrenceTime

#### 1.9.6.1.3. Type
BTree

### 1.9.6.2. idx_alarmevent_unack_occurrencetime
#### 1.9.6.2.2. Columns

- occurrenceTime

#### 1.9.6.2.3. Type
BTree

#### 1.9.6.2.4. Condition
acknowledgedTime IS NULL


### 1.9.7. Partitioning Info

- **Type:** RANGE
- **Columns:**
  
  - occurrenceTime
  
- **Strategy Details:** e.g., monthly. Improves query performance for time-bound alarm queries and simplifies data lifecycle management.

### 1.9.8. Caching Suggestions

### 1.9.8.1. RecentTimeSeries_DashboardCache
Cache recent AlarmEvent data (e.g., last 15-60 minutes) frequently accessed by dashboards. Use TTL-based or sliding window eviction.

#### 1.9.8.1.3. Scope
ApplicationLevel


## 1.10. Dashboard
User-configured data visualization layouts

### 1.10.3. Attributes

### 1.10.3.1. dashboardId
#### 1.10.3.1.2. Type
UUID

#### 1.10.3.1.3. Is Required
True

#### 1.10.3.1.4. Is Primary Key
True

### 1.10.3.2. userId
#### 1.10.3.2.2. Type
UUID

#### 1.10.3.2.3. Is Required
True

#### 1.10.3.2.4. Is Foreign Key
True

### 1.10.3.3. name
#### 1.10.3.3.2. Type
VARCHAR

#### 1.10.3.3.3. Is Required
True

#### 1.10.3.3.4. Size
100

### 1.10.3.4. layoutConfig
#### 1.10.3.4.2. Type
JSON

#### 1.10.3.4.3. Is Required
True

### 1.10.3.5. isDefault
#### 1.10.3.5.2. Type
BOOLEAN

#### 1.10.3.5.3. Is Required
True

#### 1.10.3.5.4. Default Value
false


### 1.10.4. Primary Keys

- dashboardId

### 1.10.5. Unique Constraints


### 1.10.6. Indexes


## 1.11. ReportTemplate
Customizable report definitions and scheduling

### 1.11.3. Attributes

### 1.11.3.1. templateId
#### 1.11.3.1.2. Type
UUID

#### 1.11.3.1.3. Is Required
True

#### 1.11.3.1.4. Is Primary Key
True

### 1.11.3.2. name
#### 1.11.3.2.2. Type
VARCHAR

#### 1.11.3.2.3. Is Required
True

#### 1.11.3.2.4. Size
100

### 1.11.3.3. dataSources
#### 1.11.3.3.2. Type
JSON

#### 1.11.3.3.3. Is Required
True

### 1.11.3.4. format
#### 1.11.3.4.2. Type
VARCHAR

#### 1.11.3.4.3. Is Required
True

#### 1.11.3.4.4. Size
10

#### 1.11.3.4.5. Constraints

- CHECK (format IN ('PDF', 'Excel', 'HTML'))

### 1.11.3.5. schedule
#### 1.11.3.5.2. Type
VARCHAR

#### 1.11.3.5.3. Is Required
False

#### 1.11.3.5.4. Size
20


### 1.11.4. Primary Keys

- templateId

### 1.11.5. Unique Constraints


### 1.11.6. Indexes


## 1.12. AuditLog
Immutable record of security-relevant system events

### 1.12.3. Attributes

### 1.12.3.1. logId
#### 1.12.3.1.2. Type
UUID

#### 1.12.3.1.3. Is Required
True

#### 1.12.3.1.4. Is Primary Key
True

### 1.12.3.2. eventType
#### 1.12.3.2.2. Type
VARCHAR

#### 1.12.3.2.3. Is Required
True

#### 1.12.3.2.4. Size
50

### 1.12.3.3. userId
#### 1.12.3.3.2. Type
UUID

#### 1.12.3.3.3. Is Required
False

#### 1.12.3.3.4. Is Foreign Key
True

### 1.12.3.4. timestamp
#### 1.12.3.4.2. Type
DateTimeOffset

#### 1.12.3.4.3. Is Required
True

### 1.12.3.5. details
#### 1.12.3.5.2. Type
JSON

#### 1.12.3.5.3. Is Required
True

### 1.12.3.6. ipAddress
#### 1.12.3.6.2. Type
VARCHAR

#### 1.12.3.6.3. Is Required
False

#### 1.12.3.6.4. Size
45


### 1.12.4. Primary Keys

- logId

### 1.12.5. Unique Constraints


### 1.12.6. Indexes

### 1.12.6.1. idx_auditlog_timestamp_eventtype
#### 1.12.6.1.2. Columns

- timestamp
- eventType

#### 1.12.6.1.3. Type
BTree


### 1.12.7. Partitioning Info

- **Type:** RANGE
- **Columns:**
  
  - timestamp
  
- **Strategy Details:** e.g., quarterly or yearly. Essential for managing large volumes of audit data and improving query performance.

## 1.13. DataRetentionPolicy
Configurable rules for data lifecycle management

### 1.13.3. Attributes

### 1.13.3.1. policyId
#### 1.13.3.1.2. Type
UUID

#### 1.13.3.1.3. Is Required
True

#### 1.13.3.1.4. Is Primary Key
True

### 1.13.3.2. dataType
#### 1.13.3.2.2. Type
VARCHAR

#### 1.13.3.2.3. Is Required
True

#### 1.13.3.2.4. Size
50

#### 1.13.3.2.5. Constraints

- CHECK (dataType IN ('Historical', 'Alarm', 'Audit', 'AI'))

### 1.13.3.3. retentionPeriod
#### 1.13.3.3.2. Type
INT

#### 1.13.3.3.3. Is Required
True

### 1.13.3.4. archiveLocation
#### 1.13.3.4.2. Type
VARCHAR

#### 1.13.3.4.3. Is Required
False

#### 1.13.3.4.4. Size
255

### 1.13.3.5. isActive
#### 1.13.3.5.2. Type
BOOLEAN

#### 1.13.3.5.3. Is Required
True

#### 1.13.3.5.4. Default Value
true


### 1.13.4. Primary Keys

- policyId

### 1.13.5. Unique Constraints


### 1.13.6. Indexes


## 1.14. PredictiveMaintenanceModel
AI models for equipment maintenance predictions

### 1.14.3. Attributes

### 1.14.3.1. modelId
#### 1.14.3.1.2. Type
UUID

#### 1.14.3.1.3. Is Required
True

#### 1.14.3.1.4. Is Primary Key
True

### 1.14.3.2. name
#### 1.14.3.2.2. Type
VARCHAR

#### 1.14.3.2.3. Is Required
True

#### 1.14.3.2.4. Size
100

### 1.14.3.3. version
#### 1.14.3.3.2. Type
VARCHAR

#### 1.14.3.3.3. Is Required
True

#### 1.14.3.3.4. Size
20

### 1.14.3.4. framework
#### 1.14.3.4.2. Type
VARCHAR

#### 1.14.3.4.3. Is Required
True

#### 1.14.3.4.4. Size
50

### 1.14.3.5. deploymentStatus
#### 1.14.3.5.2. Type
VARCHAR

#### 1.14.3.5.3. Is Required
True

#### 1.14.3.5.4. Size
20

### 1.14.3.6. checksum
#### 1.14.3.6.2. Type
VARCHAR

#### 1.14.3.6.3. Is Required
True

#### 1.14.3.6.4. Size
64


### 1.14.4. Primary Keys

- modelId

### 1.14.5. Unique Constraints


### 1.14.6. Indexes


## 1.15. BlockchainTransaction
Immutable records of critical data exchanges

### 1.15.3. Attributes

### 1.15.3.1. transactionId
#### 1.15.3.1.2. Type
UUID

#### 1.15.3.1.3. Is Required
True

#### 1.15.3.1.4. Is Primary Key
True

### 1.15.3.2. dataHash
#### 1.15.3.2.2. Type
VARCHAR

#### 1.15.3.2.3. Is Required
True

#### 1.15.3.2.4. Size
64

#### 1.15.3.2.5. Is Unique
True

### 1.15.3.3. timestamp
#### 1.15.3.3.2. Type
DateTimeOffset

#### 1.15.3.3.3. Is Required
True

### 1.15.3.4. sourceSystem
#### 1.15.3.4.2. Type
VARCHAR

#### 1.15.3.4.3. Is Required
True

#### 1.15.3.4.4. Size
50

### 1.15.3.5. blockchainNetwork
#### 1.15.3.5.2. Type
VARCHAR

#### 1.15.3.5.3. Is Required
True

#### 1.15.3.5.4. Size
50


### 1.15.4. Primary Keys

- transactionId

### 1.15.5. Unique Constraints

### 1.15.5.1. uq_blockchaintransaction_datahash
#### 1.15.5.1.2. Columns

- dataHash


### 1.15.6. Indexes


## 1.16. MigrationStrategy
Data migration plans from legacy systems

### 1.16.3. Attributes

### 1.16.3.1. strategyId
#### 1.16.3.1.2. Type
UUID

#### 1.16.3.1.3. Is Required
True

#### 1.16.3.1.4. Is Primary Key
True

### 1.16.3.2. sourceSystem
#### 1.16.3.2.2. Type
VARCHAR

#### 1.16.3.2.3. Is Required
True

#### 1.16.3.2.4. Size
50

### 1.16.3.3. mappingRules
#### 1.16.3.3.2. Type
JSON

#### 1.16.3.3.3. Is Required
True

### 1.16.3.4. validationProcedure
#### 1.16.3.4.2. Type
TEXT

#### 1.16.3.4.3. Is Required
True

### 1.16.3.5. lastExecuted
#### 1.16.3.5.2. Type
DateTimeOffset

#### 1.16.3.5.3. Is Required
False


### 1.16.4. Primary Keys

- strategyId

### 1.16.5. Unique Constraints


### 1.16.6. Indexes


## 1.17. UserRole
Junction table for user-role assignments

### 1.17.3. Attributes

### 1.17.3.1. userId
#### 1.17.3.1.2. Type
UUID

#### 1.17.3.1.3. Is Required
True

#### 1.17.3.1.4. Is Foreign Key
True

#### 1.17.3.1.5. Is Primary Key
True

### 1.17.3.2. roleId
#### 1.17.3.2.2. Type
UUID

#### 1.17.3.2.3. Is Required
True

#### 1.17.3.2.4. Is Foreign Key
True

#### 1.17.3.2.5. Is Primary Key
True

### 1.17.3.3. assignedAt
#### 1.17.3.3.2. Type
DateTimeOffset

#### 1.17.3.3.3. Is Required
True


### 1.17.4. Primary Keys

- userId
- roleId

### 1.17.5. Unique Constraints


### 1.17.6. Indexes


### 1.17.7. Caching Suggestions

### 1.17.7.1. UserPermissions_Cache
Contributes to the application-level cache for resolved user permissions. Invalidate the cache upon changes to users, roles, or permissions.

#### 1.17.7.1.3. Scope
ApplicationLevel


## 1.18. RolePermission
Junction table for role-permission assignments

### 1.18.3. Attributes

### 1.18.3.1. roleId
#### 1.18.3.1.2. Type
UUID

#### 1.18.3.1.3. Is Required
True

#### 1.18.3.1.4. Is Foreign Key
True

#### 1.18.3.1.5. Is Primary Key
True

### 1.18.3.2. permissionId
#### 1.18.3.2.2. Type
UUID

#### 1.18.3.2.3. Is Required
True

#### 1.18.3.2.4. Is Foreign Key
True

#### 1.18.3.2.5. Is Primary Key
True


### 1.18.4. Primary Keys

- roleId
- permissionId

### 1.18.5. Unique Constraints


### 1.18.6. Indexes


### 1.18.7. Caching Suggestions

### 1.18.7.1. UserPermissions_Cache
Contributes to the application-level cache for resolved user permissions. Invalidate the cache upon changes to users, roles, or permissions.

#### 1.18.7.1.3. Scope
ApplicationLevel




---

# 2. Diagrams

- **Diagram_Title:** Overall Database Schema  
**Diagram_Area:** Core System Entities and Relationships  
**Explanation:** This ER diagram depicts the database structure for an industrial data platform. It outlines entities for user authentication (User, Role, Permission), OPC connectivity (OpcServer, OpcTag, Subscription), and data storage (DataLog, HistoricalData, AlarmEvent). Key relationships illustrate how users are assigned roles, OPC tags are configured under servers, and how various data types are logged and associated with tags or users. The diagram also includes entities for dashboards, reporting, audit trails, and system configurations like data retention and AI models, showing their primary keys and interconnections where defined.  
**Mermaid_Text:** erDiagram

    User {
        UUID userId PK
    }
    Role {
        UUID roleId PK
    }
    Permission {
        UUID permissionId PK
    }
    UserRole {
        UUID userId PK "FK to User.userId"
        UUID roleId PK "FK to Role.roleId"
    }
    RolePermission {
        UUID roleId PK "FK to Role.roleId"
        UUID permissionId PK "FK to Permission.permissionId"
    }
    OpcServer {
        UUID serverId PK
    }
    OpcTag {
        UUID tagId PK
    }
    Subscription {
        UUID subscriptionId PK
    }
    DataLog {
        UUID logId PK
    }
    HistoricalData {
        UUID dataId PK
    }
    AlarmEvent {
        UUID alarmId PK
    }
    Dashboard {
        UUID dashboardId PK
    }
    AuditLog {
        UUID logId PK
    }
    ReportTemplate {
        UUID templateId PK
    }
    DataRetentionPolicy {
        UUID policyId PK
    }
    PredictiveMaintenanceModel {
        UUID modelId PK
    }
    BlockchainTransaction {
        UUID transactionId PK
    }
    MigrationStrategy {
        UUID strategyId PK
    }

    %% Relationships
    User ||--|{ UserRole : "User side of UserRoles"
    Role ||--|{ UserRole : "Role side of UserRoles"

    Role ||--|{ RolePermission : "Role side of RolePermissions"
    Permission ||--|{ RolePermission : "Permission side of RolePermissions"

    OpcServer ||--|{ OpcTag : "OpcServerTags"
    OpcServer ||--|{ Subscription : "OpcServerSubscriptions"

    OpcTag ||--|{ DataLog : "OpcTagDataLogs"
    User |o--o{ DataLog : "UserDataLogs"

    OpcTag ||--|{ HistoricalData : "OpcTagHistoricalData"
    OpcTag ||--|{ AlarmEvent : "OpcTagAlarmEvents"

    User ||--|{ Dashboard : "UserDashboards"
    User |o--o{ AuditLog : "UserAuditLogs"  


---

