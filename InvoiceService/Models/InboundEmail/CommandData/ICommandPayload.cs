namespace InvoiceService.Models.InboundEmail.CommandData;

public interface ICommandPayload
{
    /// <summary>
    /// Which command the payload is representing.
    /// </summary>
    public EmailCommand Command { get; }
}