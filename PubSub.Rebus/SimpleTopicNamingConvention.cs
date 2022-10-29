using Rebus.Topic;

namespace PubSub.Rebus;

public class SimpleTopicNamingConvention : ITopicNameConvention {
    public string GetTopic(Type eventType) {
        return $"{eventType.Name.ToLower()}s";
    }
}