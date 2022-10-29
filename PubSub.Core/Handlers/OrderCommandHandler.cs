using Microsoft.Extensions.Logging;
using PubSub.Core.Interfaces;
using PubSub.Core.Models;

namespace PubSub.Core.Handlers;

public class OrderCommandHandler : IOrderCommandHandler {
    private readonly IPubSubService _pubSubService;
    private readonly ILogger<OrderCommandHandler> _logger;

    public OrderCommandHandler(IPubSubService pubSubService, ILogger<OrderCommandHandler> logger) {
        _pubSubService = pubSubService ?? throw new ArgumentNullException(nameof(pubSubService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(CreateOrderCommand createOrderCommand) {
        _logger.LogInformation($"Received Create Order Command...");
        var newOrder = createOrderCommand.ToOrder();
        _logger.LogInformation($"Publishing new order with Id: {newOrder.Id}, created on {newOrder.CreatedOn}");
        _pubSubService.Publish(newOrder);
        _logger.LogInformation($"Published new order with Id: {newOrder.Id}, created on {newOrder.CreatedOn}");
        return Task.CompletedTask;
    }
}
