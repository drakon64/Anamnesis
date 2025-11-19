locals {
  dashboard_image = var.use_ghcr ? "${var.artifact_registry_location}-docker.pkg.dev/${data.google_project.project.project_id}/${google_artifact_registry_repository.artifact_registry.name}/drakon64/anamnesis-dashboard@${data.docker_registry_image.dashboard[0].sha256_digest}" : data.google_artifact_registry_docker_image.dashboard[0].self_link
}

data "docker_registry_image" "dashboard" {
  count = var.use_ghcr ? 1 : 0

  name = "ghcr.io/drakon64/anamnesis-dashboard:latest"
}

data "google_artifact_registry_docker_image" "dashboard" {
  count = var.use_ghcr ? 0 : 1

  image_name    = "anamnesis-repository:latest"
  location      = var.artifact_registry_location
  repository_id = google_artifact_registry_repository.artifact_registry.repository_id
}

resource "google_cloud_run_v2_service" "dashboard" {
  for_each = var.regions

  location = each.value
  name     = "anamnesis-dashboard"

  default_uri_disabled = true
  ingress              = "INGRESS_TRAFFIC_INTERNAL_LOAD_BALANCER"

  scaling {
    manual_instance_count = 0
    max_instance_count    = 1
    min_instance_count    = 0
  }

  template {
    containers {
      image = local.dashboard_image

      env {
        name = "ANAMNESIS_BUCKET"

        value = google_storage_bucket.bucket.name
      }

      env {
        name = "ANAMNESIS_DATABASE"

        value = google_firestore_database.database.name
      }

      env {
        name = "ANAMNESIS_PROJECT"

        value = data.google_project.project.project_id
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

    service_account = google_service_account.dashboard.email

    timeout = "10s"
  }

  depends_on = [google_project_service.cloud_run]
}

resource "google_cloud_run_v2_service_iam_member" "iap" {
  for_each = var.regions

  member = "serviceAccount:service-${data.google_project.project.number}@gcp-sa-iap.iam.gserviceaccount.com"
  name   = google_cloud_run_v2_service.dashboard[each.value].id
  role   = "roles/run.invoker"
}
