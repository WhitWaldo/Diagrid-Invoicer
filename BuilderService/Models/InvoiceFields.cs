using BuilderService.Attributes;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BuilderService.Models;

public sealed record InvoiceFields
{
    [Field("InvoiceNumber")]
    public string InvoiceNumber { get; init; }

    [Field("InvoiceDate")]
    public string InvoiceDate { get; init; }

    [Field("CustomerName")]
    public string CustomerName { get; init; }

    [Field("CustomerAddress")]
    public string AddressLine1 { get; init; }

    [Field("CustomerCity")]
    public string City { get; init; }

    [Field("CustomerState")]
    public string State { get; init; }

    [Field("CustomerZip")]
    public string PostalCode { get; init; }

    [Field("Subtotal")]
    public string Subtotal { get; init; }

    [Field("SalesTax")]
    public string SalesTax { get; init; }

    [Field("TotalDue")]
    public string TotalDue { get; init; }

    [Field("FullDueDate")]
    public string FullDueDate { get; init; }

    [Field("ShortDueDate")]
    public string ShortDueDate { get; init; }

    public HashSet<InvoiceLineItem> LineItems { get; init; } = [];
}

public sealed record InvoiceLineItem
{
    public string Quantity { get; init; }
    public string Description { get; init; }
    public string UnitPrice { get; init; }
    public string Total { get; init; }
}