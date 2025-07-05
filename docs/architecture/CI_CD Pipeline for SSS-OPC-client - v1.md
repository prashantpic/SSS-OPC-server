# Specification

# 1. Pipelines

## 1.1. Server Microservice CI/CD
Builds, tests, scans, and deploys a single server-side microservice through staging to production.

### 1.1.4. Stages

### 1.1.4.1. CI Build & Scan
#### 1.1.4.1.2. Steps

- dotnet restore
- dotnet build --configuration Release
- dotnet test --configuration Release --no-build --logger trx
- run-static-analysis
- run-dependency-scan

#### 1.1.4.1.3. Environment

- **Build_Config:** Release
- **Dotnet_Cli_Telemetry_Optout:** 1

#### 1.1.4.1.4. Quality Gates

- **Name:** Unit Tests  
**Criteria:**
    
    - all unit tests pass
    
**Blocking:** True  
- **Name:** Code Coverage  
**Criteria:**
    
    - coverage >= 70% (adjustable per service)
    
**Blocking:** False  
- **Name:** Static Analysis  
**Criteria:**
    
    - zero high severity issues
    
**Blocking:** True  
- **Name:** Dependency Scan  
**Criteria:**
    
    - zero critical CVEs
    
**Blocking:** True  

### 1.1.4.2. Containerize & Push
#### 1.1.4.2.2. Steps

- docker build -t ${SERVICE_NAME}:${BUILD_ID} .
- docker scan ${SERVICE_NAME}:${BUILD_ID}
- docker push ${DOCKER_REGISTRY}/${SERVICE_NAME}:${BUILD_ID}

#### 1.1.4.2.3. Environment

- **Docker_Buildkit:** 1
- **Docker_Registry:** your-docker-registry.example.com
- **Service_Name:** replace-me
- **Build_Id:** ${PIPELINE_BUILD_ID}

#### 1.1.4.2.4. Quality Gates

- **Name:** Container Security Scan  
**Criteria:**
    
    - zero critical vulnerabilities
    
**Blocking:** True  

### 1.1.4.3. Integration Tests
#### 1.1.4.3.2. Steps

- deploy-test-environment --service ${SERVICE_NAME} --image ${DOCKER_REGISTRY}/${SERVICE_NAME}:${BUILD_ID}
- run-integration-tests --service ${SERVICE_NAME}

#### 1.1.4.3.3. Environment

- **Test_Env_Namespace:** test-${BUILD_ID}
- **Test_Db_Conn_String:** masked
- **Test_Mq_Endpoint:** masked

#### 1.1.4.3.4. Quality Gates

- **Name:** Integration Tests  
**Criteria:**
    
    - all integration tests pass
    
**Blocking:** True  

### 1.1.4.4. Deploy to Staging
#### 1.1.4.4.2. Steps

- apply-db-migrations --environment Staging --service ${SERVICE_NAME}
- deploy-kubernetes --namespace staging --service ${SERVICE_NAME} --image ${DOCKER_REGISTRY}/${SERVICE_NAME}:${BUILD_ID} --strategy rolling-update
- run-smoke-tests --environment Staging --service ${SERVICE_NAME}
- run-health-checks --environment Staging --service ${SERVICE_NAME}

#### 1.1.4.4.3. Environment

- **Kube_Namespace:** staging
- **Db_Conn_String_Staging:** masked

#### 1.1.4.4.4. Quality Gates

- **Name:** Smoke Tests & Health  
**Criteria:**
    
    - smoke tests pass
    - health checks pass
    
**Blocking:** True  
- **Name:** Manual Approval  
**Criteria:**
    
    - Approved for Staging Testing
    
**Blocking:** True  

### 1.1.4.5. Staging Validation
#### 1.1.4.5.2. Steps

- run-functional-e2e-tests --environment Staging --service ${SERVICE_NAME}
- run-dast-scan --environment Staging --service ${SERVICE_NAME}

#### 1.1.4.5.3. Environment

- **Staging_Api_Endpoint:** https://staging.example.com/api/${SERVICE_NAME}

#### 1.1.4.5.4. Quality Gates

- **Name:** Functional/E2E Tests  
**Criteria:**
    
    - all functional/e2e tests pass
    
**Blocking:** True  
- **Name:** DAST Scan  
**Criteria:**
    
    - zero high severity vulnerabilities
    
**Blocking:** True  

### 1.1.4.6. Deploy to Production
#### 1.1.4.6.2. Steps

- apply-db-migrations --environment Production --service ${SERVICE_NAME} --safe
- deploy-kubernetes --namespace production --service ${SERVICE_NAME} --image ${DOCKER_REGISTRY}/${SERVICE_NAME}:${BUILD_ID} --strategy blue-green
- run-smoke-tests --environment Production --service ${SERVICE_NAME}
- run-health-checks --environment Production --service ${SERVICE_NAME}
- monitor-production-metrics --service ${SERVICE_NAME} --duration 5m

#### 1.1.4.6.3. Environment

- **Kube_Namespace:** production
- **Db_Conn_String_Prod:** masked

#### 1.1.4.6.4. Quality Gates

- **Name:** Manual Approval  
**Criteria:**
    
    - Approved for Production
    
**Blocking:** True  
- **Name:** Smoke Tests & Health  
**Criteria:**
    
    - smoke tests pass
    - health checks pass
    
**Blocking:** True  
- **Name:** Production Metrics Stability  
**Criteria:**
    
    - error rate <= 0.1%
    - p99 latency < defined_threshold
    
**Blocking:** True  


## 1.2. Core OPC Client Release
Builds, tests, scans, and packages the cross-platform Core OPC Client Service for distribution.

### 1.2.4. Stages

### 1.2.4.1. CI Build & Scan (Multi-platform)
#### 1.2.4.1.2. Steps

- dotnet restore
- dotnet build --configuration Release --runtime win-x64 --self-contained true /p:PublishTrimmed=true
- dotnet build --configuration Release --runtime linux-x64 --self-contained true /p:PublishTrimmed=true
- dotnet test --configuration Release --no-build --logger trx
- run-static-analysis
- run-dependency-scan

#### 1.2.4.1.3. Environment

- **Build_Config:** Release
- **Dotnet_Cli_Telemetry_Optout:** 1

#### 1.2.4.1.4. Quality Gates

- **Name:** Unit Tests  
**Criteria:**
    
    - all unit tests pass
    
**Blocking:** True  
- **Name:** Static Analysis  
**Criteria:**
    
    - zero high severity issues
    
**Blocking:** True  
- **Name:** Dependency Scan  
**Criteria:**
    
    - zero critical CVEs
    
**Blocking:** True  

### 1.2.4.2. Integration Tests (OPC Sim)
#### 1.2.4.2.2. Steps

- start-opc-simulators
- run-client-integration-tests
- stop-opc-simulators

#### 1.2.4.2.3. Environment

- **Opc_Sim_Config:** default

#### 1.2.4.2.4. Quality Gates

- **Name:** Integration Tests  
**Criteria:**
    
    - all integration tests pass
    
**Blocking:** True  

### 1.2.4.3. Packaging
#### 1.2.4.3.2. Steps

- dotnet publish --configuration Release --runtime win-x64 --self-contained true -o ./publish/win-x64
- create-windows-installer --path ./publish/win-x64 --version ${BUILD_VERSION}
- dotnet publish --configuration Release --runtime linux-x64 --self-contained true -o ./publish/linux-x64
- create-linux-package --path ./publish/linux-x64 --version ${BUILD_VERSION} --format tar.gz
- docker build -t ${DOCKER_REGISTRY}/opc-client:${BUILD_VERSION}-linux -f Dockerfile.linux .
- docker push ${DOCKER_REGISTRY}/opc-client:${BUILD_VERSION}-linux

#### 1.2.4.3.3. Environment

- **Build_Version:** ${PIPELINE_BUILD_VERSION}
- **Docker_Registry:** your-docker-registry.example.com

### 1.2.4.4. Release Artifacts
#### 1.2.4.4.2. Steps

- upload-artifact --path ./installers/*.msi --repo client-releases
- upload-artifact --path ./packages/*.tar.gz --repo client-releases
- upload-artifact --path ./docker-images.json --repo client-releases
- create-github-release --tag v${BUILD_VERSION} --artifacts *.msi,*.tar.gz

#### 1.2.4.4.3. Environment

- **Artifact_Repo_Url:** https://artifacts.example.com/repository

#### 1.2.4.4.4. Quality Gates

- **Name:** Manual Approval  
**Criteria:**
    
    - Approved for Release
    
**Blocking:** True  


## 1.3. Presentation (UI & Gateway) CI/CD
Builds, tests, scans, and deploys the Blazor UI and API Gateway through staging to production.

### 1.3.4. Stages

### 1.3.4.1. CI Build & Scan
#### 1.3.4.1.2. Steps

- dotnet restore
- dotnet build --configuration Release
- dotnet publish --configuration Release --no-build -o ./publish
- dotnet test --configuration Release --no-build --logger trx
- run-static-analysis
- run-dependency-scan
- run-accessibility-scan --path ./publish/wwwroot

#### 1.3.4.1.3. Environment

- **Build_Config:** Release
- **Dotnet_Cli_Telemetry_Optout:** 1

#### 1.3.4.1.4. Quality Gates

- **Name:** Unit Tests  
**Criteria:**
    
    - all unit tests pass
    
**Blocking:** True  
- **Name:** Static Analysis  
**Criteria:**
    
    - zero high severity issues
    
**Blocking:** True  
- **Name:** Dependency Scan  
**Criteria:**
    
    - zero critical CVEs
    
**Blocking:** True  
- **Name:** Accessibility Scan (WCAG 2.1 AA)  
**Criteria:**
    
    - zero accessibility violations at Level AA
    
**Blocking:** False  

### 1.3.4.2. Containerize & Push
#### 1.3.4.2.2. Steps

- docker build -t presentation:${BUILD_ID} .
- docker scan presentation:${BUILD_ID}
- docker push ${DOCKER_REGISTRY}/presentation:${BUILD_ID}

#### 1.3.4.2.3. Environment

- **Docker_Buildkit:** 1
- **Docker_Registry:** your-docker-registry.example.com
- **Build_Id:** ${PIPELINE_BUILD_ID}

#### 1.3.4.2.4. Quality Gates

- **Name:** Container Security Scan  
**Criteria:**
    
    - zero critical vulnerabilities
    
**Blocking:** True  

### 1.3.4.3. Integration Tests (Gateway)
#### 1.3.4.3.2. Steps

- deploy-test-environment --service presentation --image ${DOCKER_REGISTRY}/presentation:${BUILD_ID}
- run-gateway-integration-tests

#### 1.3.4.3.3. Environment

- **Test_Env_Namespace:** test-presentation-${BUILD_ID}
- **Test_Backend_Endpoint:** http://mock-backend-service

#### 1.3.4.3.4. Quality Gates

- **Name:** Integration Tests  
**Criteria:**
    
    - all integration tests pass
    
**Blocking:** True  

### 1.3.4.4. Deploy to Staging
#### 1.3.4.4.2. Steps

- deploy-kubernetes --namespace staging --service presentation --image ${DOCKER_REGISTRY}/presentation:${BUILD_ID} --strategy rolling-update
- run-smoke-tests --environment Staging --service presentation
- run-health-checks --environment Staging --service presentation

#### 1.3.4.4.3. Environment

- **Kube_Namespace:** staging

#### 1.3.4.4.4. Quality Gates

- **Name:** Smoke Tests & Health  
**Criteria:**
    
    - smoke tests pass
    - health checks pass
    
**Blocking:** True  
- **Name:** Manual Approval  
**Criteria:**
    
    - Approved for Staging Testing
    
**Blocking:** True  

### 1.3.4.5. Staging Validation
#### 1.3.4.5.2. Steps

- run-ui-e2e-tests --environment Staging --service presentation
- run-dast-scan --environment Staging --service presentation
- run-accessibility-scan --url https://staging.example.com

#### 1.3.4.5.3. Environment

- **Staging_Ui_Endpoint:** https://staging.example.com

#### 1.3.4.5.4. Quality Gates

- **Name:** UI E2E Tests  
**Criteria:**
    
    - all e2e tests pass
    
**Blocking:** True  
- **Name:** DAST Scan  
**Criteria:**
    
    - zero high severity vulnerabilities
    
**Blocking:** True  
- **Name:** Accessibility Scan (WCAG 2.1 AA)  
**Criteria:**
    
    - zero accessibility violations at Level AA
    
**Blocking:** True  

### 1.3.4.6. Deploy to Production
#### 1.3.4.6.2. Steps

- deploy-kubernetes --namespace production --service presentation --image ${DOCKER_REGISTRY}/presentation:${BUILD_ID} --strategy blue-green
- run-smoke-tests --environment Production --service presentation
- run-health-checks --environment Production --service presentation
- monitor-production-metrics --service presentation --duration 5m

#### 1.3.4.6.3. Environment

- **Kube_Namespace:** production

#### 1.3.4.6.4. Quality Gates

- **Name:** Manual Approval  
**Criteria:**
    
    - Approved for Production
    
**Blocking:** True  
- **Name:** Smoke Tests & Health  
**Criteria:**
    
    - smoke tests pass
    - health checks pass
    
**Blocking:** True  
- **Name:** Production Metrics Stability  
**Criteria:**
    
    - error rate <= 0.05%
    - p99 latency < defined_threshold
    
**Blocking:** True  




---

# 2. Configuration

- **Artifact Repository:** your-docker-registry.example.com, https://artifacts.example.com/repository
- **Default Branch:** main
- **Retention Policy:** 90d
- **Notification Channel:** slack#opc-deployments
- **Rollback Strategy:** kubernetes-rollback-or-bluegreen-switch


---

