using CheckYoSelfAI.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using ImagePickSource = CheckYoSelfAI.Services.Interfaces.ImageSource;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CheckYoSelfAI.ViewModels;

public class DocumentUploadViewModel : BaseViewModel
{
    private enum ProcessingPipelineStage
    {
        Upload,
        Classify,
        Route,
        Extract,
        Normalize
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

    public DocumentUploadViewModel(IImageService imageService, ILogger<DocumentUploadViewModel> logger)
        : base(logger)
    {
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));

        Title = "Document Upload";

        CaptureImageCommand = new AsyncRelayCommand(() => PickImageAsync(ImagePickSource.Camera), () => IsNotBusy);
        SelectImageCommand = new AsyncRelayCommand(() => PickImageAsync(ImagePickSource.PhotoLibrary), () => IsNotBusy);
        ClearImageCommand = new RelayCommand(ClearSelection, () => IsNotBusy && HasSelectedImage);

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

        ResetProcessingTimeline();
    }

    public IAsyncRelayCommand CaptureImageCommand { get; }

    public IAsyncRelayCommand SelectImageCommand { get; }

    public IRelayCommand ClearImageCommand { get; }

    public ObservableCollection<ProcessingTimelineStage> ProcessingTimelineStages { get; }

    public Microsoft.Maui.Controls.ImageSource? PreviewImageSource
    {
        get => _previewImageSource;
        private set
        {
            if (SetProperty(ref _previewImageSource, value))
            {
                OnPropertyChanged(nameof(HasSelectedImage));
                ClearImageCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasSelectedImage => PreviewImageSource != null;

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

    protected override void OnBusyChanged(bool isBusy)
    {
        base.OnBusyChanged(isBusy);
        CaptureImageCommand.NotifyCanExecuteChanged();
        SelectImageCommand.NotifyCanExecuteChanged();
        ClearImageCommand.NotifyCanExecuteChanged();
    }

    private async Task PickImageAsync(ImagePickSource source)
    {
        await ExecuteWithBusyAsync(async _ =>
        {
            StatusMessage = null;
            ValidationWarnings = null;
            ResetProcessingTimeline();
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
            StatusMessage = "Image ready for processing. Classification will begin when processing starts.";
        }, "Unable to select image right now.");
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
    }

    private void ResetProcessingTimeline()
    {
        foreach (var stage in ProcessingTimelineStages)
        {
            stage.SetPending();
        }

        _activeTimelineStage = null;
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

        _activeTimelineStage = timelineStage;
        _activeTimelineStageStartedAt = DateTimeOffset.UtcNow;

        timelineStage.SetInProgress();
        CurrentProcessingStage = timelineStage.Title;
        TimelineErrorMessage = null;
    }

    private void CompleteActiveProcessingStage()
    {
        if (_activeTimelineStage == null)
        {
            return;
        }

        _activeTimelineStage.SetCompleted(GetActiveStageDuration());
        _activeTimelineStage = null;
        _activeTimelineStageStartedAt = null;
        CurrentProcessingStage = null;
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
        _activeTimelineStageStartedAt = null;
        CurrentProcessingStage = null;
        TimelineErrorMessage = errorMessage;
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
}
