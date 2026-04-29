using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services.Interfaces;

namespace CheckYoSelfAI.Tests.Common;

internal static class TestDataFactory
{
    public static AzureSettings CreateValidAzureSettings() => new()
    {
        Endpoint = "https://demo-resource.cognitiveservices.azure.com/",
        ApiKey = "0123456789abcdef0123456789abcdef",
        Region = "eastus"
    };

    public static AzureSettings CreateInvalidAzureSettings() => new()
    {
        Endpoint = "not-a-url",
        ApiKey = "bad-key",
        Region = ""
    };

    public static ImageValidationOptions CreateStrictValidationOptions() => new()
    {
        MaxFileSizeBytes = 5 * 1024 * 1024,
        MinFileSizeBytes = 1024,
        AllowedFormats = new() { "JPEG", "PNG" },
        MinWidth = 200,
        MinHeight = 200,
        MaxWidth = 5000,
        MaxHeight = 5000,
        PerformQualityAnalysis = false
    };

    public static DocumentInput CreateDocumentInput(string fileName = "check-demo.png")
    {
        var bytes = new byte[2048];
        new Random(42).NextBytes(bytes);

        return new DocumentInput
        {
            Id = Guid.NewGuid().ToString("N"),
            FileName = fileName,
            ContentType = "image/png",
            FileSize = bytes.Length,
            UploadedAt = DateTime.UtcNow,
            Content = new MemoryStream(bytes)
        };
    }
}
