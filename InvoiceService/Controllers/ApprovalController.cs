using Dapr.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

namespace InvoiceService.Controllers;

[ApiController]
[Route("pending")]
[AllowAnonymous]
public class ApprovalController(ILoggerFactory? loggerFactory, DaprWorkflowClient daprWorkflowClient) : ControllerBase
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<ApprovalController>() ??
                                       NullLoggerFactory.Instance.CreateLogger<ApprovalController>();

    /// <summary>
    /// Endpoint is invoked to accept a given invoice.
    /// </summary>
    /// <param name="approvalId">The ID of the approval request being accepted.</param>
    [HttpGet("{approvalId}/accept")]
    public async Task<IActionResult> ApproveInvoiceAsync(string approvalId)
    {
        try
        {
            _logger.LogInformation("Received request to accept approval ID {approvalId}", approvalId);

            //Note that the approval ID is also the instance ID for finding the workflow to submit to
            await daprWorkflowClient.RaiseEventAsync(approvalId, Constants.InvoiceApprovalResponseEventName, true);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while processing request to accept approval ID {approvalId}", approvalId);
            return new BadRequestResult();
        }
    }

    /// <summary>
    /// Endpoint is invoked to reject a given invoice.
    /// </summary>
    /// <param name="approvalId">The ID of the approval request being rejected.</param>
    [HttpGet("{approvalId}/reject")]
    public async Task<IActionResult> RejectInvoiceAsync(string approvalId)
    {
        try
        {
            _logger.LogInformation("Received request to reject approval ID {approvalId}", approvalId);

            //Note that the approval ID is also the instance ID for finding the workflow to submit to
            await daprWorkflowClient.RaiseEventAsync(approvalId, Constants.InvoiceApprovalResponseEventName, false);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while processing request to reject approval ID {approvalId}", approvalId);
            return new BadRequestResult();
        }
    }
}