using OpenQA.Selenium;

namespace CheckYoSelfAI.Tests.UI.PageObjects;

internal sealed class MainPageObject(IWebDriver driver) : BasePageObject(driver)
{
    public IWebElement CaptureImageButton => FindByAccessibilityId("Capture Image");

    public IWebElement SelectFileButton => FindByAccessibilityId("Select File");

    public IWebElement TimelineHeader => FindByAccessibilityId("Processing Timeline");

    public IReadOnlyCollection<IWebElement> StageCards => FindAllByAccessibilityId("Upload");
}
