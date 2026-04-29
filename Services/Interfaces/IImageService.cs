namespace CheckYoSelfAI.Services.Interfaces;

/// <summary>
/// Service interface for image operations including capture, validation, and processing.
/// Provides platform-agnostic image handling for document capture scenarios.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Picks an image from the specified source (camera or photo library).
    /// </summary>
    /// <param name="source">The source to pick the image from</param>
    /// <param name="allowsEditing">Whether to allow basic editing (crop/rotate) after capture</param>
    /// <returns>ImageResult containing the picked image or error information</returns>
    Task<ImageResult> PickImageAsync(ImageSource source, bool allowsEditing = true);

    /// <summary>
    /// Picks multiple images from the photo library.
    /// </summary>
    /// <param name="maxCount">Maximum number of images to select (default: 10)</param>
    /// <returns>Collection of ImageResult objects</returns>
    Task<IEnumerable<ImageResult>> PickMultipleImagesAsync(int maxCount = 10);

    /// <summary>
    /// Validates an image for format, size, and quality requirements.
    /// </summary>
    /// <param name="imagePath">Path to the image file</param>
    /// <param name="validationOptions">Validation criteria to apply</param>
    /// <returns>ImageValidationResult containing validation status and details</returns>
    Task<ImageValidationResult> ValidateImageAsync(string imagePath, ImageValidationOptions? validationOptions = null);

    /// <summary>
    /// Validates image data from a stream.
    /// </summary>
    /// <param name="imageStream">Stream containing image data</param>
    /// <param name="validationOptions">Validation criteria to apply</param>
    /// <returns>ImageValidationResult containing validation status and details</returns>
    Task<ImageValidationResult> ValidateImageAsync(Stream imageStream, ImageValidationOptions? validationOptions = null);

    /// <summary>
    /// Compresses an image to reduce file size while maintaining acceptable quality.
    /// </summary>
    /// <param name="sourcePath">Path to the source image</param>
    /// <param name="destinationPath">Path for the compressed image</param>
    /// <param name="compressionOptions">Compression settings to apply</param>
    /// <returns>ImageCompressionResult containing compression details</returns>
    Task<ImageCompressionResult> CompressImageAsync(string sourcePath, string destinationPath, ImageCompressionOptions? compressionOptions = null);

    /// <summary>
    /// Compresses an image from a stream.
    /// </summary>
    /// <param name="sourceStream">Stream containing source image data</param>
    /// <param name="destinationStream">Stream to write compressed image data</param>
    /// <param name="compressionOptions">Compression settings to apply</param>
    /// <returns>ImageCompressionResult containing compression details</returns>
    Task<ImageCompressionResult> CompressImageAsync(Stream sourceStream, Stream destinationStream, ImageCompressionOptions? compressionOptions = null);

    /// <summary>
    /// Extracts metadata and information from an image file.
    /// </summary>
    /// <param name="imagePath">Path to the image file</param>
    /// <returns>ImageInfo containing metadata and properties</returns>
    Task<ImageInfo> GetImageInfoAsync(string imagePath);

    /// <summary>
    /// Extracts metadata and information from an image stream.
    /// </summary>
    /// <param name="imageStream">Stream containing image data</param>
    /// <returns>ImageInfo containing metadata and properties</returns>
    Task<ImageInfo> GetImageInfoAsync(Stream imageStream);

    /// <summary>
    /// Saves an image to the device's photo library or documents folder.
    /// </summary>
    /// <param name="imageData">Image data to save</param>
    /// <param name="filename">Filename for the saved image</param>
    /// <param name="saveLocation">Location to save the image</param>
    /// <returns>Path to the saved image file</returns>
    Task<string> SaveImageAsync(byte[] imageData, string filename, ImageSaveLocation saveLocation = ImageSaveLocation.Documents);

    /// <summary>
    /// Saves an image stream to the device's photo library or documents folder.
    /// </summary>
    /// <param name="imageStream">Stream containing image data</param>
    /// <param name="filename">Filename for the saved image</param>
    /// <param name="saveLocation">Location to save the image</param>
    /// <returns>Path to the saved image file</returns>
    Task<string> SaveImageAsync(Stream imageStream, string filename, ImageSaveLocation saveLocation = ImageSaveLocation.Documents);

    /// <summary>
    /// Deletes an image file from the device storage.
    /// </summary>
    /// <param name="imagePath">Path to the image file to delete</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteImageAsync(string imagePath);

    /// <summary>
    /// Checks if the device has camera capabilities.
    /// </summary>
    /// <returns>True if camera is available, false otherwise</returns>
    Task<bool> IsCameraAvailableAsync();

    /// <summary>
    /// Checks if the device has photo library access.
    /// </summary>
    /// <returns>True if photo library is accessible, false otherwise</returns>
    Task<bool> IsPhotoLibraryAvailableAsync();

    /// <summary>
    /// Requests camera permission from the user.
    /// </summary>
    /// <returns>Permission status after request</returns>
    Task<PermissionStatus> RequestCameraPermissionAsync();

    /// <summary>
    /// Requests photo library permission from the user.
    /// </summary>
    /// <returns>Permission status after request</returns>
    Task<PermissionStatus> RequestPhotoLibraryPermissionAsync();
}

/// <summary>
/// Source for image picking operations.
/// </summary>
public enum ImageSource
{
    /// <summary>
    /// Pick from camera (take new photo).
    /// </summary>
    Camera,

    /// <summary>
    /// Pick from photo library/gallery.
    /// </summary>
    PhotoLibrary,

    /// <summary>
    /// Allow user to choose between camera and photo library.
    /// </summary>
    UserChoice
}

/// <summary>
/// Location to save images.
/// </summary>
public enum ImageSaveLocation
{
    /// <summary>
    /// Save to app's documents folder.
    /// </summary>
    Documents,

    /// <summary>
    /// Save to device's photo library.
    /// </summary>
    PhotoLibrary,

    /// <summary>
    /// Save to app's temporary folder (may be cleaned up by system).
    /// </summary>
    Temporary,

    /// <summary>
    /// Save to app's cache folder.
    /// </summary>
    Cache
}

/// <summary>
/// Permission status for image-related operations.
/// </summary>
public enum PermissionStatus
{
    /// <summary>
    /// Permission status is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Permission has been denied by the user.
    /// </summary>
    Denied,

    /// <summary>
    /// Permission has been granted by the user.
    /// </summary>
    Granted,

    /// <summary>
    /// Permission is restricted (e.g., parental controls).
    /// </summary>
    Restricted,

    /// <summary>
    /// Permission request is not supported on this platform.
    /// </summary>
    NotSupported
}

/// <summary>
/// Result of an image picking operation.
/// </summary>
public class ImageResult
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Path to the picked image file (if successful).
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Image data as byte array (if available).
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// Original filename of the picked image.
    /// </summary>
    public string? OriginalFilename { get; set; }

    /// <summary>
    /// Content type/MIME type of the image.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detailed error information.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Source from which the image was picked.
    /// </summary>
    public ImageSource Source { get; set; }

    /// <summary>
    /// Timestamp when the image was picked.
    /// </summary>
    public DateTime PickedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful ImageResult.
    /// </summary>
    public static ImageResult Success(string imagePath, string? originalFilename = null, string? contentType = null, ImageSource source = ImageSource.UserChoice)
    {
        return new ImageResult
        {
            IsSuccess = true,
            ImagePath = imagePath,
            OriginalFilename = originalFilename,
            ContentType = contentType,
            Source = source
        };
    }

    /// <summary>
    /// Creates a failed ImageResult.
    /// </summary>
    public static ImageResult Failure(string errorMessage, Exception? exception = null, ImageSource source = ImageSource.UserChoice)
    {
        return new ImageResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Exception = exception,
            Source = source
        };
    }
}

/// <summary>
/// Options for image validation.
/// </summary>
public class ImageValidationOptions
{
    /// <summary>
    /// Maximum allowed file size in bytes (default: 10MB).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Minimum allowed file size in bytes (default: 1KB).
    /// </summary>
    public long MinFileSizeBytes { get; set; } = 1024; // 1KB

    /// <summary>
    /// Maximum allowed image width in pixels (default: 4000px).
    /// </summary>
    public int MaxWidth { get; set; } = 4000;

    /// <summary>
    /// Maximum allowed image height in pixels (default: 4000px).
    /// </summary>
    public int MaxHeight { get; set; } = 4000;

    /// <summary>
    /// Minimum allowed image width in pixels (default: 100px).
    /// </summary>
    public int MinWidth { get; set; } = 100;

    /// <summary>
    /// Minimum allowed image height in pixels (default: 100px).
    /// </summary>
    public int MinHeight { get; set; } = 100;

    /// <summary>
    /// Allowed image formats (default: JPEG, PNG).
    /// </summary>
    public HashSet<string> AllowedFormats { get; set; } = new() { "JPEG", "JPG", "PNG" };

    /// <summary>
    /// Whether to perform detailed quality analysis (default: false).
    /// </summary>
    public bool PerformQualityAnalysis { get; set; } = false;

    /// <summary>
    /// Minimum required image quality score (0-100, default: 30).
    /// </summary>
    public int MinQualityScore { get; set; } = 30;

    /// <summary>
    /// Creates default validation options suitable for document capture.
    /// </summary>
    public static ImageValidationOptions ForDocuments()
    {
        return new ImageValidationOptions
        {
            MaxFileSizeBytes = 15 * 1024 * 1024, // 15MB for high-quality document scans
            MinFileSizeBytes = 50 * 1024, // 50KB minimum
            MaxWidth = 5000,
            MaxHeight = 5000,
            MinWidth = 200, // Ensure document is readable
            MinHeight = 200,
            AllowedFormats = new() { "JPEG", "JPG", "PNG" },
            PerformQualityAnalysis = true,
            MinQualityScore = 40 // Higher quality for OCR processing
        };
    }
}

/// <summary>
/// Result of image validation.
/// </summary>
public class ImageValidationResult
{
    /// <summary>
    /// Indicates whether the image passed validation.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings (non-blocking issues).
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Image quality score (0-100) if quality analysis was performed.
    /// </summary>
    public int? QualityScore { get; set; }

    /// <summary>
    /// Detected image format.
    /// </summary>
    public string? DetectedFormat { get; set; }

    /// <summary>
    /// Image file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ImageValidationResult Valid(string format, long fileSize, int width, int height)
    {
        return new ImageValidationResult
        {
            IsValid = true,
            DetectedFormat = format,
            FileSizeBytes = fileSize,
            Width = width,
            Height = height
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static ImageValidationResult Invalid(params string[] errors)
    {
        return new ImageValidationResult
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }
}

/// <summary>
/// Options for image compression.
/// </summary>
public class ImageCompressionOptions
{
    /// <summary>
    /// Target quality for compression (0-100, default: 85).
    /// </summary>
    public int Quality { get; set; } = 85;

    /// <summary>
    /// Maximum width for the compressed image (default: 2000px).
    /// </summary>
    public int MaxWidth { get; set; } = 2000;

    /// <summary>
    /// Maximum height for the compressed image (default: 2000px).
    /// </summary>
    public int MaxHeight { get; set; } = 2000;

    /// <summary>
    /// Target file size in bytes (0 for no size limit, default: 0).
    /// </summary>
    public long TargetFileSizeBytes { get; set; } = 0;

    /// <summary>
    /// Output format for the compressed image (default: JPEG).
    /// </summary>
    public string OutputFormat { get; set; } = "JPEG";

    /// <summary>
    /// Whether to preserve EXIF metadata (default: false for privacy).
    /// </summary>
    public bool PreserveExifData { get; set; } = false;

    /// <summary>
    /// Creates compression options optimized for document processing.
    /// </summary>
    public static ImageCompressionOptions ForDocuments()
    {
        return new ImageCompressionOptions
        {
            Quality = 90, // High quality for OCR processing
            MaxWidth = 3000,
            MaxHeight = 3000,
            TargetFileSizeBytes = 5 * 1024 * 1024, // 5MB target
            OutputFormat = "JPEG",
            PreserveExifData = false
        };
    }

    /// <summary>
    /// Creates compression options for thumbnail generation.
    /// </summary>
    public static ImageCompressionOptions ForThumbnails()
    {
        return new ImageCompressionOptions
        {
            Quality = 75,
            MaxWidth = 300,
            MaxHeight = 300,
            TargetFileSizeBytes = 100 * 1024, // 100KB target
            OutputFormat = "JPEG",
            PreserveExifData = false
        };
    }
}

/// <summary>
/// Result of image compression.
/// </summary>
public class ImageCompressionResult
{
    /// <summary>
    /// Indicates whether compression was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Path to the compressed image file.
    /// </summary>
    public string? CompressedImagePath { get; set; }

    /// <summary>
    /// Original file size in bytes.
    /// </summary>
    public long OriginalSizeBytes { get; set; }

    /// <summary>
    /// Compressed file size in bytes.
    /// </summary>
    public long CompressedSizeBytes { get; set; }

    /// <summary>
    /// Compression ratio (compressed size / original size).
    /// </summary>
    public double CompressionRatio => OriginalSizeBytes > 0 ? (double)CompressedSizeBytes / OriginalSizeBytes : 0;

    /// <summary>
    /// Space saved in bytes.
    /// </summary>
    public long SpaceSavedBytes => OriginalSizeBytes - CompressedSizeBytes;

    /// <summary>
    /// Space saved as percentage.
    /// </summary>
    public double SpaceSavedPercentage => OriginalSizeBytes > 0 ? (double)SpaceSavedBytes / OriginalSizeBytes * 100 : 0;

    /// <summary>
    /// Error message if compression failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Exception details if compression failed.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Creates a successful compression result.
    /// </summary>
    public static ImageCompressionResult Success(string compressedPath, long originalSize, long compressedSize)
    {
        return new ImageCompressionResult
        {
            IsSuccess = true,
            CompressedImagePath = compressedPath,
            OriginalSizeBytes = originalSize,
            CompressedSizeBytes = compressedSize
        };
    }

    /// <summary>
    /// Creates a failed compression result.
    /// </summary>
    public static ImageCompressionResult Failure(string errorMessage, Exception? exception = null)
    {
        return new ImageCompressionResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Information about an image file.
/// </summary>
public class ImageInfo
{
    /// <summary>
    /// Image file path.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Image format (JPEG, PNG, etc.).
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Color depth in bits per pixel.
    /// </summary>
    public int? ColorDepth { get; set; }

    /// <summary>
    /// Horizontal DPI/resolution.
    /// </summary>
    public double? HorizontalResolution { get; set; }

    /// <summary>
    /// Vertical DPI/resolution.
    /// </summary>
    public double? VerticalResolution { get; set; }

    /// <summary>
    /// Date/time when the image was created.
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Date/time when the image was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// EXIF metadata (if available and preserved).
    /// </summary>
    public Dictionary<string, object> ExifData { get; set; } = new();

    /// <summary>
    /// Whether the image has transparency.
    /// </summary>
    public bool HasTransparency { get; set; }

    /// <summary>
    /// Image orientation (from EXIF data).
    /// </summary>
    public int? Orientation { get; set; }

    /// <summary>
    /// Estimated image quality score (0-100).
    /// </summary>
    public int? QualityScore { get; set; }

    /// <summary>
    /// Gets formatted file size string.
    /// </summary>
    public string FormattedFileSize
    {
        get
        {
            var size = FileSizeBytes;
            string[] units = { "B", "KB", "MB", "GB" };
            int unitIndex = 0;
            double displaySize = size;

            while (displaySize >= 1024 && unitIndex < units.Length - 1)
            {
                displaySize /= 1024;
                unitIndex++;
            }

            return $"{displaySize:F1} {units[unitIndex]}";
        }
    }

    /// <summary>
    /// Gets formatted resolution string.
    /// </summary>
    public string FormattedResolution => $"{Width} x {Height}";

    /// <summary>
    /// Gets megapixel count.
    /// </summary>
    public double Megapixels => (Width * Height) / 1_000_000.0;
}

/// <summary>
/// Exception thrown when an image operation fails.
/// </summary>
public class ImageException : Exception
{
    public string? ImagePath { get; }
    public string? Operation { get; }
    public new ImageSource? Source { get; }  // Use 'new' to explicitly hide the inherited Source property

    public ImageException(string message) : base(message) { }

    public ImageException(string message, string? imagePath, string? operation) : base(message)
    {
        ImagePath = imagePath;
        Operation = operation;
    }

    public ImageException(string message, string? imagePath, string? operation, ImageSource? source) : base(message)
    {
        ImagePath = imagePath;
        Operation = operation;
        Source = source;
    }

    public ImageException(string message, Exception innerException) : base(message, innerException) { }

    public ImageException(string message, Exception innerException, string? imagePath, string? operation) 
        : base(message, innerException)
    {
        ImagePath = imagePath;
        Operation = operation;
    }

    public ImageException(string message, Exception innerException, string? imagePath, string? operation, ImageSource? source) 
        : base(message, innerException)
    {
        ImagePath = imagePath;
        Operation = operation;
        Source = source;
    }
}