locals {
  image = var.use_ghcr ? "${var.region}-docker.pkg.dev/${data.google_project.project.project_id}/${google_artifact_registry_repository.artifact_registry.name}/drakon64/anamnesis@${data.docker_registry_image.anamnesis[0].sha256_digest}" : data.google_artifact_registry_docker_image.anamnesis[0].self_link
}

resource "google_project_service" "cloud_run" {
  service = "run.googleapis.com"
}

data "docker_registry_image" "anamnesis" {
  count = var.use_ghcr ? 1 : 0

  name = "ghcr.io/drakon64/anamnesis:latest"
}

data "google_artifact_registry_docker_image" "anamnesis" {
  count = var.use_ghcr ? 0 : 1

  image_name    = "anamnesis:latest"
  location      = var.region
  repository_id = google_artifact_registry_repository.artifact_registry.repository_id
}

resource "google_cloud_run_v2_service" "service" {
  location = var.region
  name     = "anamnesis"

  default_uri_disabled = true
  ingress              = "INGRESS_TRAFFIC_INTERNAL_LOAD_BALANCER"

  template {
    containers {
      image = local.image

      env {
        name = "ANAMNESIS_BUCKET"

        value = google_storage_bucket.bucket.name
      }

      resources {
        cpu_idle = true

        limits = {
          cpu    = "1000m"
          memory = "614Mi"
        }
      }

      startup_probe {
        failure_threshold = 10
        period_seconds    = 1

        tcp_socket {
          port = 8080
        }
      }
    }

    max_instance_request_concurrency = 1000

    scaling {
      max_instance_count = 1
      min_instance_count = 0
    }

    service_account = google_service_account.anamnesis.email

    timeout = "10s"
  }

  depends_on = [google_project_service.cloud_run]
}
