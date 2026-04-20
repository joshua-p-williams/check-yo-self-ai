using CheckYoSelfAI.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using ImagePickSource = CheckYoSelfAI.Services.Interfaces.ImageSource;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.ViewModels;

public class DocumentUploadViewModel : BaseViewModel
{
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

    private Microsoft.Maui.Controls.ImageSource? _previewImageSource;
    private string _selectedFileName = "No file selected";
    private string _detectedFormat = "-";
    private string _fileSizeDisplay = "-";
    private string _dimensionsDisplay = "-";
    private string _lastModifiedDisplay = "-";
    private string? _statusMessage;
    private string? _validationWarnings;

    public DocumentUploadViewModel(IImageService imageService, ILogger<DocumentUploadViewModel> logger)
        : base(logger)
    {
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));

        Title = "Document Upload";

        CaptureImageCommand = new AsyncRelayCommand(() => PickImageAsync(ImagePickSource.Camera), () => IsNotBusy);
        SelectImageCommand = new AsyncRelayCommand(() => PickImageAsync(ImagePickSource.PhotoLibrary), () => IsNotBusy);
        ClearImageCommand = new RelayCommand(ClearSelection, () => IsNotBusy && HasSelectedImage);
    }

    public IAsyncRelayCommand CaptureImageCommand { get; }

    public IAsyncRelayCommand SelectImageCommand { get; }

    public IRelayCommand ClearImageCommand { get; }

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

            var imageResult = await _imageService.PickImageAsync(source);
            if (!imageResult.IsSuccess || string.IsNullOrWhiteSpace(imageResult.ImagePath))
            {
                StatusMessage = imageResult.ErrorMessage ?? "Image selection was cancelled.";
                return;
            }

            var validationResult = await _imageService.ValidateImageAsync(imageResult.ImagePath, UploadValidationOptions);
            if (!validationResult.IsValid)
            {
                StatusMessage = validationResult.Errors.FirstOrDefault() ?? "Selected file did not pass validation.";
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
            StatusMessage = "Image ready for processing.";
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
    }
}
