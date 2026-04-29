using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text.Json;
using ImagePickSource = CheckYoSelfAI.Services.Interfaces.ImageSource;

namespace CheckYoSelfAI.ViewModels;

public class DocumentUploadViewModel : BaseViewModel
{
    private const double ClassificationConfidenceThreshold = 0.80;
    private const double ExtractionConfidenceThreshold = 0.75;

    private enum ProcessingPipelineStage
    {
        Upload,
        Classify,
        Route,
        Extract,
        Normalize
    }

    private enum ResultPanel
    {
        UploadedImage,
        ImageMetadata,
        Classification,
        Route,
        Extract,
        Normalize,
        Warnings
    }

    private static readonly ImageValidationOptions UploadValidationOptions = new()
    {
        MaxFileSizeBytes = 10 * 1024 * 1024,
        MinFileSizeBytes = 1024,
        AllowedFormats = new() { "JPEG", "JPG", "PNG" },
        MinWidth = 200,
        MinHeight = 200,
        MaxWidth = 5000,
        MaxHeight = 5000,
        PerformQualityAnalysis = false
    };

    private readonly IImageService _imageService;
    private readonly Dictionary<ProcessingPipelineStage, ProcessingTimelineStage> _timelineStageMap;

    private Microsoft.Maui.Controls.ImageSource? _previewImageSource;
    private ProcessingTimelineStage? _activeTimelineStage;
    private ProcessingPipelineStage? _activePipelineStage;
    private DateTimeOffset? _activeTimelineStageStartedAt;
    private string? _currentProcessingStage;
    private string _selectedFileName = "No file selected";
    private string _detectedFormat = "-";
    private string _fileSizeDisplay = "-";
    private string _dimensionsDisplay = "-";
    private string _lastModifiedDisplay = "-";
    private string? _statusMessage;
    private string? _timelineErrorMessage;
    private string? _validationWarnings;
    private ClassificationResult? _classificationResult;
    private ExtractionResult? _extractionResult;
    private NormalizedDocument? _normalizedDocument;
    private string? _routedModelId;
    private string? _routedModelVersion;
    private string? _routingReasoning;
    private string? _modelDocumentationUrl;
    private bool _showRawNormalizedResult;
    private string? _rawExtractionResult;
    private string? _rawNormalizedResult;
    private string _warningTitle = string.Empty;
    private string _warningMessage = string.Empty;
    private bool _showProcessingWarning;
    private ResultPanel _selectedResultPanel = ResultPanel.UploadedImage;

    public DocumentUploadViewModel(IImageService imageService, ILogger<DocumentUploadViewModel> logger)
        : base(logger)
    {
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));

        Title = "Document Upload";
        ConfidenceThresholdSummary = "Auto-processing thresholds: classify ≥ 80%, extract ≥ 75%.";

        CaptureImageCommand = new AsyncRelayCommand(() => PickImageAsync(ImagePickSource.Camera), () => IsNotBusy);
        SelectImageCommand = new AsyncRelayCommand(() => PickImageAsync(ImagePickSource.PhotoLibrary), () => IsNotBusy);
        ClearImageCommand = new RelayCommand(ClearSelection, () => IsNotBusy && HasSelectedImage);
        ProcessNextStageCommand = new AsyncRelayCommand(ProcessNextStageAsync, CanProcessNextStage);
        ProcessAllStagesCommand = new AsyncRelayCommand(ProcessAllStagesAsync, CanProcessAllStages);
        OverrideClassificationCommand = new RelayCommand<DocumentType>(OverrideClassification, CanOverrideClassification);
        ToggleNormalizedResultViewCommand = new RelayCommand(ToggleNormalizedResultView, () => HasNormalizedResult);
        ExportNormalizedResultCommand = new AsyncRelayCommand(ExportNormalizedResultAsync, () => IsNotBusy && HasNormalizedResult);
        ShareNormalizedResultCommand = new AsyncRelayCommand(ShareNormalizedResultAsync, () => IsNotBusy && HasNormalizedResult);
        OpenModelDocumentationCommand = new AsyncRelayCommand(OpenModelDocumentationAsync, CanOpenModelDocumentation);
        TriggerManualReviewCommand = new RelayCommand(TriggerManualReview, () => HasProcessingWarning);
        TryFallbackProcessingCommand = new RelayCommand(ApplyFallbackRouting, () => HasProcessingWarning && ClassificationResult != null);
        SelectResultTabCommand = new RelayCommand<string>(SelectResultTab, CanSelectResultTab);

        ProcessingTimelineStages =
        [
            new ProcessingTimelineStage("Upload", "Receive and validate the image payload.", 1),
            new ProcessingTimelineStage("Classify", "Identify document type and confidence.", 2),
            new ProcessingTimelineStage("Route", "Select the best extraction model.", 3),
            new ProcessingTimelineStage("Extract", "Run model extraction for field data.", 4),
            new ProcessingTimelineStage("Normalize", "Map extracted output to unified shape.", 5, isLast: true)
        ];

        _timelineStageMap = new Dictionary<ProcessingPipelineStage, ProcessingTimelineStage>
        {
            [ProcessingPipelineStage.Upload] = ProcessingTimelineStages[0],
            [ProcessingPipelineStage.Classify] = ProcessingTimelineStages[1],
            [ProcessingPipelineStage.Route] = ProcessingTimelineStages[2],
            [ProcessingPipelineStage.Extract] = ProcessingTimelineStages[3],
            [ProcessingPipelineStage.Normalize] = ProcessingTimelineStages[4]
        };

        AlternativeClassificationOptions = [];
        RoutedModelCapabilities = [];
        ExpectedModelFields = [];
        ExtractedFieldItems = [];
        NormalizedFieldItems = [];
        WarningSuggestions = [];

        ResetProcessingTimeline();
        ClearProcessingOutputs();
    }

    public IAsyncRelayCommand CaptureImageCommand { get; }

    public IAsyncRelayCommand SelectImageCommand { get; }

    public IRelayCommand ClearImageCommand { get; }

    public IAsyncRelayCommand ProcessNextStageCommand { get; }

    public IAsyncRelayCommand ProcessAllStagesCommand { get; }

    public IRelayCommand<DocumentType> OverrideClassificationCommand { get; }

    public IRelayCommand ToggleNormalizedResultViewCommand { get; }

    public IAsyncRelayCommand ExportNormalizedResultCommand { get; }

    public IAsyncRelayCommand ShareNormalizedResultCommand { get; }

    public IAsyncRelayCommand OpenModelDocumentationCommand { get; }

    public IRelayCommand TriggerManualReviewCommand { get; }

    public IRelayCommand TryFallbackProcessingCommand { get; }

    public IRelayCommand<string> SelectResultTabCommand { get; }

    public ObservableCollection<ProcessingTimelineStage> ProcessingTimelineStages { get; }

    public ObservableCollection<AlternativeClassification> AlternativeClassificationOptions { get; }

    public ObservableCollection<string> RoutedModelCapabilities { get; }

    public ObservableCollection<string> ExpectedModelFields { get; }

    public ObservableCollection<ExtractedFieldDisplayItem> ExtractedFieldItems { get; }

    public ObservableCollection<NormalizedFieldDisplayItem> NormalizedFieldItems { get; }

    public ObservableCollection<string> WarningSuggestions { get; }

    public Microsoft.Maui.Controls.ImageSource? PreviewImageSource
    {
        get => _previewImageSource;
        private set
        {
            if (SetProperty(ref _previewImageSource, value))
            {
                OnPropertyChanged(nameof(HasSelectedImage));
                OnPropertyChanged(nameof(ShowUploadActions));
                RefreshCommandStates();
            }
        }
    }

    public bool HasSelectedImage => PreviewImageSource != null;

    public bool ShowUploadActions => !HasSelectedImage;

    public bool ShowResultTabs => HasSelectedImage;

    public bool HasUploadedImagePanel => HasSelectedImage;

    public bool HasImageMetadataPanel => HasSelectedImage;

    public bool HasClassificationPanel => HasClassificationResult;

    public bool HasRoutePanel => HasRoutingDecision;

    public bool HasExtractPanel => HasExtractedFields;

    public bool HasNormalizePanel => HasNormalizedResult;

    public bool HasWarningsPanel => HasProcessingWarning;

    public bool IsUploadedImageSelected => _selectedResultPanel == ResultPanel.UploadedImage;

    public bool IsImageMetadataSelected => _selectedResultPanel == ResultPanel.ImageMetadata;

    public bool IsClassificationSelected => _selectedResultPanel == ResultPanel.Classification;

    public bool IsRouteSelected => _selectedResultPanel == ResultPanel.Route;

    public bool IsExtractSelected => _selectedResultPanel == ResultPanel.Extract;

    public bool IsNormalizeSelected => _selectedResultPanel == ResultPanel.Normalize;

    public bool IsWarningsSelected => _selectedResultPanel == ResultPanel.Warnings;

    public bool IsUploadedImagePanelVisible => HasUploadedImagePanel && IsUploadedImageSelected;

    public bool IsImageMetadataPanelVisible => HasImageMetadataPanel && IsImageMetadataSelected;

    public bool IsClassificationPanelVisible => HasClassificationPanel && IsClassificationSelected;

    public bool IsRoutePanelVisible => HasRoutePanel && IsRouteSelected;

    public bool IsExtractPanelVisible => HasExtractPanel && IsExtractSelected;

    public bool IsNormalizePanelVisible => HasNormalizePanel && IsNormalizeSelected;

    public bool IsWarningsPanelVisible => HasWarningsPanel && IsWarningsSelected;

    public string SelectedFileName
    {
        get => _selectedFileName;
        private set => SetProperty(ref _selectedFileName, value);
    }

    public string DetectedFormat
    {
        get => _detectedFormat;
        private set => SetProperty(ref _detectedFormat, value);
    }

    public string FileSizeDisplay
    {
        get => _fileSizeDisplay;
        private set => SetProperty(ref _fileSizeDisplay, value);
    }

    public string DimensionsDisplay
    {
        get => _dimensionsDisplay;
        private set => SetProperty(ref _dimensionsDisplay, value);
    }

    public string LastModifiedDisplay
    {
        get => _lastModifiedDisplay;
        private set => SetProperty(ref _lastModifiedDisplay, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public string? CurrentProcessingStage
    {
        get => _currentProcessingStage;
        private set
        {
            if (SetProperty(ref _currentProcessingStage, value))
            {
                OnPropertyChanged(nameof(HasCurrentProcessingStage));
            }
        }
    }

    public bool HasCurrentProcessingStage => !string.IsNullOrWhiteSpace(CurrentProcessingStage);

    public string? TimelineErrorMessage
    {
        get => _timelineErrorMessage;
        private set
        {
            if (SetProperty(ref _timelineErrorMessage, value))
            {
                OnPropertyChanged(nameof(HasTimelineError));
            }
        }
    }

    public bool HasTimelineError => !string.IsNullOrWhiteSpace(TimelineErrorMessage);

    public string? ValidationWarnings
    {
        get => _validationWarnings;
        private set
        {
            if (SetProperty(ref _validationWarnings, value))
            {
                OnPropertyChanged(nameof(HasValidationWarnings));
            }
        }
    }

    public bool HasValidationWarnings => !string.IsNullOrWhiteSpace(ValidationWarnings);

    public ClassificationResult? ClassificationResult
    {
        get => _classificationResult;
        private set
        {
            if (SetProperty(ref _classificationResult, value))
            {
                OnPropertyChanged(nameof(HasClassificationResult));
                OnPropertyChanged(nameof(ClassifiedDocumentTypeDisplay));
                OnPropertyChanged(nameof(ClassificationConfidence));
                OnPropertyChanged(nameof(ClassificationConfidenceDisplay));
                OnPropertyChanged(nameof(IsHighClassificationConfidence));
                OnPropertyChanged(nameof(IsMediumClassificationConfidence));
                OnPropertyChanged(nameof(IsLowClassificationConfidence));
                OnPropertyChanged(nameof(ClassificationReasoningDetails));
                OnPropertyChanged(nameof(HasClassificationReasoning));
                RefreshCommandStates();
            }
        }
    }

    public bool HasClassificationResult => ClassificationResult != null;

    public string ClassifiedDocumentTypeDisplay => ClassificationResult?.DocumentType switch
    {
        DocumentType.BankCheck => "Bank Check",
        DocumentType.DepositSlip => "Deposit Slip",
        DocumentType.Unknown => "Unknown",
        DocumentType.Other => "Other",
        _ => "Unknown"
    };

    public double ClassificationConfidence => ClassificationResult?.Confidence ?? 0;

    public string ClassificationConfidenceDisplay => ClassificationConfidence.ToString("P1");

    public bool IsHighClassificationConfidence => ClassificationConfidence >= ClassificationConfidenceThreshold;

    public bool IsMediumClassificationConfidence => ClassificationConfidence >= 0.65 && ClassificationConfidence < ClassificationConfidenceThreshold;

    public bool IsLowClassificationConfidence => ClassificationConfidence < 0.65;

    public string? ClassificationReasoningDetails => ClassificationResult?.ReasoningDetails;

    public bool HasClassificationReasoning => !string.IsNullOrWhiteSpace(ClassificationReasoningDetails);

    public ExtractionResult? ExtractionResult
    {
        get => _extractionResult;
        private set
        {
            if (SetProperty(ref _extractionResult, value))
            {
                OnPropertyChanged(nameof(HasExtractionResult));
                OnPropertyChanged(nameof(ExtractionConfidence));
                OnPropertyChanged(nameof(ExtractionConfidenceDisplay));
                OnPropertyChanged(nameof(IsHighExtractionConfidence));
                OnPropertyChanged(nameof(HasRawExtractionResult));
            }
        }
    }

    public bool HasExtractionResult => ExtractionResult != null;

    public double ExtractionConfidence => ExtractionResult?.OverallConfidence ?? 0;

    public string ExtractionConfidenceDisplay => ExtractionConfidence.ToString("P1");

    public bool IsHighExtractionConfidence => ExtractionConfidence >= ExtractionConfidenceThreshold;

    public bool HasExtractedFields => ExtractedFieldItems.Count > 0;

    public NormalizedDocument? NormalizedDocument
    {
        get => _normalizedDocument;
        private set
        {
            if (SetProperty(ref _normalizedDocument, value))
            {
                OnPropertyChanged(nameof(HasNormalizedResult));
                OnPropertyChanged(nameof(NormalizedSummary));
                OnPropertyChanged(nameof(NormalizedDocumentTypeDisplay));
                RefreshCommandStates();
            }
        }
    }

    public bool HasNormalizedResult => NormalizedDocument != null;

    public string NormalizedSummary => NormalizedDocument?.GetSummary() ?? "No normalized result available yet.";

    public string NormalizedDocumentTypeDisplay => NormalizedDocument?.Type switch
    {
        DocumentType.BankCheck => "Bank Check",
        DocumentType.DepositSlip => "Deposit Slip",
        DocumentType.Unknown => "Unknown",
        DocumentType.Other => "Other",
        _ => "Unknown"
    };

    public bool HasNormalizedFields => NormalizedFieldItems.Count > 0;

    public bool ShowRawNormalizedResult
    {
        get => _showRawNormalizedResult;
        private set
        {
            if (SetProperty(ref _showRawNormalizedResult, value))
            {
                OnPropertyChanged(nameof(IsFormattedNormalizedView));
                OnPropertyChanged(nameof(IsRawNormalizedView));
                OnPropertyChanged(nameof(NormalizedViewToggleText));
            }
        }
    }

    public bool IsFormattedNormalizedView => !ShowRawNormalizedResult;

    public bool IsRawNormalizedView => ShowRawNormalizedResult;

    public string NormalizedViewToggleText => ShowRawNormalizedResult ? "Show Formatted" : "Show Raw JSON";

    public string? RawExtractionResult
    {
        get => _rawExtractionResult;
        private set
        {
            if (SetProperty(ref _rawExtractionResult, value))
            {
                OnPropertyChanged(nameof(HasRawExtractionResult));
            }
        }
    }

    public bool HasRawExtractionResult => !string.IsNullOrWhiteSpace(RawExtractionResult);

    public string? RawNormalizedResult
    {
        get => _rawNormalizedResult;
        private set
        {
            if (SetProperty(ref _rawNormalizedResult, value))
            {
                OnPropertyChanged(nameof(HasRawNormalizedResult));
            }
        }
    }

    public bool HasRawNormalizedResult => !string.IsNullOrWhiteSpace(RawNormalizedResult);

    public string? RoutedModelId
    {
        get => _routedModelId;
        private set
        {
            if (SetProperty(ref _routedModelId, value))
            {
                OnPropertyChanged(nameof(HasRoutingDecision));
                RefreshCommandStates();
            }
        }
    }

    public string? RoutedModelVersion
    {
        get => _routedModelVersion;
        private set => SetProperty(ref _routedModelVersion, value);
    }

    public string? RoutingReasoning
    {
        get => _routingReasoning;
        private set
        {
            if (SetProperty(ref _routingReasoning, value))
            {
                OnPropertyChanged(nameof(HasRoutingReasoning));
            }
        }
    }

    public bool HasRoutingReasoning => !string.IsNullOrWhiteSpace(RoutingReasoning);

    public bool HasRoutingDecision => !string.IsNullOrWhiteSpace(RoutedModelId);

    public bool HasModelCapabilities => RoutedModelCapabilities.Count > 0;

    public bool HasExpectedModelFields => ExpectedModelFields.Count > 0;

    public string? ModelDocumentationUrl
    {
        get => _modelDocumentationUrl;
        private set
        {
            if (SetProperty(ref _modelDocumentationUrl, value))
            {
                OnPropertyChanged(nameof(HasModelDocumentationUrl));
                RefreshCommandStates();
            }
        }
    }

    public bool HasModelDocumentationUrl => !string.IsNullOrWhiteSpace(ModelDocumentationUrl);

    public string WarningTitle
    {
        get => _warningTitle;
        private set => SetProperty(ref _warningTitle, value);
    }

    public string WarningMessage
    {
        get => _warningMessage;
        private set => SetProperty(ref _warningMessage, value);
    }

    public bool HasProcessingWarning
    {
        get => _showProcessingWarning;
        private set
        {
            if (SetProperty(ref _showProcessingWarning, value))
            {
                OnPropertyChanged(nameof(HasWarningSuggestions));
                RefreshCommandStates();
            }
        }
    }

    public bool HasWarningSuggestions => HasProcessingWarning && WarningSuggestions.Count > 0;

    public string ConfidenceThresholdSummary { get; }

    public string NextStepHint
    {
        get
        {
            if (!HasSelectedImage)
            {
                return "Upload an image to enable guided processing steps.";
            }

            return _activePipelineStage switch
            {
                ProcessingPipelineStage.Classify => "Step 1 of 4: run classification to identify the document type.",
                ProcessingPipelineStage.Route => "Step 2 of 4: run routing to choose the extraction model.",
                ProcessingPipelineStage.Extract => "Step 3 of 4: run extraction to capture fields with confidence.",
                ProcessingPipelineStage.Normalize => "Step 4 of 4: normalize extracted fields into a unified result view.",
                _ => "All demo processing stages are complete."
            };
        }
    }

    public string NextStepActionText
    {
        get
        {
            if (!HasSelectedImage)
            {
                return "Run Next Step";
            }

            return _activePipelineStage switch
            {
                ProcessingPipelineStage.Classify => "Classify",
                ProcessingPipelineStage.Route => "Route",
                ProcessingPipelineStage.Extract => "Extract",
                ProcessingPipelineStage.Normalize => "Normalize",
                _ => "Complete"
            };
        }
    }

    protected override void OnBusyChanged(bool isBusy)
    {
        base.OnBusyChanged(isBusy);
        RefreshCommandStates();
    }

    private void RefreshCommandStates()
    {
        EnsureValidSelectedResultPanel();

        CaptureImageCommand.NotifyCanExecuteChanged();
        SelectImageCommand.NotifyCanExecuteChanged();
        ClearImageCommand.NotifyCanExecuteChanged();
        ProcessNextStageCommand.NotifyCanExecuteChanged();
        ProcessAllStagesCommand.NotifyCanExecuteChanged();
        OverrideClassificationCommand.NotifyCanExecuteChanged();
        ToggleNormalizedResultViewCommand.NotifyCanExecuteChanged();
        ExportNormalizedResultCommand.NotifyCanExecuteChanged();
        ShareNormalizedResultCommand.NotifyCanExecuteChanged();
        OpenModelDocumentationCommand.NotifyCanExecuteChanged();
        TriggerManualReviewCommand.NotifyCanExecuteChanged();
        TryFallbackProcessingCommand.NotifyCanExecuteChanged();
        SelectResultTabCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(NextStepHint));
        OnPropertyChanged(nameof(NextStepActionText));
        OnPropertyChanged(nameof(ShowResultTabs));
        OnPropertyChanged(nameof(HasUploadedImagePanel));
        OnPropertyChanged(nameof(HasImageMetadataPanel));
        OnPropertyChanged(nameof(HasClassificationPanel));
        OnPropertyChanged(nameof(HasRoutePanel));
        OnPropertyChanged(nameof(HasExtractPanel));
        OnPropertyChanged(nameof(HasNormalizePanel));
        OnPropertyChanged(nameof(HasWarningsPanel));
        OnPropertyChanged(nameof(IsUploadedImageSelected));
        OnPropertyChanged(nameof(IsImageMetadataSelected));
        OnPropertyChanged(nameof(IsClassificationSelected));
        OnPropertyChanged(nameof(IsRouteSelected));
        OnPropertyChanged(nameof(IsExtractSelected));
        OnPropertyChanged(nameof(IsNormalizeSelected));
        OnPropertyChanged(nameof(IsWarningsSelected));
        OnPropertyChanged(nameof(IsUploadedImagePanelVisible));
        OnPropertyChanged(nameof(IsImageMetadataPanelVisible));
        OnPropertyChanged(nameof(IsClassificationPanelVisible));
        OnPropertyChanged(nameof(IsRoutePanelVisible));
        OnPropertyChanged(nameof(IsExtractPanelVisible));
        OnPropertyChanged(nameof(IsNormalizePanelVisible));
        OnPropertyChanged(nameof(IsWarningsPanelVisible));
    }

    private void EnsureValidSelectedResultPanel()
    {
        if (CanDisplayPanel(_selectedResultPanel))
        {
            return;
        }

        _selectedResultPanel = ResultPanel.UploadedImage;
        if (CanDisplayPanel(_selectedResultPanel))
        {
            return;
        }

        _selectedResultPanel = ResultPanel.ImageMetadata;
        if (CanDisplayPanel(_selectedResultPanel))
        {
            return;
        }

        _selectedResultPanel = ResultPanel.Classification;
        if (CanDisplayPanel(_selectedResultPanel))
        {
            return;
        }

        _selectedResultPanel = ResultPanel.Route;
        if (CanDisplayPanel(_selectedResultPanel))
        {
            return;
        }

        _selectedResultPanel = ResultPanel.Extract;
        if (CanDisplayPanel(_selectedResultPanel))
        {
            return;
        }

        _selectedResultPanel = ResultPanel.Normalize;
        if (CanDisplayPanel(_selectedResultPanel))
        {
            return;
        }

        _selectedResultPanel = ResultPanel.Warnings;
    }

    private bool CanDisplayPanel(ResultPanel panel)
    {
        return panel switch
        {
            ResultPanel.UploadedImage => HasUploadedImagePanel,
            ResultPanel.ImageMetadata => HasImageMetadataPanel,
            ResultPanel.Classification => HasClassificationPanel,
            ResultPanel.Route => HasRoutePanel,
            ResultPanel.Extract => HasExtractPanel,
            ResultPanel.Normalize => HasNormalizePanel,
            ResultPanel.Warnings => HasWarningsPanel,
            _ => false
        };
    }

    private bool CanSelectResultTab(string? tabKey)
    {
        if (string.IsNullOrWhiteSpace(tabKey))
        {
            return false;
        }

        return tabKey.Trim().ToLowerInvariant() switch
        {
            "image" => HasUploadedImagePanel,
            "metadata" => HasImageMetadataPanel,
            "classification" => HasClassificationPanel,
            "route" => HasRoutePanel,
            "extract" => HasExtractPanel,
            "normalize" => HasNormalizePanel,
            "warnings" => HasWarningsPanel,
            _ => false
        };
    }

    private void SelectResultTab(string? tabKey)
    {
        if (!CanSelectResultTab(tabKey))
        {
            return;
        }

        var selectedPanel = tabKey!.Trim().ToLowerInvariant() switch
        {
            "image" => ResultPanel.UploadedImage,
            "metadata" => ResultPanel.ImageMetadata,
            "classification" => ResultPanel.Classification,
            "route" => ResultPanel.Route,
            "extract" => ResultPanel.Extract,
            "normalize" => ResultPanel.Normalize,
            "warnings" => ResultPanel.Warnings,
            _ => ResultPanel.UploadedImage
        };

        if (_selectedResultPanel == selectedPanel)
        {
            return;
        }

        _selectedResultPanel = selectedPanel;
        RefreshCommandStates();
    }

    private bool CanProcessNextStage()
    {
        return IsNotBusy
            && HasSelectedImage
            && _activePipelineStage is ProcessingPipelineStage.Classify
                or ProcessingPipelineStage.Route
                or ProcessingPipelineStage.Extract
                or ProcessingPipelineStage.Normalize;
    }

    private bool CanProcessAllStages()
    {
        return CanProcessNextStage();
    }

    private bool HasRemainingPipelineStages()
    {
        return HasSelectedImage
            && _activePipelineStage is ProcessingPipelineStage.Classify
                or ProcessingPipelineStage.Route
                or ProcessingPipelineStage.Extract
                or ProcessingPipelineStage.Normalize;
    }

    private bool CanOverrideClassification(DocumentType type)
    {
        return IsNotBusy
            && ClassificationResult != null
            && ClassificationResult.DocumentType != type;
    }

    private bool CanOpenModelDocumentation()
    {
        return IsNotBusy && HasModelDocumentationUrl;
    }

    private async Task PickImageAsync(ImagePickSource source)
    {
        await ExecuteWithBusyAsync(async _ =>
        {
            StatusMessage = null;
            ValidationWarnings = null;
            ResetProcessingTimeline();
            ClearProcessingOutputs();
            StartProcessingStage(ProcessingPipelineStage.Upload);

            var imageResult = await _imageService.PickImageAsync(source);
            if (!imageResult.IsSuccess || string.IsNullOrWhiteSpace(imageResult.ImagePath))
            {
                var message = imageResult.ErrorMessage ?? "Image selection was cancelled.";
                FailActiveProcessingStage(message);
                StatusMessage = message;
                return;
            }

            var validationResult = await _imageService.ValidateImageAsync(imageResult.ImagePath, UploadValidationOptions);
            if (!validationResult.IsValid)
            {
                var message = validationResult.Errors.FirstOrDefault() ?? "Selected file did not pass validation.";
                FailActiveProcessingStage(message);
                StatusMessage = message;
                ValidationWarnings = null;
                return;
            }

            if (validationResult.Warnings.Count > 0)
            {
                ValidationWarnings = string.Join(Environment.NewLine, validationResult.Warnings);
            }

            var imageInfo = await _imageService.GetImageInfoAsync(imageResult.ImagePath);

            PreviewImageSource = Microsoft.Maui.Controls.ImageSource.FromFile(imageResult.ImagePath);
            SelectedFileName = imageResult.OriginalFilename ?? Path.GetFileName(imageResult.ImagePath);
            DetectedFormat = imageInfo.Format ?? validationResult.DetectedFormat ?? "Unknown";
            FileSizeDisplay = imageInfo.FormattedFileSize;
            DimensionsDisplay = imageInfo.FormattedResolution;
            LastModifiedDisplay = imageInfo.ModifiedDate?.ToLocalTime().ToString("g") ?? "Unknown";

            CompleteActiveProcessingStage();
            StartProcessingStage(ProcessingPipelineStage.Classify);
            StatusMessage = "Image ready. Use 'Run Next Step' to progress through the demo pipeline.";
            RefreshCommandStates();
        }, "Unable to select image right now.");
    }

    private async Task ProcessNextStageAsync()
    {
        await ExecuteWithBusyAsync(async _ =>
        {
            await AdvancePipelineStageAsync();
        }, "Unable to advance processing stage right now.");
    }

    private async Task ProcessAllStagesAsync()
    {
        await ExecuteWithBusyAsync(async _ =>
        {
            while (HasRemainingPipelineStages())
            {
                await Task.Delay(250);
                await AdvancePipelineStageAsync();
            }
        }, "Unable to run the full processing demo right now.");
    }

    private async Task AdvancePipelineStageAsync()
    {
        if (_activePipelineStage == null)
        {
            StatusMessage = "No remaining processing steps.";
            return;
        }

        var currentStage = _activePipelineStage.Value;

        await Task.Delay(350);

        switch (currentStage)
        {
            case ProcessingPipelineStage.Classify:
                RunClassificationStage();
                break;
            case ProcessingPipelineStage.Route:
                RunRoutingStage();
                break;
            case ProcessingPipelineStage.Extract:
                RunExtractionStage();
                break;
            case ProcessingPipelineStage.Normalize:
                RunNormalizationStage();
                break;
            default:
                return;
        }

        CompleteActiveProcessingStage();
        var nextStage = GetNextStage(currentStage);
        if (nextStage.HasValue)
        {
            StartProcessingStage(nextStage.Value);
        }
        else
        {
            StatusMessage = "Demo processing complete. Review results below or adjust with manual override.";
        }

        EvaluateWarningState();
        RefreshCommandStates();
    }

    private void RunClassificationStage()
    {
        var inferredType = InferDocumentTypeFromFileName();

        ClassificationResult = new ClassificationResult
        {
            DocumentId = Guid.NewGuid().ToString("N"),
            DocumentType = inferredType,
            Confidence = inferredType == DocumentType.DepositSlip ? 0.86 : 0.74,
            ModelVersion = "doc-classifier-v1.2-demo",
            ReasoningDetails = inferredType == DocumentType.DepositSlip
                ? "Detected deposit item grid, subtotal lines, and account holder section typical of deposit slips."
                : "Detected MICR line and check-style payee/date layout with moderate confidence due to image angle.",
            Alternatives =
            [
                new AlternativeClassification
                {
                    DocumentType = inferredType == DocumentType.DepositSlip ? DocumentType.BankCheck : DocumentType.DepositSlip,
                    Confidence = inferredType == DocumentType.DepositSlip ? 0.11 : 0.20,
                    Reasoning = "Some shared banking layout patterns were detected."
                },
                new AlternativeClassification
                {
                    DocumentType = DocumentType.Other,
                    Confidence = 0.03,
                    Reasoning = "Low probability fallback for unsupported forms."
                }
            ],
            ProcessingTime = TimeSpan.FromMilliseconds(430)
        };

        AlternativeClassificationOptions.Clear();
        foreach (var alternative in ClassificationResult.Alternatives.OrderByDescending(option => option.Confidence))
        {
            AlternativeClassificationOptions.Add(alternative);
        }

        StatusMessage = $"Classification complete: {ClassifiedDocumentTypeDisplay} ({ClassificationConfidenceDisplay}).";
        SelectResultTab("classification");
    }

    private void RunRoutingStage()
    {
        var classifiedType = ClassificationResult?.DocumentType ?? DocumentType.Unknown;
        var route = GetRouteForDocumentType(classifiedType, isFallback: false);

        RoutedModelId = route.ModelId;
        RoutedModelVersion = route.ModelVersion;
        RoutingReasoning = route.Reasoning;
        ModelDocumentationUrl = route.DocumentationUrl;

        RoutedModelCapabilities.Clear();
        foreach (var capability in route.Capabilities)
        {
            RoutedModelCapabilities.Add(capability);
        }

        ExpectedModelFields.Clear();
        foreach (var expectedField in route.ExpectedFields)
        {
            ExpectedModelFields.Add(expectedField);
        }

        OnPropertyChanged(nameof(HasModelCapabilities));
        OnPropertyChanged(nameof(HasExpectedModelFields));

        StatusMessage = $"Routing complete: {RoutedModelId} selected.";
        SelectResultTab("route");
    }

    private void RunExtractionStage()
    {
        if (ClassificationResult == null || string.IsNullOrWhiteSpace(RoutedModelId))
        {
            StatusMessage = "Extraction cannot run before classification and routing.";
            return;
        }

        var extractedFields = BuildExtractedFields(ClassificationResult.DocumentType);

        ExtractionResult = new ExtractionResult
        {
            DocumentId = ClassificationResult.DocumentId,
            DocumentType = ClassificationResult.DocumentType,
            ModelId = RoutedModelId,
            ModelVersion = RoutedModelVersion,
            OverallConfidence = extractedFields.Values.Average(field => field.Confidence),
            ExtractedFields = extractedFields,
            ProcessingTime = TimeSpan.FromMilliseconds(620)
        };

        ExtractedFieldItems.Clear();
        foreach (var field in ExtractionResult.ExtractedFields.OrderBy(field => field.Key))
        {
            ExtractedFieldItems.Add(new ExtractedFieldDisplayItem
            {
                FieldName = field.Key,
                FieldValue = field.Value.Value ?? "-",
                Confidence = field.Value.Confidence
            });
        }

        RawExtractionResult = JsonSerializer.Serialize(ExtractionResult, new JsonSerializerOptions { WriteIndented = true });
        OnPropertyChanged(nameof(HasExtractedFields));

        StatusMessage = $"Extraction complete with {ExtractionConfidenceDisplay} confidence.";
        SelectResultTab("extract");
    }

    private void RunNormalizationStage()
    {
        if (ClassificationResult == null || ExtractionResult == null)
        {
            StatusMessage = "Normalization requires completed classification and extraction.";
            return;
        }

        NormalizedDocument = BuildNormalizedDocument(ClassificationResult.DocumentType, ExtractionResult);

        NormalizedFieldItems.Clear();
        foreach (var field in GetNormalizedFields(NormalizedDocument))
        {
            NormalizedFieldItems.Add(field);
        }

        ShowRawNormalizedResult = false;
        RawNormalizedResult = JsonSerializer.Serialize(NormalizedDocument, new JsonSerializerOptions { WriteIndented = true });
        OnPropertyChanged(nameof(HasNormalizedFields));

        StatusMessage = "Normalization complete. You can now review formatted or raw output and export/share results.";
        SelectResultTab("normalize");
    }

    private static ProcessingPipelineStage? GetNextStage(ProcessingPipelineStage? currentStage)
    {
        return currentStage switch
        {
            ProcessingPipelineStage.Classify => ProcessingPipelineStage.Route,
            ProcessingPipelineStage.Route => ProcessingPipelineStage.Extract,
            ProcessingPipelineStage.Extract => ProcessingPipelineStage.Normalize,
            _ => null
        };
    }

    private void OverrideClassification(DocumentType type)
    {
        if (ClassificationResult == null)
        {
            return;
        }

        ClassificationResult.DocumentType = type;
        ClassificationResult.Confidence = Math.Max(ClassificationResult.Confidence, 0.70);
        ClassificationResult.ReasoningDetails = "Classification manually overridden in demo mode. Routing and downstream outputs were reset.";

        ResetStagesFrom(ProcessingPipelineStage.Route);
        ClearDownstreamOutputsFromRouting();
        StartProcessingStage(ProcessingPipelineStage.Route);

        StatusMessage = $"Manual override applied: document type set to {type}. Continue with routing.";
        EvaluateWarningState();
        OnPropertyChanged(nameof(ClassifiedDocumentTypeDisplay));
        OnPropertyChanged(nameof(ClassificationConfidence));
        OnPropertyChanged(nameof(ClassificationConfidenceDisplay));
        OnPropertyChanged(nameof(ClassificationReasoningDetails));
        RefreshCommandStates();
    }

    private void ApplyFallbackRouting()
    {
        if (ClassificationResult == null)
        {
            return;
        }

        var route = GetRouteForDocumentType(ClassificationResult.DocumentType, isFallback: true);

        RoutedModelId = route.ModelId;
        RoutedModelVersion = route.ModelVersion;
        RoutingReasoning = route.Reasoning;
        ModelDocumentationUrl = route.DocumentationUrl;

        RoutedModelCapabilities.Clear();
        foreach (var capability in route.Capabilities)
        {
            RoutedModelCapabilities.Add(capability);
        }

        ExpectedModelFields.Clear();
        foreach (var expectedField in route.ExpectedFields)
        {
            ExpectedModelFields.Add(expectedField);
        }

        ResetStagesFrom(ProcessingPipelineStage.Extract);
        ClearDownstreamOutputsFromExtraction();

        _timelineStageMap[ProcessingPipelineStage.Route].SetCompleted(TimeSpan.FromMilliseconds(120));
        StartProcessingStage(ProcessingPipelineStage.Extract);

        StatusMessage = "Fallback model applied. Continue with extraction to compare output quality.";
        EvaluateWarningState();
        RefreshCommandStates();
    }

    private void TriggerManualReview()
    {
        StatusMessage = "Manual review requested. In a production flow this would open a review queue task.";
    }

    private void ToggleNormalizedResultView()
    {
        ShowRawNormalizedResult = !ShowRawNormalizedResult;
    }

    private async Task ExportNormalizedResultAsync()
    {
        await ExecuteWithBusyAsync(async _ =>
        {
            if (string.IsNullOrWhiteSpace(RawNormalizedResult))
            {
                StatusMessage = "No normalized result is available to export.";
                return;
            }

            await Clipboard.Default.SetTextAsync(RawNormalizedResult);
            StatusMessage = "Normalized JSON copied to clipboard.";
        }, "Unable to export normalized result right now.");
    }

    private async Task ShareNormalizedResultAsync()
    {
        await ExecuteWithBusyAsync(async _ =>
        {
            if (string.IsNullOrWhiteSpace(RawNormalizedResult))
            {
                StatusMessage = "No normalized result is available to share.";
                return;
            }

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Normalized document result",
                Text = RawNormalizedResult
            });

            StatusMessage = "Share sheet opened for normalized result.";
        }, "Unable to share normalized result right now.");
    }

    private async Task OpenModelDocumentationAsync()
    {
        await ExecuteWithBusyAsync(async _ =>
        {
            if (string.IsNullOrWhiteSpace(ModelDocumentationUrl))
            {
                StatusMessage = "No model documentation link is available.";
                return;
            }

            await Launcher.Default.OpenAsync(ModelDocumentationUrl);
            StatusMessage = "Opened model documentation.";
        }, "Unable to open model documentation right now.");
    }

    private void ClearSelection()
    {
        PreviewImageSource = null;
        SelectedFileName = "No file selected";
        DetectedFormat = "-";
        FileSizeDisplay = "-";
        DimensionsDisplay = "-";
        LastModifiedDisplay = "-";
        StatusMessage = null;
        ValidationWarnings = null;

        ResetProcessingTimeline();
        ClearProcessingOutputs();
        RefreshCommandStates();
    }

    private void ResetProcessingTimeline()
    {
        foreach (var stage in ProcessingTimelineStages)
        {
            stage.SetPending();
        }

        _activeTimelineStage = null;
        _activePipelineStage = null;
        _activeTimelineStageStartedAt = null;
        CurrentProcessingStage = null;
        TimelineErrorMessage = null;
    }

    private void StartProcessingStage(ProcessingPipelineStage stage)
    {
        if (!_timelineStageMap.TryGetValue(stage, out var timelineStage))
        {
            return;
        }

        _activePipelineStage = stage;
        _activeTimelineStage = timelineStage;
        _activeTimelineStageStartedAt = DateTimeOffset.UtcNow;

        timelineStage.SetInProgress();
        CurrentProcessingStage = timelineStage.Title;
        TimelineErrorMessage = null;
        RefreshCommandStates();
    }

    private void CompleteActiveProcessingStage()
    {
        if (_activeTimelineStage == null)
        {
            return;
        }

        var completedStage = _activePipelineStage;
        _activeTimelineStage.SetCompleted(GetActiveStageDuration());
        _activeTimelineStage = null;
        _activePipelineStage = null;
        _activeTimelineStageStartedAt = null;
        CurrentProcessingStage = null;

        if (completedStage == ProcessingPipelineStage.Normalize)
        {
            OnPropertyChanged(nameof(NextStepHint));
        }
    }

    private void FailActiveProcessingStage(string errorMessage)
    {
        if (_activeTimelineStage == null)
        {
            TimelineErrorMessage = errorMessage;
            return;
        }

        _activeTimelineStage.SetFailed(GetActiveStageDuration(), errorMessage);
        _activeTimelineStage = null;
        _activePipelineStage = null;
        _activeTimelineStageStartedAt = null;
        CurrentProcessingStage = null;
        TimelineErrorMessage = errorMessage;
        RefreshCommandStates();
    }

    private void ResetStagesFrom(ProcessingPipelineStage stage)
    {
        foreach (var stageEntry in _timelineStageMap.Where(entry => entry.Key >= stage))
        {
            stageEntry.Value.SetPending();
        }

        _activeTimelineStage = null;
        _activePipelineStage = null;
        _activeTimelineStageStartedAt = null;
        CurrentProcessingStage = null;
    }

    private TimeSpan GetActiveStageDuration()
    {
        if (!_activeTimelineStageStartedAt.HasValue)
        {
            return TimeSpan.Zero;
        }

        var duration = DateTimeOffset.UtcNow - _activeTimelineStageStartedAt.Value;
        return duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
    }

    private void ClearProcessingOutputs()
    {
        ClassificationResult = null;
        AlternativeClassificationOptions.Clear();

        ClearDownstreamOutputsFromRouting();

        WarningTitle = string.Empty;
        WarningMessage = string.Empty;
        WarningSuggestions.Clear();
        HasProcessingWarning = false;

        OnPropertyChanged(nameof(HasModelCapabilities));
        OnPropertyChanged(nameof(HasExpectedModelFields));
        OnPropertyChanged(nameof(HasExtractedFields));
        OnPropertyChanged(nameof(HasNormalizedFields));
    }

    private void ClearDownstreamOutputsFromRouting()
    {
        RoutedModelId = null;
        RoutedModelVersion = null;
        RoutingReasoning = null;
        ModelDocumentationUrl = null;
        RoutedModelCapabilities.Clear();
        ExpectedModelFields.Clear();
        OnPropertyChanged(nameof(HasModelCapabilities));
        OnPropertyChanged(nameof(HasExpectedModelFields));

        ClearDownstreamOutputsFromExtraction();
    }

    private void ClearDownstreamOutputsFromExtraction()
    {
        ExtractionResult = null;
        ExtractedFieldItems.Clear();
        RawExtractionResult = null;
        OnPropertyChanged(nameof(HasExtractedFields));

        NormalizedDocument = null;
        NormalizedFieldItems.Clear();
        RawNormalizedResult = null;
        ShowRawNormalizedResult = false;
        OnPropertyChanged(nameof(HasNormalizedFields));
    }

    private void EvaluateWarningState()
    {
        WarningSuggestions.Clear();
        var hadWarning = HasProcessingWarning;

        if (ClassificationResult == null)
        {
            WarningTitle = string.Empty;
            WarningMessage = string.Empty;
            HasProcessingWarning = false;
            return;
        }

        if (ClassificationResult.DocumentType is DocumentType.Unknown or DocumentType.Other)
        {
            WarningTitle = "Unsupported or uncertain document type";
            WarningMessage = "The classifier selected an unsupported type. Use manual override or fallback routing before extraction.";
            WarningSuggestions.Add("Use a manual override from the alternatives list.");
            WarningSuggestions.Add("Try fallback routing to the generic document model.");
            WarningSuggestions.Add("Capture a clearer image if document type remains uncertain.");
            HasProcessingWarning = true;
            return;
        }

        if (ClassificationResult.Confidence < ClassificationConfidenceThreshold)
        {
            WarningTitle = "Low classification confidence";
            WarningMessage = $"Classification confidence ({ClassificationConfidenceDisplay}) is below the {ClassificationConfidenceThreshold:P0} threshold.";
            WarningSuggestions.Add("Review alternatives and apply manual override if needed.");
            WarningSuggestions.Add("Continue with routing and compare extracted confidence results.");
            WarningSuggestions.Add("Use fallback model if extraction quality is poor.");
            HasProcessingWarning = true;
            if (!hadWarning)
            {
                SelectResultTab("warnings");
            }
            return;
        }

        if (ExtractionResult != null && ExtractionResult.OverallConfidence < ExtractionConfidenceThreshold)
        {
            WarningTitle = "Low extraction confidence";
            WarningMessage = $"Extraction confidence ({ExtractionConfidenceDisplay}) is below the {ExtractionConfidenceThreshold:P0} threshold.";
            WarningSuggestions.Add("Use fallback routing then rerun extraction.");
            WarningSuggestions.Add("Trigger manual review for field-level verification.");
            WarningSuggestions.Add("Review low-confidence extracted fields before normalization export.");
            HasProcessingWarning = true;
            if (!hadWarning)
            {
                SelectResultTab("warnings");
            }
            return;
        }

        if (NormalizedDocument != null && NormalizedDocument.RequiresReview())
        {
            WarningTitle = "Manual review recommended";
            WarningMessage = "Normalization completed with conditions that should be reviewed before automated processing.";
            WarningSuggestions.Add("Trigger manual review workflow.");
            WarningSuggestions.Add("Inspect warnings and confidence thresholds.");
            WarningSuggestions.Add("Re-run with fallback model if needed.");
            HasProcessingWarning = true;
            if (!hadWarning)
            {
                SelectResultTab("warnings");
            }
            return;
        }

        WarningTitle = string.Empty;
        WarningMessage = string.Empty;
        HasProcessingWarning = false;
    }

    private DocumentType InferDocumentTypeFromFileName()
    {
        if (SelectedFileName.Contains("deposit", StringComparison.OrdinalIgnoreCase) ||
            SelectedFileName.Contains("slip", StringComparison.OrdinalIgnoreCase))
        {
            return DocumentType.DepositSlip;
        }

        return DocumentType.BankCheck;
    }

    private static (string ModelId, string ModelVersion, string Reasoning, string DocumentationUrl, IReadOnlyList<string> Capabilities, IReadOnlyList<string> ExpectedFields)
        GetRouteForDocumentType(DocumentType type, bool isFallback)
    {
        if (isFallback)
        {
            return (
                "prebuilt-document",
                "2024-11-30",
                "Fallback route selected for broader document tolerance and manual review scenarios.",
                "https://learn.microsoft.com/azure/ai-services/document-intelligence/prebuilt/general-document",
                ["General key-value extraction", "Layout and table recovery", "Cross-template resilience"],
                ["DocumentTypeHint", "Amount", "AccountNumber", "DocumentDate", "ReferenceNumber"]);
        }

        return type switch
        {
            DocumentType.DepositSlip =>
            (
                "custom-deposit-slip-v3",
                "3.4.0",
                "Deposit slip classification routed to custom model optimized for line-item totals and endorsement zones.",
                "https://learn.microsoft.com/azure/ai-services/document-intelligence/model-overview",
                ["Deposit item segmentation", "Cash/check subtotal extraction", "Transit account parsing"],
                ["DepositSlipNumber", "Amount", "CashAmount", "CheckAmount", "AccountNumber", "RoutingNumber"]
            ),
            _ =>
            (
                "prebuilt-check.us",
                "2024-11-30",
                "Check document routed to prebuilt US check model for MICR and payer/payee field extraction.",
                "https://learn.microsoft.com/azure/ai-services/document-intelligence/prebuilt/check",
                ["MICR line parsing", "Payee and payer extraction", "Check amount and date capture"],
                ["CheckNumber", "Amount", "PayToName", "AccountNumber", "RoutingNumber", "Memo"]
            )
        };
    }

    private static Dictionary<string, FieldValue> BuildExtractedFields(DocumentType type)
    {
        if (type == DocumentType.DepositSlip)
        {
            return new Dictionary<string, FieldValue>
            {
                ["DepositSlipNumber"] = new() { Value = "DS-90214", Confidence = 0.90 },
                ["Amount"] = new() { Value = "1500.75", Confidence = 0.88 },
                ["CashAmount"] = new() { Value = "200.00", Confidence = 0.82 },
                ["CheckAmount"] = new() { Value = "1300.75", Confidence = 0.84 },
                ["AccountNumber"] = new() { Value = "****7821", Confidence = 0.79 },
                ["RoutingNumber"] = new() { Value = "021000021", Confidence = 0.80 }
            };
        }

        return new Dictionary<string, FieldValue>
        {
            ["CheckNumber"] = new() { Value = "1058", Confidence = 0.78 },
            ["Amount"] = new() { Value = "824.50", Confidence = 0.81 },
            ["PayToName"] = new() { Value = "Contoso Office Supplies", Confidence = 0.74 },
            ["AccountNumber"] = new() { Value = "****1187", Confidence = 0.72 },
            ["RoutingNumber"] = new() { Value = "123456789", Confidence = 0.70 },
            ["Memo"] = new() { Value = "Q2 office restock", Confidence = 0.66 }
        };
    }

    private static NormalizedDocument BuildNormalizedDocument(DocumentType type, ExtractionResult extractionResult)
    {
        if (type == DocumentType.DepositSlip)
        {
            return new NormalizedDocument
            {
                DocumentId = extractionResult.DocumentId,
                Type = DocumentType.DepositSlip,
                Amount = decimal.TryParse(extractionResult.GetFieldValue("Amount"), out var depositAmount) ? depositAmount : 0,
                AccountNumber = extractionResult.GetFieldValue("AccountNumber"),
                RoutingNumber = extractionResult.GetFieldValue("RoutingNumber"),
                DepositSlipNumber = extractionResult.GetFieldValue("DepositSlipNumber"),
                CashAmount = decimal.TryParse(extractionResult.GetFieldValue("CashAmount"), out var cashAmount) ? cashAmount : 0,
                CheckAmount = decimal.TryParse(extractionResult.GetFieldValue("CheckAmount"), out var checkAmount) ? checkAmount : 0,
                ProcessingModelId = extractionResult.ModelId,
                ProcessingConfidence = extractionResult.OverallConfidence,
                ProcessingWarnings = extractionResult.GetLowConfidenceFields(0.72).Keys
                    .Select(field => $"Low confidence for '{field}'")
                    .ToList(),
                DepositItems =
                [
                    new DepositItem
                    {
                        ItemType = DepositItemType.Cash,
                        Amount = decimal.TryParse(extractionResult.GetFieldValue("CashAmount"), out var parsedCash) ? parsedCash : 0,
                        Confidence = extractionResult.ExtractedFields.TryGetValue("CashAmount", out var cashField) ? cashField.Confidence : 0
                    },
                    new DepositItem
                    {
                        ItemType = DepositItemType.Check,
                        CheckNumber = "4471",
                        Amount = decimal.TryParse(extractionResult.GetFieldValue("CheckAmount"), out var parsedCheckAmount) ? parsedCheckAmount : 0,
                        Confidence = extractionResult.ExtractedFields.TryGetValue("CheckAmount", out var checkField) ? checkField.Confidence : 0
                    }
                ]
            };
        }

        return new NormalizedDocument
        {
            DocumentId = extractionResult.DocumentId,
            Type = DocumentType.BankCheck,
            Amount = decimal.TryParse(extractionResult.GetFieldValue("Amount"), out var checkAmountValue) ? checkAmountValue : 0,
            AccountNumber = extractionResult.GetFieldValue("AccountNumber"),
            RoutingNumber = extractionResult.GetFieldValue("RoutingNumber"),
            CheckNumber = extractionResult.GetFieldValue("CheckNumber"),
            PayToName = extractionResult.GetFieldValue("PayToName"),
            Memo = extractionResult.GetFieldValue("Memo"),
            ProcessingModelId = extractionResult.ModelId,
            ProcessingConfidence = extractionResult.OverallConfidence,
            ProcessingWarnings = extractionResult.GetLowConfidenceFields(0.72).Keys
                .Select(field => $"Low confidence for '{field}'")
                .ToList()
        };
    }

    private static IEnumerable<NormalizedFieldDisplayItem> GetNormalizedFields(NormalizedDocument document)
    {
        var commonFields = new List<NormalizedFieldDisplayItem>
        {
            new() { Label = "Document Type", Value = document.Type.ToString() },
            new() { Label = "Amount", Value = document.Amount?.ToString("C") ?? "-" },
            new() { Label = "Account Number", Value = document.AccountNumber ?? "-" },
            new() { Label = "Routing Number", Value = document.RoutingNumber ?? "-" },
            new() { Label = "Model ID", Value = document.ProcessingModelId ?? "-" },
            new() { Label = "Processing Confidence", Value = document.ProcessingConfidence.ToString("P1") }
        };

        if (document.Type == DocumentType.BankCheck)
        {
            commonFields.AddRange(
            [
                new() { Label = "Check Number", Value = document.CheckNumber ?? "-" },
                new() { Label = "Pay To", Value = document.PayToName ?? "-" },
                new() { Label = "Memo", Value = document.Memo ?? "-" }
            ]);
        }

        if (document.Type == DocumentType.DepositSlip)
        {
            commonFields.AddRange(
            [
                new() { Label = "Deposit Slip #", Value = document.DepositSlipNumber ?? "-" },
                new() { Label = "Cash Amount", Value = document.CashAmount?.ToString("C") ?? "-" },
                new() { Label = "Check Amount", Value = document.CheckAmount?.ToString("C") ?? "-" },
                new() { Label = "Deposit Items", Value = document.DepositItems.Count.ToString() }
            ]);
        }

        return commonFields;
    }
}
