resource "google_service_account" "repository" {
  account_id = "anamnesis"
}

resource "google_service_account" "dashboard" {
  account_id = "anamnesis-dashboard"
}
