resource "google_project_service" "firestore" {
  service = "firestore.googleapis.com"
}

resource "google_firestore_database" "database" {
  location_id = var.firestore_location
  name        = "anamnesis"
  type        = "FIRESTORE_NATIVE"

  depends_on = [google_project_service.firestore]
}
