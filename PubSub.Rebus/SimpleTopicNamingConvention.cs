using System;
using Rebus.Topic;

namespace PubSub.Rebus;

public class CustomMappingTopicNamingConvention : ITopicNameConvention {
    private readonly Dictionary<Type, string> _typeMappings;
    public CustomMappingTopicNamingConvention(Dictionary<Type, string> typeMappings) {
        _typeMappings = typeMappings ?? throw new ArgumentNullException(nameof(typeMappings));
    }
    public string GetTopic(Type eventType) {
        if (_typeMappings.ContainsKey(eventType)) {
            return _typeMappings[eventType];
        }
        return $"{eventType.Name.ToLower()}s";
    }
}

public class SimpleTopicNamingConvention : ITopicNameConvention {
    public string GetTopic(Type eventType) {
        return $"{eventType.Name.ToLower()}s";
    }
}