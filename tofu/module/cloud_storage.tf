resource "google_storage_bucket" "bucket" {
  location = var.bucket_location
  name     = var.bucket_name

  public_access_prevention    = "enforced"
  uniform_bucket_level_access = true
}
