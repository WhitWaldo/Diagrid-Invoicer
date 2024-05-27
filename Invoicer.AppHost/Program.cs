var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.InvoiceService>("invoiceservice");

builder.AddProject<Projects.BuilderService>("builderservice");

builder.Build().Run();
