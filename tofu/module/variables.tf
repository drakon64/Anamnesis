variable "artifact_registry_region" {
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

variable "use_ghcr" {
  description = "Use pre-built Anamnesis images from GitHub Packages"
  default     = true
}
