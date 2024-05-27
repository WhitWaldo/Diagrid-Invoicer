using Dapr.Client;
using Dapr.Workflow;
using InvoiceService.Email;
using InvoiceService.Email.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

namespace InvoiceService.Workflows.Activities.SubmitInvoice;

/// <summary>
/// This activity is responsible for sending the email to the administrator seeking approval of the given invoice.
/// </summary>
public sealed class EmailApprovalToAdminActivity(ILoggerFactory? loggerFactory, DaprClient dapr, EmailPayloadBuilder emailBuilder, IConfiguration configuration) : WorkflowActivity<EmailApprovalActivityData, object?>
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<EmailApprovalToAdminActivity>() ??
                                       NullLoggerFactory.Instance.CreateLogger<EmailApprovalToAdminActivity>();

    /// <summary>
    /// The administrator's email address to send the approval to.
    /// </summary>
    private readonly string _administratorEmailAddress = configuration.GetValue<string>(Constants.EnvironmentVariableNames.AdminEmailAddress);
    /// <summary>
    /// The base domain used by the API for building the approval/rejection requests
    /// </summary>
    private readonly string _apiBaseDomain =
        configuration.GetValue<string>(Constants.EnvironmentVariableNames.ApiBaseDomain);

    /// <summary>
    /// Override to implement async (non-blocking) workflow activity logic.
    /// </summary>
    /// <param name="context">Provides access to additional context for the current activity execution.</param>
    /// <param name="input">The deserialized activity input.</param>
    /// <returns>The output of the activity as a task.</returns>
    public override async Task<object?> RunAsync(WorkflowActivityContext context, EmailApprovalActivityData input)
    {
        //1. Start by downloading the PDF resource as we'll need to attach it to the email
        _logger.LogInformation("Retrieving PDF from {stateName} state named {blobName}", Constants.BlobStateName, input.InvoiceNumber);
        var invoicePdf = await dapr.GetStateAsync<byte[]>(Constants.BlobStateName, input.InvoiceNumber);
        _logger.LogInformation("Retrieved PDF from {stateName} state named {blobName}", Constants.BlobStateName,
            input.InvoiceNumber);

        //2. Create the template data and email binding payload
        var approvalUrl = $"https://{_apiBaseDomain}/pending/{input.ApprovalId}/accept";
        var rejectionUrl = $"https://{_apiBaseDomain}/pending/{input.ApprovalId}/reject";

        var emailTemplateData =
            new ApprovalEmailData(input.CustomerName, input.Total.ToString("C2"), approvalUrl, rejectionUrl);
        var emailAttachment = emailBuilder.BuildAttachment("Invoice.pdf", invoicePdf);
        var serializedEmailPayload = emailBuilder.BuildSerializedPayload(_administratorEmailAddress,
            $"Invoice approval needed - {input.CustomerName} - {input.InvoiceDate.ToString("MMMM yyyy")}", emailTemplateData,
            TemplateMap.RejectedTemplateId, emailAttachment);
        _logger.LogInformation("Built email template payload to pass to SendGrid output binding");
        
        //3. Send the email via the Dapr output binding
        await dapr.InvokeBindingAsync(Constants.SendGridBindingName, "post", serializedEmailPayload);
        _logger.LogInformation("Successfully emitted output binding to SendGrid for {invoiceNumber}",
            input.InvoiceNumber);
        return null;
    }
}

public sealed record EmailApprovalActivityData(string InvoiceNumber, DateOnly InvoiceDate, string CustomerName, decimal Total, string ApprovalId);