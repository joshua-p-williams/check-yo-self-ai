using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.Services;

/// <summary>
/// Implementation of INavigationService using MAUI Shell navigation.
/// Provides a clean abstraction over Shell with error handling and parameter support.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(route, nameof(route));

            _logger.LogDebug("Navigating to route: {Route}", route);

            if (parameters != null && parameters.Any())
            {
                _logger.LogDebug("Navigation parameters: {ParameterCount} items", parameters.Count);
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }

            _logger.LogDebug("Successfully navigated to route: {Route}", route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to route: {Route}", route);
            throw new NavigationException($"Failed to navigate to route '{route}'", ex, route, parameters, "NavigateTo");
        }
    }

    /// <inheritdoc />
    public async Task NavigateToAsync<TPage>(IDictionary<string, object>? parameters = null) where TPage : class
    {
        try
        {
            var pageType = typeof(TPage);
            var route = pageType.Name;

            _logger.LogDebug("Navigating to page type: {PageType}", pageType.Name);

            // Try to find a registered route for this page type
            // If not found, use the class name as the route
            await NavigateToAsync(route, parameters);
        }
        catch (Exception ex)
        {
            var pageType = typeof(TPage);
            _logger.LogError(ex, "Failed to navigate to page type: {PageType}", pageType.Name);
            throw new NavigationException($"Failed to navigate to page type '{pageType.Name}'", ex, pageType.Name, parameters, "NavigateToGeneric");
        }
    }

    /// <inheritdoc />
    public async Task GoBackAsync(IDictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogDebug("Navigating back");

            if (parameters != null && parameters.Any())
            {
                _logger.LogDebug("Going back with parameters: {ParameterCount} items", parameters.Count);
                await Shell.Current.GoToAsync("..", parameters);
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }

            _logger.LogDebug("Successfully navigated back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate back");
            throw new NavigationException("Failed to navigate back", ex, "..", parameters, "GoBack");
        }
    }

    /// <inheritdoc />
    public async Task GoToRootAsync()
    {
        try
        {
            _logger.LogDebug("Navigating to root");
            await Shell.Current.GoToAsync("///");
            _logger.LogDebug("Successfully navigated to root");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to root");
            throw new NavigationException("Failed to navigate to root", ex, "///", "GoToRoot");
        }
    }

    /// <inheritdoc />
    public async Task DisplayAlertAsync(string title, string message, string cancel = "OK")
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
            ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

            _logger.LogDebug("Displaying alert: {Title}", title);

            var currentPage = GetCurrentPage();
            if (currentPage != null)
            {
                // Use the async version to avoid the obsolete warning
                await currentPage.DisplayAlertAsync(title, message, cancel);
            }
            else
            {
                _logger.LogWarning("No current page available for alert display");
                throw new NavigationException("No current page available for alert display", null, "DisplayAlert");
            }

            _logger.LogDebug("Alert displayed successfully: {Title}", title);
        }
        catch (NavigationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to display alert: {Title}", title);
            throw new NavigationException($"Failed to display alert '{title}'", ex, null, "DisplayAlert");
        }
    }

    /// <inheritdoc />
    public async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
            ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
            ArgumentException.ThrowIfNullOrWhiteSpace(accept, nameof(accept));
            ArgumentException.ThrowIfNullOrWhiteSpace(cancel, nameof(cancel));

            _logger.LogDebug("Displaying confirmation alert: {Title}", title);

            var currentPage = GetCurrentPage();
            if (currentPage != null)
            {
                var result = await currentPage.DisplayAlertAsync(title, message, accept, cancel);
                _logger.LogDebug("Confirmation alert result: {Result} for title: {Title}", result, title);
                return result;
            }
            else
            {
                _logger.LogWarning("No current page available for confirmation alert display");
                throw new NavigationException("No current page available for confirmation alert display", null, "DisplayConfirmationAlert");
            }
        }
        catch (NavigationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to display confirmation alert: {Title}", title);
            throw new NavigationException($"Failed to display confirmation alert '{title}'", ex, null, "DisplayConfirmationAlert");
        }
    }

    /// <inheritdoc />
    public async Task<string?> DisplayActionSheetAsync(string title, string cancel, string? destruction = null, params string[] buttons)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
            ArgumentException.ThrowIfNullOrWhiteSpace(cancel, nameof(cancel));

            _logger.LogDebug("Displaying action sheet: {Title} with {ButtonCount} buttons", title, buttons?.Length ?? 0);

            var currentPage = GetCurrentPage();
            if (currentPage != null)
            {
                var result = await currentPage.DisplayActionSheetAsync(title, cancel, destruction, buttons);
                _logger.LogDebug("Action sheet result: {Result} for title: {Title}", result ?? "null", title);
                return result;
            }
            else
            {
                _logger.LogWarning("No current page available for action sheet display");
                throw new NavigationException("No current page available for action sheet display", null, "DisplayActionSheet");
            }
        }
        catch (NavigationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to display action sheet: {Title}", title);
            throw new NavigationException($"Failed to display action sheet '{title}'", ex, null, "DisplayActionSheet");
        }
    }

    /// <inheritdoc />
    public async Task<string?> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", 
        string? placeholder = null, int maxLength = -1, Keyboard? keyboard = null, string initialValue = "")
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
            ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

            _logger.LogDebug("Displaying prompt: {Title}", title);

            var currentPage = GetCurrentPage();
            if (currentPage != null)
            {
                var result = await currentPage.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue);
                _logger.LogDebug("Prompt result received for title: {Title}", title);
                return result;
            }
            else
            {
                _logger.LogWarning("No current page available for prompt display");
                throw new NavigationException("No current page available for prompt display", null, "DisplayPrompt");
            }
        }
        catch (NavigationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to display prompt: {Title}", title);
            throw new NavigationException($"Failed to display prompt '{title}'", ex, null, "DisplayPrompt");
        }
    }

    /// <inheritdoc />
    public async Task<int> GetNavigationStackCountAsync()
    {
        try
        {
            var navigationStack = Shell.Current.Navigation.NavigationStack;
            var count = navigationStack?.Count ?? 0;

            _logger.LogDebug("Navigation stack count: {Count}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get navigation stack count");
            throw new NavigationException("Failed to get navigation stack count", ex, null, "GetNavigationStackCount");
        }
    }

    /// <inheritdoc />
    public async Task<bool> CanGoBackAsync()
    {
        try
        {
            var navigationStack = Shell.Current.Navigation.NavigationStack;
            var canGoBack = navigationStack != null && navigationStack.Count > 1;
            _logger.LogDebug("Can go back: {CanGoBack}", canGoBack);
            return canGoBack;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine if can go back");
            throw new NavigationException("Failed to determine if can go back", ex, null, "CanGoBack");
        }
    }

    /// <inheritdoc />
    public Page? GetCurrentPage()
    {
        try
        {
            // Try multiple approaches to get the current page

            // Method 1: Through Shell.Current
            var currentPage = Shell.Current?.CurrentPage;
            if (currentPage != null)
            {
                _logger.LogDebug("Current page found via Shell.Current: {PageType}", currentPage.GetType().Name);
                return currentPage;
            }

            // Method 2: Through main window page navigation stack
            var mainWindow = Application.Current?.Windows?.FirstOrDefault();
            var mainPage = mainWindow?.Page;
            if (mainPage is Shell shell && shell.Navigation.NavigationStack.Any())
            {
                currentPage = shell.Navigation.NavigationStack.Last();
                _logger.LogDebug("Current page found via navigation stack: {PageType}", currentPage.GetType().Name);
                return currentPage;
            }

            // Method 3: Main page itself if it's not a Shell
            if (mainPage != null && mainPage is not Shell)
            {
                _logger.LogDebug("Current page is MainPage: {PageType}", mainPage.GetType().Name);
                return mainPage;
            }

            _logger.LogDebug("No current page found");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current page");
            return null;
        }
    }

    /// <inheritdoc />
    public void RegisterRoute(string route, Type pageType)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(route, nameof(route));
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            _logger.LogDebug("Registering route: {Route} -> {PageType}", route, pageType.Name);

            Routing.RegisterRoute(route, pageType);

            _logger.LogDebug("Successfully registered route: {Route} -> {PageType}", route, pageType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register route: {Route} -> {PageType}", route, pageType?.Name ?? "null");
            throw new NavigationException($"Failed to register route '{route}' for page type '{pageType?.Name}'", ex, route, "RegisterRoute");
        }
    }

    /// <inheritdoc />
    public void RegisterRoute<TPage>(string route) where TPage : Page
    {
        try
        {
            var pageType = typeof(TPage);
            RegisterRoute(route, pageType);
        }
        catch (Exception ex)
        {
            var pageType = typeof(TPage);
            _logger.LogError(ex, "Failed to register generic route: {Route} -> {PageType}", route, pageType.Name);
            throw new NavigationException($"Failed to register route '{route}' for page type '{pageType.Name}'", ex, route, "RegisterGenericRoute");
        }
    }

    /// <inheritdoc />
    public void UnregisterRoute(string route)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(route, nameof(route));

            _logger.LogDebug("Unregistering route: {Route}", route);

            Routing.UnRegisterRoute(route);

            _logger.LogDebug("Successfully unregistered route: {Route}", route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister route: {Route}", route);
            throw new NavigationException($"Failed to unregister route '{route}'", ex, route, "UnregisterRoute");
        }
    }
}