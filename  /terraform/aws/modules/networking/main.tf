# /--------------------------------------------------------------------------------
# V A R I A B L E S
# --------------------------------------------------------------------------------/

variable "aws_region" {
  description = "The AWS region to deploy resources in."
  type        = string
}

variable "environment_name" {
  description = "The name of the environment (e.g., prod, staging)."
  type        = string
}

variable "vpc_cidr_block" {
  description = "The CIDR block for the VPC."
  type        = string
}

variable "public_subnet_cidrs" {
  description = "A list of CIDR blocks for public subnets."
  type        = list(string)
}

variable "private_subnet_cidrs" {
  description = "A list of CIDR blocks for private subnets."
  type        = list(string)
}

variable "availability_zones" {
  description = "A list of Availability Zones to deploy subnets into."
  type        = list(string)
}

variable "on_prem_gateway_ip" {
  description = "Public IP address of the on-premise customer gateway for the Site-to-Site VPN."
  type        = string
}

# /--------------------------------------------------------------------------------
# R E S O U R C E S
# --------------------------------------------------------------------------------/

locals {
  common_tags = {
    Environment = var.environment_name
    Project     = "SSS-OPC-Client"
    ManagedBy   = "Terraform"
  }
}

# --- VPC ---
resource "aws_vpc" "main" {
  cidr_block           = var.vpc_cidr_block
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-vpc"
  })
}

# --- Internet Gateway for Public Subnets ---
resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-igw"
  })
}

# --- Public Subnets ---
resource "aws_subnet" "public" {
  count                   = length(var.public_subnet_cidrs)
  vpc_id                  = aws_vpc.main.id
  cidr_block              = var.public_subnet_cidrs[count.index]
  availability_zone       = var.availability_zones[count.index]
  map_public_ip_on_launch = true

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-public-subnet-${var.availability_zones[count.index]}"
  })
}

# --- Elastic IPs & NAT Gateways for Private Subnets (HA Setup) ---
resource "aws_eip" "nat" {
  count = length(var.public_subnet_cidrs)
  vpc   = true
  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-nat-eip-${var.availability_zones[count.index]}"
  })
}

resource "aws_nat_gateway" "main" {
  count         = length(var.public_subnet_cidrs)
  allocation_id = aws_eip.nat[count.index].id
  subnet_id     = aws_subnet.public[count.index].id

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-nat-gw-${var.availability_zones[count.index]}"
  })

  depends_on = [aws_internet_gateway.main]
}

# --- Private Subnets ---
resource "aws_subnet" "private" {
  count             = length(var.private_subnet_cidrs)
  vpc_id            = aws_vpc.main.id
  cidr_block        = var.private_subnet_cidrs[count.index]
  availability_zone = var.availability_zones[count.index]

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-private-subnet-${var.availability_zones[count.index]}"
  })
}

# --- Routing for Public Subnets ---
resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-public-rt"
  })
}

resource "aws_route_table_association" "public" {
  count          = length(aws_subnet.public)
  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

# --- Routing for Private Subnets (One route table per AZ for HA) ---
resource "aws_route_table" "private" {
  count  = length(aws_subnet.private)
  vpc_id = aws_vpc.main.id

  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.main[count.index].id
  }

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-private-rt-${var.availability_zones[count.index]}"
  })
}

resource "aws_route_table_association" "private" {
  count          = length(aws_subnet.private)
  subnet_id      = aws_subnet.private[count.index].id
  route_table_id = aws_route_table.private[count.index].id
}

# --- Security Groups ---
resource "aws_security_group" "eks_nodes" {
  name        = "${var.environment_name}-eks-node-sg"
  description = "Security group for EKS worker nodes"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    self        = true
    description = "Allow all traffic between nodes"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(local.common_tags, { Name = "${var.environment_name}-eks-node-sg" })
}

resource "aws_security_group" "rds" {
  name        = "${var.environment_name}-rds-sg"
  description = "Security group for RDS instances"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.eks_nodes.id]
    description     = "Allow PostgreSQL access from EKS nodes"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(local.common_tags, { Name = "${var.environment_name}-rds-sg" })
}

resource "aws_security_group" "elasticache" {
  name        = "${var.environment_name}-elasticache-sg"
  description = "Security group for ElastiCache for Redis"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port       = 6379
    to_port         = 6379
    protocol        = "tcp"
    security_groups = [aws_security_group.eks_nodes.id]
    description     = "Allow Redis access from EKS nodes"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(local.common_tags, { Name = "${var.environment_name}-elasticache-sg" })
}

# --- Site-to-Site VPN (REQ-SAP-016) ---
resource "aws_customer_gateway" "on_prem" {
  bgp_asn    = 65000
  ip_address = var.on_prem_gateway_ip
  type       = "ipsec.1"

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-on-prem-cgw"
  })
}

resource "aws_vpn_gateway" "main" {
  vpc_id = aws_vpc.main.id

  tags = merge(local.common_tags, {
    Name = "${var.environment_name}-vpn-gw"
  })
}

# /--------------------------------------------------------------------------------
# O U T P U T S
# --------------------------------------------------------------------------------/

output "vpc_id" {
  description = "The ID of the created VPC."
  value       = aws_vpc.main.id
}

output "public_subnet_ids" {
  description = "A list of the public subnet IDs."
  value       = aws_subnet.public[*].id
}

output "private_subnet_ids" {
  description = "A list of the private subnet IDs."
  value       = aws_subnet.private[*].id
}

output "default_security_group_id" {
  description = "The default security group ID for the VPC."
  value       = aws_vpc.main.default_security_group_id
}

output "eks_node_security_group_id" {
  description = "The ID of the security group for EKS nodes."
  value       = aws_security_group.eks_nodes.id
}

output "rds_security_group_id" {
  description = "The ID of the security group for RDS."
  value       = aws_security_group.rds.id
}

output "elasticache_security_group_id" {
  description = "The ID of the security group for ElastiCache."
  value       = aws_security_group.elasticache.id
}