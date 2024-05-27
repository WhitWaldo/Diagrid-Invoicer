namespace InvoiceService.Models.InboundEmail.CommandData;

public sealed record GenerateInvoicePayload(HashSet<Guid> CustomerIds) : ICommandPayload
{
    public EmailCommand Command => EmailCommand.GenerateInvoice;
}