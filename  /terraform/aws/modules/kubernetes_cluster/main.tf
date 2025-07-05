# /--------------------------------------------------------------------------------
# V A R I A B L E S
# --------------------------------------------------------------------------------/

variable "cluster_name" {
  description = "The name for the EKS cluster."
  type        = string
}

variable "environment_name" {
  description = "The name of the environment (e.g., prod, staging)."
  type        = string
}

variable "vpc_id" {
  description = "The ID of the VPC where the cluster will be deployed."
  type        = string
}

variable "subnet_ids" {
  description = "A list of subnet IDs for the EKS worker nodes."
  type        = list(string)
}

variable "cluster_version" {
  description = "The Kubernetes version for the EKS cluster."
  type        = string
}

variable "general_instance_type" {
  description = "The EC2 instance type for general-purpose workloads."
  type        = string
}

variable "gpu_instance_type" {
  description = "The EC2 instance type for GPU-enabled AI/ML workloads."
  type        = string
}

# /--------------------------------------------------------------------------------
# L O C A L S
# --------------------------------------------------------------------------------/

locals {
  common_tags = {
    Environment = var.environment_name
    Project     = "SSS-OPC-Client"
    ManagedBy   = "Terraform"
    Cluster     = var.cluster_name
  }
}

# /--------------------------------------------------------------------------------
# I A M   R O L E S   &   P O L I C I E S
# --------------------------------------------------------------------------------/

# --- IAM Role for EKS Control Plane ---
resource "aws_iam_role" "cluster" {
  name = "${var.cluster_name}-cluster-role"

  assume_role_policy = jsonencode({
    Version   = "2012-10-17",
    Statement = [{
      Effect    = "Allow",
      Principal = {
        Service = "eks.amazonaws.com"
      },
      Action = "sts:AssumeRole"
    }]
  })

  tags = local.common_tags
}

resource "aws_iam_role_policy_attachment" "cluster_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSClusterPolicy"
  role       = aws_iam_role.cluster.name
}

# --- IAM Role for EKS Worker Nodes ---
resource "aws_iam_role" "nodes" {
  name = "${var.cluster_name}-node-group-role"

  assume_role_policy = jsonencode({
    Version   = "2012-10-17",
    Statement = [{
      Effect    = "Allow",
      Principal = {
        Service = "ec2.amazonaws.com"
      },
      Action = "sts:AssumeRole"
    }]
  })

  tags = local.common_tags
}

resource "aws_iam_role_policy_attachment" "nodes_worker_node_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy"
  role       = aws_iam_role.nodes.name
}

resource "aws_iam_role_policy_attachment" "nodes_cni_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy"
  role       = aws_iam_role.nodes.name
}

resource "aws_iam_role_policy_attachment" "nodes_ecr_read_only_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
  role       = aws_iam_role.nodes.name
}

# /--------------------------------------------------------------------------------
# E K S   C L U S T E R
# --------------------------------------------------------------------------------/

resource "aws_eks_cluster" "main" {
  name     = var.cluster_name
  role_arn = aws_iam_role.cluster.arn
  version  = var.cluster_version

  vpc_config {
    subnet_ids              = var.subnet_ids
    endpoint_private_access = true
    endpoint_public_access  = true # Can be restricted if needed
  }

  depends_on = [
    aws_iam_role_policy_attachment.cluster_policy,
  ]

  tags = local.common_tags
}

# --- EKS Node Group for General Workloads ---
resource "aws_eks_node_group" "general" {
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = "${var.cluster_name}-general-nodes"
  node_role_arn   = aws_iam_role.nodes.arn
  subnet_ids      = var.subnet_ids
  instance_types  = [var.general_instance_type]

  scaling_config {
    desired_size = 2
    max_size     = 4
    min_size     = 1
  }

  update_config {
    max_unavailable = 1
  }

  depends_on = [
    aws_iam_role_policy_attachment.nodes_worker_node_policy,
    aws_iam_role_policy_attachment.nodes_cni_policy,
    aws_iam_role_policy_attachment.nodes_ecr_read_only_policy,
  ]

  tags = merge(local.common_tags, {
    "k8s.io/cluster-autoscaler/enabled"             = "true"
    "k8s.io/cluster-autoscaler/${var.cluster_name}" = "owned"
    "NodeType"                                      = "general-purpose"
  })
}

# --- EKS Node Group for GPU Workloads ---
resource "aws_eks_node_group" "gpu" {
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = "${var.cluster_name}-gpu-nodes"
  node_role_arn   = aws_iam_role.nodes.arn
  subnet_ids      = var.subnet_ids
  instance_types  = [var.gpu_instance_type]

  scaling_config {
    desired_size = 1
    max_size     = 2
    min_size     = 0
  }

  taint {
    key    = "eks.amazonaws.com/compute-type"
    value  = "gpu"
    effect = "NO_SCHEDULE"
  }

  update_config {
    max_unavailable = 1
  }

  depends_on = [
    aws_iam_role_policy_attachment.nodes_worker_node_policy,
    aws_iam_role_policy_attachment.nodes_cni_policy,
    aws_iam_role_policy_attachment.nodes_ecr_read_only_policy,
  ]

  tags = merge(local.common_tags, {
    "k8s.io/cluster-autoscaler/enabled"             = "true"
    "k8s.io/cluster-autoscaler/${var.cluster_name}" = "owned"
    "NodeType"                                      = "gpu-ml"
  })
}

# /--------------------------------------------------------------------------------
# O U T P U T S
# --------------------------------------------------------------------------------/

output "cluster_endpoint" {
  description = "The endpoint for your EKS cluster's API server."
  value       = aws_eks_cluster.main.endpoint
}

output "cluster_ca_certificate" {
  description = "The base64 encoded certificate data required to communicate with your cluster."
  value       = aws_eks_cluster.main.certificate_authority[0].data
}

output "cluster_oidc_issuer_url" {
  description = "The OIDC issuer URL for the cluster."
  value       = aws_eks_cluster.main.identity[0].oidc[0].issuer
}