using System.ComponentModel.DataAnnotations;

namespace CheckYoSelfAI.Services.Interfaces;

/// <summary>
/// Service interface for managing application settings and preferences.
/// Provides secure storage for sensitive data and regular preferences for application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value by key with optional default value.
    /// </summary>
    /// <typeparam name="T">The type of the setting value</typeparam>
    /// <param name="key">The setting key</param>
    /// <param name="defaultValue">Default value to return if key doesn't exist</param>
    /// <returns>The setting value or default if not found</returns>
    Task<T?> GetAsync<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Sets a setting value by key.
    /// </summary>
    /// <typeparam name="T">The type of the setting value</typeparam>
    /// <param name="key">The setting key</param>
    /// <param name="value">The value to store</param>
    /// <returns>Task representing the async operation</returns>
    Task SetAsync<T>(string key, T value);

    /// <summary>
    /// Gets a secure setting value (encrypted storage) by key with optional default value.
    /// Use this for sensitive data like API keys, tokens, etc.
    /// </summary>
    /// <typeparam name="T">The type of the secure setting value</typeparam>
    /// <param name="key">The secure setting key</param>
    /// <param name="defaultValue">Default value to return if key doesn't exist</param>
    /// <returns>The secure setting value or default if not found</returns>
    Task<T?> GetSecureAsync<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Sets a secure setting value (encrypted storage) by key.
    /// Use this for sensitive data like API keys, tokens, etc.
    /// </summary>
    /// <typeparam name="T">The type of the secure setting value</typeparam>
    /// <param name="key">The secure setting key</param>
    /// <param name="value">The value to store securely</param>
    /// <returns>Task representing the async operation</returns>
    Task SetSecureAsync<T>(string key, T value);

    /// <summary>
    /// Checks if a setting key exists in regular storage.
    /// </summary>
    /// <param name="key">The setting key to check</param>
    /// <returns>True if the key exists, false otherwise</returns>
    Task<bool> ContainsKeyAsync(string key);

    /// <summary>
    /// Checks if a secure setting key exists in secure storage.
    /// </summary>
    /// <param name="key">The secure setting key to check</param>
    /// <returns>True if the key exists, false otherwise</returns>
    Task<bool> ContainsSecureKeyAsync(string key);

    /// <summary>
    /// Removes a setting by key from regular storage.
    /// </summary>
    /// <param name="key">The setting key to remove</param>
    /// <returns>True if the key was removed, false if it didn't exist</returns>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// Removes a secure setting by key from secure storage.
    /// </summary>
    /// <param name="key">The secure setting key to remove</param>
    /// <returns>True if the key was removed, false if it didn't exist</returns>
    Task<bool> RemoveSecureAsync(string key);

    /// <summary>
    /// Clears all settings from regular storage.
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task ClearAllAsync();

    /// <summary>
    /// Clears all settings from secure storage.
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task ClearAllSecureAsync();

    /// <summary>
    /// Gets all setting keys from regular storage.
    /// </summary>
    /// <returns>Collection of setting keys</returns>
    Task<IEnumerable<string>> GetAllKeysAsync();

    /// <summary>
    /// Validates a settings object using data annotations.
    /// </summary>
    /// <typeparam name="T">The type of the settings object</typeparam>
    /// <param name="settings">The settings object to validate</param>
    /// <returns>Validation results</returns>
    Task<IEnumerable<ValidationResult>> ValidateSettingsAsync<T>(T settings) where T : class;
}

/// <summary>
/// Common setting keys used throughout the application.
/// Centralizes key definitions to prevent typos and ensure consistency.
/// </summary>
public static class SettingKeys
{
    // Azure Document Intelligence Settings (Secure)
    public const string AzureEndpoint = "azure_endpoint";
    public const string AzureApiKey = "azure_api_key";
    public const string AzureRegion = "azure_region";

    // Application Preferences (Regular)
    public const string Theme = "app_theme";
    public const string Language = "app_language";
    public const string FirstRun = "first_run";
    public const string LastUpdateCheck = "last_update_check";

    // Document Processing Settings (Regular)
    public const string DefaultImageQuality = "default_image_quality";
    public const string MaxImageSize = "max_image_size";
    public const string AutoSaveResults = "auto_save_results";

    // User Interface Settings (Regular)
    public const string ShowConfidenceScores = "show_confidence_scores";
    public const string EnableHapticFeedback = "enable_haptic_feedback";
    public const string CameraPermissionAsked = "camera_permission_asked";
    public const string PhotoLibraryPermissionAsked = "photo_library_permission_asked";

    // Debug and Development Settings (Regular)
    public const string EnableLogging = "enable_logging";
    public const string LogLevel = "log_level";
    public const string DeveloperMode = "developer_mode";
}

/// <summary>
/// Exception thrown when a settings operation fails.
/// </summary>
public class SettingsException : Exception
{
    public string? SettingKey { get; }
    public string? Operation { get; }

    public SettingsException(string message) : base(message) { }

    public SettingsException(string message, string? settingKey, string? operation) : base(message)
    {
        SettingKey = settingKey;
        Operation = operation;
    }

    public SettingsException(string message, Exception innerException) : base(message, innerException) { }

    public SettingsException(string message, Exception innerException, string? settingKey, string? operation) 
        : base(message, innerException)
    {
        SettingKey = settingKey;
        Operation = operation;
    }
}