using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CheckYoSelfAI.Services;

public class DocumentOrchestrationService : IDocumentOrchestrationService
{
    private readonly IDocumentClassifierService _classifierService;
    private readonly ICheckAnalyzerService _checkAnalyzerService;
    private readonly IDepositSlipAnalyzerService _depositSlipAnalyzerService;
    private readonly ILogger<DocumentOrchestrationService> _logger;

    private readonly ConcurrentDictionary<string, ProcessingStatusInfo> _statusById = new();
    private PipelineConfiguration _configuration = new();

    public DocumentOrchestrationService(
        IDocumentClassifierService classifierService,
        ICheckAnalyzerService checkAnalyzerService,
        IDepositSlipAnalyzerService depositSlipAnalyzerService,
        ILogger<DocumentOrchestrationService> logger)
    {
        _classifierService = classifierService;
        _checkAnalyzerService = checkAnalyzerService;
        _depositSlipAnalyzerService = depositSlipAnalyzerService;
        _logger = logger;
    }

    public event EventHandler<ProcessingStartedEventArgs>? ProcessingStarted;
    public event EventHandler<ProcessingCompletedEventArgs>? ProcessingCompleted;
    public event EventHandler<StageCompletedEventArgs>? StageCompleted;

    public async Task<ProcessingResult> ProcessDocumentAsync(DocumentInput document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var processingId = Guid.NewGuid().ToString("N");
        var startedAt = DateTime.UtcNow;

        var result = new ProcessingResult
        {
            Id = processingId,
            DocumentInput = document,
            Status = ProcessingStatus.InProgress,
            StartedAt = startedAt
        };

        UpdateStatus(processingId, ProcessingStatus.InProgress, ProcessingStage.Validation, 5, startedAt, "Validating input document");

        ProcessingStarted?.Invoke(this, new ProcessingStartedEventArgs
        {
            ProcessingId = processingId,
            Document = document,
            StartedAt = startedAt
        });

        try
        {
            if (!document.IsValid())
            {
                throw new DocumentClassificationException("Document input is invalid for processing.", document.Id);
            }

            var stageStart = DateTime.UtcNow;
            result.ClassificationResult = await ClassifyDocumentAsync(document, cancellationToken);
            RaiseStageCompleted(processingId, ProcessingStage.Classification, stageStart, result.ClassificationResult);
            UpdateStatus(processingId, ProcessingStatus.InProgress, ProcessingStage.Routing, 35, startedAt, "Routing to extraction model");

            stageStart = DateTime.UtcNow;
            result.ExtractionResult = await ExtractDocumentAsync(document, result.ClassificationResult.DocumentType, cancellationToken);
            RaiseStageCompleted(processingId, ProcessingStage.Extraction, stageStart, result.ExtractionResult);
            UpdateStatus(processingId, ProcessingStatus.InProgress, ProcessingStage.Normalization, 70, startedAt, "Normalizing extraction result");

            stageStart = DateTime.UtcNow;
            result.NormalizedDocument = await NormalizeResultAsync(result.ExtractionResult, cancellationToken);
            RaiseStageCompleted(processingId, ProcessingStage.Normalization, stageStart, result.NormalizedDocument);

            result.Status = ProcessingStatus.Completed;
            result.CompletedAt = DateTime.UtcNow;
            result.TotalProcessingTime = result.CompletedAt.Value - result.StartedAt;

            UpdateStatus(processingId, ProcessingStatus.Completed, ProcessingStage.Completed, 100, startedAt, "Processing completed");

            ProcessingCompleted?.Invoke(this, new ProcessingCompletedEventArgs
            {
                ProcessingId = processingId,
                Result = result,
                IsSuccessful = true,
                CompletedAt = result.CompletedAt.Value
            });

            return result;
        }
        catch (Exception ex)
        {
            result.Status = ProcessingStatus.Failed;
            result.CompletedAt = DateTime.UtcNow;
            result.TotalProcessingTime = result.CompletedAt.Value - result.StartedAt;
            result.Errors.Add(new ProcessingError
            {
                Code = "PROCESSING_FAILED",
                Message = ex.Message,
                Stage = _statusById.TryGetValue(processingId, out var currentStatus)
                    ? currentStatus.CurrentStage.ToString()
                    : ProcessingStage.Failed.ToString(),
                Details = ex.ToString(),
                IsCritical = true
            });

            UpdateStatus(processingId, ProcessingStatus.Failed, ProcessingStage.Failed, 100, startedAt, ex.Message);
            _logger.LogError(ex, "Failed processing document {DocumentId}", document.Id);

            ProcessingCompleted?.Invoke(this, new ProcessingCompletedEventArgs
            {
                ProcessingId = processingId,
                Result = result,
                IsSuccessful = false,
                CompletedAt = result.CompletedAt.Value
            });

            return result;
        }
    }

    public Task<ClassificationResult> ClassifyDocumentAsync(DocumentInput document, CancellationToken cancellationToken = default)
    {
        return _classifierService.ClassifyDocumentAsync(document, cancellationToken);
    }

    public Task<ExtractionResult> ExtractDocumentAsync(DocumentInput document, DocumentType documentType, CancellationToken cancellationToken = default)
    {
        return documentType switch
        {
            DocumentType.BankCheck => _checkAnalyzerService.AnalyzeCheckAsync(document, cancellationToken),
            DocumentType.DepositSlip => _depositSlipAnalyzerService.AnalyzeDepositSlipAsync(document, cancellationToken),
            _ => throw new ArgumentException($"Unsupported document type '{documentType}' for extraction.", nameof(documentType))
        };
    }

    public Task<NormalizedDocument> NormalizeResultAsync(ExtractionResult extraction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(extraction);

        return extraction.DocumentType switch
        {
            DocumentType.BankCheck => _checkAnalyzerService.MapToNormalizedDocumentAsync(extraction, cancellationToken),
            DocumentType.DepositSlip => _depositSlipAnalyzerService.MapToNormalizedDocumentAsync(extraction, cancellationToken),
            _ => throw DocumentNormalizationException.UnsupportedDocumentType(extraction.DocumentType, extraction.DocumentId)
        };
    }

    public Task<ConfidenceWarnings> AssessProcessingQualityAsync(ProcessingResult processingResult, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(processingResult);

        var extractionConfidence = processingResult.ExtractionResult?.OverallConfidence ?? 0;
        var classificationConfidence = processingResult.ClassificationResult?.Confidence ?? 0;
        var normalizationConfidence = processingResult.NormalizedDocument?.ProcessingConfidence ?? extractionConfidence;

        var thresholds = _configuration.QualityAssessment.ConfidenceThresholds;

        var warnings = new ConfidenceWarnings
        {
            DocumentId = processingResult.DocumentInput?.Id ?? processingResult.Id,
            OverallConfidenceScore = (classificationConfidence + extractionConfidence + normalizationConfidence) / 3,
            Thresholds = thresholds,
            ClassificationConfidence = new StageConfidence
            {
                Score = classificationConfidence,
                Level = thresholds.GetConfidenceLevel(classificationConfidence)
            },
            ExtractionConfidence = new StageConfidence
            {
                Score = extractionConfidence,
                Level = thresholds.GetConfidenceLevel(extractionConfidence)
            },
            NormalizationConfidence = new StageConfidence
            {
                Score = normalizationConfidence,
                Level = thresholds.GetConfidenceLevel(normalizationConfidence)
            }
        };

        warnings.OverallConfidenceLevel = thresholds.GetConfidenceLevel(warnings.OverallConfidenceScore);

        if (warnings.RequiresManualReview())
        {
            warnings.RecommendedActions.Add(new RecommendedAction
            {
                ActionType = ActionType.ManualReview,
                Priority = ActionPriority.High,
                Description = "Manual review recommended due to low confidence.",
                Reason = "At least one processing stage fell below confidence thresholds."
            });
        }
        else
        {
            warnings.RecommendedActions.Add(new RecommendedAction
            {
                ActionType = ActionType.Proceed,
                Priority = ActionPriority.Low,
                Description = "Proceed with automated processing.",
                Reason = "Confidence is above configured thresholds."
            });
        }

        return Task.FromResult(warnings);
    }

    public async Task<IEnumerable<ProcessingResult>> BatchProcessDocumentsAsync(IEnumerable<DocumentInput> documents, int maxConcurrency = 3, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var semaphore = new SemaphoreSlim(Math.Max(1, maxConcurrency));
        var tasks = documents.Select(async document =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await ProcessDocumentAsync(document, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks);
    }

    public Task<ProcessingStatusInfo> GetProcessingStatusAsync(string processingId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processingId);

        if (_statusById.TryGetValue(processingId, out var status))
        {
            return Task.FromResult(status);
        }

        return Task.FromResult(new ProcessingStatusInfo
        {
            ProcessingId = processingId,
            Status = ProcessingStatus.NotStarted,
            CurrentStage = ProcessingStage.NotStarted,
            ProgressPercentage = 0,
            StartedAt = DateTime.UtcNow,
            ElapsedTime = TimeSpan.Zero,
            StageDescription = "Processing ID was not found."
        });
    }

    public Task<bool> CancelProcessingAsync(string processingId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processingId);

        if (!_statusById.TryGetValue(processingId, out var status) || status.Status != ProcessingStatus.InProgress)
        {
            return Task.FromResult(false);
        }

        status.Status = ProcessingStatus.Cancelled;
        status.CurrentStage = ProcessingStage.Failed;
        status.StageDescription = "Processing was cancelled.";
        status.ProgressPercentage = Math.Max(status.ProgressPercentage, 0);

        return Task.FromResult(true);
    }

    public Task<PipelineConfiguration> GetPipelineConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_configuration);
    }

    public Task UpdatePipelineConfigurationAsync(PipelineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = configuration;
        return Task.CompletedTask;
    }

    public async Task<PipelineHealthResult> CheckPipelineHealthAsync(CancellationToken cancellationToken = default)
    {
        var result = new PipelineHealthResult
        {
            OverallStatus = HealthStatus.Healthy
        };

        var classifierHealthy = await _classifierService.ValidateModelAvailabilityAsync(cancellationToken);
        var checkHealthy = await _checkAnalyzerService.TestServiceHealthAsync(cancellationToken);
        var depositHealthy = await _depositSlipAnalyzerService.TestServiceHealthAsync(cancellationToken);

        result.ComponentStatuses["Classifier"] = new ComponentHealthStatus
        {
            ComponentName = "Classifier",
            Status = classifierHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            ResponseTime = TimeSpan.FromMilliseconds(10),
            Details = classifierHealthy ? "Classifier service available." : "Classifier service unavailable."
        };

        result.ComponentStatuses["CheckAnalyzer"] = new ComponentHealthStatus
        {
            ComponentName = "CheckAnalyzer",
            Status = checkHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            ResponseTime = TimeSpan.FromMilliseconds(10),
            Details = checkHealthy ? "Check analyzer service healthy." : "Check analyzer service unhealthy."
        };

        result.ComponentStatuses["DepositSlipAnalyzer"] = new ComponentHealthStatus
        {
            ComponentName = "DepositSlipAnalyzer",
            Status = depositHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            ResponseTime = TimeSpan.FromMilliseconds(10),
            Details = depositHealthy ? "Deposit analyzer service healthy." : "Deposit analyzer service unhealthy."
        };

        if (!classifierHealthy || !checkHealthy || !depositHealthy)
        {
            result.OverallStatus = HealthStatus.Degraded;
            result.Issues.Add(new HealthIssue
            {
                Component = "Pipeline",
                Severity = HealthIssueSeverity.Warning,
                Description = "One or more pipeline components reported unhealthy status."
            });
        }

        return result;
    }

    private void RaiseStageCompleted(string processingId, ProcessingStage stage, DateTime stageStartedAt, object? stageResult)
    {
        StageCompleted?.Invoke(this, new StageCompletedEventArgs
        {
            ProcessingId = processingId,
            Stage = stage,
            StageDuration = DateTime.UtcNow - stageStartedAt,
            StageResult = stageResult,
            CompletedAt = DateTime.UtcNow
        });
    }

    private void UpdateStatus(
        string processingId,
        ProcessingStatus status,
        ProcessingStage stage,
        int progress,
        DateTime startedAt,
        string description)
    {
        var now = DateTime.UtcNow;

        _statusById[processingId] = new ProcessingStatusInfo
        {
            ProcessingId = processingId,
            Status = status,
            CurrentStage = stage,
            ProgressPercentage = progress,
            StartedAt = startedAt,
            ElapsedTime = now - startedAt,
            StageDescription = description
        };
    }
}
