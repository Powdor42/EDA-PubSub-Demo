namespace PubSub.Core.Models;

public enum PubSubPacakgeType {
    Rebus,
    MassTransit,
    AzureServiceBusNative
}

public enum PubSubAppMode {
    Client,
    Server,
    Infra
}