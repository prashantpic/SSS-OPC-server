# Software Design Specification (SDS) for ops.iac-kubernetes

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design specification for the `ops.iac-kubernetes` repository. The primary purpose of this repository is to define, provision, and manage the entire cloud and container infrastructure for the SSS-OPC-Client platform using an Infrastructure as Code (IaC) approach. This ensures that deployments are automated, repeatable, version-controlled, and secure.

### 1.2. Scope
The scope of this repository includes:
- **Cloud Infrastructure Provisioning:** Terraform scripts to create all necessary cloud resources on AWS, including networking, databases, caching layers, and Kubernetes clusters.
- **Containerization:** Dockerfiles to build optimized, production-ready container images for all .NET microservices and the core OPC client service.
- **Application Deployment:** A comprehensive Helm chart structure to deploy, configure, and manage the lifecycle of all application components on Kubernetes.

### 1.3. Technologies
- **Infrastructure as Code:** Terraform (HCL)
- **Containerization:** Docker
- **Container Orchestration:** Kubernetes
- **Package Management:** Helm
- **Configuration/Manifests:** YAML

## 2. System Architecture & Design Principles

The design of this repository follows a modular and environment-agnostic approach, adhering to DevOps best practices.

- **Modularity:** Infrastructure is broken down into reusable Terraform modules (e.g., networking, database). Applications are deployed via reusable Helm sub-charts. This enhances maintainability and consistency.
- **Environment Parity:** The structure supports multiple environments (e.g., development, staging, production) by composing the same modules and charts with different environment-specific variable files (`.tfvars` for Terraform, `values.yaml` for Helm).
- **Security by Design:** All security configurations, including network access controls (Security Groups), identity management (IAM roles), and secrets management, are explicitly defined as code. Secure defaults are prioritized.
- **Automation-First:** The repository is designed to be consumed by CI/CD pipelines. All provisioning and deployment actions should be executable through automated scripts without manual intervention.

## 3. Detailed Component Design

### 3.1. Terraform Infrastructure (Cloud Provider: AWS)

The Terraform code is structured into reusable `modules` and environment-specific compositions (e.g., `prod`, `staging`).

#### 3.1.1. Networking Module (`terraform/aws/modules/networking/`)
This module provisions the foundational network infrastructure.
- **Resources:**
    - `aws_vpc`: The main virtual private cloud.
    - `aws_subnet`: Multiple public and private subnets across at least two Availability Zones for high availability.
    - `aws_internet_gateway`: To provide internet access for public subnets.
    - `aws_nat_gateway`: Deployed in public subnets with an Elastic IP to allow outbound internet access for resources in private subnets.
    - `aws_route_table` & `aws_route_table_association`: To manage traffic routing.
    - `aws_security_group`: To define firewall rules for EKS nodes, RDS instances, and ElastiCache.
    - `aws_vpn_gateway` & `aws_customer_gateway`: To establish a secure Site-to-Site VPN connection to on-premise networks, fulfilling **REQ-SAP-016**.
- **Inputs:** `vpc_cidr_block`, `public_subnet_cidrs`, `private_subnet_cidrs`, `environment_name`, `on_prem_gateway_ip`.
- **Outputs:** `vpc_id`, `public_subnet_ids`, `private_subnet_ids`, `default_security_group_id`.

#### 3.1.2. Kubernetes Cluster Module (`terraform/aws/modules/kubernetes_cluster/`)
This module provisions the Amazon EKS cluster.
- **Resources:**
    - `aws_eks_cluster`: The EKS control plane.
    - `aws_eks_node_group`: At least two node groups: one for general-purpose workloads and one with GPU-enabled instance types for AI/ML workloads.
    - `aws_iam_role` & `aws_iam_policy_attachment`: To create necessary IAM roles for the EKS cluster and its nodes.
- **Functionality:** Implements **REQ-SAP-018** by provisioning the Kubernetes environment.
- **Inputs:** `vpc_id`, `subnet_ids`, `cluster_version`, `general_instance_type`, `gpu_instance_type`.
- **Outputs:** `cluster_endpoint`, `cluster_ca_certificate`, `cluster_oidc_issuer_url`.

#### 3.1.3. Database Module (`terraform/aws/modules/database/`)
This module provisions managed databases.
- **Resources:**
    - `aws_rds_cluster`: A PostgreSQL cluster configured for high availability (Multi-AZ).
    - `aws_timestream_database` & `aws_timestream_table`: For storing historical and time-series data.
- **Security:** `storage_encrypted` and `performance_insights_enabled` will be set to `true`. Databases will be placed in private subnets.
- **Inputs:** `db_username`, `db_password_secret_arn`, `vpc_id`, `subnet_ids`.
- **Outputs:** `rds_cluster_endpoint`, `timestream_db_name`.

#### 3.1.4. Caching Module (`terraform/aws/modules/caching/`)
This module provisions the distributed cache, fulfilling **REQ-DLP-013**.
- **Resources:**
    - `aws_elasticache_replication_group`: A Redis cluster with cluster-mode enabled for scalability and high availability.
- **Security:** Placed in private subnets, with a dedicated security group allowing access only from the EKS cluster.
- **Inputs:** `vpc_id`, `subnet_ids`, `node_type`.
- **Outputs:** `redis_primary_endpoint_address`.

#### 3.1.5. Environment Composition (`terraform/aws/prod/`)
This directory composes the modules for a specific environment.
- **`main.tf`:** Instantiates the `networking`, `kubernetes_cluster`, `database`, and `caching` modules. It passes the outputs from the networking module as inputs to the other modules.
- **`terraform.tfvars`:** Contains environment-specific values (e.g., production-grade instance types, CIDR blocks, domain names).
- **`backend.tf`:** Configures the Terraform remote state backend (e.g., using AWS S3 and DynamoDB) for state locking and collaboration.

### 3.2. Containerization (Docker)

Dockerfiles will be created for each microservice and the Core OPC Client to satisfy **REQ-SAP-017**.
- **Location:** `docker/<service-name>/Dockerfile`
- **Standard Dockerfile Structure (Multi-stage):**
    dockerfile
    # --- Build Stage ---
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY ["src/Services/ApiGateway/ApiGateway.csproj", "src/Services/ApiGateway/"]
    # ... copy other project files
    RUN dotnet restore "src/Services/ApiGateway/ApiGateway.csproj"
    COPY . .
    WORKDIR "/src/src/Services/ApiGateway"
    RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

    FROM build AS publish
    RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

    # --- Final Stage ---
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
    WORKDIR /app
    COPY --from=publish /app/publish .
    # Create a non-root user for security
    RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
    USER appuser
    ENTRYPOINT ["dotnet", "ApiGateway.dll"]
    

### 3.3. Kubernetes Deployments (Helm)

A "Chart of Charts" approach will be used to manage all Kubernetes deployments, satisfying **REQ-SAP-018**.

#### 3.3.1. Parent Helm Chart (`kubernetes/helm/sss-opc-platform/`)
- **`Chart.yaml`:**
    - Defines the application name (`sss-opc-platform`) and version.
    - Lists all microservice sub-charts (e.g., `api-gateway`, `auth-service`, `data-service`) under the `dependencies` key.
- **`values.yaml`:**
    - Provides global values accessible by all sub-charts (e.g., `global.imageRegistry`, `global.domainName`).
    - Contains configuration sections for each sub-chart, allowing for centralized override of their default values (e.g., `apiGateway.replicaCount`, `dataService.resources`).

#### 3.3.2. Sub-chart Structure (`charts/<service-name>/`)
Each microservice will have a dedicated sub-chart with the following standard templates:
- **`templates/deployment.yaml`:**
    - Defines a Kubernetes `Deployment`.
    - `replicas`, `image.repository`, `image.tag` are templated from `values.yaml`.
    - `resources.requests` and `resources.limits` (CPU/Memory) are templated.
    - Environment variables are sourced from a `ConfigMap` and `Secret`.
- **`templates/service.yaml`:**
    - Defines a `ClusterIP` service to expose the deployment inside the cluster.
- **`templates/ingress.yaml`:**
    - Defines an `Ingress` resource to expose the service externally.
    - Conditionally enabled via `if .Values.ingress.enabled`.
    - `hosts` and `tls.secretName` are templated to configure routing and HTTPS for secure external access, satisfying **REQ-SAP-016**.
- **`templates/_helpers.tpl`:**
    - Contains common helper templates, like for generating resource names.
- **`values.yaml`:**
    - Contains the default configuration values for this specific service (e.g., `replicaCount: 1`, `image.tag: "latest"`).
- **`Chart.yaml`:**
    - Defines the metadata for the sub-chart itself.

## 4. Requirement Traceability Matrix

| Requirement ID | Description | Implementation Artifact(s) |
| :--- | :--- | :--- |
| **REQ-SAP-015** | Support hybrid cloud deployment models. | `terraform/aws/` scripts provision the cloud part. `docker/core-opc-client/Dockerfile` enables on-premise/edge deployment. |
| **REQ-SAP-016** | Secure channel for hybrid cloud communication. | `terraform/aws/modules/networking/main.tf` (VPN Gateway), `kubernetes/helm/.../ingress.yaml` (TLS termination). |
| **REQ-SAP-017** | Components deployable using Docker. | All files under the `docker/` directory (e.g., `docker/core-opc-client/Dockerfile`). |
| **REQ-SAP-018** | Compatibility with Kubernetes and Helm charts. | All files under the `kubernetes/helm/` directory, especially the `sss-opc-platform` chart. |
| **REQ-DLP-013** | Implement caching strategies (Redis). | `terraform/aws/modules/caching/main.tf` provisions AWS ElastiCache for Redis. |