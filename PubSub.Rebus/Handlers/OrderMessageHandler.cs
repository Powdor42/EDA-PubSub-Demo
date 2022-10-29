using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PubSub.Core.Handlers;
using PubSub.Core.Models;
using Rebus.Handlers;
using Rebus.Pipeline;

namespace PubSub.Rebus.Handlers;

public class OrderMessageHandler : IHandleMessages<Order> {
    private readonly IOrderHandler _orderHandler;
    private readonly ILogger<OrderMessageHandler> _logger;

    public OrderMessageHandler(IOrderHandler orderHandler, ILogger<OrderMessageHandler> logger) {
        _orderHandler = orderHandler ?? throw new ArgumentNullException(nameof(orderHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(Order message) {
        var sbr = (ServiceBusReceiver)MessageContext.Current.TransactionContext.Items["asb-message-receiver"];

        _logger.LogDebug("Received order message from {1}:\r\n {0}", JsonConvert.SerializeObject(message), sbr.EntityPath);

        await _orderHandler.Handle(message);
    }
}
