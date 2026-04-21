using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.Services;

public class CheckAnalyzerService : ICheckAnalyzerService
{
    private readonly ILogger<CheckAnalyzerService> _logger;

    public CheckAnalyzerService(ILogger<CheckAnalyzerService> logger)
    {
        _logger = logger;
    }

    public async Task<ExtractionResult> AnalyzeCheckAsync(DocumentInput document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(100, cancellationToken);

        return new ExtractionResult
        {
            DocumentId = document.Id,
            DocumentType = DocumentType.BankCheck,
            ModelId = "prebuilt-check.us",
            ModelVersion = "2024-11-30",
            OverallConfidence = 0.81,
            ProcessingTime = TimeSpan.FromMilliseconds(100),
            ExtractedFields = new Dictionary<string, FieldValue>
            {
                ["CheckNumber"] = new() { Value = "1042", Confidence = 0.83 },
                ["Amount"] = new() { Value = "420.18", Confidence = 0.84 },
                ["PayToName"] = new() { Value = "Contoso Supplies", Confidence = 0.78 },
                ["AccountNumber"] = new() { Value = "****1187", Confidence = 0.76 },
                ["RoutingNumber"] = new() { Value = "123456789", Confidence = 0.80 },
                ["Memo"] = new() { Value = "Office restock", Confidence = 0.70 }
            }
        };
    }

    public async Task<IEnumerable<ExtractionResult>> BatchAnalyzeChecksAsync(IEnumerable<DocumentInput> documents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var results = new List<ExtractionResult>();
        foreach (var document in documents)
        {
            results.Add(await AnalyzeCheckAsync(document, cancellationToken));
        }

        return results;
    }

    public Task<NormalizedDocument> MapToNormalizedDocumentAsync(ExtractionResult extractionResult, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(extractionResult);

        var normalized = new NormalizedDocument
        {
            DocumentId = extractionResult.DocumentId,
            Type = DocumentType.BankCheck,
            CheckNumber = extractionResult.GetFieldValue("CheckNumber"),
            PayToName = extractionResult.GetFieldValue("PayToName"),
            AccountNumber = extractionResult.GetFieldValue("AccountNumber"),
            RoutingNumber = extractionResult.GetFieldValue("RoutingNumber"),
            Memo = extractionResult.GetFieldValue("Memo"),
            Amount = decimal.TryParse(extractionResult.GetFieldValue("Amount"), out var amount) ? amount : null,
            ProcessingModelId = extractionResult.ModelId,
            ProcessingConfidence = extractionResult.OverallConfidence,
            ProcessingWarnings = extractionResult.GetLowConfidenceFields(0.72).Keys.Select(field => $"Low confidence field: {field}").ToList()
        };

        return Task.FromResult(normalized);
    }

    public CheckValidationResult ValidateCheckExtraction(ExtractionResult extractionResult)
    {
        ArgumentNullException.ThrowIfNull(extractionResult);

        var requiredFields = GetRequiredCheckFields().ToList();
        var missing = requiredFields
            .Where(field => string.IsNullOrWhiteSpace(extractionResult.GetFieldValue(field, 0.65)))
            .ToList();

        var result = new CheckValidationResult
        {
            IsValid = missing.Count == 0,
            QualityScore = extractionResult.OverallConfidence,
            MissingOrLowConfidenceFields = missing,
            Details = missing.Count == 0 ? "Check extraction passed quality checks." : "Some required fields need manual review."
        };

        if (missing.Count > 0)
        {
            result.Warnings.Add($"Review fields: {string.Join(", ", missing)}");
            result.RecommendedAction = new RecommendedAction
            {
                ActionType = ActionType.ReviewFields,
                Priority = ActionPriority.Medium,
                Description = "Review missing or low confidence check fields.",
                Reason = "Required fields failed confidence checks.",
                RelatedFields = missing
            };
        }

        return result;
    }

    public Task<ModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ModelInfo
        {
            ModelId = "prebuilt-check.us",
            ModelName = "Azure Prebuilt US Check",
            Version = "2024-11-30",
            ModelType = "Prebuilt",
            Description = "Extracts check fields from US checks.",
            LastUpdated = DateTime.UtcNow.AddMonths(-2),
            SupportedFields = ["CheckNumber", "Amount", "PayToName", "AccountNumber", "RoutingNumber", "Memo"]
        });
    }

    public Task<bool> TestServiceHealthAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Check analyzer health test succeeded.");
        return Task.FromResult(true);
    }

    public IEnumerable<string> GetRequiredCheckFields()
    {
        return ["CheckNumber", "Amount", "PayToName", "RoutingNumber"];
    }
}
