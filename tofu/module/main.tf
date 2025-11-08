terraform {
  required_providers {
    docker = {
      source  = "kreuzwerker/docker"
      version = "~> 3"
    }

    google = {
      source = "hashicorp/google"
    }
  }
}

provider "docker" {
  disable_docker_daemon_check = true
}

provider "google" {
  project = var.project
  region  = var.region
}

data "google_project" "project" {}
