using Dapr.Client;
using Dapr.Workflow;
using InvoiceService.Email;
using InvoiceService.Email.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

namespace InvoiceService.Workflows.Activities.SubmitInvoice;

/// <summary>
/// This activity is responsible for emailing the approved invoice to the customer via the SendGrid Dapr output binding.
/// </summary>
public sealed class EmailCustomerWithInvoiceActivity(ILoggerFactory? loggerFactory, DaprClient dapr, IConfiguration configuration, EmailPayloadBuilder emailBuilder) : WorkflowActivity<EmailCustomerActivityData, object?>
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<EmailCustomerWithInvoiceActivity>() ??
                                       NullLoggerFactory.Instance.CreateLogger<EmailCustomerWithInvoiceActivity>();

    /// <summary>
    /// The administrator's email address (used as a bcc on the email sent to the customer).
    /// </summary>
    private readonly string _administratorEmailAddress = configuration.GetValue<string>(Constants.EnvironmentVariableNames.AdminEmailAddress);

    /// <summary>
    /// Override to implement async (non-blocking) workflow activity logic.
    /// </summary>
    /// <param name="context">Provides access to additional context for the current activity execution.</param>
    /// <param name="input">The deserialized activity input.</param>
    /// <returns>The output of the activity as a task.</returns>
    public override async Task<object?> RunAsync(WorkflowActivityContext context, EmailCustomerActivityData input)
    {
        //The input contains the name of the PDF resource in the blob state
        //1. Start by downloading the PDF resource as we'll need to attach it to the email
        _logger.LogInformation("Retrieving PDF from {stateName} state named {blobName}", Constants.BlobStateName, input.InvoiceNumber);
        var invoicePdf = await dapr.GetStateAsync<byte[]>(Constants.BlobStateName, input.InvoiceNumber);
        _logger.LogInformation("Retrieved PDF from {stateName} state named {blobName}", Constants.BlobStateName,
            input.InvoiceNumber);

        //2. Create the template data and email binding payload
        var emailTemplateData =
            new CustomerEmailData(input.InvoiceDate.ToString("MMMM"), input.InvoiceDate.ToString("yyyy"));
        var emailAttachment = emailBuilder.BuildAttachment("Invoice.pdf", invoicePdf);
        var serializedEmailPayload = emailBuilder.BuildSerializedPayload(input.CustomerEmailAddress,
            $"Invoice - {input.InvoiceDate.ToString("MMMM yyyy")}", emailTemplateData, TemplateMap.CustomerTemplateId,
            emailAttachment, _administratorEmailAddress);
        _logger.LogInformation("Built email template payload to pass to SendGrid output binding for {invoiceNumber}", input.InvoiceNumber);
        
        //3. Send the email via the Dapr output binding
        await dapr.InvokeBindingAsync(Constants.SendGridBindingName, "post", serializedEmailPayload);
        _logger.LogInformation("Successfully emitted output binding to SendGrid for {invoiceNumber}",
            input.InvoiceNumber);
        return null;
    }   
}

public sealed record EmailCustomerActivityData(string InvoiceNumber, DateOnly InvoiceDate, string CustomerEmailAddress);