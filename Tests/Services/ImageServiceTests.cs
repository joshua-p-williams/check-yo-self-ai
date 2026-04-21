using CheckYoSelfAI.Services;
using CheckYoSelfAI.Services.Interfaces;
using CheckYoSelfAI.Tests.Common;
using Moq;
using Xunit;

namespace CheckYoSelfAI.Tests.Services;

public class ImageServiceTests
{
    private static ImageService CreateService()
    {
        var settings = new Mock<ISettingsService>();
        return new ImageService(TestLogger.Create<ImageService>(), settings.Object);
    }

    [Fact]
    public async Task ValidateImageAsync_ReturnsInvalid_WhenPathDoesNotExist()
    {
        var service = CreateService();

        var result = await service.ValidateImageAsync("missing-image-path.png");

        Assert.False(result.IsValid);
        Assert.Contains("does not exist", result.Errors.First());
    }

    [Fact]
    public async Task ValidateImageAsync_ReturnsInvalid_WhenStreamIsNotImageData()
    {
        var service = CreateService();
        await using var stream = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8]);

        var result = await service.ValidateImageAsync(stream, TestDataFactory.CreateStrictValidationOptions());

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
