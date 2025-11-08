resource "google_storage_bucket" "bucket" {
  location = var.bucket_location
  name     = var.bucket_name

  public_access_prevention    = "enforced"
  uniform_bucket_level_access = true
}

resource "google_storage_bucket_iam_member" "anamnesis" {
  bucket = google_storage_bucket.bucket.name
  member = google_service_account.anamnesis.member
  role   = "roles/storage.objectViewer"
}
