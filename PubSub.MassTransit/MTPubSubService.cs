using MassTransit;
using PubSub.Core.Handlers;

namespace PubSub.MassTransit; 
public class MTPubSubService : IPubSubService {
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpointProvider _sendEndpoint;

    public MTPubSubService(IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpoint) {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _sendEndpoint = sendEndpoint ?? throw new ArgumentNullException(nameof(sendEndpoint));
    }

    public async Task Publish<T>(T message) where T : class {
        await _publishEndpoint.Publish<T>(message);
    }

    public async Task Send<T>(T message) where T : class {
        //var sendEndpoint = await _sendEndpoint.GetSendEndpoint(new Uri("queue:order-queue"));
        //await sendEndpoint.Send<T>(message);
        await _publishEndpoint.Publish<T>(message);
    }

    public Task Subcribe<T>(string topicName) {
        throw new NotImplementedException();
    }
}
