namespace PubSub.Core.Models;

public record CreateOrderCommand {
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public Contact Contact { get; init; } = null!;
    public Order ToOrder() {
        return new Order() {
            Id = Guid.NewGuid(),
            Description = Description,
            Price = Price ?? default,
            CreatedOn = DateTimeOffset.Now,
            Contact = new Contact() {
                Id = Contact.Id ?? Guid.NewGuid(),
                FullName = Contact.FullName
            }
        };
    }
}
