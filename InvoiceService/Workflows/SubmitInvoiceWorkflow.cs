using Dapr.Workflow;
using InvoiceService.Workflows.Activities.SubmitInvoice;
using Shared;
using Shared.Models;

namespace InvoiceService.Workflows;

public sealed class SubmitInvoiceWorkflow : Workflow<BuiltInvoice, object?>
{
    /// <summary>Override to implement workflow logic.</summary>
    /// <param name="context">The workflow context.</param>
    /// <param name="input">The deserialized workflow input.</param>
    /// <returns>The output of the workflow as a task.</returns>
    public override async Task<object?> RunAsync(WorkflowContext context, BuiltInvoice input)
    {
        try
        {
            //Email the invoice to the administrator for approval
            await context.CallActivityAsync(nameof(EmailApprovalToAdminActivity),
                new EmailApprovalActivityData(input.InvoiceNumber, input.InvoiceDate, input.CustomerName, input.Total,
                    context.InstanceId));

            //Pause and wait for the administrator to approve/reject the invoice
            var isApprovedResult =
                await context.WaitForExternalEventAsync<bool>(eventName: Constants.InvoiceApprovalResponseEventName, TimeSpan.FromDays(1));

            if (isApprovedResult)
            {
                //Retrieve the customer's email address
                var customerEmailAddress =
                    await context.CallActivityAsync<string>(nameof(GetCustomerEmailAddressActivity), input.CustomerId);
                
                //Send the invoice to the customer
                await context.CallActivityAsync(nameof(EmailCustomerWithInvoiceActivity),
                    new EmailCustomerActivityData(input.InvoiceNumber, input.InvoiceDate, customerEmailAddress));
            }
            else
            {
                //Notify the admin that the invoice was rejected
                await context.CallActivityAsync(nameof(EmailAdminWithRejectionNoticeActivity),
                    new EmailAdminRejectionActivityData(input.InvoiceNumber, input.InvoiceDate, input.Total.ToString("C2"),
                        input.CustomerName));
            }

            return null;
        }
        catch (TaskCanceledException)
        {
            //Notify the admin of the invoice rejection
            await context.CallActivityAsync(nameof(EmailAdminWithRejectionNoticeActivity),
                new EmailAdminRejectionActivityData(input.InvoiceNumber, input.InvoiceDate, input.Total.ToString("C2"),
                    input.CustomerName));
            return null;
        }
    }
}