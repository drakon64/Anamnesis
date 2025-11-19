locals {
  repository_image = var.use_ghcr ? "${var.artifact_registry_location}-docker.pkg.dev/${data.google_project.project.project_id}/${google_artifact_registry_repository.artifact_registry.name}/drakon64/anamnesis@${data.docker_registry_image.repository[0].sha256_digest}" : data.google_artifact_registry_docker_image.repository[0].self_link
}

data "docker_registry_image" "repository" {
  count = var.use_ghcr ? 1 : 0

  name = "ghcr.io/drakon64/anamnesis:latest"
}

data "google_artifact_registry_docker_image" "repository" {
  count = var.use_ghcr ? 0 : 1

  image_name    = "anamnesis:latest"
  location      = var.artifact_registry_location
  repository_id = google_artifact_registry_repository.artifact_registry.repository_id
}

resource "google_cloud_run_v2_service" "repository" {
  for_each = var.regions

  location = each.value
  name     = "anamnesis"

  default_uri_disabled = true
  ingress              = "INGRESS_TRAFFIC_INTERNAL_LOAD_BALANCER"

  scaling {
    manual_instance_count = 0
    max_instance_count    = 1
    min_instance_count    = 0
  }

  template {
    containers {
      image = local.repository_image

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

    service_account = google_service_account.repository.email

    timeout = "10s"
  }

  depends_on = [google_project_service.cloud_run]
}
