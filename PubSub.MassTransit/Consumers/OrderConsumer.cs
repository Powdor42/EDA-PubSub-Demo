using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PubSub.Core.Handlers;
using PubSub.Core.Models;

namespace PubSub.MassTransit.Consumers
{
    public class OrderConsumer : IConsumer<Order>
    {
        private readonly IOrderHandler _orderHandler;
        private readonly ILogger<OrderConsumer> _logger;

        public OrderConsumer(IOrderHandler orderHandler, ILogger<OrderConsumer> logger)
        {
            _orderHandler = orderHandler;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<Order> context)
        {
            
            _logger.LogDebug("Received order message from {1}:\r\n {0}", JsonConvert.SerializeObject(context.Message), context.DestinationAddress);

            await _orderHandler.Handle(context.Message);

        }
    }
}
