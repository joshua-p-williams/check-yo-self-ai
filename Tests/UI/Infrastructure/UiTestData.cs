namespace CheckYoSelfAI.Tests.UI.Infrastructure;

internal static class UiTestData
{
    public const string SampleCheckImage = "Resources/TestData/check-sample.png";
    public const string SampleDepositSlipImage = "Resources/TestData/deposit-slip-sample.png";

    public static IReadOnlyDictionary<string, string> ValidSettings => new Dictionary<string, string>
    {
        ["Endpoint"] = "https://demo-resource.cognitiveservices.azure.com/",
        ["ApiKey"] = "0123456789abcdef0123456789abcdef",
        ["Region"] = "eastus"
    };
}
