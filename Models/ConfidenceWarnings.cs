using System.ComponentModel.DataAnnotations;

namespace CheckYoSelfAI.Models;

/// <summary>
/// Represents confidence assessment and warnings for document processing results.
/// This DTO is used to communicate processing quality, confidence levels, and recommended actions.
/// </summary>
public class ConfidenceWarnings
{
    /// <summary>
    /// The document ID this confidence assessment relates to.
    /// </summary>
    [Required]
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Overall confidence level for the entire document processing pipeline.
    /// </summary>
    public ConfidenceLevel OverallConfidenceLevel { get; set; }

    /// <summary>
    /// Numerical confidence score (0.0 to 1.0) for the overall processing.
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Confidence score must be between 0.0 and 1.0")]
    public double OverallConfidenceScore { get; set; }

    /// <summary>
    /// Confidence assessment for the classification step.
    /// </summary>
    public StageConfidence ClassificationConfidence { get; set; } = new();

    /// <summary>
    /// Confidence assessment for the extraction step.
    /// </summary>
    public StageConfidence ExtractionConfidence { get; set; } = new();

    /// <summary>
    /// Confidence assessment for the normalization step.
    /// </summary>
    public StageConfidence NormalizationConfidence { get; set; } = new();

    /// <summary>
    /// List of all warnings generated during processing.
    /// </summary>
    public List<ProcessingWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Recommended actions based on confidence levels and warnings.
    /// </summary>
    public List<RecommendedAction> RecommendedActions { get; set; } = new();

    /// <summary>
    /// Threshold configurations used for confidence assessment.
    /// </summary>
    public ConfidenceThresholds Thresholds { get; set; } = new();

    /// <summary>
    /// Timestamp when this confidence assessment was generated.
    /// </summary>
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Determines if the processing results are acceptable for automatic processing.
    /// </summary>
    public bool IsAcceptableForAutoProcessing()
    {
        return OverallConfidenceLevel >= ConfidenceLevel.High &&
               !Warnings.Any(w => w.Severity >= WarningSeverity.Critical) &&
               ClassificationConfidence.Level >= ConfidenceLevel.Medium &&
               ExtractionConfidence.Level >= ConfidenceLevel.Medium;
    }

    /// <summary>
    /// Determines if manual review is required based on confidence and warnings.
    /// </summary>
    public bool RequiresManualReview()
    {
        return OverallConfidenceLevel <= ConfidenceLevel.Low ||
               Warnings.Any(w => w.Severity >= WarningSeverity.Error) ||
               RecommendedActions.Any(a => a.ActionType == ActionType.ManualReview || 
                                          a.ActionType == ActionType.Reject);
    }

    /// <summary>
    /// Gets a human-readable summary of the confidence assessment.
    /// </summary>
    public string GetSummary()
    {
        var summary = $"Overall confidence: {OverallConfidenceLevel} ({OverallConfidenceScore:P1})";

        if (Warnings.Any())
        {
            var criticalCount = Warnings.Count(w => w.Severity >= WarningSeverity.Critical);
            var errorCount = Warnings.Count(w => w.Severity == WarningSeverity.Error);
            var warningCount = Warnings.Count(w => w.Severity == WarningSeverity.Warning);

            summary += $" - {criticalCount} critical, {errorCount} errors, {warningCount} warnings";
        }

        if (RecommendedActions.Any())
        {
            var primaryAction = RecommendedActions.First();
            summary += $" - Recommended: {primaryAction.ActionType}";
        }

        return summary;
    }
}

/// <summary>
/// Confidence levels for processing stages and overall assessment.
/// </summary>
public enum ConfidenceLevel
{
    /// <summary>
    /// Very low confidence - likely requires reprocessing or rejection.
    /// </summary>
    VeryLow = 0,

    /// <summary>
    /// Low confidence - requires manual review.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium confidence - may proceed with caution or review.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High confidence - suitable for automatic processing.
    /// </summary>
    High = 3,

    /// <summary>
    /// Very high confidence - excellent quality processing.
    /// </summary>
    VeryHigh = 4
}

/// <summary>
/// Confidence assessment for a specific processing stage.
/// </summary>
public class StageConfidence
{
    /// <summary>
    /// The confidence level for this processing stage.
    /// </summary>
    public ConfidenceLevel Level { get; set; }

    /// <summary>
    /// Numerical confidence score (0.0 to 1.0) for this stage.
    /// </summary>
    [Range(0.0, 1.0)]
    public double Score { get; set; }

    /// <summary>
    /// Specific issues or concerns for this processing stage.
    /// </summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>
    /// Additional details about the confidence assessment.
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// Recommended actions based on confidence assessment.
/// </summary>
public class RecommendedAction
{
    /// <summary>
    /// The type of action recommended.
    /// </summary>
    public ActionType ActionType { get; set; }

    /// <summary>
    /// Priority level for this action.
    /// </summary>
    public ActionPriority Priority { get; set; }

    /// <summary>
    /// Description of the recommended action.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Reason why this action is recommended.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Specific fields or areas this action relates to.
    /// </summary>
    public List<string> RelatedFields { get; set; } = new();
}

/// <summary>
/// Types of actions that can be recommended based on confidence assessment.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Proceed with automatic processing - no human intervention needed.
    /// </summary>
    Proceed = 0,

    /// <summary>
    /// Review specific fields or aspects of the processing.
    /// </summary>
    ReviewFields = 1,

    /// <summary>
    /// Manual review of the entire document is recommended.
    /// </summary>
    ManualReview = 2,

    /// <summary>
    /// Re-capture or re-scan the document for better quality.
    /// </summary>
    Recapture = 3,

    /// <summary>
    /// Reject the document - quality is too poor for processing.
    /// </summary>
    Reject = 4
}

/// <summary>
/// Priority levels for recommended actions.
/// </summary>
public enum ActionPriority
{
    /// <summary>
    /// Low priority - action can be taken when convenient.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium priority - action should be taken reasonably soon.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High priority - action should be taken immediately.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority - processing cannot continue without this action.
    /// </summary>
    Critical = 3
}

/// <summary>
/// Configuration thresholds used for confidence assessment.
/// </summary>
public class ConfidenceThresholds
{
    /// <summary>
    /// Minimum confidence for high confidence classification (default: 0.8).
    /// </summary>
    [Range(0.0, 1.0)]
    public double HighConfidenceThreshold { get; set; } = 0.8;

    /// <summary>
    /// Minimum confidence for medium confidence classification (default: 0.6).
    /// </summary>
    [Range(0.0, 1.0)]
    public double MediumConfidenceThreshold { get; set; } = 0.6;

    /// <summary>
    /// Minimum confidence for low confidence classification (default: 0.4).
    /// </summary>
    [Range(0.0, 1.0)]
    public double LowConfidenceThreshold { get; set; } = 0.4;

    /// <summary>
    /// Minimum confidence required for automatic processing (default: 0.75).
    /// </summary>
    [Range(0.0, 1.0)]
    public double AutoProcessingThreshold { get; set; } = 0.75;

    /// <summary>
    /// Determines the confidence level for a given score.
    /// </summary>
    public ConfidenceLevel GetConfidenceLevel(double score)
    {
        if (score >= HighConfidenceThreshold + 0.1) return ConfidenceLevel.VeryHigh;
        if (score >= HighConfidenceThreshold) return ConfidenceLevel.High;
        if (score >= MediumConfidenceThreshold) return ConfidenceLevel.Medium;
        if (score >= LowConfidenceThreshold) return ConfidenceLevel.Low;
        return ConfidenceLevel.VeryLow;
    }
}