using System.ComponentModel.DataAnnotations;

namespace check_yo_self_ai.Models;

/// <summary>
/// Represents a deposit item (check or cash) on a deposit slip.
/// </summary>
public class DepositItem
{
    /// <summary>
    /// Unique identifier for this deposit item.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of deposit item (check or cash).
    /// </summary>
    public DepositItemType ItemType { get; set; }

    /// <summary>
    /// Check number if this is a check deposit item.
    /// </summary>
    public string? CheckNumber { get; set; }

    /// <summary>
    /// Amount of this deposit item.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Bank routing number for check deposits.
    /// </summary>
    public string? BankRoutingNumber { get; set; }

    /// <summary>
    /// Bank name for check deposits.
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Confidence score for the extraction of this deposit item (0.0 to 1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    public double Confidence { get; set; }

    /// <summary>
    /// Any notes or memo associated with this deposit item.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Validation for deposit item data.
    /// </summary>
    public bool IsValid()
    {
        return Amount >= 0 &&
               (ItemType == DepositItemType.Cash || !string.IsNullOrEmpty(CheckNumber));
    }
}

/// <summary>
/// Types of deposit items that can appear on a deposit slip.
/// </summary>
public enum DepositItemType
{
    /// <summary>
    /// Cash deposit.
    /// </summary>
    Cash = 0,

    /// <summary>
    /// Check deposit.
    /// </summary>
    Check = 1
}

/// <summary>
/// Represents the processing result for the entire document pipeline.
/// </summary>
public class ProcessingResult
{
    /// <summary>
    /// Unique identifier for this processing session.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The original document input.
    /// </summary>
    public DocumentInput? DocumentInput { get; set; }

    /// <summary>
    /// Result of the classification step.
    /// </summary>
    public ClassificationResult? ClassificationResult { get; set; }

    /// <summary>
    /// Result of the extraction step.
    /// </summary>
    public ExtractionResult? ExtractionResult { get; set; }

    /// <summary>
    /// The final normalized document.
    /// </summary>
    public NormalizedDocument? NormalizedDocument { get; set; }

    /// <summary>
    /// Overall status of the processing pipeline.
    /// </summary>
    public ProcessingStatus Status { get; set; } = ProcessingStatus.NotStarted;

    /// <summary>
    /// Any errors that occurred during processing.
    /// </summary>
    public List<ProcessingError> Errors { get; set; } = new();

    /// <summary>
    /// Total time taken for the entire processing pipeline.
    /// </summary>
    public TimeSpan TotalProcessingTime { get; set; }

    /// <summary>
    /// Timestamp when processing started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Timestamp when processing completed (successfully or with errors).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Indicates whether the processing completed successfully.
    /// </summary>
    public bool IsSuccessful => Status == ProcessingStatus.Completed && !Errors.Any(e => e.IsCritical);

    /// <summary>
    /// Gets a summary of the processing pipeline results.
    /// </summary>
    public string GetSummary()
    {
        if (Status == ProcessingStatus.NotStarted)
            return "Processing not started";

        if (Status == ProcessingStatus.InProgress)
            return "Processing in progress...";

        if (Status == ProcessingStatus.Failed)
            return $"Processing failed: {string.Join(", ", Errors.Select(e => e.Message))}";

        if (NormalizedDocument != null)
        {
            var confidence = ExtractionResult?.OverallConfidence ?? 0;
            return $"Processed {NormalizedDocument.Type} document with {confidence:P1} confidence";
        }

        return "Processing completed";
    }
}

/// <summary>
/// Status values for document processing pipeline.
/// </summary>
public enum ProcessingStatus
{
    /// <summary>
    /// Processing has not been started.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Processing is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Processing completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Processing failed due to errors.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Processing was cancelled by user or system.
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// Represents an error that occurred during document processing.
/// </summary>
public class ProcessingError
{
    /// <summary>
    /// Error code for programmatic handling.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The stage of processing where this error occurred.
    /// </summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information for debugging.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if this error prevents further processing.
    /// </summary>
    public bool IsCritical { get; set; }
}