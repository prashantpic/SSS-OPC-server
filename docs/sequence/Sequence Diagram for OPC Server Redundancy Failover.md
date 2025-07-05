sequenceDiagram
    actor "Core OPC Client" as REPOAPPCLIENT
    participant "Primary OPC Server" as PrimaryOpcServer
    participant "Backup OPC Server" as BackupOpcServer
    actor "Administrator / Monitoring System" as MonitoringSystem

    loop Normal Operation
        REPOAPPCLIENT-PrimaryOpcServer: 1.1 HealthCheck/Heartbeat()
        activate PrimaryOpcServer
        PrimaryOpcServer--REPOAPPCLIENT: StatusOK
        deactivate PrimaryOpcServer
        PrimaryOpcServer-REPOAPPCLIENT: 1.2 Publish(DataChanges)
    end

    note over PrimaryOpcServer: 2. Primary OPC Server becomes unresponsive
    destroy PrimaryOpcServer

    alt Primary Server Unresponsive
        loop Retry Health Check N times
            REPOAPPCLIENT-PrimaryOpcServer: 3.1.1 HealthCheck/Heartbeat()
            activate PrimaryOpcServer
            note right of REPOAPPCLIENT: REQ-DLP-013: Health check/heartbeat mechanism detects primary server failure.
            PrimaryOpcServer--REPOAPPCLIENT: Timeout / No Response
            deactivate PrimaryOpcServer
        end
        
        REPOAPPCLIENT-REPOAPPCLIENT: 3.2 triggerFailoverLogic()
        activate REPOAPPCLIENT
        note over REPOAPPCLIENT: REQ-DLP-014: Failover is triggered. RTO/RPO targets are met by quickly re-establishing connection and subscriptions.
        
        REPOAPPCLIENT-BackupOpcServer: 3.3 Connect(securityDetails)
        activate BackupOpcServer
        BackupOpcServer--REPOAPPCLIENT: ConnectResponse(Success)
        deactivate BackupOpcServer
        
        loop For each active subscription
            REPOAPPCLIENT-BackupOpcServer: 3.4.1 CreateSubscription(params)
            activate BackupOpcServer
            BackupOpcServer--REPOAPPCLIENT: CreateSubscriptionResponse(Success)
            deactivate BackupOpcServer
        end
        
        REPOAPPCLIENT-REPOAPPCLIENT: 3.5 logFailoverEvent("Primary - Backup")
        
        REPOAPPCLIENT-MonitoringSystem: 3.6 SendAlert("Failover to BackupOpcServer completed")
        activate MonitoringSystem
        note left of MonitoringSystem: REQ-DLP-015: Users are notified of the failover event.
        deactivate MonitoringSystem

        deactivate REPOAPPCLIENT
    end

    loop Normal Operation with Backup
        BackupOpcServer-REPOAPPCLIENT: 4.1 Publish(DataChanges)
    end