# Specification

# 1. Sequence Design Overview

- **Sequence_Diagram:**
  ### . Critical OPC Tag Write Operation with Audit and Blockchain Logging
  Illustrates the process of a user writing a value to a critical tag. The sequence shows the synchronous write operation to the OPC server, followed by two asynchronous processes: logging the action to a relational audit database and logging a hash of the transaction to a permissioned blockchain for tamper-evidence.

  #### .4. Purpose
  To document a complex, high-stakes business process that combines real-time control, security auditing, and innovative data integrity features.

  #### .5. Type
  BusinessProcess

  #### .6. Participant Repository Ids
  
  - REPO-APP-PRESENTATION
  - REPO-APP-CLIENT
  - REPO-APP-DATA
  - REPO-APP-INTEGRATION
  - REPO-APP-MESSAGING
  - BlockchainNetwork
  - OpcDaServer
  
  #### .7. Key Interactions
  
  - An authorized user initiates a write operation from the Blazor UI (Presentation Layer).
  - A request is sent to the Core OPC Client to perform the write.
  - Core OPC Client performs the synchronous write operation to the OPC DA/UA Server and receives confirmation.
  - Confirmation is sent back to the user's UI.
  - [Async] Core OPC Client publishes a detailed audit message (user, tag, old/new value, timestamp) to an 'audit' topic on the Message Queue.
  - [Async] Core OPC Client publishes a 'critical event' message (hash of data, timestamp, source/dest) to a 'blockchain' topic on the Message Queue.
  - The Data Service consumes the audit message and writes it to the DataLog table in the relational database.
  - The External Integration Service consumes the blockchain message, connects to the Blockchain Network, and commits the transaction via a smart contract.
  
  #### .8. Related Feature Ids
  
  - REQ-CSVC-009
  - REQ-8-007
  - REQ-8-008
  - REQ-DLP-025
  
  #### .9. Domain
  Operations & Compliance

  #### .10. Metadata
  
  - **Complexity:** High
  - **Priority:** Critical
  


---

# 2. Sequence Diagram Details

- **Success:** True
- **Cache_Created:** True
- **Status:** refreshed
- **Cache_Id:** kswlxre8o74szkiwvuu0e4l7r0ecbo3ndfhznnp8
- **Cache_Name:** cachedContents/kswlxre8o74szkiwvuu0e4l7r0ecbo3ndfhznnp8
- **Cache_Display_Name:** repositories
- **Cache_Status_Verified:** True
- **Model:** models/gemini-2.5-pro-preview-03-25
- **Workflow_Id:** I9v2neJ0O4zJsz8J
- **Execution_Id:** AIzaSyCGei_oYXMpZW-N3d-yH-RgHKXz8dsixhc
- **Project_Id:** 10
- **Record_Id:** 12
- **Cache_Type:** repositories


---

