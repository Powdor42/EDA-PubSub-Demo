using PubSub.Core.Models;

namespace PubSub.Core.Handlers;

public interface IOrderHandler {
    Task Handle(Order order);
}