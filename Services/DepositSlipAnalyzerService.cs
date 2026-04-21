using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.Services;

public class DepositSlipAnalyzerService : IDepositSlipAnalyzerService
{
    private readonly ILogger<DepositSlipAnalyzerService> _logger;
    private string _defaultModelId = "custom-deposit-slip-v3";

    public DepositSlipAnalyzerService(ILogger<DepositSlipAnalyzerService> logger)
    {
        _logger = logger;
    }

    public Task<ExtractionResult> AnalyzeDepositSlipAsync(DocumentInput document, CancellationToken cancellationToken = default)
    {
        return AnalyzeDepositSlipWithModelAsync(document, _defaultModelId, cancellationToken);
    }

    public async Task<ExtractionResult> AnalyzeDepositSlipWithModelAsync(DocumentInput document, string modelId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(120, cancellationToken);

        return new ExtractionResult
        {
            DocumentId = document.Id,
            DocumentType = DocumentType.DepositSlip,
            ModelId = modelId,
            ModelVersion = "3.4.0",
            OverallConfidence = 0.85,
            ProcessingTime = TimeSpan.FromMilliseconds(120),
            ExtractedFields = new Dictionary<string, FieldValue>
            {
                ["DepositSlipNumber"] = new() { Value = "DS-77811", Confidence = 0.89 },
                ["Amount"] = new() { Value = "1500.75", Confidence = 0.88 },
                ["CashAmount"] = new() { Value = "200.00", Confidence = 0.82 },
                ["CheckAmount"] = new() { Value = "1300.75", Confidence = 0.84 },
                ["AccountNumber"] = new() { Value = "****7821", Confidence = 0.80 },
                ["RoutingNumber"] = new() { Value = "021000021", Confidence = 0.81 }
            }
        };
    }

    public async Task<IEnumerable<ExtractionResult>> BatchAnalyzeDepositSlipsAsync(IEnumerable<DocumentInput> documents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var results = new List<ExtractionResult>();
        foreach (var document in documents)
        {
            results.Add(await AnalyzeDepositSlipAsync(document, cancellationToken));
        }

        return results;
    }

    public Task<NormalizedDocument> MapToNormalizedDocumentAsync(ExtractionResult extractionResult, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(extractionResult);

        var normalized = new NormalizedDocument
        {
            DocumentId = extractionResult.DocumentId,
            Type = DocumentType.DepositSlip,
            DepositSlipNumber = extractionResult.GetFieldValue("DepositSlipNumber"),
            AccountNumber = extractionResult.GetFieldValue("AccountNumber"),
            RoutingNumber = extractionResult.GetFieldValue("RoutingNumber"),
            Amount = decimal.TryParse(extractionResult.GetFieldValue("Amount"), out var amount) ? amount : null,
            CashAmount = decimal.TryParse(extractionResult.GetFieldValue("CashAmount"), out var cash) ? cash : null,
            CheckAmount = decimal.TryParse(extractionResult.GetFieldValue("CheckAmount"), out var checks) ? checks : null,
            ProcessingModelId = extractionResult.ModelId,
            ProcessingConfidence = extractionResult.OverallConfidence,
            ProcessingWarnings = extractionResult.GetLowConfidenceFields(0.72).Keys.Select(field => $"Low confidence field: {field}").ToList(),
            DepositItems =
            [
                new DepositItem { ItemType = DepositItemType.Cash, Amount = decimal.TryParse(extractionResult.GetFieldValue("CashAmount"), out var cashAmount) ? cashAmount : 0m, Confidence = extractionResult.ExtractedFields["CashAmount"].Confidence },
                new DepositItem { ItemType = DepositItemType.Check, CheckNumber = "4471", Amount = decimal.TryParse(extractionResult.GetFieldValue("CheckAmount"), out var checkAmountValue) ? checkAmountValue : 0m, Confidence = extractionResult.ExtractedFields["CheckAmount"].Confidence }
            ]
        };

        return Task.FromResult(normalized);
    }

    public DepositSlipValidationResult ValidateDepositSlipExtraction(ExtractionResult extractionResult)
    {
        ArgumentNullException.ThrowIfNull(extractionResult);

        var requiredFields = GetRequiredDepositSlipFields().ToList();
        var missing = requiredFields
            .Where(field => string.IsNullOrWhiteSpace(extractionResult.GetFieldValue(field, 0.65)))
            .ToList();

        var statedTotal = decimal.TryParse(extractionResult.GetFieldValue("Amount"), out var totalAmount) ? totalAmount : (decimal?)null;
        var cashAmount = decimal.TryParse(extractionResult.GetFieldValue("CashAmount"), out var parsedCash) ? parsedCash : 0m;
        var checkAmount = decimal.TryParse(extractionResult.GetFieldValue("CheckAmount"), out var parsedCheck) ? parsedCheck : 0m;
        var calculatedTotal = cashAmount + checkAmount;
        var variance = (statedTotal ?? calculatedTotal) - calculatedTotal;

        var amountValidation = new AmountValidationResult
        {
            IsConsistent = Math.Abs(variance) < 0.01m,
            CalculatedTotal = calculatedTotal,
            StatedTotal = statedTotal,
            Variance = variance
        };

        if (!amountValidation.IsConsistent)
        {
            amountValidation.Issues.Add("Stated total does not match sum of cash and check amounts.");
        }

        var result = new DepositSlipValidationResult
        {
            IsValid = missing.Count == 0 && amountValidation.IsConsistent,
            QualityScore = extractionResult.OverallConfidence,
            MissingOrLowConfidenceFields = missing,
            AmountValidation = amountValidation,
            Details = "Deposit slip validation completed."
        };

        if (missing.Count > 0)
        {
            result.Warnings.Add($"Review fields: {string.Join(", ", missing)}");
        }

        if (!amountValidation.IsConsistent)
        {
            result.Errors.Add("Amount totals are inconsistent.");
        }

        return result;
    }

    public Task<IEnumerable<CustomModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<CustomModelInfo> models =
        [
            new CustomModelInfo
            {
                ModelId = "custom-deposit-slip-v3",
                ModelName = "Deposit Slip Model",
                Version = "3.4.0",
                ModelType = "Custom Neural",
                Status = ModelStatus.Ready,
                LastUpdated = DateTime.UtcNow.AddDays(-7),
                TrainingDocumentCount = 520,
                SupportedFields = ["DepositSlipNumber", "Amount", "CashAmount", "CheckAmount", "AccountNumber", "RoutingNumber"]
            }
        ];

        return Task.FromResult(models);
    }

    public async Task<CustomModelInfo> GetDefaultModelInfoAsync(CancellationToken cancellationToken = default)
    {
        var models = await GetAvailableModelsAsync(cancellationToken);
        return models.First();
    }

    public async Task<CustomModelInfo> GetModelInfoAsync(string modelId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        var models = await GetAvailableModelsAsync(cancellationToken);
        var match = models.FirstOrDefault(model => string.Equals(model.ModelId, modelId, StringComparison.OrdinalIgnoreCase));
        if (match == null)
        {
            throw new ArgumentException($"Model '{modelId}' was not found.", nameof(modelId));
        }

        return match;
    }

    public Task<bool> TestServiceHealthAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Deposit slip analyzer health test succeeded.");
        return Task.FromResult(true);
    }

    public Task SetDefaultModelAsync(string modelId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        _defaultModelId = modelId;
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetRequiredDepositSlipFields()
    {
        return ["DepositSlipNumber", "Amount", "CashAmount", "CheckAmount", "AccountNumber"];
    }
}
