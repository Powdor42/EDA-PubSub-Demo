namespace PubSub.Core.Options;

public record AzureServicebusOptions {
    public string EndPoint { get; set; } = null!;
    public string Topic { get; set; } = null!;
    public string CommandQueue { get; set; } = null!;
    public string ListeningQueue { get; set; } = null!;
    public string SourceUri { get; set; } = null!;
    public string ErrorQueueName { get; set; } = null!;
}
