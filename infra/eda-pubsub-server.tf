//This takes an Service Principal that you created to uniquely use in this simulation for the server app (as we use Role based access on all resources).
//Make sure to also setup the AZURE_* environment variable on the server app launchSettings in order to use this identity to run the server app
data "azuread_service_principal" "pubsub_server" {
  application_id = "${var.pubsub_server_application_id}"
}

resource "azurerm_servicebus_topic" "topics_domain" {
  for_each = toset(var.server_domain_config.topics)
  name = each.value

  namespace_id = azurerm_servicebus_namespace.namespace_domain.id
  requires_duplicate_detection = true
  support_ordering = true
  max_size_in_megabytes = 1024
  #max_message_size_in_kilobytes = 1024
  enable_partitioning = false
  enable_batched_operations = true
  enable_express = false
  duplicate_detection_history_time_window = "P1DT10M"
  default_message_ttl = "P14D"
  auto_delete_on_idle = "P10675199DT2H48M5.4775807S"
}

resource "azurerm_servicebus_queue" "pubsub_server_command_queue" {
  namespace_id = azurerm_servicebus_namespace.namespace_domain.id
  name = var.pubsub_server_queues.listening_queue
  default_message_ttl = "P10675199DT2H48M5.4775807S"
  lock_duration = "PT1M"
  max_size_in_megabytes = "1024"
  dead_lettering_on_message_expiration = false
  duplicate_detection_history_time_window = "PT10M"
  max_delivery_count = 1
  enable_batched_operations = true
  auto_delete_on_idle = "P10675199DT2H48M5.4775807S"
  enable_partitioning = false
  enable_express = false
  requires_session = false
  requires_duplicate_detection = false
}

resource "azurerm_servicebus_queue" "pubsub_server_error_queue" {
  namespace_id = azurerm_servicebus_namespace.namespace_domain.id
  name = var.pubsub_server_queues.error_queue
  default_message_ttl = "P10675199DT2H48M5.4775807S"
  lock_duration = "PT1M"
  max_size_in_megabytes = "1024"
  dead_lettering_on_message_expiration = false
  duplicate_detection_history_time_window = "PT10M"
  max_delivery_count = 1
  enable_batched_operations = true
  auto_delete_on_idle = "P10675199DT2H48M5.4775807S"
  enable_partitioning = false
  enable_express = false
  requires_session = false
  requires_duplicate_detection = false
}

resource "azurerm_role_assignment" "role_data_owner_domain_topics_to_server_appidentity" {
    for_each = toset(var.server_domain_config.topics)

    scope = "${azurerm_servicebus_topic.topics_domain[each.value].id}"
    role_definition_name = "Azure Service Bus Data Owner"
    principal_id = data.azuread_service_principal.pubsub_server.object_id
}

resource "azurerm_role_assignment" "role_data_owner_on_pub_sub_server_queue_to_server_appidentity" {
    scope = azurerm_servicebus_queue.pubsub_server_command_queue.id
    role_definition_name = "Azure Service Bus Data Owner"
    principal_id = data.azuread_service_principal.pubsub_server.object_id
}

resource "azurerm_role_assignment" "role_data_owner_on_pub_sub_server_error_queue_to_server_appidentity" {
    scope = azurerm_servicebus_queue.pubsub_server_error_queue.id
    role_definition_name = "Azure Service Bus Data Owner"
    principal_id = data.azuread_service_principal.pubsub_server.object_id
}