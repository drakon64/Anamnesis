output "bucket" {
  value = google_storage_bucket.bucket.name
}

output "load_balancer_ips" {
  value = {
    v4 = module.lb-http.external_ip
    v6 = module.lb-http.external_ipv6_address
  }
}

output "services" {
  value = {
    repository : [for service in google_cloud_run_v2_service.repository : service.id]
    dashboard : [for service in google_cloud_run_v2_service.dashboard : service.id]
  }
}
