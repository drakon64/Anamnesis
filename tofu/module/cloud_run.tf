resource "google_project_service" "cloud_run" {
  service = "run.googleapis.com"
}
