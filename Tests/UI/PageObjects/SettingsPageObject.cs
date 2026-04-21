using OpenQA.Selenium;

namespace CheckYoSelfAI.Tests.UI.PageObjects;

internal sealed class SettingsPageObject(IWebDriver driver) : BasePageObject(driver)
{
    public IWebElement EndpointEntry => FindByAccessibilityId("Endpoint URL");

    public IWebElement ApiKeyEntry => FindByAccessibilityId("API Key");

    public IWebElement RegionEntry => FindByAccessibilityId("Region");

    public IWebElement TestConnectionButton => FindByAccessibilityId("Test Connection");

    public IWebElement SaveSettingsButton => FindByAccessibilityId("Save Settings");
}
