variable "artifact_registry_location" {
  type = string
}

variable "bucket_location" {
  type = string
}

variable "bucket_name" {
  type = string
}

variable "domain" {
  type = string
}

variable "project" {
  type = string
}

variable "regions" {
  type = set(string)
}

variable "state_bucket" {
  type = string
}

variable "use_ghcr" {
  description = "Use pre-built Anamnesis images from GitHub Packages"
  default     = true
}
