using System.ComponentModel.DataAnnotations;

namespace check_yo_self_ai.Models;

/// <summary>
/// Represents a document input for processing in the document pipeline.
/// Contains the document content, metadata, and processing information.
/// </summary>
public class DocumentInput
{
    /// <summary>
    /// Unique identifier for the document processing session.
    /// </summary>
    [Required]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The document content as a stream (image data).
    /// </summary>
    [Required]
    public Stream Content { get; set; } = null!;

    /// <summary>
    /// Original filename of the document.
    /// </summary>
    [Required]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME content type of the document (e.g., "image/jpeg", "image/png").
    /// </summary>
    [Required]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Size of the document content in bytes.
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "File size must be greater than 0")]
    public long FileSize { get; set; }

    /// <summary>
    /// Timestamp when the document was uploaded to the system.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata associated with the document.
    /// Can include source information, user context, etc.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Validates that the document input has all required properties.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Id) &&
               Content != null &&
               !string.IsNullOrEmpty(FileName) &&
               !string.IsNullOrEmpty(ContentType) &&
               FileSize > 0;
    }

    /// <summary>
    /// Creates a copy of the DocumentInput for processing while preserving the original.
    /// Note: Stream content is not copied - both instances will reference the same stream.
    /// </summary>
    public DocumentInput Clone()
    {
        return new DocumentInput
        {
            Id = Id,
            Content = Content,
            FileName = FileName,
            ContentType = ContentType,
            FileSize = FileSize,
            UploadedAt = UploadedAt,
            Metadata = new Dictionary<string, string>(Metadata)
        };
    }
}