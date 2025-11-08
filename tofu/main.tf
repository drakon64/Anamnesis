terraform {
  backend "gcs" {
    bucket = var.state_bucket

    prefix = "anamnesis"
  }
}

provider "google" {
  project = var.project
  region  = var.region
}

module "anamnesis" {
  # source = "./module"

  source = "anamnesis.drakon.cloud/drakon64/anamnesis/google"
  version = "0.0.1"

  bucket_location = var.bucket_location
  bucket_name     = var.bucket_name
  domain          = var.domain
  project         = var.project
  region          = var.region
}
