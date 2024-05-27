using System.Text.Json.Serialization;

namespace Shared.Models;

public sealed record LineItem(
    [property: JsonPropertyName("customerId")]
    Guid CustomerId,
    [property: JsonPropertyName("quantity")]
    string Quantity,
    [property: JsonPropertyName("description")]
    string Description,
    [property: JsonPropertyName("price")] string UnitPrice,
    [property: JsonPropertyName("total")] decimal Total)
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; } = Guid.NewGuid();
}