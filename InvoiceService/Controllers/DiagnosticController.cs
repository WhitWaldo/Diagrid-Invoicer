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
        sb.AppendLine($"HttpEndpoint: {httpEndpoint}");

        var grpcEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprGrpcEndpoint);
        sb.AppendLine($"GrpcEndpoint: {grpcEndpoint}");

        var invoiceApiToken = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprInvoiceApiToken);
        sb.AppendLine($"InvoiceApiToken: {invoiceApiToken?.Substring(0, 10)}");

        var builderApiToken = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprBuilderApiToken);
        sb.AppendLine($"BuilderApiToken: {builderApiToken?.Substring(0, 10)}");

        var adminEmailAddress = configuration.GetValue<string>(Constants.EnvironmentVariableNames.AdminEmailAddress);
        sb.AppendLine($"AdminEmailAddress: {adminEmailAddress}");

        var fromEmailAddress = configuration.GetValue<string>(Constants.EnvironmentVariableNames.FromEmailAddress);
        sb.AppendLine($"FromEmailAddress: {fromEmailAddress}");

        var customerEmailAddress =
            configuration.GetValue<string>(Constants.EnvironmentVariableNames.CustomerEmailAddress);
        sb.AppendLine($"CustomerEmailAddress: {customerEmailAddress}");
        
        sb.AppendLine($"EmailValidationCode: {emailValidationCode?.Substring(0, 4)}");

        var fromEmailName = configuration.GetValue<string>(Constants.EnvironmentVariableNames.FromEmailName);
        sb.AppendLine($"FromEmailName: {fromEmailName}");

        var syncfusionLicenseKey =
            configuration.GetValue<string>(Constants.EnvironmentVariableNames.SyncfusionLicenseKey)?[..4];
        sb.AppendLine($"SyncfusionLicenseKey: {syncfusionLicenseKey}");

        var apiBaseDomain = configuration.GetValue<string>(Constants.EnvironmentVariableNames.ApiBaseDomain);
        sb.AppendLine($"ApiBaseDomain: {apiBaseDomain}");

        return new OkObjectResult(sb.ToString());
    }
}