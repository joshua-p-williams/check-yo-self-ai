using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.Services;

/// <summary>
/// Implementation of ISettingsService using MAUI Preferences and SecureStorage.
/// Provides type-safe settings management with validation and error handling.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, T? defaultValue = default)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            _logger.LogDebug("Getting setting value for key: {Key}", key);

            // Check if the key exists first
            if (!Preferences.ContainsKey(key))
            {
                _logger.LogDebug("Key {Key} not found, returning default value", key);
                return defaultValue;
            }

            var value = Preferences.Get(key, string.Empty);
            if (string.IsNullOrEmpty(value))
            {
                _logger.LogDebug("Empty value for key {Key}, returning default value", key);
                return defaultValue;
            }

            // Handle different types
            var result = DeserializeValue<T>(value, key);
            _logger.LogDebug("Successfully retrieved setting for key: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get setting for key: {Key}", key);
            throw new SettingsException($"Failed to get setting for key '{key}'", ex, key, "Get");
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            _logger.LogDebug("Setting value for key: {Key}", key);

            var serializedValue = SerializeValue(value, key);
            Preferences.Set(key, serializedValue);

            _logger.LogDebug("Successfully set setting for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set setting for key: {Key}", key);
            throw new SettingsException($"Failed to set setting for key '{key}'", ex, key, "Set");
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetSecureAsync<T>(string key, T? defaultValue = default)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            _logger.LogDebug("Getting secure setting value for key: {Key}", key);

            var value = await SecureStorage.GetAsync(key);
            if (string.IsNullOrEmpty(value))
            {
                _logger.LogDebug("Secure key {Key} not found, returning default value", key);
                return defaultValue;
            }

            var result = DeserializeValue<T>(value, key);
            _logger.LogDebug("Successfully retrieved secure setting for key: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get secure setting for key: {Key}", key);
            throw new SettingsException($"Failed to get secure setting for key '{key}'", ex, key, "GetSecure");
        }
    }

    /// <inheritdoc />
    public async Task SetSecureAsync<T>(string key, T value)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            _logger.LogDebug("Setting secure value for key: {Key}", key);

            var serializedValue = SerializeValue(value, key);
            await SecureStorage.SetAsync(key, serializedValue);

            _logger.LogDebug("Successfully set secure setting for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secure setting for key: {Key}", key);
            throw new SettingsException($"Failed to set secure setting for key '{key}'", ex, key, "SetSecure");
        }
    }

    /// <inheritdoc />
    public async Task<bool> ContainsKeyAsync(string key)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            var exists = Preferences.ContainsKey(key);
            _logger.LogDebug("Key {Key} exists in preferences: {Exists}", key, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if key exists: {Key}", key);
            throw new SettingsException($"Failed to check if key '{key}' exists", ex, key, "ContainsKey");
        }
    }

    /// <inheritdoc />
    public async Task<bool> ContainsSecureKeyAsync(string key)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            // SecureStorage doesn't have a direct ContainsKey method, so we try to get the value
            var value = await SecureStorage.GetAsync(key);
            var exists = !string.IsNullOrEmpty(value);

            _logger.LogDebug("Secure key {Key} exists: {Exists}", key, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if secure key exists: {Key}", key);
            throw new SettingsException($"Failed to check if secure key '{key}' exists", ex, key, "ContainsSecureKey");
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            var existed = Preferences.ContainsKey(key);
            if (existed)
            {
                Preferences.Remove(key);
                _logger.LogDebug("Successfully removed setting for key: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Key {Key} did not exist, nothing to remove", key);
            }

            return existed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove setting for key: {Key}", key);
            throw new SettingsException($"Failed to remove setting for key '{key}'", ex, key, "Remove");
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveSecureAsync(string key)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            var existed = await ContainsSecureKeyAsync(key);
            if (existed)
            {
                SecureStorage.Remove(key);
                _logger.LogDebug("Successfully removed secure setting for key: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Secure key {Key} did not exist, nothing to remove", key);
            }

            return existed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove secure setting for key: {Key}", key);
            throw new SettingsException($"Failed to remove secure setting for key '{key}'", ex, key, "RemoveSecure");
        }
    }

    /// <inheritdoc />
    public async Task ClearAllAsync()
    {
        try
        {
            _logger.LogDebug("Clearing all preferences");
            Preferences.Clear();
            _logger.LogInformation("Successfully cleared all preferences");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all preferences");
            throw new SettingsException("Failed to clear all preferences", ex, null, "ClearAll");
        }
    }

    /// <inheritdoc />
    public async Task ClearAllSecureAsync()
    {
        try
        {
            _logger.LogDebug("Clearing all secure storage");
            SecureStorage.RemoveAll();
            _logger.LogInformation("Successfully cleared all secure storage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all secure storage");
            throw new SettingsException("Failed to clear all secure storage", ex, null, "ClearAllSecure");
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetAllKeysAsync()
    {
        try
        {
            // Note: MAUI Preferences doesn't provide a direct way to get all keys
            // This is a limitation of the underlying platform storage mechanisms
            // We'll return an empty collection and log a warning

            _logger.LogWarning("Getting all keys is not supported by MAUI Preferences API");
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all keys");
            throw new SettingsException("Failed to get all keys", ex, null, "GetAllKeys");
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ValidationResult>> ValidateSettingsAsync<T>(T settings) where T : class
    {
        try
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            _logger.LogDebug("Validating settings of type: {Type}", typeof(T).Name);

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(settings);

            // Perform data annotation validation
            Validator.TryValidateObject(settings, validationContext, validationResults, validateAllProperties: true);

            // If the object implements IValidatableObject, call its Validate method
            if (settings is IValidatableObject validatable)
            {
                var customValidationResults = validatable.Validate(validationContext);
                validationResults.AddRange(customValidationResults);
            }

            _logger.LogDebug("Validation completed for {Type}. Found {Count} errors", 
                typeof(T).Name, validationResults.Count);

            return validationResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate settings of type: {Type}", typeof(T).Name);
            throw new SettingsException($"Failed to validate settings of type '{typeof(T).Name}'", ex, null, "ValidateSettings");
        }
    }

    /// <summary>
    /// Serializes a value to a string for storage.
    /// </summary>
    private string SerializeValue<T>(T value, string key)
    {
        if (value == null)
        {
            return string.Empty;
        }

        try
        {
            // Handle primitive types directly
            return value switch
            {
                string str => str,
                bool b => b.ToString(),
                int i => i.ToString(),
                long l => l.ToString(),
                float f => f.ToString(),
                double d => d.ToString(),
                decimal dec => dec.ToString(),
                DateTime dt => dt.ToBinary().ToString(),
                DateTimeOffset dto => dto.ToUniversalTime().ToString("O"), // Use ISO 8601 format
                TimeSpan ts => ts.Ticks.ToString(),
                Guid g => g.ToString(),
                Enum e => e.ToString(),
                _ => JsonSerializer.Serialize(value, _jsonOptions)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize value for key: {Key}", key);
            throw new SettingsException($"Failed to serialize value for key '{key}'", ex, key, "Serialize");
        }
    }

    /// <summary>
    /// Deserializes a string value to the specified type.
    /// </summary>
    private T? DeserializeValue<T>(string value, string key)
    {
        if (string.IsNullOrEmpty(value))
        {
            return default(T);
        }

        try
        {
            var targetType = typeof(T);
            var nullableType = Nullable.GetUnderlyingType(targetType);
            var actualType = nullableType ?? targetType;

            // Handle primitive types directly
            if (actualType == typeof(string))
            {
                return (T)(object)value;
            }
            else if (actualType == typeof(bool))
            {
                return (T)(object)bool.Parse(value);
            }
            else if (actualType == typeof(int))
            {
                return (T)(object)int.Parse(value);
            }
            else if (actualType == typeof(long))
            {
                return (T)(object)long.Parse(value);
            }
            else if (actualType == typeof(float))
            {
                return (T)(object)float.Parse(value);
            }
            else if (actualType == typeof(double))
            {
                return (T)(object)double.Parse(value);
            }
            else if (actualType == typeof(decimal))
            {
                return (T)(object)decimal.Parse(value);
            }
            else if (actualType == typeof(DateTime))
            {
                return (T)(object)DateTime.FromBinary(long.Parse(value));
            }
            else if (actualType == typeof(DateTimeOffset))
            {
                return (T)(object)DateTimeOffset.Parse(value);
            }
            else if (actualType == typeof(TimeSpan))
            {
                return (T)(object)TimeSpan.FromTicks(long.Parse(value));
            }
            else if (actualType == typeof(Guid))
            {
                return (T)(object)Guid.Parse(value);
            }
            else if (actualType.IsEnum)
            {
                return (T)Enum.Parse(actualType, value);
            }
            else
            {
                // Use JSON deserialization for complex types
                return JsonSerializer.Deserialize<T>(value, _jsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize value for key: {Key}", key);
            throw new SettingsException($"Failed to deserialize value for key '{key}'", ex, key, "Deserialize");
        }
    }
}