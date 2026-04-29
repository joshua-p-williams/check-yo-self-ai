using OpenQA.Selenium.Appium;

namespace CheckYoSelfAI.Tests.UI.Infrastructure;

internal static class AppiumDriverFactory
{
    public static Uri GetServerUri(AppiumTestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return new Uri(settings.AppiumServerUrl);
    }

    public static AppiumOptions CreateOptions(AppiumTestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var options = new AppiumOptions
        {
            PlatformName = settings.PlatformName,
            AutomationName = settings.AutomationName,
            DeviceName = settings.DeviceName
        };

        options.AddAdditionalAppiumOption("app", settings.AppId);

        if (!string.IsNullOrWhiteSpace(settings.AppWaitActivity))
        {
            options.AddAdditionalAppiumOption("appWaitActivity", settings.AppWaitActivity);
        }

        return options;
    }
}
