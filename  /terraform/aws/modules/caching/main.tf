# /--------------------------------------------------------------------------------
# V A R I A B L E S
# --------------------------------------------------------------------------------/
variable "environment_name" {
  description = "The name of the environment (e.g., prod, staging)."
  type        = string
}

variable "vpc_id" {
  description = "The ID of the VPC where the cache will be deployed."
  type        = string
}

variable "private_subnet_ids" {
  description = "A list of private subnet IDs for the cache instances."
  type        = list(string)
}

variable "cache_security_group_ids" {
  description = "List of security group IDs to apply to the cache cluster."
  type        = list(string)
}

variable "node_type" {
  description = "The node type for the ElastiCache Redis cluster."
  type        = string
  default     = "cache.t3.small"
}

variable "num_node_groups" {
  description = "The number of shards (node groups) for the Redis cluster."
  type        = number
  default     = 1
}

variable "replicas_per_node_group" {
  description = "The number of replicas per shard."
  type        = number
  default     = 1
}

# /--------------------------------------------------------------------------------
# L O C A L S
# --------------------------------------------------------------------------------/
locals {
  common_tags = {
    Environment = var.environment_name
    Project     = "SSS-OPC-Client"
    ManagedBy   = "Terraform"
  }
}

# /--------------------------------------------------------------------------------
# E L A S T I C A C H E   F O R   R E D I S
# --------------------------------------------------------------------------------/

# --- Subnet Group for ElastiCache ---
resource "aws_elasticache_subnet_group" "main" {
  name       = "${var.environment_name}-redis-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-redis-subnet-group"
  })
}

# --- ElastiCache Redis Replication Group (Cluster Mode) ---
# This resource provisions a Redis cluster that supports sharding and high availability.
resource "aws_elasticache_replication_group" "main" {
  replication_group_id          = "${var.environment_name}-sss-opc-redis"
  replication_group_description = "Redis cache for SSS OPC Platform (${var.environment_name})"
  node_type                     = var.node_type
  port                          = 6379
  parameter_group_name          = "default.redis7.cluster.on"
  subnet_group_name             = aws_elasticache_subnet_group.main.name
  security_group_ids            = var.cache_security_group_ids
  engine                        = "redis"
  engine_version                = "7.0"

  # High Availability and Scalability configuration as per SDS
  automatic_failover_enabled = true
  cluster_mode {
    num_node_groups         = var.num_node_groups
    replicas_per_node_group = var.replicas_per_node_group
  }

  # Security configuration as per SDS
  at_rest_encryption_enabled = true
  transit_encryption_enabled = true
  # auth_token can be used to set a password, retrieved from Secrets Manager
  # auth_token = aws_secretsmanager_secret_version.redis_auth.secret_string

  tags = local.common_tags
}

# /--------------------------------------------------------------------------------
# O U T P U T S
# --------------------------------------------------------------------------------/
output "redis_primary_endpoint_address" {
  description = "The primary endpoint address for the Redis replication group."
  value       = aws_elasticache_replication_group.main.primary_endpoint_address
}

output "redis_primary_endpoint_port" {
  description = "The port for the Redis primary endpoint."
  value       = aws_elasticache_replication_group.main.port
}

output "redis_reader_endpoint_address" {
  description = "The reader endpoint address for the Redis replication group."
  value       = aws_elasticache_replication_group.main.reader_endpoint_address
}