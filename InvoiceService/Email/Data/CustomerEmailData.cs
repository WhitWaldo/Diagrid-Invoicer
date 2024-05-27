using System.Text.Json.Serialization;

namespace InvoiceService.Email.Data;

/// <summary>
/// The data used to populate the dynamic SendGrid template for invoices being sent to the customer.
/// </summary>
/// <param name="Month">The dated invoice month.</param>
/// <param name="Year">The dated invoice year.</param>
public sealed record CustomerEmailData(
    [property: JsonPropertyName("Month")] string Month, 
    [property: JsonPropertyName("Year")] string Year) : ITemplateData;