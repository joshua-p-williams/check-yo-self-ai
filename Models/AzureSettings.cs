using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CheckYoSelfAI.Models;

/// <summary>
/// Configuration settings for Azure AI Document Intelligence service.
/// Contains sensitive data that should be stored securely.
/// </summary>
public class AzureSettings : IValidatableObject, IEquatable<AzureSettings>
{
    /// <summary>
    /// Azure Document Intelligence endpoint URL.
    /// </summary>
    [Required(ErrorMessage = "Azure endpoint is required")]
    [Url(ErrorMessage = "Azure endpoint must be a valid URL")]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure Document Intelligence API key for authentication.
    /// </summary>
    [Required(ErrorMessage = "Azure API key is required")]
    [MinLength(32, ErrorMessage = "Azure API key must be at least 32 characters")]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Azure region for the Document Intelligence resource.
    /// </summary>
    [Required(ErrorMessage = "Azure region is required")]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Custom classifier model ID for document classification.
    /// </summary>
    public string? CustomClassifierModelId { get; set; }

    /// <summary>
    /// Custom neural model ID for deposit slip analysis.
    /// </summary>
    public string? CustomDepositSlipModelId { get; set; }

    /// <summary>
    /// Timeout in seconds for Azure API calls.
    /// </summary>
    [Range(1, 300, ErrorMessage = "Timeout must be between 1 and 300 seconds")]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts for failed API calls.
    /// </summary>
    [Range(0, 5, ErrorMessage = "Max retries must be between 0 and 5")]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets a value indicating whether this settings object is valid and ready to use.
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
    /// Gets a value indicating whether the settings are configured (not empty/default).
    /// </summary>
    [JsonIgnore]
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Endpoint) && 
                               !string.IsNullOrWhiteSpace(ApiKey) && 
                               !string.IsNullOrWhiteSpace(Region);

    /// <summary>
    /// Validates the Azure settings object.
    /// </summary>
    /// <param name="validationContext">The validation context</param>
    /// <returns>Collection of validation results</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Validate endpoint format
        if (!string.IsNullOrWhiteSpace(Endpoint))
        {
            try
            {
                var uri = new Uri(Endpoint);
                if (!uri.Host.Contains("cognitiveservices.azure.com", StringComparison.OrdinalIgnoreCase) &&
                    !uri.Host.Contains("documentintelligence.azure.com", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new ValidationResult(
                        "Endpoint should be an Azure Cognitive Services or Document Intelligence URL",
                        [nameof(Endpoint)]));
                }
            }
            catch (UriFormatException)
            {
                results.Add(new ValidationResult(
                    "Endpoint must be a valid URL",
                    [nameof(Endpoint)]));
            }
        }

        // Validate API key format (basic validation - should be hex string)
        if (!string.IsNullOrWhiteSpace(ApiKey))
        {
            if (ApiKey.Length < 32)
            {
                results.Add(new ValidationResult(
                    "API key appears to be too short for Azure services",
                    [nameof(ApiKey)]));
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(ApiKey, @"^[a-fA-F0-9]+$"))
            {
                results.Add(new ValidationResult(
                    "API key should contain only hexadecimal characters",
                    [nameof(ApiKey)]));
            }
        }

        // Validate region format
        if (!string.IsNullOrWhiteSpace(Region))
        {
            var validRegions = new[]
            {
                "eastus", "eastus2", "westus", "westus2", "westus3", "centralus", "northcentralus", "southcentralus",
                "canadacentral", "canadaeast", "brazilsouth", "northeurope", "westeurope", "uksouth", "ukwest",
                "francecentral", "germanywestcentral", "switzerlandnorth", "norwayeast", "uaenorth",
                "southafricanorth", "australiaeast", "australiasoutheast", "centralindia", "southindia",
                "eastasia", "southeastasia", "japaneast", "japanwest", "koreacentral", "koreasouth"
            };

            if (!validRegions.Contains(Region.ToLowerInvariant()))
            {
                results.Add(new ValidationResult(
                    $"Region '{Region}' is not a recognized Azure region",
                    [nameof(Region)]));
            }
        }

        return results;
    }

    /// <summary>
    /// Creates a default Azure settings object with placeholder values.
    /// </summary>
    /// <returns>Default Azure settings</returns>
    public static AzureSettings CreateDefault()
    {
        return new AzureSettings
        {
            Endpoint = "https://your-resource.cognitiveservices.azure.com/",
            ApiKey = string.Empty,
            Region = "eastus",
            TimeoutSeconds = 30,
            MaxRetries = 3
        };
    }

    /// <summary>
    /// Creates a copy of the current Azure settings with sensitive data masked.
    /// Useful for logging or display purposes.
    /// </summary>
    /// <returns>Masked copy of the settings</returns>
    public AzureSettings CreateMaskedCopy()
    {
        return new AzureSettings
        {
            Endpoint = Endpoint,
            ApiKey = string.IsNullOrWhiteSpace(ApiKey) ? string.Empty : $"***{ApiKey[^4..]}",
            Region = Region,
            CustomClassifierModelId = CustomClassifierModelId,
            CustomDepositSlipModelId = CustomDepositSlipModelId,
            TimeoutSeconds = TimeoutSeconds,
            MaxRetries = MaxRetries
        };
    }

    public bool Equals(AzureSettings? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Endpoint == other.Endpoint &&
               ApiKey == other.ApiKey &&
               Region == other.Region &&
               CustomClassifierModelId == other.CustomClassifierModelId &&
               CustomDepositSlipModelId == other.CustomDepositSlipModelId &&
               TimeoutSeconds == other.TimeoutSeconds &&
               MaxRetries == other.MaxRetries;
    }

    public override bool Equals(object? obj) => Equals(obj as AzureSettings);

    public override int GetHashCode()
    {
        return HashCode.Combine(Endpoint, ApiKey, Region, CustomClassifierModelId, 
                               CustomDepositSlipModelId, TimeoutSeconds, MaxRetries);
    }

    public static bool operator ==(AzureSettings? left, AzureSettings? right) => Equals(left, right);
    public static bool operator !=(AzureSettings? left, AzureSettings? right) => !Equals(left, right);
}