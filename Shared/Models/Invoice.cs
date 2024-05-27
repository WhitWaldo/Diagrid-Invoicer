namespace Shared.Models;

public sealed record Invoice(
    string InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly DueDate,
    decimal SalesTax,
    Guid CustomerId,
    string CustomerName,
    string CustomerAddressLine1,
    string CustomerCity,
    string CustomerState,
    string CustomerPostalCode)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public HashSet<LineItem> LineItems { get; init; } = [];

    /// <summary>
    /// The total value of the invoice.
    /// </summary>
    public decimal Total => LineItems.Sum(item => item.Total) + SalesTax;
    
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}