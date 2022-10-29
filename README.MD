
# Event driven Architecture PubSub Demo
This simple demo shows how to create 2 applications that leverage [Azure Service bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview) as a central broker to apply event driven architecture.

It is build using CloudNative CloudEvents to follow the specification for maximum interoperability. Check [cloudevents.io](https://cloudevents.io/) for more details.

Code is setup in an abstract form to demonstrate the interoperability achieved using the CloudNative event format. It uses 2 frameworks that abstract the underlying transport from the application's pub/sub mechanism. 

See for more details the used packages [Rebus](https://github.com/rebus-org/Rebus) and [MassTransit](https://github.com/MassTransit/MassTransit)

To enable transparent usage of CloudEvents with an extension package is used. See [CloudEventify](https://github.com/riezebosch/CloudEventify)

## What do the apps do.
The client application simply demo's sending commands (create order commands) to the listening queue of the server app. And it listens / subscribes to the order events publshed into the orders topic (and echo the data on the screen)

The server app is listening for order commands and augments these objects with some data and publish a order event out if this into the orders topic.

## Setup infrastructure (Azure Servicebus Topics/Subscriptions etc.)

To setup the required infrastructure you can leverage the terraform template available in the infra folder. Make sure you configure the variables.tf file. The setup can either be used with or without the setup of explicit SPN's to run the application (just to simulate the full RBAC setup that is required in this setup)

**Variables.tf:**

Base configuration:
- tenant_id : your Azure tenant-id
- location : Azure resource location (ie. westeurope)
- subscription_id : Azure subscription-id
- company_short_name : short name used in the buildup of the resource names
- environment" { default = "test" }
- developers_group_name : Azure AD group name that you want to allow management of the content of the Azure service bus (remove if you don't need it)

**PubSub.Server** configuration:
- pubsub_server_application_id : Azure AD Service principal or user that will run the PubSub.Server application (you can configure the application to run with that specific SPN by configuring the AZURE_* environment variable in launchSettings)
- server_domain_config.topics: a list of topics you want to have setup over which the app is supposed to communicate. The demo app uses only 1 topic which is to be configured in appSetting.json

**PubSub.Client** configuration:
- pubsub_client_application_id : Azure AD Service principal or user that will run the PubSub.Client application (you can configure the application to run with that specific SPN by configuring the AZURE_* environment variable in launchSettings)
- client_app_topic_subscriptions : Topics to which the client app is supposed to subscribe (make sure to match it in the appSettings of the application)
- pubsub_client_queues.listening_queue: the name of the queue that the client app listens to and to which subscriptions will forward the messages of topics.
- pubsub_client_queues.error_queue: the name of the queue that the client app listens to and to which subscriptions will forward the messages of topics.

Template varaibles.tf
``` yaml
variable "developers_group_name" { default = "<developers_group_name>" }

variable "tenant_id" { default = "<tenant_id>" }
variable "location" { default = "<location>"}
variable "subscription_id" { default = "<subscription_id>" }

variable "company_short_name" { default = "<company_short_name>"}
variable "environment" { default = "<environment>" }

variable "pubsub_server_application_id" { default = "<pubsub_server_application_id>" }
variable "pubsub_server_queues" { 
    type = object({
        listening_queue = string
        error_queue = string
    })
    default = {
        "listening_queue" = "pubsub-server-commands"
        "error_queue" = "pubsub-server-errors"
    }
}
variable "server_domain_config" {
    type = object({
        topics = list(string)
    })
    default = {
      topics = [ "orders" ]
    }    
}

variable "pubsub_client_application_id" { default = "<pubsub_client_application_id>" }
variable "pubsub_client_queues" { 
    type = object({
        listening_queue = string
        error_queue = string
    })
    default = {
        "listening_queue" = "pubsub-client-commands"
        "error_queue" = "pubsub-client-errors"
    }
}
variable "client_app_topic_subscriptions" { 
                                type = list(string) 
                                default = ["orders"] 
                            }
```

What it will setup is:
- Azure resource group with template format ``` "<company_short_name>-rg-<environment>" ```
  - Azure Servicebus namespace with template format ``` "<company_short_name>-sb-<environment>" ```
    - Topic
      - Subscriptions that forwards messages on the domain topics to the listening queue
    - client-queue
    - server-queue

Permissions are assigned to the provided SPN's and Developer AD group to allow easy access the the data plane of the service bus.

If you want to do a manual setup you need. Make sure you match your setup in the appSettings of both the client and server PubSub demo apps and provide ample permission (Role: Azure Service bus Data Owner).

## Configure the PubSub applications

To configure your application make sure to use the data used for setting up your infrastructure is used in the appSettings as per below.

Required data from the infrastructure:
- \<namespace\> : the name of the Azure Service bus namespace
- \<topic\>: the topic that has been setup in the Azure service bus namespace
- \<pubsub_server_queues.listening_queue\>: the listing queue that has been setup for the server app
- \<pubsub_server_queues.error_queue\>: the error queue that has been setup for the server app 
- \<pubsub_client_queues.listening_queue\>: the listing queue that has been setup for the client app
- \<pubsub_client_queues.error_queue\>: the error queue that has been setup for the client app 

PubSub.Server appSettings.json
``` json
{
  "AzureServicebusOptions": {
    "EndPoint": "sb://<namespace>.servicebus.windows.net",
    "Topic": "<topic>",
    "ListeningQueue": "<pubsub_server_queues.listening_queue>",
    "SourceUri": "app://pubsub.server/",
    "ErrorQueueName": "<pubsub_server_queues.error_queue>"
  }
}
```

PubSub.Client appSettings.json
``` json
{
  "AzureServicebusOptions": {
    "EndPoint": "sb://<namespace>.servicebus.windows.net",
    "Topic": "<topic>",
    "CommandQueue": "<pubsub_server_queues.listening_queue>",
    "ListeningQueue": "<pubsub_client_queues.listening_queue>",
    "SourceUri": "app://pubsub.server/",
    "ErrorQueueName": "<pubsub_client_queues.error_queue>"
  }
}
```

## Running the applcation with the SPN's

Make sure to configure the launchSettings AZURE_ environment variable according to your infra setup.

Example launchSettings.json (template can be found under properties folder of the applications).

``` json
{
  "profiles": {
    "PubSub.Client": {
      "commandName": "Project",
      "environmentVariables": {
        "AZURE_TENANT_ID": "<tenant-id>",
        "AZURE_CLIENT_ID": "<spn-client-id>",
        "AZURE_CLIENT_SECRET": "spn-client-secret"
      }
    }
  }
}
```

If you prefer to run everything under your normal user account you should remove the AZURE_CLIENT_* entries at least. You should then make sure to login on the command prompt using ``` az login ```  or ``` az login -t <tenant-id> ``` to set the context globally before you start the applications.