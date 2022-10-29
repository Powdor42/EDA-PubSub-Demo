//This takes an Service Principal that you created to uniquely use in this simulation (as we use Role based access on all resources).
//Make sure to also setup the AZURE_* environment variable on the client app launchSettings in order to use this identity to run the client app
data "azuread_service_principal" "pubsub_client" {
  application_id = "${var.pubsub_client_application_id}"
}

//The unique queue that the client will actually use to take new messages from (it listens to this single point only)
resource "azurerm_servicebus_queue" "pub_sub_client_queue" {
    namespace_id = azurerm_servicebus_namespace.namespace_domain.id
    name = "${var.pubsub_client_queues.listening_queue}"
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

resource "azurerm_role_assignment" "role_data_owner_on_pub_sub_client_queue_to_client_appidentity" {
    scope = azurerm_servicebus_queue.pub_sub_client_queue.id
    role_definition_name = "Azure Service Bus Data Owner"
    principal_id = data.azuread_service_principal.pubsub_client.id
}

//Next to a listening queue you also need an error queue per client to allow a simple retry strategy
resource "azurerm_servicebus_queue" "pubsub_client_error_queue" {
  namespace_id = azurerm_servicebus_namespace.namespace_domain.id
  name = "${var.pubsub_client_queues.error_queue}"
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

resource "azurerm_role_assignment" "role_data_owner_on_pub_sub_client_error_queue_to_client_appidentity" {
    scope = azurerm_servicebus_queue.pubsub_client_error_queue.id
    role_definition_name = "Azure Service Bus Data Owner"
    principal_id = data.azuread_service_principal.pubsub_client.object_id
}

//Next part is to setup anything topic or command-queue you want to interact with

//Subscription to each topic is setup as a forward rule to flow all messages into the client's queue (created above)
//This is a little bit special for Rebus, you need to set the name of the subscription equal to the listening queue (same subscription name on each topic)
resource "azurerm_servicebus_subscription" "client_app_topic_subscription" {
    for_each = toset(var.client_app_topic_subscriptions)
    name               = "${var.pubsub_client_queues.listening_queue}"
    topic_id           = azurerm_servicebus_topic.topics_domain[each.value].id
    max_delivery_count = 1
    forward_to         = "${var.pubsub_client_queues.listening_queue}"
}
resource "azurerm_role_assignment" "role_data_receiver_on_pub_subscription_topics_to_client_appidentity" {
    for_each = toset(var.client_app_topic_subscriptions)
    scope = azurerm_servicebus_topic.topics_domain[each.value].id
    role_definition_name = "Azure Service Bus Data Receiver"
    principal_id = data.azuread_service_principal.pubsub_client.object_id
}

//Add access to each command queue you want to use in your client (to be able to send commands)
resource "azurerm_role_assignment" "role_data_owner_on_pub_sub_command_queues_to_client_appidentity" {
    scope = azurerm_servicebus_queue.pubsub_server_command_queue.id
    role_definition_name = "Azure Service Bus Data Owner"
    principal_id = data.azuread_service_principal.pubsub_client.id
}

