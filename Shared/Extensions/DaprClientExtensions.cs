using Dapr.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shared.Extensions;

public static class DaprClientExtensions
{
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
}