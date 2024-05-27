using Dapr.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;
using Shared.Lookup;
using Shared.Models;

namespace InvoiceService.Operations;

public sealed class DataManagement(ILoggerFactory? loggerFactory, DaprClient dapr, KeyManagement keyMgmt)
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger<DataManagement> _logger = loggerFactory?.CreateLogger<DataManagement>() ??
                                                       NullLoggerFactory.Instance.CreateLogger<DataManagement>();

    /// <summary>
    /// The sales tax to charge against the total.
    /// </summary>
    private const decimal SalesTaxPercentage = (decimal)0.0825;

    /// <summary>
    /// Logs a new line item to the state.
    /// </summary>
    /// <param name="data">The line items to persist.</param>
    /// <returns></returns>
    public async Task LogLineItemAsync(List<LineItem> data)
    {
        try
        {
            _logger.LogInformation("Persisting line item information to state across {recordCount}", data.Count);

            var groupedDataByCustomerId = data.GroupBy(a => a.CustomerId).ToList();

            foreach (var customerGrouping in groupedDataByCustomerId)
            {
                _logger.LogInformation("Persisting customer information for {customerId}", customerGrouping.Key);

                //Build the key to store the data under - this is based on the customer ID so the data is partitioned by customer
                var stateKey = await GetStateKey(customerGrouping.Key);
                _logger.LogInformation("Persisting data using state key {stateKey}", stateKey);

                //Retrieve the existing state for this key
                var existingLineItems = await dapr.GetStateAsync<List<LineItem>>(Constants.KeyValueStateName,
                    stateKey, ConsistencyMode.Strong) ?? [];
                _logger.LogInformation("Retrieved {itemCount} existing line items for state key {stateKey}",
                    existingLineItems.Count, stateKey);

                //Add the records in this group to the line items
                existingLineItems.AddRange(customerGrouping.ToList());

                //Persist the data to state for no more than 3 months
                const int ttl = 60 * 60 * 24 * 31 * 3; //3 months in seconds

                await dapr.SaveStateAsync(Constants.KeyValueStateName, stateKey, existingLineItems,
                    metadata: new Dictionary<string, string>
                    {
                        { "ttlInSeconds", $"{ttl}" }
                    });
                _logger.LogInformation(
                    "Persisted the line item information to state for key {stateKey} for customer {customerId}",
                    stateKey, customerGrouping.Key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while logging line item for customer");
            throw;
        }
    }

    /// <summary>
    /// Prepares the state data for a given customer identifier.
    /// </summary>
    /// <param name="customerId">The identifier of the customer.</param>
    /// <returns>The invoice data with all the provided line items.</returns>
    public async Task<Invoice> PrepareInvoicesAsync(Guid customerId)
    {
        try
        {
            _logger.LogInformation("Preparing invoices from state data for customer {customerId}", customerId);

            //Map the customer
            var customer = CustomerMap.Customers()[customerId];
            if (customer is null)
            {
                throw new Exception($"Unable to locate specified customer {customerId}");
            }
            
            //Build the key from the customer ID
            var stateKey = await GetStateKey(customerId);
            _logger.LogInformation("Retrieving data stored under key {stateKey}", stateKey);

            //Retrieve the existing state for this key
            var existingLineItems =
                await dapr.GetStateAsync<List<LineItem>>(Constants.KeyValueStateName, stateKey, ConsistencyMode.Strong);
            _logger.LogInformation("Retrieved {itemCount} existing line items for state key {stateKey}",
                existingLineItems.Count, stateKey);

            //Rotate the keys for storing additional data since it won't be on this invoice any longer
            await keyMgmt.RotateKeysAsync(customerId);

            //Build the invoice
            var invoiceDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var dueDate = invoiceDate.AddDays(21);
            var invoiceNumber = $"{customer.InvoicePrefix}{invoiceDate.ToString("yyyyMMdd")}";
            var salesTax = Math.Round(existingLineItems.Sum(item => item.Total) * SalesTaxPercentage, 2,
                MidpointRounding.AwayFromZero);
            var invoice = new Invoice(invoiceNumber, invoiceDate, dueDate, salesTax, customer.Id, customer.Name,
                customer.AddressLine1, customer.City, customer.State, customer.PostalCode);
            _logger.LogInformation("Successfully built invoice {invoiceNumber} for customer {customerId}",
                invoiceNumber, customerId);
            
            return invoice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while preparing invoices for customer {customerId}", customerId);
            throw;
        }
    }

    private async Task<string> GetStateKey(Guid customerId)
    {
        //Retrieve the current data key from the key management
        var currentKey = await keyMgmt.GetCurrentKeyAsync(customerId);

        return $"LineItemData_{currentKey}";
    }
}