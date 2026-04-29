using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.Services;

/// <summary>
/// Classifies document inputs into supported financial document types for downstream routing.
/// </summary>
public class DocumentClassifierService : IDocumentClassifierService
{
    private readonly ILogger<DocumentClassifierService> _logger;
    private ConfidenceThresholds _thresholds = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentClassifierService"/> class.
    /// </summary>
    /// <param name="logger">Logger used for classifier diagnostics.</param>
    public DocumentClassifierService(ILogger<DocumentClassifierService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ClassificationResult> ClassifyDocumentAsync(DocumentInput document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(80, cancellationToken);

        var fileName = document.FileName ?? string.Empty;
        var fileHint = fileName.ToLowerInvariant();

        var type = fileHint.Contains("deposit") || fileHint.Contains("slip")
            ? DocumentType.DepositSlip
            : fileHint.Contains("check")
                ? DocumentType.BankCheck
                : DocumentType.Unknown;

        var confidence = type switch
        {
            DocumentType.DepositSlip => 0.89,
            DocumentType.BankCheck => 0.84,
            _ => 0.52
        };

        var result = new ClassificationResult
        {
            DocumentId = document.Id,
            DocumentType = type,
            Confidence = confidence,
            ModelVersion = "demo-classifier-v1",
            ProcessingTime = TimeSpan.FromMilliseconds(80),
            ReasoningDetails = type switch
            {
                DocumentType.DepositSlip => "Detected multi-row deposit layout and subtotal sections.",
                DocumentType.BankCheck => "Detected MICR line and check payee/date regions.",
                _ => "Unable to confidently detect known layout patterns."
            },
            Alternatives =
            [
                new AlternativeClassification
                {
                    DocumentType = type == DocumentType.DepositSlip ? DocumentType.BankCheck : DocumentType.DepositSlip,
                    Confidence = type == DocumentType.Unknown ? 0.31 : 0.12,
                    Reasoning = "Secondary matching features found."
                },
                new AlternativeClassification
                {
                    DocumentType = DocumentType.Other,
                    Confidence = type == DocumentType.Unknown ? 0.17 : 0.04,
                    Reasoning = "Generic fallback class."
                }
            ]
        };

        if (confidence < _thresholds.MediumConfidenceThreshold)
        {
            result.Warnings.Add(new ProcessingWarning
            {
                Severity = WarningSeverity.Warning,
                Message = "Classification confidence below medium threshold."
            });
        }

        _logger.LogInformation("Document {DocumentId} classified as {Type} ({Confidence:P1})", document.Id, result.DocumentType, result.Confidence);
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ClassificationResult>> BatchClassifyAsync(IEnumerable<DocumentInput> documents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var results = new List<ClassificationResult>();
        foreach (var document in documents)
        {
            results.Add(await ClassifyDocumentAsync(document, cancellationToken));
        }

        return results;
    }

    /// <inheritdoc />
    public Task<ClassifierModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default)
    {
        var info = new ClassifierModelInfo
        {
            ModelId = "demo-classifier",
            ModelName = "Demo Financial Document Classifier",
            Version = "1.0.0",
            CreatedAt = DateTime.UtcNow.AddMonths(-1),
            SupportedDocumentTypes = [DocumentType.BankCheck, DocumentType.DepositSlip, DocumentType.Other],
            Description = "Heuristic classifier for demo and integration scenarios."
        };

        return Task.FromResult(info);
    }

    /// <inheritdoc />
    public Task<bool> ValidateModelAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public ConfidenceThresholds GetConfidenceThresholds() => _thresholds;

    /// <inheritdoc />
    public void SetConfidenceThresholds(ConfidenceThresholds thresholds)
    {
        ArgumentNullException.ThrowIfNull(thresholds);
        _thresholds = thresholds;
    }
}
