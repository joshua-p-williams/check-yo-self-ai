using CheckYoSelfAI.Models;

namespace CheckYoSelfAI.Services.Interfaces;

/// <summary>
/// Service interface for US bank check analysis using Azure's prebuilt check model.
/// This service is invoked only after classification identifies the document as a check.
/// </summary>
public interface ICheckAnalyzerService
{
    /// <summary>
    /// Analyzes a pre-classified check document using Azure's prebuilt US bank check model.
    /// Extracts check-specific fields like amount, payee, check number, etc.
    /// </summary>
    /// <param name="document">The document already classified as a check</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Extraction result with check fields and confidence scores</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null</exception>
    /// <exception cref="CheckAnalysisException">Thrown when check analysis fails</exception>
    Task<ExtractionResult> AnalyzeCheckAsync(DocumentInput document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes multiple check documents in a batch operation.
    /// Useful for processing multiple checks efficiently.
    /// </summary>
    /// <param name="documents">Collection of documents classified as checks</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of extraction results in the same order as input</returns>
    /// <exception cref="ArgumentNullException">Thrown when documents collection is null</exception>
    /// <exception cref="CheckAnalysisException">Thrown when analysis fails</exception>
    Task<IEnumerable<ExtractionResult>> BatchAnalyzeChecksAsync(IEnumerable<DocumentInput> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps raw Azure check extraction results to normalized document format.
    /// Converts Azure-specific field names and formats to business-friendly schema.
    /// </summary>
    /// <param name="extractionResult">Raw extraction result from Azure</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Normalized document with check-specific fields populated</returns>
    /// <exception cref="ArgumentNullException">Thrown when extractionResult is null</exception>
    /// <exception cref="MappingException">Thrown when mapping fails</exception>
    Task<NormalizedDocument> MapToNormalizedDocumentAsync(ExtractionResult extractionResult, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a check extraction result meets quality standards.
    /// Checks for required fields, confidence thresholds, and business rules.
    /// </summary>
    /// <param name="extractionResult">The extraction result to validate</param>
    /// <returns>Validation result with quality assessment</returns>
    CheckValidationResult ValidateCheckExtraction(ExtractionResult extractionResult);

    /// <summary>
    /// Gets information about the prebuilt check model being used.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Model information for the prebuilt check analyzer</returns>
    Task<ModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the check analysis service with a sample check image.
    /// Useful for health checks and configuration validation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if service is functioning correctly, false otherwise</returns>
    Task<bool> TestServiceHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of critical check fields that must be present for successful processing.
    /// </summary>
    /// <returns>Collection of field names required for check processing</returns>
    IEnumerable<string> GetRequiredCheckFields();
}

/// <summary>
/// Result of check extraction validation.
/// </summary>
public class CheckValidationResult
{
    /// <summary>
    /// Indicates whether the check extraction passed validation.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Overall quality score for the check extraction (0.0 to 1.0).
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
    /// List of check fields that are missing or have low confidence.
    /// </summary>
    public List<string> MissingOrLowConfidenceFields { get; set; } = new();

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
/// General model information for document analysis services.
/// </summary>
public class ModelInfo
{
    /// <summary>
    /// Unique identifier for the model.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name for the model.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Version of the model.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Type of model (prebuilt, custom, etc.).
    /// </summary>
    public string ModelType { get; set; } = string.Empty;

    /// <summary>
    /// Description of the model's capabilities.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date when the model was created or last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// List of fields this model can extract.
    /// </summary>
    public List<string> SupportedFields { get; set; } = new();
}