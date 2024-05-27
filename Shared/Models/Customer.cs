namespace Shared.Models;

public record Customer(
    string Name,
    string AddressLine1,
    string City,
    string State,
    string PostalCode,
    string InvoicePrefix,
    string EmailAddress)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}