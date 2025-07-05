# This file composes the infrastructure modules for the 'production' environment.
# It wires modules together and passes environment-specific variables.

# --- Terraform and Provider Configuration ---
# NOTE: The 'backend' configuration should ideally be in its own 'backend.tf' file.
terraform {
  required_version = ">= 1.0"

  backend "s3" {
    bucket         = "sss-opc-terraform-state-prod"
    key            = "prod/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "sss-opc-terraform-locks"
    encrypt        = true
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

# --- Local variables for consistent tagging and naming ---
locals {
  environment_name = "prod"
  common_tags = {
    Environment = local.environment_name
    Project     = "SSS-OPC-Client"
    ManagedBy   = "Terraform"
  }
}

# --- Module Instantiation ---

# 1. Networking Module
# Provisions the foundational network infrastructure.
module "networking" {
  source = "../modules/networking"

  aws_region           = var.aws_region
  environment_name     = local.environment_name
  vpc_cidr_block       = var.vpc_cidr_block
  public_subnet_cidrs  = var.public_subnet_cidrs
  private_subnet_cidrs = var.private_subnet_cidrs
  availability_zones   = var.availability_zones
  on_prem_gateway_ip   = var.on_prem_gateway_ip
}

# 2. Database Module
# Provisions RDS and Timestream, depends on the networking module.
module "database" {
  source = "../modules/database"

  environment_name      = local.environment_name
  vpc_id                = module.networking.vpc_id
  private_subnet_ids    = module.networking.private_subnet_ids
  db_security_group_ids = [module.networking.rds_security_group_id]
  db_username           = var.db_username # Sourced from a secure location
  db_password           = var.db_password # Sourced from a secure location
  rds_instance_class    = "db.r6g.large"  # Production-grade instance type
}

# 3. Caching Module
# Provisions ElastiCache for Redis, depends on the networking module.
module "caching" {
  source = "../modules/caching"

  environment_name           = local.environment_name
  vpc_id                     = module.networking.vpc_id
  private_subnet_ids         = module.networking.private_subnet_ids
  cache_security_group_ids   = [module.networking.elasticache_security_group_id]
  node_type                  = "cache.r6g.large" # Production-grade node type
  num_node_groups            = 2                 # Example of sharding for production
  replicas_per_node_group    = 1
}

# 4. Kubernetes Cluster Module
# Provisions the EKS cluster, depends on the networking module.
module "kubernetes_cluster" {
  source = "../modules/kubernetes_cluster"

  cluster_name          = "${local.environment_name}-sss-opc-eks-cluster"
  environment_name      = local.environment_name
  vpc_id                = module.networking.vpc_id
  subnet_ids            = module.networking.private_subnet_ids # Worker nodes in private subnets
  cluster_version       = "1.28"
  general_instance_type = "m6i.large"   # Production-grade instance type
  gpu_instance_type     = "g4dn.xlarge" # Production-grade GPU instance type
}

# --- Root Outputs ---
# Expose key values from modules for easy access after deployment.

output "cluster_endpoint" {
  description = "EKS cluster endpoint."
  value       = module.kubernetes_cluster.cluster_endpoint
}

output "cluster_oidc_issuer_url" {
  description = "EKS cluster OIDC issuer URL."
  value       = module.kubernetes_cluster.cluster_oidc_issuer_url
}

output "rds_cluster_endpoint" {
  description = "RDS cluster endpoint."
  value       = module.database.rds_cluster_endpoint
}

output "redis_primary_endpoint_address" {
  description = "Redis primary endpoint address."
  value       = module.caching.redis_primary_endpoint_address
}