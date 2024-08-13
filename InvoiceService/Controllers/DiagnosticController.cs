using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

namespace InvoiceService.Controllers;

[ApiController]
[Route("diagnostic")]
[AllowAnonymous]
public sealed class DiagnosticController(ILoggerFactory? loggerFactory, IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<DiagnosticController>() ??
                                       NullLoggerFactory.Instance.CreateLogger<DiagnosticController>();
    
    [HttpGet("daprworkflow")]
    public async Task<ActionResult<string>> GetConfigurationDataAsync(string code)
    {
        _logger.LogInformation("Received request for configuration data with code {code}", code);

        var emailValidationCode =
            configuration.GetValue<string>(Constants.EnvironmentVariableNames.EmailValidationCode);

        if (!string.Equals(emailValidationCode, code))
        {
            _logger.LogWarning("Invalid code provided, not authorized");
            return new UnauthorizedResult();
        }

        var sb = new StringBuilder();
        
        //Get the values out of the configuration
        var httpEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprHttpEndpoint);
        sb.AppendLine($"HttpEndpoint: {httpEndpoint}<br/>");

        var grpcEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprGrpcEndpoint);
        sb.AppendLine($"GrpcEndpoint: {grpcEndpoint}<br/>");

        var invoiceApiToken = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprInvoiceApiToken);
        sb.AppendLine($"InvoiceApiToken: {invoiceApiToken}<br/>");

        var builderApiToken = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprBuilderApiToken);
        sb.AppendLine($"BuilderApiToken: {builderApiToken}<br/>");

        var adminEmailAddress = configuration.GetValue<string>(Constants.EnvironmentVariableNames.AdminEmailAddress);
        sb.AppendLine($"AdminEmailAddress: {adminEmailAddress}<br/>");

        var fromEmailAddress = configuration.GetValue<string>(Constants.EnvironmentVariableNames.FromEmailAddress);
        sb.AppendLine($"FromEmailAddress: {fromEmailAddress}<br/>");

        var customerEmailAddress =
            configuration.GetValue<string>(Constants.EnvironmentVariableNames.CustomerEmailAddress);
        sb.AppendLine($"CustomerEmailAddress: {customerEmailAddress}<br/>");
        
        sb.AppendLine($"EmailValidationCode: {emailValidationCode?[..4]}<br/>");

        var fromEmailName = configuration.GetValue<string>(Constants.EnvironmentVariableNames.FromEmailName);
        sb.AppendLine($"FromEmailName: {fromEmailName}<br/>");

        var syncfusionLicenseKey =
            configuration.GetValue<string>(Constants.EnvironmentVariableNames.SyncfusionLicenseKey)?[..4];
        sb.AppendLine($"SyncfusionLicenseKey: {syncfusionLicenseKey}");

        var apiBaseDomain = configuration.GetValue<string>(Constants.EnvironmentVariableNames.ApiBaseDomain);
        sb.AppendLine($"ApiBaseDomain: {apiBaseDomain}<br/>");

        return new OkObjectResult(sb.ToString());
    }
}