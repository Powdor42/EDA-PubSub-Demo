provider "azurerm" {
  tenant_id       = var.tenant_id
  subscription_id = var.subscription_id

  features {
    key_vault {
      purge_soft_delete_on_destroy               = false
      purge_soft_deleted_certificates_on_destroy = false
      purge_soft_deleted_keys_on_destroy         = false
      purge_soft_deleted_secrets_on_destroy      = false
    }
  }
}

data "azuread_group" "developers" {
  display_name = "${var.developers_group_name}"
}

resource "azurerm_resource_group" "eda-main-rg" {
  name = "${var.company_short_name}-rg-${var.environment}"
  location = var.location
}