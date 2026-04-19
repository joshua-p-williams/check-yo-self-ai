using check_yo_self_ai.Models;

namespace check_yo_self_ai.Services.Interfaces;

/// <summary>
/// Service interface for document classification functionality.
/// Responsible for identifying document types before routing to specialized extractors.
/// </summary>
public interface IDocumentClassifierService
{
    /// <summary>
    /// Classifies a document to determine its type (check, deposit slip, etc.).
    /// This is the entry point for the document processing pipeline.
    /// </summary>
    /// <param name="document">The document to classify</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Classification result with document type and confidence</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null</exception>
    /// <exception cref="DocumentClassificationException">Thrown when classification fails</exception>
    Task<ClassificationResult> ClassifyDocumentAsync(DocumentInput document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Classifies multiple documents in a single batch operation.
    /// Useful for processing multiple documents efficiently.
    /// </summary>
    /// <param name="documents">Collection of documents to classify</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of classification results in the same order as input</returns>
    /// <exception cref="ArgumentNullException">Thrown when documents collection is null</exception>
    /// <exception cref="DocumentClassificationException">Thrown when classification fails</exception>
    Task<IEnumerable<ClassificationResult>> BatchClassifyAsync(IEnumerable<DocumentInput> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about the classification model being used.
    /// Useful for diagnostics and version tracking.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Model information including version and capabilities</returns>
    Task<ClassifierModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the classification model is available and accessible.
    /// Should be called during application startup or configuration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if model is available and functional, false otherwise</returns>
    Task<bool> ValidateModelAvailabilityAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the confidence thresholds used for classification routing decisions.
    /// </summary>
    /// <returns>Configuration object with threshold settings</returns>
    ConfidenceThresholds GetConfidenceThresholds();

    /// <summary>
    /// Updates the confidence thresholds used for classification routing.
    /// </summary>
    /// <param name="thresholds">New threshold configuration</param>
    void SetConfidenceThresholds(ConfidenceThresholds thresholds);
}

/// <summary>
/// Information about the classification model in use.
/// </summary>
public class ClassifierModelInfo
{
    /// <summary>
    /// Unique identifier for the classification model.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name for the classification model.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Version of the classification model.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Date when the model was created or last updated.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// List of document types this model can classify.
    /// </summary>
    public List<DocumentType> SupportedDocumentTypes { get; set; } = new();

    /// <summary>
    /// Description of the model's capabilities and training data.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Performance metrics for the model (if available).
    /// </summary>
    public Dictionary<string, double>? PerformanceMetrics { get; set; }
}