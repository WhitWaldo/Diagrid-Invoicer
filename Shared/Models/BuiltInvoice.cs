namespace Shared.Models;

/// <summary>
/// Reflects an invoice that was built and is ready to send into the approval workflow.
/// </summary>
/// <param name="InvoiceNumber">The unique invoice number used as the key to look up the PDF bytes from state.</param>
/// <param name="CustomerName">The name of the customer.</param>
/// <param name="InvoiceDate">The date on the invoice.</param>
/// <param name="Total">The total value of the invoice.</param>
/// <param name="CustomerId">The unique customer identifier.</param>
public sealed record BuiltInvoice(string InvoiceNumber, string CustomerName, DateOnly InvoiceDate, decimal Total, Guid CustomerId);