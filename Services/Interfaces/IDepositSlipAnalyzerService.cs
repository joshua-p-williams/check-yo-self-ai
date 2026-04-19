using check_yo_self_ai.Models;

namespace check_yo_self_ai.Services.Interfaces;

/// <summary>
/// Service interface for deposit slip analysis using custom neural models.
/// This service handles variable-layout deposit slips after classification routing.
/// </summary>
public interface IDepositSlipAnalyzerService
{
    /// <summary>
    /// Analyzes a pre-classified deposit slip document using a custom neural model.
    /// Extracts deposit slip fields across different bank layouts and formats.
    /// </summary>
    /// <param name="document">The document already classified as a deposit slip</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Extraction result with deposit slip fields and confidence scores</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null</exception>
    /// <exception cref="DepositSlipAnalysisException">Thrown when analysis fails</exception>
    Task<ExtractionResult> AnalyzeDepositSlipAsync(DocumentInput document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a deposit slip using a specific custom model.
    /// Useful when different models are available for different bank layouts.
    /// </summary>
    /// <param name="document">The document to analyze</param>
    /// <param name="modelId">Specific custom model ID to use for analysis</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Extraction result with deposit slip fields</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null</exception>
    /// <exception cref="ArgumentException">Thrown when modelId is invalid</exception>
    /// <exception cref="DepositSlipAnalysisException">Thrown when analysis fails</exception>
    Task<ExtractionResult> AnalyzeDepositSlipWithModelAsync(DocumentInput document, string modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes multiple deposit slip documents in a batch operation.
    /// </summary>
    /// <param name="documents">Collection of documents classified as deposit slips</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of extraction results in the same order as input</returns>
    /// <exception cref="ArgumentNullException">Thrown when documents collection is null</exception>
    /// <exception cref="DepositSlipAnalysisException">Thrown when analysis fails</exception>
    Task<IEnumerable<ExtractionResult>> BatchAnalyzeDepositSlipsAsync(IEnumerable<DocumentInput> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps raw custom model extraction results to normalized document format.
    /// Converts model-specific fields to unified deposit slip business schema.
    /// </summary>
    /// <param name="extractionResult">Raw extraction result from custom model</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Normalized document with deposit slip fields populated</returns>
    /// <exception cref="ArgumentNullException">Thrown when extractionResult is null</exception>
    /// <exception cref="MappingException">Thrown when mapping fails</exception>
    Task<NormalizedDocument> MapToNormalizedDocumentAsync(ExtractionResult extractionResult, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a deposit slip extraction result meets quality standards.
    /// Checks for required fields, amount calculations, and business rules.
    /// </summary>
    /// <param name="extractionResult">The extraction result to validate</param>
    /// <returns>Validation result with quality assessment</returns>
    DepositSlipValidationResult ValidateDepositSlipExtraction(ExtractionResult extractionResult);

    /// <summary>
    /// Gets a list of available custom models for deposit slip analysis.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of available custom models with their information</returns>
    Task<IEnumerable<CustomModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about the default custom model being used.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Model information for the default deposit slip analyzer</returns>
    Task<CustomModelInfo> GetDefaultModelInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about a specific custom model.
    /// </summary>
    /// <param name="modelId">The model ID to get information for</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Detailed information about the specified model</returns>
    /// <exception cref="ArgumentException">Thrown when modelId is invalid or not found</exception>
    Task<CustomModelInfo> GetModelInfoAsync(string modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the deposit slip analysis service with a sample document.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if service is functioning correctly, false otherwise</returns>
    Task<bool> TestServiceHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the default custom model to use for deposit slip analysis.
    /// </summary>
    /// <param name="modelId">The model ID to set as default</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <exception cref="ArgumentException">Thrown when modelId is invalid</exception>
    Task SetDefaultModelAsync(string modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of critical deposit slip fields that must be present for successful processing.
    /// </summary>
    /// <returns>Collection of field names required for deposit slip processing</returns>
    IEnumerable<string> GetRequiredDepositSlipFields();
}

/// <summary>
/// Result of deposit slip extraction validation.
/// </summary>
public class DepositSlipValidationResult
{
    /// <summary>
    /// Indicates whether the deposit slip extraction passed validation.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Overall quality score for the deposit slip extraction (0.0 to 1.0).
    /// </summary>
    public double QualityScore { get; set; }

    /// <summary>
    /// List of validation errors that prevent processing.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings that may affect accuracy.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// List of deposit slip fields that are missing or have low confidence.
    /// </summary>
    public List<string> MissingOrLowConfidenceFields { get; set; } = new();

    /// <summary>
    /// Validation result for deposit amount calculations.
    /// </summary>
    public AmountValidationResult? AmountValidation { get; set; }

    /// <summary>
    /// Recommended action based on validation results.
    /// </summary>
    public RecommendedAction? RecommendedAction { get; set; }

    /// <summary>
    /// Additional details about the validation assessment.
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// Validation result for deposit slip amount calculations.
/// </summary>
public class AmountValidationResult
{
    /// <summary>
    /// Indicates whether amount calculations are consistent.
    /// </summary>
    public bool IsConsistent { get; set; }

    /// <summary>
    /// Total amount calculated from deposit items.
    /// </summary>
    public decimal CalculatedTotal { get; set; }

    /// <summary>
    /// Total amount stated on the deposit slip.
    /// </summary>
    public decimal? StatedTotal { get; set; }

    /// <summary>
    /// Difference between calculated and stated totals.
    /// </summary>
    public decimal Variance { get; set; }

    /// <summary>
    /// Issues found in amount calculations.
    /// </summary>
    public List<string> Issues { get; set; } = new();
}

/// <summary>
/// Information about a custom model used for document analysis.
/// </summary>
public class CustomModelInfo : ModelInfo
{
    /// <summary>
    /// Status of the custom model (training, ready, error, etc.).
    /// </summary>
    public ModelStatus Status { get; set; }

    /// <summary>
    /// Accuracy metrics for the custom model.
    /// </summary>
    public ModelAccuracyMetrics? AccuracyMetrics { get; set; }

    /// <summary>
    /// Number of documents used to train the model.
    /// </summary>
    public int TrainingDocumentCount { get; set; }

    /// <summary>
    /// Date when the model training was completed.
    /// </summary>
    public DateTime? TrainingCompletedAt { get; set; }

    /// <summary>
    /// List of document layout variants this model was trained on.
    /// </summary>
    public List<string> SupportedLayouts { get; set; } = new();

    /// <summary>
    /// Configuration settings used for this model.
    /// </summary>
    public Dictionary<string, object>? ModelConfiguration { get; set; }
}

/// <summary>
/// Status values for custom models.
/// </summary>
public enum ModelStatus
{
    /// <summary>
    /// Model is currently being trained.
    /// </summary>
    Training = 0,

    /// <summary>
    /// Model is ready for use.
    /// </summary>
    Ready = 1,

    /// <summary>
    /// Model training failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Model has been disabled or deprecated.
    /// </summary>
    Disabled = 3
}

/// <summary>
/// Accuracy metrics for a custom model.
/// </summary>
public class ModelAccuracyMetrics
{
    /// <summary>
    /// Overall accuracy percentage (0.0 to 1.0).
    /// </summary>
    public double OverallAccuracy { get; set; }

    /// <summary>
    /// Precision score for the model.
    /// </summary>
    public double Precision { get; set; }

    /// <summary>
    /// Recall score for the model.
    /// </summary>
    public double Recall { get; set; }

    /// <summary>
    /// F1 score for the model.
    /// </summary>
    public double F1Score { get; set; }

    /// <summary>
    /// Field-level accuracy scores.
    /// </summary>
    public Dictionary<string, double> FieldAccuracy { get; set; } = new();
}