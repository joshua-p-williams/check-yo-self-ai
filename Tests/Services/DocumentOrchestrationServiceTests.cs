using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services;
using CheckYoSelfAI.Services.Interfaces;
using CheckYoSelfAI.Tests.Common;
using Moq;
using Xunit;

namespace CheckYoSelfAI.Tests.Services;

public class DocumentOrchestrationServiceTests
{
    [Fact]
    public async Task ExtractDocumentAsync_Throws_ForUnsupportedDocumentType()
    {
        var service = CreateService();
        var document = TestDataFactory.CreateDocumentInput("unknown-document.png");

        await Assert.ThrowsAsync<ArgumentException>(() => service.ExtractDocumentAsync(document, DocumentType.Unknown));
    }

    [Fact]
    public async Task ProcessDocumentAsync_ReturnsFailedResult_WhenClassifierThrows()
    {
        var classifier = new Mock<IDocumentClassifierService>();
        classifier
            .Setup(x => x.ClassifyDocumentAsync(It.IsAny<DocumentInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DocumentClassificationException("classifier failed"));

        var checkAnalyzer = new Mock<ICheckAnalyzerService>();
        var depositAnalyzer = new Mock<IDepositSlipAnalyzerService>();

        var service = new DocumentOrchestrationService(
            classifier.Object,
            checkAnalyzer.Object,
            depositAnalyzer.Object,
            TestLogger.Create<DocumentOrchestrationService>());

        var document = TestDataFactory.CreateDocumentInput();

        var result = await service.ProcessDocumentAsync(document);

        Assert.Equal(ProcessingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.NotEmpty(result.Errors);
    }

    private static DocumentOrchestrationService CreateService()
    {
        var classifier = new Mock<IDocumentClassifierService>();
        classifier
            .Setup(x => x.ClassifyDocumentAsync(It.IsAny<DocumentInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassificationResult { DocumentId = Guid.NewGuid().ToString("N"), DocumentType = DocumentType.BankCheck, Confidence = 0.9 });

        var checkAnalyzer = new Mock<ICheckAnalyzerService>();
        var depositAnalyzer = new Mock<IDepositSlipAnalyzerService>();

        return new DocumentOrchestrationService(
            classifier.Object,
            checkAnalyzer.Object,
            depositAnalyzer.Object,
            TestLogger.Create<DocumentOrchestrationService>());
    }
}
