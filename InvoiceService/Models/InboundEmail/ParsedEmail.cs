using System.Globalization;
using System.Text.Json.Serialization;
using AngleSharp;
using AngleSharp.Html.Parser;
using InvoiceService.Models.InboundEmail.CommandData;
using Shared.Models;

namespace InvoiceService.Models.InboundEmail;

public sealed record ParsedEmail
{
    /// <summary>
    /// The text comprising the email.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    /// The HTML body comprising the email.
    /// </summary>
    [JsonPropertyName("html")]
    public string? Html { get; init; }

    /// <summary>
    /// The envelope metadata indicating who the email is supposedly from and who it was sent to.
    /// </summary>
    [JsonPropertyName("envelope")]
    public Envelope? Envelope { get; init; }

    /// <summary>
    /// The SPF record evaluation.
    /// </summary>
    [JsonPropertyName("SPF")]
    public string? SPF { get; init; }

    /// <summary>
    /// The raw to field.
    /// </summary>
    [JsonPropertyName("to")]
    public string ToRaw { get; init; } = string.Empty;

    /// <summary>
    /// The raw from field.
    /// </summary>
    [JsonPropertyName("from")]
    public string FromRaw { get; init; } = string.Empty;

    /// <summary>
    /// The number of attachments included on the email.
    /// </summary>
    [JsonPropertyName("attachments")]
    public int Attachments { get; init; }

    /// <summary>
    /// The various headers on the email.
    /// </summary>
    [JsonPropertyName("headers")]
    public string Headers { get; init; }

    /// <summary>
    /// The various charsets used for each field.
    /// </summary>
    [JsonPropertyName("charsets")]
    public Dictionary<string, string> Charsets { get; init; } = [];

    //public Dictionary<string, string> ParsedHeaders => Headers.Split(";\r\n", StringSplitOptions.RemoveEmptyEntries)
    //    .Select(line => line.Split('='))
    //    .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    
    /// <summary>
    /// The IP address of the sender.
    /// </summary>
    [JsonPropertyName("sender_ip")]
    public string? SenderIp { get; init; }

    /// <summary>
    /// The subject of the inbound email.
    /// </summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; init; }

    /// <summary>
    /// Validates whether the record passed the SPF check.
    /// </summary>
    [JsonIgnore]
    public bool PassedSpf => string.Equals(SPF, "pass");

    /// <summary>
    /// Performs simple validation to prevent users from sending malicious emails into the system.
    /// </summary>
    /// <param name="validationCode">The validation code required for "authentication".</param>
    /// <returns></returns>
    public bool IsAuthenticated(string validationCode) => Text?.Contains($"Code={validationCode};") ?? false; 
    
    /// <summary>
    /// Parses the payload of the email message and indicates the command contained within, if any.
    /// </summary>
    /// <returns></returns>
    public ICommandPayload? ParsePayload()
    {
        if (string.IsNullOrWhiteSpace(Html) || string.IsNullOrWhiteSpace(Subject))
            return null;
        
        return Subject?.ToLowerInvariant() switch
        {
            "add line item" => ParseAddLineItemCommand(),
            "generate invoice" => ParseGenerateCommand(),
            _ => new UnknownPayload()
        };
    }

    private GenerateInvoicePayload ParseGenerateCommand()
    {
        var context = BrowsingContext.New();
        var parser = context.GetService<IHtmlParser>();
        var document = parser?.ParseDocument(Html!);

        var lineItemTable = document?.QuerySelector("table>tbody");
        var rows = lineItemTable?.QuerySelectorAll("tr").Skip(1); //Skip the first one since it's just headers

        //The table should have just one header: CustomerId
        var parsedCustomerIds = new HashSet<Guid>();
        if (rows is null)
            return new GenerateInvoicePayload(parsedCustomerIds);

        foreach (var line in rows)
        {
            var columns = line.QuerySelectorAll("td>p");

            if (columns.Length != 1)
                continue;

            try
            {
                var customerId = Guid.Parse(columns[0].TextContent.Trim());
                parsedCustomerIds.Add(customerId);
            }
            catch
            {
                //Nothing to do but continue parsing on the next row
            }
        }

        return new GenerateInvoicePayload(parsedCustomerIds);
    }

    private ParseLineItemPayload ParseAddLineItemCommand()
    {
        var context = BrowsingContext.New();
        var parser = context.GetService<IHtmlParser>();
        var document = parser?.ParseDocument(Html!);

        var lineItemTable = document?.QuerySelector("table>tbody");
        var lineItems = lineItemTable?.QuerySelectorAll("tr").Skip(1); //Skip the first one since it's just headers

        //The table should have 5 headers: CustomerId, Quantity, Description, UnitPrice and Total

        var parsedLineItems = new List<LineItem>();
        if (lineItems == null)
            return new ParseLineItemPayload(parsedLineItems);

        foreach (var lineItemRow in lineItems)
        {
            var columns = lineItemRow.QuerySelectorAll("td>p");

            if (columns.Length != 5)
                continue;

            try
            {
                var customerId = Guid.Parse(columns[0].TextContent.Trim());
                var quantity = columns[1].TextContent.Trim();
                var description = columns[2].TextContent.Trim();
                var unitPrice = columns[3].TextContent.Trim();
                var total = decimal.Parse(columns[4].TextContent.Trim(),
                    NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint |
                    NumberStyles.AllowThousands);

                var lineItem = new LineItem(customerId, quantity, description, unitPrice, total);
                parsedLineItems.Add(lineItem);
            }
            catch
            {
                // Just skip to the next row
            }
        }

        return new ParseLineItemPayload(parsedLineItems);
    }
}