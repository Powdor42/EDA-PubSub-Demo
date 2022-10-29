using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PubSub.Core.Interfaces;
using PubSub.Core.Models;
using Rebus.Handlers;
using Rebus.Pipeline;

namespace PubSub.Rebus.Handlers;

public class OrderCommandMessageHandler : IHandleMessages<CreateOrderCommand> {
    private readonly IOrderCommandHandler _orderCommandHandler;
    private readonly ILogger<OrderCommandMessageHandler> _logger;

    public OrderCommandMessageHandler(IOrderCommandHandler orderCommandHandler, ILogger<OrderCommandMessageHandler> logger) {
        _orderCommandHandler = orderCommandHandler ?? throw new ArgumentNullException(nameof(orderCommandHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(CreateOrderCommand message) {
        var sbr = (ServiceBusReceiver)MessageContext.Current.TransactionContext.Items["asb-message-receiver"];

        _logger.LogDebug("Received create order command from {1}:\r\n {0}", JsonConvert.SerializeObject(message), sbr.EntityPath);

        _orderCommandHandler.Handle(message);

        return Task.CompletedTask;
    }
}