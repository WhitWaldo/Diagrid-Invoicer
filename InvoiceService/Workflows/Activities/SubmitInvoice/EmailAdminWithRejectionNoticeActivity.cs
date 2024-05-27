using Dapr.Client;
using Dapr.Workflow;
using InvoiceService.Email;
using InvoiceService.Email.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

namespace InvoiceService.Workflows.Activities.SubmitInvoice;

/// <summary>
/// This activity is responsible for emailing the notification to the administrator indicating that the invoice wasn't sent to the customer
/// because it was either rejected during the approval process or the approval timed out (de facto rejection).
/// </summary>
public sealed class EmailAdminWithRejectionNoticeActivity(ILoggerFactory? loggerFactory, IConfiguration configuration, DaprClient dapr, EmailPayloadBuilder emailBuilder) : WorkflowActivity<EmailAdminRejectionActivityData, object?>
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<EmailAdminWithRejectionNoticeActivity>() ??
                                       NullLoggerFactory.Instance.CreateLogger<EmailAdminWithRejectionNoticeActivity>();

    /// <summary>
    /// The administrator's email address to send the email to.
    /// </summary>
    private readonly string _administratorEmailAddress = configuration.GetValue<string>(Constants.EnvironmentVariableNames.AdminEmailAddress);

    /// <summary>
    /// Override to implement async (non-blocking) workflow activity logic.
    /// </summary>
    /// <param name="context">Provides access to additional context for the current activity execution.</param>
    /// <param name="input">The deserialized activity input.</param>
    /// <returns>The output of the activity as a task.</returns>
    public override async Task<object?> RunAsync(WorkflowActivityContext context, EmailAdminRejectionActivityData input)
    {
        //1. Start by downloading the PDF resource as we'll need to attach it to the email
        _logger.LogInformation("Retrieving PDF from {stateName} state named {blobName}", Constants.BlobStateName, input.InvoiceNumber);
        var invoicePdf = await dapr.GetStateAsync<byte[]>(Constants.BlobStateName, input.InvoiceNumber);
        _logger.LogInformation("Retrieved PDF from {stateName} state named {blobName}", Constants.BlobStateName,
            input.InvoiceNumber);

        //2. Create the template data and email binding payload
        var emailTemplateData = new RejectedEmailData(input.CustomerName, input.InvoiceTotal);
        var emailAttachment = emailBuilder.BuildAttachment("Invoice.pdf", invoicePdf);
        var serializedEmailPayload = emailBuilder.BuildSerializedPayload(_administratorEmailAddress,
            $"Invoice Rejected - {input.CustomerName} - {input.InvoiceDate.ToString("MMMM yyyy")}", emailTemplateData,
            TemplateMap.RejectedTemplateId, emailAttachment);
        _logger.LogInformation("Built email template payload to pass to SendGrid output binding");

        //3. Send the email via the Dapr output binding
        await dapr.InvokeBindingAsync(Constants.SendGridBindingName, "post", serializedEmailPayload);
        _logger.LogInformation("Successfully emitted output binding to SendGrid for {invoiceNumber}",
            input.InvoiceNumber);
        return null;
    }
}

public sealed record EmailAdminRejectionActivityData(string InvoiceNumber, DateOnly InvoiceDate, string InvoiceTotal, string CustomerName);