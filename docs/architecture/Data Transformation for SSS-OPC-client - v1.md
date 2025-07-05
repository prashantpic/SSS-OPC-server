# Specification

# 1. Configuration

- **Mappings:**
  
  - **Source:** CoreOpcClientService.OpcTagValue  
**Target:** ServerSideApplication_DataService.HistoricalDataPoint  
**Transformation:** direct  
**Configuration:**
    
    - **Timestamp:** serverTimestamp
    - **Tag Id:** opcItemId
    - **Value:** opcValue
    - **Quality:** opcQuality
    
  - **Source:** CoreOpcClientService.OpcAlarmEvent  
**Target:** ServerSideApplication_DataService.AlarmEventRecord  
**Transformation:** direct  
**Configuration:**
    
    - **Timestamp:** eventTimestamp
    - **Source Node:** opcSourceNode
    - **Event Type:** opcEventType
    - **Severity:** opcSeverity
    - **Condition Name:** opcConditionName
    - **Message:** opcMessage
    
  - **Source:** CoreOpcClientService.OpcWriteAuditRecord  
**Target:** ServerSideApplication_DataService.AuditLogEntry  
**Transformation:** direct  
**Configuration:**
    
    - **Timestamp:** writeTimestamp
    - **User:** clientUser
    - **Tag Id:** opcTagId
    - **Old Value:** opcOldValue
    - **New Value:** opcNewValue
    
  - **Source:** ServerSideApplication_AiService.PredictionResult  
**Target:** ServerSideApplication_ReportingService.ReportData  
**Transformation:** direct  
**Configuration:**
    
    - **Predicted Value:** forecastValue
    - **Confidence:** predictionConfidence
    
  - **Source:** ServerSideApplication_AiService.AnomalyDetectionResult  
**Target:** ServerSideApplication_ReportingService.ReportData  
**Transformation:** direct  
**Configuration:**
    
    - **Anomalous Data Points:** anomalyList
    
  - **Source:** CoreOpcClientService.OpcTagConfiguration  
**Target:** ServerSideApplication_ManagementService.ClientConfiguration  
**Transformation:** direct  
**Configuration:**
    
    - **Opc Server Address:** serverEndpoint
    - **Opc Tag List:** monitoredTags
    
  - **Source:** ServerSideApplication_AiService.AiModelMetadata  
**Target:** ServerSideApplication_DataService.AiModelArtifact  
**Transformation:** direct  
**Configuration:**
    
    - **Model Parameters:** modelParams
    - **Feature Vectors:** featureData
    
  
- **Validations:**
  
  - **Field:** CoreOpcClientService.OpcTagValue.serverTimestamp  
**Rules:**
    
    - required
    - timestampFormat
    
  - **Field:** CoreOpcClientService.OpcTagValue.opcValue  
**Rules:**
    
    - dataTypeCheck
    
  - **Field:** CoreOpcClientService.OpcAlarmEvent.eventTimestamp  
**Rules:**
    
    - required
    - timestampFormat
    
  - **Field:** CoreOpcClientService.OpcWriteAuditRecord.writeTimestamp  
**Rules:**
    
    - required
    - timestampFormat
    
  - **Field:** ServerSideApplication_ManagementService.ClientConfiguration.serverEndpoint  
**Rules:**
    
    - required
    - urlFormat
    
  - **Field:** ServerSideApplication_AiService.AiModelMetadata.modelName  
**Rules:**
    
    - required
    - stringFormat
    
  


---

