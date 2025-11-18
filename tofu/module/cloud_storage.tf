resource "google_storage_bucket" "bucket" {
  location = var.bucket_location
  name     = var.bucket_name

  hierarchical_namespace {
    enabled = true
  }

  public_access_prevention    = "enforced"
  uniform_bucket_level_access = true
}

resource "google_storage_bucket_iam_member" "anamnesis" {
  for_each = toset([
    google_service_account.repository.member,
    google_service_account.dashboard.member,
  ])

  bucket = google_storage_bucket.bucket.name
  member = each.value
  role   = "roles/storage.objectViewer"
}
