using AutoMapper;
using BuilderService;
using BuilderService.Extensions;
using BuilderService.Mapper;
using BuilderService.Operations;
using Dapr.Workflow;
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
builder.Services.AddDaprClient((c, dapr ) =>
{
    var configuration = c.GetRequiredService<IConfiguration>();
    var httpEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprHttpEndpoint);
    var grpcEndpoint = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprGrpcEndpoint);
    var apiToken = configuration.GetValue<string>(Constants.EnvironmentVariableNames.DaprBuilderApiToken);

    dapr.UseHttpEndpoint(httpEndpoint);
    dapr.UseGrpcEndpoint(grpcEndpoint);
    dapr.UseDaprApiToken(apiToken);
});

builder.Services.AddSingleton<SyncfusionLicenseRegistration>();
builder.Services.AddSingleton(_ =>
{
    var configuration = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<InvoiceBuilderProfile>();
    });
    return configuration;
});
builder.Services.AddSingleton<InvoiceGenerator>();

builder.Services.AddDaprWorkflowClient();

//builder.Services.AddDaprWorkflow();

var app = builder.Build();
var syncfusionRegistration = app.Services.GetRequiredService<SyncfusionLicenseRegistration>();
syncfusionRegistration.RegisterLicense(Constants.EnvironmentVariableNames.SyncfusionLicenseKey); 

app.MapHealthChecks("/health");
app.UseHttpLogging();
app.UseCloudEvents();
app.MapSubscribeHandler();
app.UseAuthorization();
app.MapControllers();

app.Run();
