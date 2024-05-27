using Dapr.Workflow;
using InvoiceService.Email;
using InvoiceService.Operations;
using InvoiceService.Workflows;
using InvoiceService.Workflows.Activities.SubmitInvoice;
using Shared;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);
//Add the various environment variables prefixed with "catalyst_";
builder.Configuration.AddEnvironmentVariables(Constants.EnvironmentVariableNames.VariablePrefix);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddDaprClient((c, dapr) =>
{
    var configuration = c.GetRequiredService<IConfiguration>();
    var httpEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprHttpEndpoint);
    var grpcEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprGrpcEndpoint);
    var apiToken = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprInvoiceApiToken);

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
    options.RegisterActivity<EmailApprovalToAdminActivity>();
    options.RegisterActivity<EmailCustomerWithInvoiceActivity>();
    options.RegisterActivity<EmailAdminWithRejectionNoticeActivity>();
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseCloudEvents();
app.UseAuthorization();
app.MapControllers();

app.Run();
