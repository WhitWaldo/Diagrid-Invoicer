﻿using System.Text.Json;
using Dapr.Client;
using InvoiceService.Models.InboundEmail;
using InvoiceService.Models.InboundEmail.CommandData;
using InvoiceService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

namespace InvoiceService.Controllers;

[ApiController]
[Route("email")]
[AllowAnonymous]
public sealed class InboundEmailController(ILoggerFactory? loggerFactory, DataManagement stateMgmt, DaprClient client, IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<InboundEmailController>() ??
                                       NullLoggerFactory.Instance.CreateLogger<InboundEmailController>();

    /// <summary>
    /// Parses the inbound email received by the endpoint and if it makes sense and is from the appropriate people, the operational activity is taken.
    /// </summary>
    /// <returns></returns>
    [HttpPost("receive")]
    public async Task<IActionResult> ParseInboundEmailAsync([FromForm] ParsedEmail parsedEmailValue)
    {
        try
        {
            _logger.LogInformation("Received email at inbound email processing endpoint with body {body}", parsedEmailValue.ToString());
            
            ////Validate that the email is from the administrator - Doesn't parse raw from property for some reason
            //var adminEmailAddress =
            //    configuration.GetValue<string>(Constants.EnvironmentVariableNames.AdminEmailAddress);
            //if (!string.Equals(parsedEmailValue.Envelope?.From, adminEmailAddress, StringComparison.InvariantCultureIgnoreCase))
            var emailValidationCode =
                configuration.GetValue<string>(Constants.EnvironmentVariableNames.EmailValidationCode)!;
            if (!parsedEmailValue.IsAuthenticated(emailValidationCode))
            {
                _logger.LogInformation("The email did not pass authentication as it did not contain the required code value");
                //_logger.LogInformation(
                //    "As the email was sent from {emailFromAddress} and not the administrator email {adminEmailAddress}, the message is being ignored",
                //    parsedEmailValue.Envelope?.From ?? "", adminEmailAddress);
                return new StatusCodeResult(401);
            }

            var parsedEmailCommandPayload = parsedEmailValue.ParsePayload();
            if (parsedEmailCommandPayload is null)
            {
                _logger.LogError("Parsing of the email payload failed with unknown reason");
                return new StatusCodeResult(500);
            }

            if (parsedEmailCommandPayload.Command == EmailCommand.Unknown)
            {
                _logger.LogError(
                    "Parsing of the email payload failed because the command in the subject was not recognized");
                return new StatusCodeResult(500);
            }

            switch (parsedEmailCommandPayload)
            {
                case ParseLineItemPayload lineItemPayload:
                    await stateMgmt.LogLineItemAsync(lineItemPayload.LineItems);
                    _logger.LogInformation("Successfully logged line item data to state");
                    break;
                case GenerateInvoicePayload generatePayload:
                {
                    //Retrieve the invoice data from state for each of the given customer IDs in the payload and push into the pubsub queue
                    foreach (var customerId in generatePayload.CustomerIds)
                    {
                        var invoice = await stateMgmt.PrepareInvoicesAsync(customerId);
                        _logger.LogInformation("Created invoice data for customer {customerId} - publishing to pubsub", customerId);
                        await client.PublishEventAsync(Constants.PubSubName, Constants.GenerateInvoiceTopicName, invoice);
                        _logger.LogInformation("Published invoice data for {customerId} to pubsub", customerId);
                    }

                    break;
                }
            }
            
            _logger.LogInformation("Successfully processed inbound email");
            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while processing inbound email");
            return new StatusCodeResult(500);
        }
    }
}