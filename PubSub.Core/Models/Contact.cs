namespace PubSub.Core.Models;
public record Contact
{
    public Guid? Id { get; init; }
    public string? FullName { get; init; }
}