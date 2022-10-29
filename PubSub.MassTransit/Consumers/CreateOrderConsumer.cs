using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PubSub.Core.Interfaces;
using PubSub.Core.Models;

namespace PubSub.MassTransit.Consumers
{
    public class CreateOrderConsumer : IConsumer<CreateOrderCommand>
    {
        private readonly IOrderCommandHandler _orderCommandHandler;
        private readonly ILogger<CreateOrderConsumer> _logger;

        public CreateOrderConsumer(IOrderCommandHandler orderCommandHandler, ILogger<CreateOrderConsumer> logger)
        {
            _orderCommandHandler = orderCommandHandler ?? throw new ArgumentNullException(nameof(orderCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Consume(ConsumeContext<CreateOrderCommand> context)
        {
            _logger.LogDebug("Received order message from {1}:\r\n {0}", JsonConvert.SerializeObject(context.Message), context.DestinationAddress);

            _orderCommandHandler.Handle(context.Message);

            return Task.CompletedTask;
        }
    }
}
