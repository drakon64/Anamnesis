output "bucket" {
  value = module.anamnesis.bucket
}

output "load_balancer_ips" {
  value = module.anamnesis.load_balancer_ips
}

output "services" {
  value = module.anamnesis.services
}
