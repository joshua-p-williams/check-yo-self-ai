using check_yo_self_ai.Models;

namespace check_yo_self_ai.Services.Interfaces;

/// <summary>
/// Service interface for orchestrating the complete document processing pipeline.
/// Coordinates the upload → classify → route → extract → normalize → assess workflow.
/// </summary>
public interface IDocumentOrchestrationService
{
    /// <summary>
    /// Processes a document through the complete pipeline from input to normalized result.
    /// This is the main entry point for end-to-end document processing.
    /// </summary>
    /// <param name="document">The document to process</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Complete processing result including all pipeline stages</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null</exception>
    /// <exception cref="DocumentProcessingException">Thrown when processing fails</exception>
    Task<ProcessingResult> ProcessDocumentAsync(DocumentInput document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Classifies a document to determine its type.
    /// This is the first stage in the processing pipeline.
    /// </summary>
    /// <param name="document">The document to classify</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Classification result with document type and confidence</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null</exception>
    /// <exception cref="DocumentClassificationException">Thrown when classification fails</exception>
    Task<ClassificationResult> ClassifyDocumentAsync(DocumentInput document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts data from a document using the appropriate model based on document type.
    /// Routes to check analyzer for checks, deposit slip analyzer for deposit slips.
    /// </summary>
    /// <param name="document">The document to extract data from</param>
    /// <param name="documentType">The type of document (from classification)</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Extraction result with field data and confidence scores</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null</exception>
    /// <exception cref="ArgumentException">Thrown when documentType is unsupported</exception>
    /// <exception cref="DocumentExtractionException">Thrown when extraction fails</exception>
    Task<ExtractionResult> ExtractDocumentAsync(DocumentInput document, DocumentType documentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Normalizes extraction results into a unified business document format.
    /// Converts raw extraction data to standardized schema regardless of source model.
    /// </summary>
    /// <param name="extraction">The extraction result to normalize</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Normalized document with standardized business fields</returns>
    /// <exception cref="ArgumentNullException">Thrown when extraction is null</exception>
    /// <exception cref="DocumentNormalizationException">Thrown when normalization fails</exception>
    Task<NormalizedDocument> NormalizeResultAsync(ExtractionResult extraction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assesses the confidence and quality of processing results.
    /// Provides recommendations for manual review or automatic processing.
    /// </summary>
    /// <param name="processingResult">The complete processing result to assess</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Confidence assessment with warnings and recommendations</returns>
    /// <exception cref="ArgumentNullException">Thrown when processingResult is null</exception>
    Task<ConfidenceWarnings> AssessProcessingQualityAsync(ProcessingResult processingResult, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes multiple documents in parallel with configurable concurrency.
    /// </summary>
    /// <param name="documents">Collection of documents to process</param>
    /// <param name="maxConcurrency">Maximum number of concurrent processing operations</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of processing results in the same order as input</returns>
    Task<IEnumerable<ProcessingResult>> BatchProcessDocumentsAsync(IEnumerable<DocumentInput> documents, int maxConcurrency = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a document processing operation.
    /// Useful for long-running operations and progress tracking.
    /// </summary>
    /// <param name="processingId">The unique identifier for the processing operation</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Current processing status and progress information</returns>
    Task<ProcessingStatusInfo> GetProcessingStatusAsync(string processingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a document processing operation if it's still in progress.
    /// </summary>
    /// <param name="processingId">The unique identifier for the processing operation</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if cancellation was successful, false if already completed</returns>
    Task<bool> CancelProcessingAsync(string processingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the pipeline configuration including model settings and thresholds.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Current pipeline configuration</returns>
    Task<PipelineConfiguration> GetPipelineConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the pipeline configuration with new settings.
    /// </summary>
    /// <param name="configuration">New pipeline configuration</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    Task UpdatePipelineConfigurationAsync(PipelineConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the health of all pipeline services and dependencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Health check result with status of all components</returns>
    Task<PipelineHealthResult> CheckPipelineHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when a document processing operation starts.
    /// </summary>
    event EventHandler<ProcessingStartedEventArgs>? ProcessingStarted;

    /// <summary>
    /// Event raised when a document processing operation completes.
    /// </summary>
    event EventHandler<ProcessingCompletedEventArgs>? ProcessingCompleted;

    /// <summary>
    /// Event raised when a processing stage is completed (classification, extraction, etc.).
    /// </summary>
    event EventHandler<StageCompletedEventArgs>? StageCompleted;
}

/// <summary>
/// Information about the current status of a processing operation.
/// </summary>
public class ProcessingStatusInfo
{
    /// <summary>
    /// Unique identifier for the processing operation.
    /// </summary>
    public string ProcessingId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the processing operation.
    /// </summary>
    public ProcessingStatus Status { get; set; }

    /// <summary>
    /// Current stage of the processing pipeline.
    /// </summary>
    public ProcessingStage CurrentStage { get; set; }

    /// <summary>
    /// Percentage of completion (0 to 100).
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Time when the processing started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Elapsed time since processing started.
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    /// <summary>
    /// Estimated time remaining (if calculable).
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Current stage description.
    /// </summary>
    public string? StageDescription { get; set; }

    /// <summary>
    /// Any warnings or issues encountered during processing.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Stages in the document processing pipeline.
/// </summary>
public enum ProcessingStage
{
    /// <summary>
    /// Processing has not started yet.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Validating the input document.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Classifying the document type.
    /// </summary>
    Classification = 2,

    /// <summary>
    /// Routing to the appropriate analyzer.
    /// </summary>
    Routing = 3,

    /// <summary>
    /// Extracting data from the document.
    /// </summary>
    Extraction = 4,

    /// <summary>
    /// Normalizing the extracted data.
    /// </summary>
    Normalization = 5,

    /// <summary>
    /// Assessing quality and confidence.
    /// </summary>
    QualityAssessment = 6,

    /// <summary>
    /// Processing completed successfully.
    /// </summary>
    Completed = 7,

    /// <summary>
    /// Processing failed with errors.
    /// </summary>
    Failed = 8
}

/// <summary>
/// Configuration settings for the document processing pipeline.
/// </summary>
public class PipelineConfiguration
{
    /// <summary>
    /// Configuration for document classification.
    /// </summary>
    public ClassificationConfiguration Classification { get; set; } = new();

    /// <summary>
    /// Configuration for document extraction.
    /// </summary>
    public ExtractionConfiguration Extraction { get; set; } = new();

    /// <summary>
    /// Configuration for quality assessment.
    /// </summary>
    public QualityAssessmentConfiguration QualityAssessment { get; set; } = new();

    /// <summary>
    /// General pipeline settings.
    /// </summary>
    public PipelineSettings Settings { get; set; } = new();
}

/// <summary>
/// Configuration for document classification stage.
/// </summary>
public class ClassificationConfiguration
{
    /// <summary>
    /// ID of the classification model to use.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Minimum confidence required for automatic routing.
    /// </summary>
    public double MinimumConfidenceForAutoRouting { get; set; } = 0.8;

    /// <summary>
    /// Whether to allow manual override of classification.
    /// </summary>
    public bool AllowManualOverride { get; set; } = true;
}

/// <summary>
/// Configuration for document extraction stage.
/// </summary>
public class ExtractionConfiguration
{
    /// <summary>
    /// Model ID for check extraction (typically "prebuilt-check.us").
    /// </summary>
    public string CheckModelId { get; set; } = "prebuilt-check.us";

    /// <summary>
    /// Default model ID for deposit slip extraction.
    /// </summary>
    public string DefaultDepositSlipModelId { get; set; } = string.Empty;

    /// <summary>
    /// Timeout for extraction operations.
    /// </summary>
    public TimeSpan ExtractionTimeout { get; set; } = TimeSpan.FromMinutes(2);
}

/// <summary>
/// Configuration for quality assessment stage.
/// </summary>
public class QualityAssessmentConfiguration
{
    /// <summary>
    /// Confidence thresholds for assessment.
    /// </summary>
    public ConfidenceThresholds ConfidenceThresholds { get; set; } = new();

    /// <summary>
    /// Whether to perform automatic quality assessment.
    /// </summary>
    public bool EnableAutomaticAssessment { get; set; } = true;
}

/// <summary>
/// General pipeline settings.
/// </summary>
public class PipelineSettings
{
    /// <summary>
    /// Maximum file size allowed for processing (in bytes).
    /// </summary>
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Supported content types for documents.
    /// </summary>
    public List<string> SupportedContentTypes { get; set; } = new() { "image/jpeg", "image/png" };

    /// <summary>
    /// Whether to save processing results for audit purposes.
    /// </summary>
    public bool SaveProcessingResults { get; set; } = true;

    /// <summary>
    /// Default timeout for all operations.
    /// </summary>
    public TimeSpan DefaultOperationTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Result of pipeline health check.
/// </summary>
public class PipelineHealthResult
{
    /// <summary>
    /// Overall health status of the pipeline.
    /// </summary>
    public HealthStatus OverallStatus { get; set; }

    /// <summary>
    /// Health status of individual components.
    /// </summary>
    public Dictionary<string, ComponentHealthStatus> ComponentStatuses { get; set; } = new();

    /// <summary>
    /// Timestamp when the health check was performed.
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Any issues found during health check.
    /// </summary>
    public List<HealthIssue> Issues { get; set; } = new();
}

/// <summary>
/// Health status values.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// All components are healthy.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// Some components have warnings but are functional.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Some components are unhealthy but others are working.
    /// </summary>
    Degraded = 2,

    /// <summary>
    /// Critical components are not working.
    /// </summary>
    Unhealthy = 3
}

/// <summary>
/// Health status for an individual component.
/// </summary>
public class ComponentHealthStatus
{
    /// <summary>
    /// Name of the component.
    /// </summary>
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>
    /// Health status of the component.
    /// </summary>
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Response time for health check.
    /// </summary>
    public TimeSpan ResponseTime { get; set; }

    /// <summary>
    /// Additional status details.
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// Health issue found during health check.
/// </summary>
public class HealthIssue
{
    /// <summary>
    /// Component where the issue was found.
    /// </summary>
    public string Component { get; set; } = string.Empty;

    /// <summary>
    /// Description of the issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity of the issue.
    /// </summary>
    public HealthIssueSeverity Severity { get; set; }
}

/// <summary>
/// Severity levels for health issues.
/// </summary>
public enum HealthIssueSeverity
{
    /// <summary>
    /// Informational - no action needed.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning - should be addressed soon.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error - affects functionality.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical - prevents operation.
    /// </summary>
    Critical = 3
}

/// <summary>
/// Event arguments for processing started event.
/// </summary>
public class ProcessingStartedEventArgs : EventArgs
{
    /// <summary>
    /// The processing ID for the operation that started.
    /// </summary>
    public string ProcessingId { get; set; } = string.Empty;

    /// <summary>
    /// The document being processed.
    /// </summary>
    public DocumentInput Document { get; set; } = null!;

    /// <summary>
    /// Timestamp when processing started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event arguments for processing completed event.
/// </summary>
public class ProcessingCompletedEventArgs : EventArgs
{
    /// <summary>
    /// The processing ID for the operation that completed.
    /// </summary>
    public string ProcessingId { get; set; } = string.Empty;

    /// <summary>
    /// The complete processing result.
    /// </summary>
    public ProcessingResult Result { get; set; } = null!;

    /// <summary>
    /// Whether the processing completed successfully.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Timestamp when processing completed.
    /// </summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event arguments for stage completed event.
/// </summary>
public class StageCompletedEventArgs : EventArgs
{
    /// <summary>
    /// The processing ID for the operation.
    /// </summary>
    public string ProcessingId { get; set; } = string.Empty;

    /// <summary>
    /// The stage that was completed.
    /// </summary>
    public ProcessingStage Stage { get; set; }

    /// <summary>
    /// Time taken for this stage.
    /// </summary>
    public TimeSpan StageDuration { get; set; }

    /// <summary>
    /// Result of the completed stage.
    /// </summary>
    public object? StageResult { get; set; }

    /// <summary>
    /// Timestamp when the stage completed.
    /// </summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}