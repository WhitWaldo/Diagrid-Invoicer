using AutoMapper;
using BuilderService.Models;
using BuilderService.Operations;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;
using Shared.Models;

namespace BuilderService.Controllers;

[ApiController]
[Route("invoices")]
public class InvoiceController(ILoggerFactory? loggerFactory, DaprClient dapr, InvoiceGenerator generator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<InvoiceController>() ??
                                       NullLoggerFactory.Instance.CreateLogger<InvoiceController>();

    /// <summary>
    /// Subscribes to a messaging service and builds the invoice given the customer and line item data.
    /// </summary>
    /// <returns></returns>
    [Topic(Constants.PubSubName, Constants.GenerateInvoiceTopicName)]
    [HttpPost("build")]
    public async Task<IActionResult> BuildCustomerInvoiceAsync(Invoice invoice)
    {
        try
        {
            _logger.LogInformation("Received request to build customer invoice for {invoiceNumber}",
                invoice.InvoiceNumber);

            //Map to the target type
            var invoiceFields = mapper.Map<InvoiceFields>(invoice);
            _logger.LogInformation("Mapped event data to invoice fields for processing invoice {invoiceNumber}", invoice.InvoiceNumber);

            //Generate the bytes comprising the PDF
            var pdfBytes = await generator.GenerateInvoiceAsync(invoiceFields);
            _logger.LogInformation("Successfully generated PDF invoice with byte length {byteLength} for {invoiceNumber}", pdfBytes.LongLength, invoice.InvoiceNumber);

            //Persist the bytes to a storage blob
            await dapr.SaveStateAsync(Constants.BlobStateName, invoice.InvoiceNumber, pdfBytes);
            _logger.LogInformation("Successfully persisted PDF bytes to blob state for {invoiceNumber}", invoice.InvoiceNumber);

            //Publish message indicating that the invoice generation is completed
            await dapr.PublishEventAsync(Constants.PubSubName, Constants.FileBuiltPubSubName,
                new BuiltInvoice(invoice.InvoiceNumber, invoice.CustomerName, invoice.InvoiceDate, invoice.Total,
                    invoice.CustomerId));
            _logger.LogInformation("Successfully published invoice generation completion event to message queue for {invoiceNumber}", invoice.InvoiceNumber);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while building customer invoice for {invoiceNumber}", invoice.InvoiceNumber);
            return new StatusCodeResult(500);
        }
    }
}