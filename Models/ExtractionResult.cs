using System.ComponentModel.DataAnnotations;

namespace CheckYoSelfAI.Models;

/// <summary>
/// Represents the raw extraction result from Azure Document Intelligence or other extraction models.
/// Contains field-level extraction data with confidence scoring and processing metadata.
/// </summary>
public class ExtractionResult
{
    /// <summary>
    /// The unique identifier of the document that was processed.
    /// </summary>
    [Required]
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the extraction model used (e.g., "prebuilt-check.us", custom model ID).
    /// </summary>
    [Required]
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// The type of document that was processed.
    /// </summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// Dictionary of extracted fields with their values and confidence scores.
    /// Key is the field name, value contains the extracted data and metadata.
    /// </summary>
    public Dictionary<string, FieldValue> ExtractedFields { get; set; } = new();

    /// <summary>
    /// Overall confidence score for the entire extraction (0.0 to 1.0).
    /// Typically represents an average or weighted confidence across all fields.
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Overall confidence must be between 0.0 and 1.0")]
    public double OverallConfidence { get; set; }

    /// <summary>
    /// The raw JSON response from the extraction service.
    /// Useful for debugging, detailed analysis, or future reprocessing.
    /// </summary>
    public string? RawResponse { get; set; }

    /// <summary>
    /// List of warnings or issues encountered during extraction.
    /// </summary>
    public List<ProcessingWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Time taken to perform the extraction.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Timestamp when the extraction was completed.
    /// </summary>
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Version of the extraction model used.
    /// </summary>
    public string? ModelVersion { get; set; }

    /// <summary>
    /// Gets the value of a specific field if it exists and meets confidence threshold.
    /// </summary>
    /// <param name="fieldName">Name of the field to retrieve</param>
    /// <param name="minimumConfidence">Minimum confidence required (default: 0.5)</param>
    /// <returns>The field value if found and confident enough, null otherwise</returns>
    public string? GetFieldValue(string fieldName, double minimumConfidence = 0.5)
    {
        if (ExtractedFields.TryGetValue(fieldName, out var fieldValue) &&
            fieldValue.Confidence >= minimumConfidence)
        {
            return fieldValue.Value;
        }
        return null;
    }

    /// <summary>
    /// Gets all fields that meet or exceed the specified confidence threshold.
    /// </summary>
    /// <param name="minimumConfidence">Minimum confidence threshold (default: 0.7)</param>
    /// <returns>Dictionary of high-confidence fields</returns>
    public Dictionary<string, FieldValue> GetHighConfidenceFields(double minimumConfidence = 0.7)
    {
        return ExtractedFields
            .Where(kvp => kvp.Value.Confidence >= minimumConfidence)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Gets all fields that fall below the specified confidence threshold.
    /// </summary>
    /// <param name="maximumConfidence">Maximum confidence threshold (default: 0.5)</param>
    /// <returns>Dictionary of low-confidence fields that may need review</returns>
    public Dictionary<string, FieldValue> GetLowConfidenceFields(double maximumConfidence = 0.5)
    {
        return ExtractedFields
            .Where(kvp => kvp.Value.Confidence < maximumConfidence)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Determines if the extraction quality is sufficient for automatic processing.
    /// </summary>
    /// <param name="minimumOverallConfidence">Minimum overall confidence required (default: 0.8)</param>
    /// <param name="criticalFields">Fields that must be present for processing</param>
    public bool IsHighQuality(double minimumOverallConfidence = 0.8, params string[] criticalFields)
    {
        // Check overall confidence
        if (OverallConfidence < minimumOverallConfidence)
            return false;

        // Check for critical errors
        if (Warnings.Any(w => w.Severity >= WarningSeverity.Critical))
            return false;

        // Check that all critical fields are present and confident
        foreach (var field in criticalFields)
        {
            if (!ExtractedFields.ContainsKey(field) || 
                ExtractedFields[field].Confidence < minimumOverallConfidence)
            {
                return false;
            }
        }

        return true;
    }
}