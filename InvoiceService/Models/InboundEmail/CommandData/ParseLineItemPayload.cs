using Shared.Models;

namespace InvoiceService.Models.InboundEmail.CommandData;

public sealed record ParseLineItemPayload(List<LineItem> LineItems) : ICommandPayload
{
    public EmailCommand Command => EmailCommand.AddLineItem;
}