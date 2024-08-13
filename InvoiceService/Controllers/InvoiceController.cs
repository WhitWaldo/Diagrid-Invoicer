using Dapr;
using Dapr.Workflow;
using InvoiceService.Utilities;
using InvoiceService.Workflows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;
using Shared.Models;

namespace InvoiceService.Controllers;

[ApiController]
[Route("invoices")]
[AllowAnonymous]
public class InvoiceController(ILoggerFactory? loggerFactory, DaprWorkflowClient workflowClient) : ControllerBase
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<InvoiceController>() ??
                                       NullLoggerFactory.Instance.CreateLogger<InvoiceController>();
    
    /// <summary>
    /// Subscribes to a messaging service and retrieves the invoice number of a newly built invoice PDF.
    /// </summary>
    /// <param name="invoiceDetails"></param>
    /// <returns></returns>
    [Topic(Constants.PubSubName, Constants.FileBuiltPubSubName)]
    [HttpPost("built")]
    public async Task<IActionResult> ProcessBuiltInvoiceAsync(BuiltInvoice invoiceDetails)
    {
        try
        {
            _logger.LogInformation("Received request from messaging service with processed invoice payment for invoice number {invoiceNumber}", invoiceDetails.InvoiceNumber);

            //Generate an approval ID
            var approvalId = await WorkflowValueBuilder.CreateApprovalIdAsync(invoiceDetails.InvoiceNumber);
            
            _logger.LogInformation("Generated an approval ID {workflowId} for the built invoice", approvalId);

            //Schedule the workflow to start immediately using the approval ID as the instance identifier
            await workflowClient.ScheduleNewWorkflowAsync(nameof(SubmitInvoiceWorkflow), approvalId, invoiceDetails);

            _logger.LogInformation(
                "Scheduling the workflow for {workflowId} for customer ID {customerId} and customer name {customerName}",
                approvalId, invoiceDetails.CustomerId, invoiceDetails.CustomerName);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while processing built invoice for {invoiceNumber}", invoiceDetails.InvoiceNumber);
            return new StatusCodeResult(500);
        }
    }
}