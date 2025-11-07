terraform {
  required_providers {
    google = {
      source = "hashicorp/google"
    }
  }
}

data "google_project" "project" {}
