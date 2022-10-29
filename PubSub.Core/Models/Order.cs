namespace PubSub.Core.Models;
public record Order {
    public Guid Id { get; init; }
    public Contact? Contact { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}