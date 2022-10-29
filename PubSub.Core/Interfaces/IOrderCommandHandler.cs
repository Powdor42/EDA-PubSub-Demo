using PubSub.Core.Models;

namespace PubSub.Core.Interfaces;

public interface IOrderCommandHandler {
    Task Handle(CreateOrderCommand createOrderCommand);
}