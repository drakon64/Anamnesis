resource "google_service_account" "anamnesis" {
  account_id = "anamnesis"
}

# resource "google_service_account_iam_member" "iam" {
#   member             = google_service_account.anamnesis.member
#   role               = "roles/iam.serviceAccountUser"
#   service_account_id = google_service_account.anamnesis.id
# }
