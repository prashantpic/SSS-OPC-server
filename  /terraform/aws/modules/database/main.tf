# /--------------------------------------------------------------------------------
# V A R I A B L E S
# --------------------------------------------------------------------------------/
variable "environment_name" {
  description = "The name of the environment (e.g., prod, staging)."
  type        = string
}

variable "vpc_id" {
  description = "The ID of the VPC where the databases will be deployed."
  type        = string
}

variable "private_subnet_ids" {
  description = "A list of private subnet IDs for the database instances."
  type        = list(string)
}

variable "db_security_group_ids" {
  description = "List of security group IDs to apply to the database cluster."
  type        = list(string)
}

variable "db_username" {
  description = "The master username for the RDS cluster."
  type        = string
}

variable "db_password" {
  description = "The master password for the RDS cluster."
  type        = string
  sensitive   = true
}

variable "rds_instance_class" {
  description = "The instance class for the RDS cluster instances."
  type        = string
  default     = "db.t3.medium"
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
# R D S   P O S T G R E S Q L   C L U S T E R
# --------------------------------------------------------------------------------/

# --- Subnet Group for RDS ---
resource "aws_db_subnet_group" "main" {
  name       = "${var.environment_name}-rds-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-rds-subnet-group"
  })
}

# --- RDS Aurora PostgreSQL Cluster ---
resource "aws_rds_cluster" "main" {
  cluster_identifier      = "${var.environment_name}-sss-opc-aurora-cluster"
  engine                  = "aurora-postgresql"
  engine_mode             = "provisioned"
  engine_version          = "13.7"
  database_name           = "sss_opc_platform"
  master_username         = var.db_username
  master_password         = var.db_password
  db_subnet_group_name    = aws_db_subnet_group.main.name
  vpc_security_group_ids  = var.db_security_group_ids
  skip_final_snapshot     = true # Set to false for production
  backup_retention_period = 7      # Days
  preferred_backup_window = "02:00-03:00"

  # Security Features as per SDS
  storage_encrypted          = true
  performance_insights_enabled = true
  # Deletion protection should be enabled for production environments
  deletion_protection = false

  tags = local.common_tags
}

# --- RDS Cluster Instances for High Availability ---
resource "aws_rds_cluster_instance" "main" {
  count              = 2 # For Multi-AZ deployment
  identifier         = "${var.environment_name}-sss-opc-aurora-instance-${count.index}"
  cluster_identifier = aws_rds_cluster.main.id
  instance_class     = var.rds_instance_class
  engine             = aws_rds_cluster.main.engine
  engine_version     = aws_rds_cluster.main.engine_version

  performance_insights_enabled = true
  publicly_accessible          = false

  tags = local.common_tags
}

# /--------------------------------------------------------------------------------
# A M A Z O N   T I M E S T R E A M
# --------------------------------------------------------------------------------/
resource "aws_timestreamwrite_database" "main" {
  database_name = "${var.environment_name}-sss-opc-historical-db"

  tags = local.common_tags
}

resource "aws_timestreamwrite_table" "main" {
  database_name = aws_timestreamwrite_database.main.database_name
  table_name    = "OpcTagData"

  retention_properties {
    memory_store_retention_period_in_hours = 24
    magnetic_store_retention_period_in_days = 365
  }

  tags = local.common_tags
}

# /--------------------------------------------------------------------------------
# O U T P U T S
# --------------------------------------------------------------------------------/
output "rds_cluster_endpoint" {
  description = "The connection endpoint for the RDS cluster."
  value       = aws_rds_cluster.main.endpoint
}

output "rds_cluster_reader_endpoint" {
  description = "The reader connection endpoint for the RDS cluster."
  value       = aws_rds_cluster.main.reader_endpoint
}

output "rds_cluster_id" {
  description = "The ID of the RDS cluster."
  value       = aws_rds_cluster.main.id
}

output "timestream_db_name" {
  description = "The name of the Timestream database."
  value       = aws_timestreamwrite_database.main.database_name
}

output "timestream_table_name" {
  description = "The name of the Timestream table for OPC data."
  value       = aws_timestreamwrite_table.main.table_name
}