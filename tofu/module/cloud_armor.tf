module "cloud_armor" {
  source  = "GoogleCloudPlatform/cloud-armor/google"
  version = "7.0.0"

  name       = "anamnesis"
  project_id = data.google_project.project.project_id

  default_rule_action = "allow"
  
  pre_configured_rules = {
    "xss" = {
      action = "deny(502)"
      priority          = 0
      sensitivity_level = 2
      target_rule_set   = "xss-v33-stable"
    }

    "lfi" = {
      action = "deny(502)"
      priority          = 1
      sensitivity_level = 1
      target_rule_set   = "lfi-v33-stable"
    }

    "rfi" = {
      action = "deny(502)"
      priority          = 2
      sensitivity_level = 2
      target_rule_set   = "rfi-v33-stable"
    }

    "rce" = {
      action = "deny(502)"
      priority          = 3
      sensitivity_level = 3
      target_rule_set   = "rce-v33-stable"
    }
    
    "methodenforcement" = {
      action = "deny(502)"
      priority = 4
      sensitivity_level = 1
      target_rule_set = "methodenforcement-v33-stable"
    }
    
    "scannerdetection" = {
      action = "deny(502)"
      priority = 5
      sensitivity_level = 2
      target_rule_set = "scannerdetection-v33-stable"
    }

    "protocolattack" = {
      action = "deny(502)"
      priority = 6
      sensitivity_level = 3
      target_rule_set = "protocolattack-v33-stable"
    }

    "sessionfixation" = {
      action = "deny(502)"
      priority = 7
      sensitivity_level = 1
      target_rule_set = "sessionfixation-v33-stable"
    }
  }

  depends_on = [google_project_service.compute]
}
