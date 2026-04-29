namespace CheckYoSelfAI.Models;

/// <summary>
/// Represents the type of financial document being processed.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Document type could not be determined or is not supported.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// US bank check document.
    /// </summary>
    BankCheck = 1,

    /// <summary>
    /// Bank deposit slip document.
    /// </summary>
    DepositSlip = 2,

    /// <summary>
    /// Other financial document type not specifically supported.
    /// </summary>
    Other = 99
}

/// <summary>
/// Represents an alternative classification result with lower confidence.
/// </summary>
public class AlternativeClassification
{
    /// <summary>
    /// The alternative document type identified.
    /// </summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// Confidence score for this alternative classification (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Reasoning or evidence for this alternative classification.
    /// </summary>
    public string? Reasoning { get; set; }
}

/// <summary>
/// Represents a field value extracted from a document with confidence scoring.
/// </summary>
public class FieldValue
{
    /// <summary>
    /// The extracted value as a string.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Confidence score for this field extraction (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Bounding box coordinates for where this field was found in the document.
    /// </summary>
    public BoundingBox? BoundingBox { get; set; }

    /// <summary>
    /// The data type of this field (text, number, date, etc.).
    /// </summary>
    public string? DataType { get; set; }
}

/// <summary>
/// Represents a bounding box for field location in a document.
/// </summary>
public class BoundingBox
{
    /// <summary>
    /// X coordinate of the top-left corner.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate of the top-left corner.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Width of the bounding box.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Height of the bounding box.
    /// </summary>
    public double Height { get; set; }
}

/// <summary>
/// Represents a processing warning or issue during document analysis.
/// </summary>
public class ProcessingWarning
{
    /// <summary>
    /// The severity level of the warning.
    /// </summary>
    public WarningSeverity Severity { get; set; }

    /// <summary>
    /// A descriptive message about the warning.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The field or area of the document this warning relates to.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Additional details about the warning.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp when the warning was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Severity levels for processing warnings.
/// </summary>
public enum WarningSeverity
{
    /// <summary>
    /// Informational message, processing can continue normally.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning that may affect accuracy but processing can continue.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error that significantly affects processing quality.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical error that prevents successful processing.
    /// </summary>
    Critical = 3
}