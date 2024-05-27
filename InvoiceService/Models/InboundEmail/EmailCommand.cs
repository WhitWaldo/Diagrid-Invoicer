namespace InvoiceService.Models.InboundEmail;

/// <summary>
/// Various commands used to exercise control over the system.
/// </summary>
public enum EmailCommand
{
    /// <summary>
    /// Indicates that a line item should be added to the specified clients.
    /// </summary>
    AddLineItem,
    /// <summary>
    /// Indicates that an invoice should be generated for the specified clients.
    /// </summary>
    GenerateInvoice,
    /// <summary>
    /// Indicates that the command was not recognized and should be ignored.
    /// </summary>
    Unknown
}