using Dapr.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

namespace InvoiceService.Workflows.Activities.SubmitInvoice;

/// <summary>
/// This activity is responsible for retrieving the customer's email address given an identifier.
/// </summary>
public sealed class GetCustomerEmailAddressActivity(ILoggerFactory? loggerFactory, IConfiguration configuration) : WorkflowActivity<Guid, string>
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<GetCustomerEmailAddressActivity>() ??
                                       NullLoggerFactory.Instance.CreateLogger<GetCustomerEmailAddressActivity>();

    /// <summary>
    /// Override to implement async (non-blocking) workflow activity logic.
    /// </summary>
    /// <param name="context">Provides access to additional context for the current activity execution.</param>
    /// <param name="input">The deserialized activity input.</param>
    /// <returns>The output of the activity as a task.</returns>
    public override Task<string> RunAsync(WorkflowActivityContext context, Guid input)
    {
        //Typically customer data would be stored in a database, but that's outside the scope of this proof-of-concept.
        //As a result, it's stored in a customer map in this demonstration - but we don't really want to publish personal email addresses
        //here as this is being published to open source. Rather, we'll pull this from an environment variable for this application.
        
        //var customerEmailAddress = CustomerMap.Customers()
        //    .FirstOrDefault(customer => customer.Key == input.CustomerId).Value.EmailAddress;

        var customerEmailAddress =
            configuration.GetValue<string>(Constants.EnvironmentVariableNames.CustomerEmailAddress);
        _logger.LogInformation("Successfully retrieved email address for customer for {customerId}", input);
        return Task.FromResult(customerEmailAddress!);
    }
}