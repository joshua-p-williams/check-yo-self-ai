using CheckYoSelfAI.Services.Interfaces;
using CheckYoSelfAI.Tests.Common;
using CheckYoSelfAI.ViewModels;
using Moq;
using Xunit;

namespace CheckYoSelfAI.Tests.ViewModels;

public class SettingsPageViewModelTests
{
    [Fact]
    public async Task SaveSettingsCommand_SavesSettings_AndNavigatesBack()
    {
        var settings = new Mock<ISettingsService>();
        settings.Setup(x => x.GetAsync(SettingKeys.AzureEndpoint, It.IsAny<string>())).ReturnsAsync("https://demo.cognitiveservices.azure.com/");
        settings.Setup(x => x.GetSecureAsync(SettingKeys.AzureApiKey, It.IsAny<string>())).ReturnsAsync("0123456789abcdef0123456789abcdef");
        settings.Setup(x => x.GetAsync(SettingKeys.AzureRegion, It.IsAny<string>())).ReturnsAsync("eastus");
        settings.Setup(x => x.GetAsync(SettingKeys.AutoSaveResults, true)).ReturnsAsync(true);
        settings.Setup(x => x.GetAsync(SettingKeys.ShowConfidenceScores, true)).ReturnsAsync(true);
        settings.Setup(x => x.ValidateSettingsAsync(It.IsAny<CheckYoSelfAI.Models.AzureSettings>())).ReturnsAsync([]);

        var navigation = new Mock<INavigationService>();
        navigation.Setup(x => x.CanGoBackAsync()).ReturnsAsync(true);

        var viewModel = new SettingsPageViewModel(settings.Object, navigation.Object, TestLogger.Create<SettingsPageViewModel>());
        await viewModel.InitializeAsync();

        viewModel.Endpoint = "https://updated.cognitiveservices.azure.com/";
        viewModel.ApiKey = "abcdefabcdefabcdefabcdefabcdefab";
        viewModel.Region = "eastus";

        await viewModel.SaveSettingsCommand.ExecuteAsync(null);

        settings.Verify(x => x.SetAsync(SettingKeys.AzureEndpoint, "https://updated.cognitiveservices.azure.com/"), Times.Once);
        settings.Verify(x => x.SetSecureAsync(SettingKeys.AzureApiKey, "abcdefabcdefabcdefabcdefabcdefab"), Times.Once);
        navigation.Verify(x => x.GoBackAsync(null), Times.Once);
    }

    [Fact]
    public void Endpoint_UpdatesValidationError_InRealTime()
    {
        var settings = new Mock<ISettingsService>();
        var navigation = new Mock<INavigationService>();

        var viewModel = new SettingsPageViewModel(settings.Object, navigation.Object, TestLogger.Create<SettingsPageViewModel>());

        viewModel.Endpoint = "not-valid";

        Assert.True(viewModel.HasEndpointError);
        Assert.Contains("valid URL", viewModel.EndpointError);
    }
}
