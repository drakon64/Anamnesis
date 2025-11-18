resource "google_service_account" "repository" {
  account_id = "anamnesis"
}

resource "google_service_account" "dashboard" {
  account_id = "anamnesis-dashboard"
}

resource "google_project_iam_member" "dashboard" {
  member  = google_service_account.dashboard.member
  project = data.google_project.project.id
  role    = "roles/datastore.viewer"
}
