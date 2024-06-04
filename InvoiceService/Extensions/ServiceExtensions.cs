using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Dapr.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InvoiceService.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();

        return builder;
    }

    /// <summary>
    /// Registers Dapr client with support for injection of other services.
    /// </summary>
    /// <remarks>
    /// Hopefully this won't be necessary for long once https://github.com/dapr/dotnet-sdk/pull/1289 is accepted
    /// </remarks>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    public static void AddDaprClient(this IServiceCollection services,
        Action<IServiceProvider, DaprClientBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.TryAddSingleton(serviceProvider =>
        {
            var builder = new DaprClientBuilder();
            configure?.Invoke(serviceProvider, builder);

            return builder.Build();
        });
    }
    
    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });

            // Uncomment the following line to enable the Prometheus endpoint (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
            // app.MapPrometheusScrapingEndpoint();
        }

        return app;
    }
}