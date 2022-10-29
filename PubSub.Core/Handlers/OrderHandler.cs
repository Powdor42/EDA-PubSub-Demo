using Microsoft.Extensions.Logging;
using PubSub.Core.Handlers;
using PubSub.Core.Models;

namespace PubSub.Core.Handlers;

public class OrderHandler : IOrderHandler {
    private readonly ILogger<OrderCommandHandler> _logger;

    public OrderHandler(ILogger<OrderCommandHandler> logger) {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(Order order) {
        _logger.LogInformation($"Received Order event with orderId: {order.Id}, created on {order.CreatedOn}");
        _logger.LogDebug(order.ToString());
        return Task.CompletedTask;
    }
}
