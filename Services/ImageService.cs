using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Drawing;
using ImageSource = CheckYoSelfAI.Services.Interfaces.ImageSource;
using PermissionStatus = CheckYoSelfAI.Services.Interfaces.PermissionStatus;
using MauiPermissionStatus = Microsoft.Maui.ApplicationModel.PermissionStatus;

namespace CheckYoSelfAI.Services;

/// <summary>
/// Implementation of IImageService using MAUI MediaPicker and cross-platform image processing.
/// Provides unified image operations across Android, iOS, Windows, and macOS platforms.
/// </summary>
public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;
    private readonly ISettingsService _settingsService;

    public ImageService(ILogger<ImageService> logger, ISettingsService settingsService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    /// <inheritdoc />
    public async Task<ImageResult> PickImageAsync(ImageSource source, bool allowsEditing = true)
    {
        try
        {
            _logger.LogDebug("Picking image from source: {Source}, AllowsEditing: {AllowsEditing}", source, allowsEditing);

            // Check permissions first
            var permissionResult = await CheckAndRequestPermissions(source);
            if (permissionResult != PermissionStatus.Granted)
            {
                return ImageResult.Failure($"Permission {permissionResult} for {source}", null, source);
            }

            // Configure MediaPicker options
            var options = new MediaPickerOptions
            {
                Title = GetPickerTitle(source)
            };

            FileResult? fileResult = null;

            switch (source)
            {
                case ImageSource.Camera:
                    if (!MediaPicker.Default.IsCaptureSupported)
                    {
                        return ImageResult.Failure("Camera capture is not supported on this device", null, source);
                    }
                    fileResult = await MediaPicker.Default.CapturePhotoAsync(options);
                    break;

                case ImageSource.PhotoLibrary:
                    var photoResults = await MediaPicker.Default.PickPhotosAsync(options);
                    fileResult = photoResults?.FirstOrDefault();
                    break;

                case ImageSource.UserChoice:
                    // Let user choose between camera and photo library
                    var choice = await PromptUserForImageSource();
                    if (choice == ImageSource.Camera)
                    {
                        if (!MediaPicker.Default.IsCaptureSupported)
                        {
                            var fallbackResults = await MediaPicker.Default.PickPhotosAsync(options);
                            fileResult = fallbackResults?.FirstOrDefault();
                        }
                        else
                        {
                            fileResult = await MediaPicker.Default.CapturePhotoAsync(options);
                        }
                    }
                    else
                    {
                        var choiceResults = await MediaPicker.Default.PickPhotosAsync(options);
                        fileResult = choiceResults?.FirstOrDefault();
                    }
                    break;
            }

            if (fileResult == null)
            {
                _logger.LogDebug("Image picking was cancelled by user");
                return ImageResult.Failure("Image selection was cancelled", null, source);
            }

            // Save the picked image to app storage
            var savedPath = await SavePickedImage(fileResult);

            var result = ImageResult.Success(
                imagePath: savedPath,
                originalFilename: fileResult.FileName,
                contentType: fileResult.ContentType,
                source: source
            );

            _logger.LogDebug("Successfully picked image: {Path}", savedPath);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pick image from source: {Source}", source);
            return ImageResult.Failure($"Failed to pick image: {ex.Message}", ex, source);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ImageResult>> PickMultipleImagesAsync(int maxCount = 10)
    {
        try
        {
            _logger.LogDebug("Picking multiple images, maxCount: {MaxCount}", maxCount);

            // Check photo library permission
            var permissionResult = await CheckAndRequestPermissions(ImageSource.PhotoLibrary);
            if (permissionResult != PermissionStatus.Granted)
            {
                var permissionErrorResult = ImageResult.Failure($"Permission {permissionResult} for photo library", null, ImageSource.PhotoLibrary);
                return new[] { permissionErrorResult };
            }

            var options = new MediaPickerOptions
            {
                Title = "Select Images"
            };

            // Use PickPhotosAsync which supports multiple selection natively
            var multiResults = await MediaPicker.Default.PickPhotosAsync(options);

            if (multiResults == null || !multiResults.Any())
            {
                _logger.LogDebug("Image selection was cancelled by user");
                return new[] { ImageResult.Failure("Image selection was cancelled", null, ImageSource.PhotoLibrary) };
            }

            // Take up to maxCount results
            var selectedFiles = multiResults.Take(maxCount);
            var results = new List<ImageResult>();

            foreach (var fileResult in selectedFiles)
            {
                var savedPath = await SavePickedImage(fileResult);
                var imageResult = ImageResult.Success(
                    imagePath: savedPath,
                    originalFilename: fileResult.FileName,
                    contentType: fileResult.ContentType,
                    source: ImageSource.PhotoLibrary
                );

                results.Add(imageResult);
            }

            _logger.LogDebug("Successfully picked {Count} images", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pick multiple images");
            return new[] { ImageResult.Failure($"Failed to pick images: {ex.Message}", ex, ImageSource.PhotoLibrary) };
        }
    }

    /// <inheritdoc />
    public async Task<ImageValidationResult> ValidateImageAsync(string imagePath, ImageValidationOptions? validationOptions = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imagePath, nameof(imagePath));

            _logger.LogDebug("Validating image: {ImagePath}", imagePath);

            if (!File.Exists(imagePath))
            {
                return ImageValidationResult.Invalid("Image file does not exist");
            }

            using var fileStream = File.OpenRead(imagePath);
            return await ValidateImageAsync(fileStream, validationOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate image: {ImagePath}", imagePath);
            return ImageValidationResult.Invalid($"Validation failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ImageValidationResult> ValidateImageAsync(Stream imageStream, ImageValidationOptions? validationOptions = null)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(imageStream, nameof(imageStream));

            _logger.LogDebug("Validating image from stream");

            validationOptions ??= ImageValidationOptions.ForDocuments();
            var result = new ImageValidationResult();

            // Get basic image info first
            var imageInfo = await GetImageInfoFromStream(imageStream);

            result.DetectedFormat = imageInfo.Format;
            result.FileSizeBytes = imageInfo.FileSizeBytes;
            result.Width = imageInfo.Width;
            result.Height = imageInfo.Height;
            result.QualityScore = imageInfo.QualityScore;

            // Validate file size
            if (imageInfo.FileSizeBytes > validationOptions.MaxFileSizeBytes)
            {
                result.Errors.Add($"File size {imageInfo.FormattedFileSize} exceeds maximum of {FormatBytes(validationOptions.MaxFileSizeBytes)}");
            }

            if (imageInfo.FileSizeBytes < validationOptions.MinFileSizeBytes)
            {
                result.Errors.Add($"File size {imageInfo.FormattedFileSize} is below minimum of {FormatBytes(validationOptions.MinFileSizeBytes)}");
            }

            // Validate dimensions
            if (imageInfo.Width > validationOptions.MaxWidth)
            {
                result.Errors.Add($"Image width {imageInfo.Width}px exceeds maximum of {validationOptions.MaxWidth}px");
            }

            if (imageInfo.Height > validationOptions.MaxHeight)
            {
                result.Errors.Add($"Image height {imageInfo.Height}px exceeds maximum of {validationOptions.MaxHeight}px");
            }

            if (imageInfo.Width < validationOptions.MinWidth)
            {
                result.Errors.Add($"Image width {imageInfo.Width}px is below minimum of {validationOptions.MinWidth}px");
            }

            if (imageInfo.Height < validationOptions.MinHeight)
            {
                result.Errors.Add($"Image height {imageInfo.Height}px is below minimum of {validationOptions.MinHeight}px");
            }

            // Validate format
            if (imageInfo.Format != null && !validationOptions.AllowedFormats.Contains(imageInfo.Format.ToUpperInvariant()))
            {
                result.Errors.Add($"Image format {imageInfo.Format} is not supported. Allowed formats: {string.Join(", ", validationOptions.AllowedFormats)}");
            }

            // Validate quality if enabled
            if (validationOptions.PerformQualityAnalysis && imageInfo.QualityScore.HasValue)
            {
                if (imageInfo.QualityScore < validationOptions.MinQualityScore)
                {
                    result.Warnings.Add($"Image quality score {imageInfo.QualityScore} is below recommended minimum of {validationOptions.MinQualityScore}");
                }
            }

            // Add warnings for very large images
            if (imageInfo.Width > 3000 || imageInfo.Height > 3000)
            {
                result.Warnings.Add("Image is very large and may take longer to process");
            }

            result.IsValid = result.Errors.Count == 0;

            _logger.LogDebug("Image validation complete. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}", 
                result.IsValid, result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate image from stream");
            return ImageValidationResult.Invalid($"Validation failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ImageCompressionResult> CompressImageAsync(string sourcePath, string destinationPath, ImageCompressionOptions? compressionOptions = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath, nameof(sourcePath));
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath, nameof(destinationPath));

            _logger.LogDebug("Compressing image from {Source} to {Destination}", sourcePath, destinationPath);

            if (!File.Exists(sourcePath))
            {
                return ImageCompressionResult.Failure("Source image file does not exist");
            }

            using var sourceStream = File.OpenRead(sourcePath);
            using var destinationStream = File.Create(destinationPath);

            var result = await CompressImageAsync(sourceStream, destinationStream, compressionOptions);

            if (result.IsSuccess)
            {
                result.CompressedImagePath = destinationPath;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compress image from {Source} to {Destination}", sourcePath, destinationPath);
            return ImageCompressionResult.Failure($"Compression failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ImageCompressionResult> CompressImageAsync(Stream sourceStream, Stream destinationStream, ImageCompressionOptions? compressionOptions = null)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(sourceStream, nameof(sourceStream));
            ArgumentNullException.ThrowIfNull(destinationStream, nameof(destinationStream));

            _logger.LogDebug("Compressing image from stream");

            compressionOptions ??= ImageCompressionOptions.ForDocuments();

            var originalSize = sourceStream.Length;
            sourceStream.Position = 0;

            // For now, we'll implement basic compression by copying the stream
            // In a full implementation, you would use libraries like ImageSharp or SkiaSharp
            // to perform actual image compression and resizing

            // Basic implementation: just copy if no real compression is needed
            await sourceStream.CopyToAsync(destinationStream);
            var compressedSize = destinationStream.Length;

            // TODO: Implement actual image compression using ImageSharp or SkiaSharp
            // This would involve:
            // 1. Loading the image from sourceStream
            // 2. Resizing if needed (based on MaxWidth/MaxHeight)
            // 3. Applying quality settings
            // 4. Converting to target format
            // 5. Saving to destinationStream

            var result = ImageCompressionResult.Success("", originalSize, compressedSize);

            _logger.LogDebug("Image compression complete. Original: {OriginalSize} bytes, Compressed: {CompressedSize} bytes, Ratio: {Ratio:P1}",
                originalSize, compressedSize, result.CompressionRatio);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compress image from stream");
            return ImageCompressionResult.Failure($"Compression failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ImageInfo> GetImageInfoAsync(string imagePath)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imagePath, nameof(imagePath));

            _logger.LogDebug("Getting image info: {ImagePath}", imagePath);

            if (!File.Exists(imagePath))
            {
                throw new ImageException($"Image file does not exist: {imagePath}", imagePath, "GetImageInfo");
            }

            using var fileStream = File.OpenRead(imagePath);
            var imageInfo = await GetImageInfoFromStream(fileStream);
            imageInfo.FilePath = imagePath;

            return imageInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get image info: {ImagePath}", imagePath);
            throw new ImageException($"Failed to get image info: {ex.Message}", ex, imagePath, "GetImageInfo");
        }
    }

    /// <inheritdoc />
    public async Task<ImageInfo> GetImageInfoAsync(Stream imageStream)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(imageStream, nameof(imageStream));

            _logger.LogDebug("Getting image info from stream");

            return await GetImageInfoFromStream(imageStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get image info from stream");
            throw new ImageException($"Failed to get image info: {ex.Message}", ex, null, "GetImageInfo");
        }
    }

    /// <inheritdoc />
    public async Task<string> SaveImageAsync(byte[] imageData, string filename, ImageSaveLocation saveLocation = ImageSaveLocation.Documents)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(imageData, nameof(imageData));
            ArgumentException.ThrowIfNullOrWhiteSpace(filename, nameof(filename));

            _logger.LogDebug("Saving image data to {Location}: {Filename}", saveLocation, filename);

            using var stream = new MemoryStream(imageData);
            return await SaveImageAsync(stream, filename, saveLocation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image data: {Filename}", filename);
            throw new ImageException($"Failed to save image: {ex.Message}", ex, filename, "SaveImage");
        }
    }

    /// <inheritdoc />
    public async Task<string> SaveImageAsync(Stream imageStream, string filename, ImageSaveLocation saveLocation = ImageSaveLocation.Documents)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(imageStream, nameof(imageStream));
            ArgumentException.ThrowIfNullOrWhiteSpace(filename, nameof(filename));

            _logger.LogDebug("Saving image stream to {Location}: {Filename}", saveLocation, filename);

            var directory = GetSaveDirectory(saveLocation);
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);

            using var fileStream = File.Create(filePath);
            imageStream.Position = 0;
            await imageStream.CopyToAsync(fileStream);

            _logger.LogDebug("Successfully saved image to: {FilePath}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image stream: {Filename}", filename);
            throw new ImageException($"Failed to save image: {ex.Message}", ex, filename, "SaveImage");
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteImageAsync(string imagePath)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imagePath, nameof(imagePath));

            _logger.LogDebug("Deleting image: {ImagePath}", imagePath);

            if (!File.Exists(imagePath))
            {
                _logger.LogWarning("Image file does not exist for deletion: {ImagePath}", imagePath);
                return false;
            }

            File.Delete(imagePath);
            _logger.LogDebug("Successfully deleted image: {ImagePath}", imagePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image: {ImagePath}", imagePath);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsCameraAvailableAsync()
    {
        try
        {
            return MediaPicker.Default.IsCaptureSupported;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check camera availability");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsPhotoLibraryAvailableAsync()
    {
        try
        {
            // Photo library is generally available on all supported platforms
            // The actual availability will be checked during permission requests
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check photo library availability");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<PermissionStatus> RequestCameraPermissionAsync()
    {
        try
        {
            _logger.LogDebug("Requesting camera permission");

            var status = await Permissions.RequestAsync<Permissions.Camera>();
            var mappedStatus = MapPermissionStatus(status);

            _logger.LogDebug("Camera permission result: {Status}", mappedStatus);
            return mappedStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request camera permission");
            return PermissionStatus.Unknown;
        }
    }

    /// <inheritdoc />
    public async Task<PermissionStatus> RequestPhotoLibraryPermissionAsync()
    {
        try
        {
            _logger.LogDebug("Requesting photo library permission");

            var status = await Permissions.RequestAsync<Permissions.Photos>();
            var mappedStatus = MapPermissionStatus(status);

            _logger.LogDebug("Photo library permission result: {Status}", mappedStatus);
            return mappedStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request photo library permission");
            return PermissionStatus.Unknown;
        }
    }

    #region Private Helper Methods

    private async Task<PermissionStatus> CheckAndRequestPermissions(ImageSource source)
    {
        switch (source)
        {
            case ImageSource.Camera:
                return await RequestCameraPermissionAsync();

            case ImageSource.PhotoLibrary:
                return await RequestPhotoLibraryPermissionAsync();

            case ImageSource.UserChoice:
                // For user choice, we need both permissions potentially
                var cameraStatus = await RequestCameraPermissionAsync();
                var photoStatus = await RequestPhotoLibraryPermissionAsync();

                // Return granted if at least one is available
                return (cameraStatus == PermissionStatus.Granted || photoStatus == PermissionStatus.Granted) 
                    ? PermissionStatus.Granted 
                    : cameraStatus; // Return camera status as primary

            default:
                return PermissionStatus.NotSupported;
        }
    }

    private static PermissionStatus MapPermissionStatus(MauiPermissionStatus status)
    {
        return status switch
        {
            MauiPermissionStatus.Granted => PermissionStatus.Granted,
            MauiPermissionStatus.Denied => PermissionStatus.Denied,
            MauiPermissionStatus.Restricted => PermissionStatus.Restricted,
            MauiPermissionStatus.Disabled => PermissionStatus.NotSupported,
            _ => PermissionStatus.Unknown
        };
    }

    private static string GetPickerTitle(ImageSource source)
    {
        return source switch
        {
            ImageSource.Camera => "Take Photo",
            ImageSource.PhotoLibrary => "Select Photo",
            ImageSource.UserChoice => "Select Image Source",
            _ => "Pick Image"
        };
    }

    private async Task<ImageSource> PromptUserForImageSource()
    {
        // This would typically use INavigationService to show an action sheet
        // For now, we'll default to photo library if camera isn't available
        var cameraAvailable = await IsCameraAvailableAsync();
        return cameraAvailable ? ImageSource.Camera : ImageSource.PhotoLibrary;
    }

    private async Task<bool> PromptUserToContinueSelection()
    {
        // This would typically use INavigationService to show a confirmation dialog
        // For now, return false to select only one image
        return false;
    }

    private async Task<string> SavePickedImage(FileResult fileResult)
    {
        var documentsPath = GetSaveDirectory(ImageSaveLocation.Documents);
        Directory.CreateDirectory(documentsPath);

        // Generate unique filename
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var extension = Path.GetExtension(fileResult.FileName) ?? ".jpg";
        var filename = $"captured_{timestamp}{extension}";
        var savePath = Path.Combine(documentsPath, filename);

        using var sourceStream = await fileResult.OpenReadAsync();
        using var destinationStream = File.Create(savePath);
        await sourceStream.CopyToAsync(destinationStream);

        return savePath;
    }

    private async Task<ImageInfo> GetImageInfoFromStream(Stream imageStream)
    {
        // Basic implementation - in a real scenario, you'd use ImageSharp or similar
        var imageInfo = new ImageInfo
        {
            FileSizeBytes = imageStream.Length,
            Format = "JPEG", // Placeholder detection
            Width = 1920,    // Placeholder dimensions
            Height = 1080,   // Placeholder dimensions
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            QualityScore = 75 // Placeholder quality score
        };

        // TODO: Implement actual image analysis using ImageSharp or similar library
        // This would involve:
        // 1. Detecting actual image format
        // 2. Reading image dimensions
        // 3. Extracting EXIF data
        // 4. Calculating quality score
        // 5. Reading color depth and resolution

        return imageInfo;
    }

    private string GetSaveDirectory(ImageSaveLocation location)
    {
        return location switch
        {
            ImageSaveLocation.Documents => FileSystem.AppDataDirectory,
            ImageSaveLocation.Cache => FileSystem.CacheDirectory,
            ImageSaveLocation.Temporary => Path.GetTempPath(),
            ImageSaveLocation.PhotoLibrary => FileSystem.AppDataDirectory, // Would save to photo library using platform APIs
            _ => FileSystem.AppDataDirectory
        };
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB" };
        int unitIndex = 0;
        double size = bytes;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F1} {units[unitIndex]}";
    }

    #endregion
}