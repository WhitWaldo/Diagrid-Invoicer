namespace InvoiceService.Models.InboundEmail.CommandData;

public sealed record UnknownPayload : ICommandPayload
{
    public EmailCommand Command => EmailCommand.Unknown;
}