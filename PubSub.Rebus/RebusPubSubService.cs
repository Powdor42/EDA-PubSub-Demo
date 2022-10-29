using Microsoft.Extensions.Logging;
using PubSub.Core.Handlers;
using Rebus.Bus;

namespace PubSub.Rebus {
    public class RebusPubSubService : IPubSubService {
        private readonly IBus _bus;
        private readonly ILogger<RebusPubSubService> _logger;

        public RebusPubSubService(IBus bus, ILogger<RebusPubSubService> logger) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Publish<T>(T message) where T : class {
            await _bus.Publish(message);
        }

        public async Task Send<T>(T message) where T : class {
            await _bus.Send(message);
        }

        public async Task Subcribe<T>(string topicName) {
            await _bus.Subscribe<T>();
        }
    }
}
