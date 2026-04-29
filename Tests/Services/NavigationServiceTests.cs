using CheckYoSelfAI.Services;
using CheckYoSelfAI.Services.Interfaces;
using CheckYoSelfAI.Tests.Common;
using Microsoft.Maui.Controls;
using Xunit;

namespace CheckYoSelfAI.Tests.Services;

public class NavigationServiceTests
{
    [Fact]
    public void GetCurrentPage_ReturnsNull_WhenNoShellContextExists()
    {
        var service = new NavigationService(TestLogger.Create<NavigationService>());

        var page = service.GetCurrentPage();

        Assert.Null(page);
    }

    [Fact]
    public void RegisterRoute_ThrowsNavigationException_WhenRouteIsInvalid()
    {
        var service = new NavigationService(TestLogger.Create<NavigationService>());

        var exception = Assert.Throws<NavigationException>(() => service.RegisterRoute(string.Empty, typeof(ContentPage)));

        Assert.Contains("Failed to register route", exception.Message);
    }
}
