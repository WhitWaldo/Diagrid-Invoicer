using Dapr;
using Dapr.Workflow;
using InvoiceService.Operations;
using InvoiceService.Utilities;
using InvoiceService.Workflows;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;
using Shared.Models;

namespace InvoiceService.Controllers;

[ApiController]
[Route("invoices")]
public class InvoiceController(ILoggerFactory? loggerFactory, DataManagement stateMgmt, DaprWorkflowClient workflowClient) : ControllerBase
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
    [Topic(Constants.PubSubName, Constants.FileBuiltQueueTopicName)]
    [HttpPost("built")]
    public async Task<IActionResult> ProcessBuiltInvoiceAsync(BuiltInvoice invoiceDetails)
    {
        try
        {
            _logger.LogInformation("Received request from messaging service with processed invoice payment for invoice number {invoiceNumber}", invoiceDetails.InvoiceNumber);

            //Generate an approval ID
            var approvalId = await WorkflowValueBuilder.CreateApprovalIdAsync(invoiceDetails.InvoiceNumber);
            
            //Schedule the workflow to start immediately using the approval ID as the instance identifier
            await workflowClient.ScheduleNewWorkflowAsync(nameof(SubmitInvoiceWorkflow), approvalId, invoiceDetails);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while processing built invoice for {invoiceNumber}", invoiceDetails.InvoiceNumber);
            return new StatusCodeResult(500);
        }
    }
}