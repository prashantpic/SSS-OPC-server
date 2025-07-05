# Specification

# 1. Policies

## 1.1. server-cpu-horizontal-scale
### 1.1.2. Type
Horizontal

### 1.1.3. Rules

- **Metric:** CPU_UTILIZATION  
**Threshold:** 70  
**Operator:** GREATER_THAN  
**Scale Change:** 1  
**Cooldown:**
    
    - **Scale Up Seconds:** 180
    - **Scale Down Seconds:** 300
    
- **Metric:** CPU_UTILIZATION  
**Threshold:** 40  
**Operator:** LESS_THAN  
**Scale Change:** -1  
**Cooldown:**
    
    - **Scale Up Seconds:** 180
    - **Scale Down Seconds:** 300
    

## 1.2. server-memory-horizontal-scale
### 1.2.2. Type
Horizontal

### 1.2.3. Rules

- **Metric:** MEMORY_USAGE_PERCENT  
**Threshold:** 75  
**Operator:** GREATER_THAN  
**Scale Change:** 1  
**Cooldown:**
    
    - **Scale Up Seconds:** 240
    - **Scale Down Seconds:** 480
    
- **Metric:** MEMORY_USAGE_PERCENT  
**Threshold:** 50  
**Operator:** LESS_THAN  
**Scale Change:** -1  
**Cooldown:**
    
    - **Scale Up Seconds:** 240
    - **Scale Down Seconds:** 480
    

## 1.3. data-ingestion-queue-scale
### 1.3.2. Type
Horizontal

### 1.3.3. Rules

- **Metric:** MESSAGE_QUEUE_LENGTH_DATA_INGESTION  
**Threshold:** 100  
**Operator:** GREATER_THAN  
**Scale Change:** 2  
**Cooldown:**
    
    - **Scale Up Seconds:** 120
    - **Scale Down Seconds:** 300
    
- **Metric:** MESSAGE_QUEUE_LENGTH_DATA_INGESTION  
**Threshold:** 20  
**Operator:** LESS_THAN  
**Scale Change:** -1  
**Cooldown:**
    
    - **Scale Up Seconds:** 120
    - **Scale Down Seconds:** 300
    

## 1.4. api-gateway-latency-scale
### 1.4.2. Type
Horizontal

### 1.4.3. Rules

- **Metric:** HTTP_REQUEST_LATENCY_P95_MS  
**Threshold:** 200  
**Operator:** GREATER_THAN  
**Scale Change:** 1  
**Cooldown:**
    
    - **Scale Up Seconds:** 120
    - **Scale Down Seconds:** 480
    
- **Metric:** HTTP_REQUEST_LATENCY_P95_MS  
**Threshold:** 100  
**Operator:** LESS_THAN  
**Scale Change:** -1  
**Cooldown:**
    
    - **Scale Up Seconds:** 120
    - **Scale Down Seconds:** 480
    

## 1.5. data-query-latency-scale
### 1.5.2. Type
Horizontal

### 1.5.3. Rules

- **Metric:** TSDB_QUERY_LATENCY_P95_MS  
**Threshold:** 1500  
**Operator:** GREATER_THAN  
**Scale Change:** 1  
**Cooldown:**
    
    - **Scale Up Seconds:** 180
    - **Scale Down Seconds:** 600
    
- **Metric:** TSDB_QUERY_LATENCY_P95_MS  
**Threshold:** 1000  
**Operator:** LESS_THAN  
**Scale Change:** -1  
**Cooldown:**
    
    - **Scale Up Seconds:** 180
    - **Scale Down Seconds:** 600
    

## 1.6. ai-service-throughput-scale
### 1.6.2. Type
Horizontal

### 1.6.3. Rules

- **Metric:** AI_PREDICTION_THROUGHPUT_PER_INSTANCE  
**Threshold:** 50  
**Operator:** GREATER_THAN  
**Scale Change:** 1  
**Cooldown:**
    
    - **Scale Up Seconds:** 180
    - **Scale Down Seconds:** 480
    
- **Metric:** AI_PREDICTION_THROUGHPUT_PER_INSTANCE  
**Threshold:** 10  
**Operator:** LESS_THAN  
**Scale Change:** -1  
**Cooldown:**
    
    - **Scale Up Seconds:** 180
    - **Scale Down Seconds:** 480
    

## 1.7. reporting-scheduled-scale-up
### 1.7.2. Type
Manual

### 1.7.3. Rules

- **Metric:** TIME_OF_DAY_HOUR  
**Threshold:** 1  
**Operator:** EQUALS  
**Scale Change:** 2  
**Cooldown:**
    
    - **Scale Up Seconds:** 0
    - **Scale Down Seconds:** 0
    

## 1.8. reporting-scheduled-scale-down
### 1.8.2. Type
Manual

### 1.8.3. Rules

- **Metric:** TIME_OF_DAY_HOUR  
**Threshold:** 3  
**Operator:** EQUALS  
**Scale Change:** -1  
**Cooldown:**
    
    - **Scale Up Seconds:** 0
    - **Scale Down Seconds:** 0
    
- **Metric:** TIME_OF_DAY_HOUR  
**Threshold:** 4  
**Operator:** EQUALS  
**Scale Change:** -1  
**Cooldown:**
    
    - **Scale Up Seconds:** 0
    - **Scale Down Seconds:** 0
    



---

# 2. Configuration

- **Min Instances:** 3
- **Max Instances:** 20
- **Default Timeout:** 30s
- **Region:** us-east-1
- **Resource Group:** production-opcsystem
- **Notification Endpoint:** https://alerts.example.com/scaling
- **Logging Level:** INFO
- **Vpc Id:** vpc-opcsystem-prod
- **Instance Type:** m6g.large
- **Enable Detailed Monitoring:** true
- **Scaling Mode:** reactive


---

