using OpenQA.Selenium;

namespace CheckYoSelfAI.Tests.UI.PageObjects;

internal abstract class BasePageObject
{
    protected BasePageObject(IWebDriver driver)
    {
        Driver = driver;
    }

    protected IWebDriver Driver { get; }

    protected IWebElement FindByAccessibilityId(string accessibilityId)
    {
        return Driver.FindElement(By.Id(accessibilityId));
    }

    protected IReadOnlyCollection<IWebElement> FindAllByAccessibilityId(string accessibilityId)
    {
        return Driver.FindElements(By.Id(accessibilityId));
    }
}
