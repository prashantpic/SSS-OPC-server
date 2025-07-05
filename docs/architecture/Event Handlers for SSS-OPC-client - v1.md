# Specification

# 1. Environments



---

# 2. Configuration

- **Event Bus:**
  
  - **Provider:** RabbitMQ
  - **Topics:**
    
    - opc.client.data.realtime
    - opc.client.events.alarms
    - opc.client.audit.writes
    
  - **Schema:** JSON
  - **Configuration:**
    
    - **Message Ttl:** 60000
    
  
- **Processing:**
  
  - **Strategy:** async
  - **Handlers:**
    
    - DataIngestionProcessor
    - NotificationService
    
  
- **Global Settings:**
  
  - **Timeout:** 30s
  - **Max Retries:** 3
  - **Dead Letter Queue:** opc.client.deadletter
  - **Logging:** structured
  


---

