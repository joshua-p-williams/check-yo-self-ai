using System.Net.Http;

namespace CheckYoSelfAI.Tests.UI.Infrastructure;

internal static class UiTestEnvironment
{
    public static string? GetSkipReason()
    {
        var settings = AppiumTestSettings.FromEnvironment();

        if (!settings.IsConfigured)
        {
            return "UI_TEST_APP_ID is not set. Use Tests/UI/run-ui-tests.ps1 to auto-detect and set it.";
        }

        if (settings.AppId.Contains(Path.DirectorySeparatorChar) && !File.Exists(settings.AppId))
        {
            return $"UI_TEST_APP_ID points to a missing file: {settings.AppId}";
        }

        if (!IsAppiumServerReachable(settings.AppiumServerUrl))
        {
            return $"Appium server is not reachable at {settings.AppiumServerUrl}.";
        }

        return null;
    }

    private static bool IsAppiumServerReachable(string serverUrl)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            using var response = client.GetAsync(new Uri(new Uri(serverUrl), "/status")).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
