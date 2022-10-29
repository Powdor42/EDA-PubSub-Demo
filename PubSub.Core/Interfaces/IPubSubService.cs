namespace PubSub.Core.Handlers;

public interface IPubSubService {
    Task Publish<T>(T message) where T : class;
    Task Send<T>(T message) where T : class;
    Task Subcribe<T>(string topicName);
}
