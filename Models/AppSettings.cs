using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CheckYoSelfAI.Models;

/// <summary>
/// Application-wide settings and preferences.
/// Contains non-sensitive data stored in regular preferences.
/// </summary>
public class AppSettings : IValidatableObject, IEquatable<AppSettings>
{
    /// <summary>
    /// Application theme preference.
    /// </summary>
    [Required]
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>
    /// Application language/locale preference.
    /// </summary>
    [Required]
    [RegularExpression(@"^[a-z]{2}(-[A-Z]{2})?$", ErrorMessage = "Language must be in format 'en' or 'en-US'")]
    public string Language { get; set; } = "en";

    /// <summary>
    /// Indicates if this is the first run of the application.
    /// </summary>
    public bool IsFirstRun { get; set; } = true;

    /// <summary>
    /// Last time the application checked for updates.
    /// </summary>
    public DateTime? LastUpdateCheck { get; set; }

    /// <summary>
    /// Default image quality for document capture (1-100).
    /// </summary>
    [Range(1, 100, ErrorMessage = "Image quality must be between 1 and 100")]
    public int DefaultImageQuality { get; set; } = 85;

    /// <summary>
    /// Maximum image size in MB before compression is applied.
    /// </summary>
    [Range(1, 50, ErrorMessage = "Max image size must be between 1 and 50 MB")]
    public int MaxImageSizeMB { get; set; } = 10;

    /// <summary>
    /// Whether to automatically save processing results.
    /// </summary>
    public bool AutoSaveResults { get; set; } = true;

    /// <summary>
    /// Whether to show confidence scores in the UI.
    /// </summary>
    public bool ShowConfidenceScores { get; set; } = true;

    /// <summary>
    /// Whether to enable haptic feedback for user interactions.
    /// </summary>
    public bool EnableHapticFeedback { get; set; } = true;

    /// <summary>
    /// Whether the camera permission has been requested.
    /// </summary>
    public bool CameraPermissionAsked { get; set; } = false;

    /// <summary>
    /// Whether the photo library permission has been requested.
    /// </summary>
    public bool PhotoLibraryPermissionAsked { get; set; } = false;

    /// <summary>
    /// Whether detailed logging is enabled.
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// Minimum log level to capture.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Whether developer mode features are enabled.
    /// </summary>
    public bool DeveloperMode { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether this settings object is valid.
    /// </summary>
    [JsonIgnore]
    public bool IsValid
    {
        get
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this);
            return Validator.TryValidateObject(this, validationContext, validationResults, true);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the settings have been customized from defaults.
    /// </summary>
    [JsonIgnore]
    public bool IsCustomized
    {
        get
        {
            var defaultSettings = CreateDefault();
            return !Equals(defaultSettings);
        }
    }

    /// <summary>
    /// Validates the application settings object.
    /// </summary>
    /// <param name="validationContext">The validation context</param>
    /// <returns>Collection of validation results</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Validate theme enum
        if (!Enum.IsDefined(typeof(AppTheme), Theme))
        {
            results.Add(new ValidationResult(
                "Theme value is not valid",
                [nameof(Theme)]));
        }

        // Validate log level enum
        if (!Enum.IsDefined(typeof(LogLevel), LogLevel))
        {
            results.Add(new ValidationResult(
                "Log level value is not valid",
                [nameof(LogLevel)]));
        }

        // Validate language format
        if (!string.IsNullOrWhiteSpace(Language))
        {
            var supportedLanguages = new[] { "en", "en-US", "es", "es-ES", "fr", "fr-FR", "de", "de-DE" };
            if (!supportedLanguages.Contains(Language))
            {
                results.Add(new ValidationResult(
                    $"Language '{Language}' is not currently supported",
                    [nameof(Language)]));
            }
        }

        // Validate update check date
        if (LastUpdateCheck.HasValue && LastUpdateCheck.Value > DateTime.UtcNow)
        {
            results.Add(new ValidationResult(
                "Last update check cannot be in the future",
                [nameof(LastUpdateCheck)]));
        }

        return results;
    }

    /// <summary>
    /// Creates default application settings.
    /// </summary>
    /// <returns>Default application settings</returns>
    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            Theme = AppTheme.System,
            Language = "en",
            IsFirstRun = true,
            LastUpdateCheck = null,
            DefaultImageQuality = 85,
            MaxImageSizeMB = 10,
            AutoSaveResults = true,
            ShowConfidenceScores = true,
            EnableHapticFeedback = true,
            CameraPermissionAsked = false,
            PhotoLibraryPermissionAsked = false,
            EnableLogging = false,
            LogLevel = LogLevel.Information,
            DeveloperMode = false
        };
    }

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        var defaultSettings = CreateDefault();

        Theme = defaultSettings.Theme;
        Language = defaultSettings.Language;
        // Keep IsFirstRun and permission states as they are
        LastUpdateCheck = defaultSettings.LastUpdateCheck;
        DefaultImageQuality = defaultSettings.DefaultImageQuality;
        MaxImageSizeMB = defaultSettings.MaxImageSizeMB;
        AutoSaveResults = defaultSettings.AutoSaveResults;
        ShowConfidenceScores = defaultSettings.ShowConfidenceScores;
        EnableHapticFeedback = defaultSettings.EnableHapticFeedback;
        EnableLogging = defaultSettings.EnableLogging;
        LogLevel = defaultSettings.LogLevel;
        DeveloperMode = defaultSettings.DeveloperMode;
    }

    public bool Equals(AppSettings? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Theme == other.Theme &&
               Language == other.Language &&
               IsFirstRun == other.IsFirstRun &&
               LastUpdateCheck == other.LastUpdateCheck &&
               DefaultImageQuality == other.DefaultImageQuality &&
               MaxImageSizeMB == other.MaxImageSizeMB &&
               AutoSaveResults == other.AutoSaveResults &&
               ShowConfidenceScores == other.ShowConfidenceScores &&
               EnableHapticFeedback == other.EnableHapticFeedback &&
               CameraPermissionAsked == other.CameraPermissionAsked &&
               PhotoLibraryPermissionAsked == other.PhotoLibraryPermissionAsked &&
               EnableLogging == other.EnableLogging &&
               LogLevel == other.LogLevel &&
               DeveloperMode == other.DeveloperMode;
    }

    public override bool Equals(object? obj) => Equals(obj as AppSettings);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Theme);
        hash.Add(Language);
        hash.Add(IsFirstRun);
        hash.Add(LastUpdateCheck);
        hash.Add(DefaultImageQuality);
        hash.Add(MaxImageSizeMB);
        hash.Add(AutoSaveResults);
        hash.Add(ShowConfidenceScores);
        hash.Add(EnableHapticFeedback);
        hash.Add(CameraPermissionAsked);
        hash.Add(PhotoLibraryPermissionAsked);
        hash.Add(EnableLogging);
        hash.Add(LogLevel);
        hash.Add(DeveloperMode);
        return hash.ToHashCode();
    }

    public static bool operator ==(AppSettings? left, AppSettings? right) => Equals(left, right);
    public static bool operator !=(AppSettings? left, AppSettings? right) => !Equals(left, right);
}

/// <summary>
/// Application theme options.
/// </summary>
public enum AppTheme
{
    /// <summary>
    /// Follow system theme setting.
    /// </summary>
    System = 0,

    /// <summary>
    /// Always use light theme.
    /// </summary>
    Light = 1,

    /// <summary>
    /// Always use dark theme.
    /// </summary>
    Dark = 2
}

/// <summary>
/// Log level options for application logging.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Log everything including trace information.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Log debug information and above.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Log informational messages and above.
    /// </summary>
    Information = 2,

    /// <summary>
    /// Log warnings and above.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Log errors and above.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Log only critical errors.
    /// </summary>
    Critical = 5,

    /// <summary>
    /// Disable logging.
    /// </summary>
    None = 6
}