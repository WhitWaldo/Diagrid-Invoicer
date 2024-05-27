using System.Text.Json.Serialization;

namespace InvoiceService.Email.Data;

/// <summary>
/// The data used to populate the dynamic SendGrid template for emails indicating that the
/// invoice was rejected or approval timed out.
/// </summary>
/// <param name="CustomerName">The name of the customer.</param>
/// <param name="Total">The formatted total of the invoice.</param>
public sealed record RejectedEmailData(
    [property: JsonPropertyName("CustomerName")] string CustomerName, 
    [property: JsonPropertyName("Total")] string Total) : ITemplateData;