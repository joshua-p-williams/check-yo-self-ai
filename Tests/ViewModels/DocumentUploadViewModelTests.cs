using CheckYoSelfAI.Services.Interfaces;
using CheckYoSelfAI.Tests.Common;
using CheckYoSelfAI.ViewModels;
using Moq;
using ImageSource = CheckYoSelfAI.Services.Interfaces.ImageSource;
using Xunit;

namespace CheckYoSelfAI.Tests.ViewModels;

public class DocumentUploadViewModelTests
{
    [Fact]
    public async Task SelectImageCommand_SetsPreviewMetadataAndStage()
    {
        var tempImagePath = Path.Combine(Path.GetTempPath(), $"upload-test-{Guid.NewGuid():N}.png");
        await File.WriteAllBytesAsync(tempImagePath, [137, 80, 78, 71, 13, 10, 26, 10]);

        var imageService = new Mock<IImageService>();
        imageService
            .Setup(x => x.PickImageAsync(ImageSource.PhotoLibrary, It.IsAny<bool>()))
            .ReturnsAsync(ImageResult.Success(tempImagePath, "sample-check.png", "image/png", ImageSource.PhotoLibrary));

        imageService
            .Setup(x => x.ValidateImageAsync(tempImagePath, It.IsAny<ImageValidationOptions>()))
            .ReturnsAsync(ImageValidationResult.Valid("PNG", 4096, 1024, 768));

        imageService
            .Setup(x => x.GetImageInfoAsync(tempImagePath))
            .ReturnsAsync(new ImageInfo
            {
                FilePath = tempImagePath,
                Format = "PNG",
                Width = 1024,
                Height = 768,
                FileSizeBytes = 4096,
                ModifiedDate = DateTime.UtcNow
            });

        var viewModel = new DocumentUploadViewModel(imageService.Object, TestLogger.Create<DocumentUploadViewModel>());

        await viewModel.SelectImageCommand.ExecuteAsync(null);

        imageService.Verify(x => x.PickImageAsync(ImageSource.PhotoLibrary, It.IsAny<bool>()), Times.Once);
        imageService.Verify(x => x.ValidateImageAsync(tempImagePath, It.IsAny<ImageValidationOptions>()), Times.Once);
        imageService.Verify(x => x.GetImageInfoAsync(tempImagePath), Times.Once);

        if (viewModel.HasSelectedImage)
        {
            Assert.Equal("sample-check.png", viewModel.SelectedFileName);
            Assert.Equal("PNG", viewModel.DetectedFormat);
            Assert.Equal("Classify", viewModel.CurrentProcessingStage);
            Assert.Contains("Image ready", viewModel.StatusMessage);
        }
        else
        {
            Assert.False(string.IsNullOrWhiteSpace(viewModel.ErrorMessage));
            Assert.Contains("Unable to select image", viewModel.ErrorMessage);
        }

        File.Delete(tempImagePath);
    }

    [Fact]
    public async Task SelectImageCommand_SetsError_WhenValidationFails()
    {
        var imageService = new Mock<IImageService>();
        imageService
            .Setup(x => x.PickImageAsync(ImageSource.PhotoLibrary, It.IsAny<bool>()))
            .ReturnsAsync(ImageResult.Success("sample-path.png", "sample-check.png", "image/png", ImageSource.PhotoLibrary));

        imageService
            .Setup(x => x.ValidateImageAsync("sample-path.png", It.IsAny<ImageValidationOptions>()))
            .ReturnsAsync(ImageValidationResult.Invalid("validation failed"));

        var viewModel = new DocumentUploadViewModel(imageService.Object, TestLogger.Create<DocumentUploadViewModel>());

        await viewModel.SelectImageCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasSelectedImage);
        Assert.Equal("validation failed", viewModel.StatusMessage);
        Assert.True(viewModel.HasTimelineError);
    }
}
