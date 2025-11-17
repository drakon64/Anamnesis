resource "google_project_service" "compute" {
  service = "compute.googleapis.com"
}

resource "google_compute_url_map" "lb" {
  name = "anamnesis-url-map"

  default_service = module.lb-http.backend_services["dashboard"].id

  host_rule {
    hosts        = ["*"]
    path_matcher = "path-matcher-1"
  }

  path_matcher {
    default_service = module.lb-http.backend_services["dashboard"].id
    name            = "path-matcher-1"

    path_rule {
      paths = [
        "/.well-known/terraform.json",
        "/terraform/*"
      ]

      service = module.lb-http.backend_services["repository"].id
    }
  }
}

resource "google_compute_ssl_policy" "tls" {
  name = "anamnesis"

  min_tls_version = "TLS_1_2"
  profile         = "RESTRICTED"
}

module "lb-http" {
  source  = "terraform-google-modules/lb-http/google//modules/serverless_negs"
  version = "14.0.0"

  name    = "anamnesis"
  project = data.google_project.project.project_id

  security_policy = module.cloud_armor.policy.id

  backends = {
    repository = {
      enable_cdn = false
      groups     = []

      log_config = {
        enable = false
      }

      serverless_neg_backends = [for region in var.regions : {
        region = region

        service = {
          name = google_cloud_run_v2_service.repository[region].name
        }

        type = "cloud-run"
      }]
    }

    dashboard = {
      enable_cdn = false
      groups     = []

      log_config = {
        enable = false
      }

      serverless_neg_backends = [for region in var.regions : {
        region = region

        service = {
          name = google_cloud_run_v2_service.dashboard[region].name
        }

        type = "cloud-run"
      }]
    }
  }

  create_ipv6_address             = true
  enable_ipv6                     = true
  create_url_map = false
  http_forward                    = false
  load_balancing_scheme           = "EXTERNAL_MANAGED"
  managed_ssl_certificate_domains = [var.domain]
  ssl                             = true
  ssl_policy                      = google_compute_ssl_policy.tls.self_link
  url_map = google_compute_url_map.lb.id

  depends_on = [google_project_service.compute]
}
