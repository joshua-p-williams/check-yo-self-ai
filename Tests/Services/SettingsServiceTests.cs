using CheckYoSelfAI.Services;
using CheckYoSelfAI.Tests.Common;
using Xunit;

namespace CheckYoSelfAI.Tests.Services;

public class SettingsServiceTests
{
    [Fact]
    public async Task ValidateSettingsAsync_ReturnsErrors_ForInvalidAzureSettings()
    {
        var service = new SettingsService(TestLogger.Create<SettingsService>());
        var invalid = TestDataFactory.CreateInvalidAzureSettings();

        var results = (await service.ValidateSettingsAsync(invalid)).ToList();

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task ValidateSettingsAsync_ReturnsNoErrors_ForValidAzureSettings()
    {
        var service = new SettingsService(TestLogger.Create<SettingsService>());
        var valid = TestDataFactory.CreateValidAzureSettings();

        var results = (await service.ValidateSettingsAsync(valid)).ToList();

        Assert.Empty(results);
    }
}
