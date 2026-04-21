namespace CheckYoSelfAI.Tests.UI.Infrastructure;

internal sealed class AppiumTestSettings
{
    public string PlatformName { get; init; } = "Windows";

    public string AppiumServerUrl { get; init; } = "http://127.0.0.1:4723";

    public string AppId { get; init; } = string.Empty;

    public string DeviceName { get; init; } = "WindowsPC";

    public string AutomationName { get; init; } = "Windows";

    public string AppWaitActivity { get; init; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(AppId);

    public static AppiumTestSettings FromEnvironment()
    {
        return new AppiumTestSettings
        {
            PlatformName = Environment.GetEnvironmentVariable("UI_TEST_PLATFORM") ?? "Windows",
            AppiumServerUrl = Environment.GetEnvironmentVariable("UI_TEST_APPIUM_SERVER") ?? "http://127.0.0.1:4723",
            AppId = Environment.GetEnvironmentVariable("UI_TEST_APP_ID") ?? string.Empty,
            DeviceName = Environment.GetEnvironmentVariable("UI_TEST_DEVICE") ?? "WindowsPC",
            AutomationName = Environment.GetEnvironmentVariable("UI_TEST_AUTOMATION") ?? "Windows",
            AppWaitActivity = Environment.GetEnvironmentVariable("UI_TEST_APP_WAIT_ACTIVITY") ?? string.Empty
        };
    }
}
