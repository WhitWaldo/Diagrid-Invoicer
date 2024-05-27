using System.Text.Json.Serialization;

namespace InvoiceService.Models.InboundEmail;

public sealed record Envelope
{
    [JsonPropertyName("to")]
    public List<string> To { get; init; } = [];

    [JsonPropertyName("from")]
    public string From { get; init; }
}