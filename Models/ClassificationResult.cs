using System.ComponentModel.DataAnnotations;

namespace CheckYoSelfAI.Models;

/// <summary>
/// Represents the result of document classification in the processing pipeline.
/// Contains the identified document type, confidence scoring, and alternative options.
/// </summary>
public class ClassificationResult
{
    /// <summary>
    /// The unique identifier of the document that was classified.
    /// </summary>
    [Required]
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// The primary document type identified by the classifier.
    /// </summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// Confidence score for the primary classification (0.0 to 1.0).
    /// Higher values indicate greater confidence in the classification.
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Confidence must be between 0.0 and 1.0")]
    public double Confidence { get; set; }

    /// <summary>
    /// Version of the classification model used for this result.
    /// </summary>
    public string? ModelVersion { get; set; }

    /// <summary>
    /// Alternative classification possibilities with their confidence scores.
    /// Useful for cases where the primary classification has lower confidence.
    /// </summary>
    public List<AlternativeClassification> Alternatives { get; set; } = new();

    /// <summary>
    /// Detailed reasoning or evidence that led to this classification decision.
    /// May include features detected, patterns recognized, etc.
    /// </summary>
    public string? ReasoningDetails { get; set; }

    /// <summary>
    /// Time taken to perform the classification.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Timestamp when the classification was completed.
    /// </summary>
    public DateTime ClassifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Any warnings or issues encountered during classification.
    /// </summary>
    public List<ProcessingWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Indicates whether the classification confidence is above acceptable thresholds.
    /// </summary>
    /// <param name="minimumConfidence">Minimum confidence threshold (default: 0.7)</param>
    public bool IsHighConfidence(double minimumConfidence = 0.7)
    {
        return Confidence >= minimumConfidence;
    }

    /// <summary>
    /// Gets the best alternative classification if the primary confidence is low.
    /// </summary>
    public AlternativeClassification? GetBestAlternative()
    {
        return Alternatives
            .OrderByDescending(a => a.Confidence)
            .FirstOrDefault();
    }

    /// <summary>
    /// Determines if manual review is recommended based on confidence scores.
    /// </summary>
    /// <param name="reviewThreshold">Confidence threshold below which review is recommended (default: 0.8)</param>
    public bool RequiresManualReview(double reviewThreshold = 0.8)
    {
        return Confidence < reviewThreshold || 
               DocumentType == DocumentType.Unknown ||
               Warnings.Any(w => w.Severity >= WarningSeverity.Error);
    }
}