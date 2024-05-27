using System.Text.Json.Serialization;

namespace InvoiceService.Email.Data;

/// <summary>
/// The data used to populate the dynamic SendGrid template for invoice approval emails.
/// </summary>
/// <param name="CustomerName">The name of the customer.</param>
/// <param name="Total">The formatted total of the invoice.</param>
public sealed record ApprovalEmailData(
    [property: JsonPropertyName("CustomerName")] string CustomerName, 
    [property: JsonPropertyName("Total")] string Total,
    [property: JsonPropertyName("ApprovalUrl")] string ApprovalUrl,
    [property: JsonPropertyName("RejectionUrl")] string RejectionUrl) : ITemplateData;