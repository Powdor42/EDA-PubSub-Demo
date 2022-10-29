resource "azurerm_servicebus_namespace" "namespace_domain" {
  name =  "${var.company_short_name}-sb-${var.environment}"
  location = azurerm_resource_group.eda-main-rg.location
  resource_group_name = azurerm_resource_group.eda-main-rg.name
  sku = "Standard"
  zone_redundant = false
  minimum_tls_version = 1.2
  local_auth_enabled = false
  public_network_access_enabled = true
  tags = {
    terraform   = "true"
    environment = var.environment
    application = "${var.company_short_name}-global"
  }
}

resource "azurerm_role_assignment" "role_assignment_data_receiver_sbns_to_developer_group" {
    scope = azurerm_servicebus_namespace.namespace_domain.id
    role_definition_name = "Azure Service Bus Data Owner"
    principal_id = data.azuread_group.developers.object_id
}