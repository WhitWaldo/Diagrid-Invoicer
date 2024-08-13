using Dapr.Workflow;
using InvoiceService.Email;
using InvoiceService.Extensions;
using InvoiceService.Operations;
using InvoiceService.Workflows;
using InvoiceService.Workflows.Activities.SubmitInvoice;
using Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(opt =>
{
    opt.ClearProviders();
    opt.AddConsole();
});
builder.Services.AddHttpLogging(opt =>
{
    opt.CombineLogs = true;
});

//Add the various environment variables prefixed with "catalyst_";
builder.Configuration.AddEnvironmentVariables(Constants.EnvironmentVariableNames.VariablePrefix);

builder.Services.AddHealthChecks();

builder.Services.AddControllers();

builder.Services.AddDaprClient((c, dapr) =>
{
    var configuration = c.GetRequiredService<IConfiguration>();
    var logger = c.GetRequiredService<ILogger>();

    var httpEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprHttpEndpoint);
    logger.LogInformation("HttpEndpoint: {httpEndpoint}", httpEndpoint);
    
    var grpcEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprGrpcEndpoint);
    logger.LogInformation("GrpcEndpoint: {grpcEndpoint}", grpcEndpoint);
    
    var apiToken = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprInvoiceApiToken);
    logger.LogInformation("ApiToken: {apiToken}", apiToken?[..5]);

    dapr.UseHttpEndpoint(httpEndpoint);
    dapr.UseGrpcEndpoint(grpcEndpoint);
    dapr.UseDaprApiToken(apiToken);
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<KeyManagement>();
builder.Services.AddSingleton<DataManagement>();
builder.Services.AddSingleton<EmailPayloadBuilder>();
builder.Services.AddDaprWorkflowClient();

builder.Services.AddDaprWorkflow(options =>
{
    options.RegisterWorkflow<SubmitInvoiceWorkflow>();
    options.RegisterActivity<GetCustomerEmailAddressActivity>();
    options.RegisterActivity<EmailApprovalToAdminActivity>();
    options.RegisterActivity<EmailCustomerWithInvoiceActivity>();
    options.RegisterActivity<EmailAdminWithRejectionNoticeActivity>();
});

var app = builder.Build();

app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
app.UseHttpLogging();
app.UseCloudEvents();
app.UseAuthorization();
app.MapControllers();

app.Run();
