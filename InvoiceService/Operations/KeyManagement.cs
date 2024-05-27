using Dapr.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

namespace InvoiceService.Operations;

/// <summary>
/// Provides management of the key used as a prefix for storing line item data.
/// </summary>
public sealed class KeyManagement(ILoggerFactory? loggerFactory, DaprClient dapr)
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<KeyManagement>() ??
                                       NullLoggerFactory.Instance.CreateLogger<KeyManagement>();

    /// <summary>
    /// The name of the state containing the keys.
    /// </summary>
    private const string KeyStateName = "Keys";

    /// <summary>
    /// Retrieves the current key from state.
    /// </summary>
    /// <param name="customerId">The ID of the customer to rotate keys for.</param>
    /// <returns>The current key to be used as a data prefix.</returns>
    public async Task<Guid> GetCurrentKeyAsync(Guid customerId)
    {
        _logger.LogInformation("Retrieving current data management key from state");
        var keyName = BuildKeyName(customerId);
        var keys = await dapr.GetStateAsync<List<Guid>>(Constants.KeyValueStateName, keyName, ConsistencyMode.Strong) ?? [];

        if (keys.Count == 0)
        {
            _logger.LogInformation("Unable to locate any keys in state - rotating the keys to generate a new one");

            //Rotate the keys and return the latest one it produces
            var newKey = await RotateKeysAsync(customerId);
            keys.Add(newKey);
            _logger.LogInformation("Added new key to state");
        }

        return keys.Last();
    }

    /// <summary>
    /// Rotates the keys in the data store.
    /// </summary>
    /// <param name="customerId">The ID of the customer to rotate keys for.</param>
    /// <returns>The latest generated key.</returns>
    public async Task<Guid> RotateKeysAsync(Guid customerId)
    {
        try
        {
            _logger.LogInformation("Rotating data keys");
            var newKey = Guid.NewGuid();
            var keyName = BuildKeyName(customerId);
            var existingKeys = await dapr.GetStateAsync<List<Guid>>(Constants.KeyValueStateName, keyName, ConsistencyMode.Strong) ?? [];

            _logger.LogInformation("Retrieved existing keys from data and discovered {numberKeys} keys present", existingKeys.Count);

            //Shouldn't contain more than 3 keys after adding the new one. Since we add new keys to the end, remove from the beginning.
            while (existingKeys.Count > 2)
            {
                _logger.LogInformation("As there are {numberKeys} keys present, removing from the front of the list", existingKeys.Count);
                existingKeys.RemoveAt(0);
            }

            existingKeys.Add(newKey);

            //Write back to the state
            _logger.LogInformation("Writing updated key list back to state with {numberKeys} keys", existingKeys.Count);
            await dapr.SaveStateAsync(Constants.KeyValueStateName, KeyStateName, existingKeys);

            _logger.LogInformation("Successfully rotated data keys");
            return newKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while rotating data keys");
            throw;
        }
    }

    private string BuildKeyName(Guid customerId) => $"Keys_{customerId}";
}