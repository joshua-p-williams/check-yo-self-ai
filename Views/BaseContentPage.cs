using Microsoft.Extensions.Logging;
using check_yo_self_ai.ViewModels;

namespace check_yo_self_ai.Views;

/// <summary>
/// Base class for all ContentPages in the application.
/// Provides common functionality including ViewModel binding, lifecycle management,
/// loading state handling, and data binding context configuration.
/// </summary>
/// <typeparam name="TViewModel">The type of ViewModel associated with this page</typeparam>
public abstract class BaseContentPage<TViewModel> : ContentPage, IDisposable
    where TViewModel : BaseViewModel
{
    protected readonly ILogger? _logger;
    private bool _hasAppeared;
    private bool _disposed;

    #region Constructor

    protected BaseContentPage(ILogger? logger = null)
    {
        _logger = logger;

        // Set up data binding context when ViewModel is assigned
        BindingContextChanged += OnBindingContextChanged;

        _logger?.LogDebug("Created {PageType}", GetType().Name);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the strongly-typed ViewModel for this page.
    /// </summary>
    protected TViewModel? ViewModel => BindingContext as TViewModel;

    /// <summary>
    /// Indicates whether the page has appeared at least once.
    /// </summary>
    protected bool HasAppeared => _hasAppeared;

    #endregion

    #region Lifecycle Events

    /// <summary>
    /// Called when the page is appearing.
    /// Handles ViewModel initialization and lifecycle events.
    /// </summary>
    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            _logger?.LogDebug("{PageType} is appearing", GetType().Name);

            // Initialize ViewModel if not already initialized
            if (ViewModel != null && !ViewModel.IsInitialized)
            {
                await ViewModel.InitializeAsync();
            }

            // Call ViewModel OnAppearing
            if (ViewModel != null)
            {
                await ViewModel.OnAppearingAsync();
            }

            // Call custom appearing logic
            await OnAppearingAsync();

            _hasAppeared = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during {PageType} OnAppearing", GetType().Name);
            await HandlePageException(ex, "Error loading page");
        }
    }

    /// <summary>
    /// Called when the page is disappearing.
    /// Handles cleanup and ViewModel lifecycle events.
    /// </summary>
    protected override async void OnDisappearing()
    {
        try
        {
            base.OnDisappearing();

            _logger?.LogDebug("{PageType} is disappearing", GetType().Name);

            // Call ViewModel OnDisappearing
            if (ViewModel != null)
            {
                await ViewModel.OnDisappearingAsync();
            }

            // Call custom disappearing logic
            await OnDisappearingAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during {PageType} OnDisappearing", GetType().Name);
            // Don't show error to user during page disappearing
        }
    }

    /// <summary>
    /// Virtual method for custom appearing logic.
    /// Override this method to perform custom actions when the page appears.
    /// </summary>
    /// <returns>Task representing the appearing operation</returns>
    protected virtual Task OnAppearingAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Virtual method for custom disappearing logic.
    /// Override this method to perform custom cleanup when the page disappears.
    /// </summary>
    /// <returns>Task representing the disappearing operation</returns>
    protected virtual Task OnDisappearingAsync()
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Data Binding Context Management

    /// <summary>
    /// Called when the BindingContext changes.
    /// Sets up ViewModel event handlers and configurations.
    /// </summary>
    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            _logger?.LogDebug("{PageType} ViewModel bound: {ViewModelType}", GetType().Name, ViewModel.GetType().Name);

            // Configure ViewModel-specific settings
            ConfigureViewModel(ViewModel);
        }
    }

    /// <summary>
    /// Configures the ViewModel when it's assigned to this page.
    /// Override this method to set up ViewModel-specific configurations.
    /// </summary>
    /// <param name="viewModel">The ViewModel to configure</param>
    protected virtual void ConfigureViewModel(TViewModel viewModel)
    {
        // Override in derived classes for custom ViewModel configuration
    }

    #endregion

    #region Loading State Management

    /// <summary>
    /// Shows a loading indicator to the user.
    /// Override this method to customize loading state display.
    /// </summary>
    /// <param name="message">Optional loading message to display</param>
    protected virtual void ShowLoading(string? message = null)
    {
        _logger?.LogDebug("{PageType} showing loading state: {Message}", GetType().Name, message);

        // Default implementation - override in derived classes
        // This could show a loading overlay, progress bar, etc.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (ViewModel != null)
            {
                ViewModel.IsBusy = true;
            }
        });
    }

    /// <summary>
    /// Hides the loading indicator.
    /// Override this method to customize loading state hiding.
    /// </summary>
    protected virtual void HideLoading()
    {
        _logger?.LogDebug("{PageType} hiding loading state", GetType().Name);

        // Default implementation - override in derived classes
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (ViewModel != null)
            {
                ViewModel.IsBusy = false;
            }
        });
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Handles exceptions that occur during page operations.
    /// Override this method to provide custom error handling.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="userMessage">User-friendly error message</param>
    /// <returns>Task representing the error handling operation</returns>
    protected virtual async Task HandlePageException(Exception exception, string userMessage)
    {
        _logger?.LogError(exception, "Page exception in {PageType}: {UserMessage}", GetType().Name, userMessage);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                HideLoading();
                await ShowErrorMessage(userMessage, exception);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to show error message in {PageType}", GetType().Name);
            }
        });
    }

    /// <summary>
    /// Shows an error message to the user.
    /// Override this method to customize error message display.
    /// </summary>
    /// <param name="message">Error message to display</param>
    /// <param name="exception">Optional exception for additional details</param>
    /// <returns>Task representing the error display operation</returns>
    protected virtual async Task ShowErrorMessage(string message, Exception? exception = null)
    {
        // Default implementation using DisplayAlert
        // Override in derived classes for custom error display
        var title = "Error";
        var okButton = "OK";

        if (exception != null)
        {
            _logger?.LogDebug("Showing error dialog: {Message} (Exception: {ExceptionType})", message, exception.GetType().Name);
        }

        await DisplayAlert(title, message, okButton);
    }

    #endregion

    #region Navigation Helpers

    /// <summary>
    /// Safely navigates to another page with error handling.
    /// </summary>
    /// <param name="route">The navigation route</param>
    /// <param name="parameters">Optional navigation parameters</param>
    /// <returns>Task representing the navigation operation</returns>
    protected virtual async Task SafeNavigateAsync(string route, IDictionary<string, object>? parameters = null)
    {
        try
        {
            _logger?.LogDebug("{PageType} navigating to {Route}", GetType().Name, route);

            if (parameters != null && parameters.Count > 0)
            {
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }
        }
        catch (Exception ex)
        {
            await HandlePageException(ex, "Navigation failed");
        }
    }

    /// <summary>
    /// Safely navigates back with error handling.
    /// </summary>
    /// <returns>Task representing the navigation operation</returns>
    protected virtual async Task SafeGoBackAsync()
    {
        try
        {
            _logger?.LogDebug("{PageType} navigating back", GetType().Name);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await HandlePageException(ex, "Navigation failed");
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Executes an operation on the main thread safely.
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <returns>Task representing the operation</returns>
    protected async Task ExecuteOnMainThreadAsync(Action action)
    {
        await MainThread.InvokeOnMainThreadAsync(action);
    }

    /// <summary>
    /// Executes an async operation on the main thread safely.
    /// </summary>
    /// <param name="func">The async function to execute</param>
    /// <returns>Task representing the operation</returns>
    protected async Task ExecuteOnMainThreadAsync(Func<Task> func)
    {
        await MainThread.InvokeOnMainThreadAsync(func);
    }

    /// <summary>
    /// Checks if the page is currently visible and safe for UI operations.
    /// </summary>
    /// <returns>True if the page is safe for UI operations</returns>
    protected bool IsPageSafeForUIOperations()
    {
        return !_disposed && ViewModel != null && _hasAppeared;
    }

    #endregion

    #region IDisposable Support

    /// <summary>
    /// Releases the unmanaged resources used by the BaseContentPage and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _logger?.LogDebug("Disposing {PageType}", GetType().Name);

                // Unsubscribe from events
                BindingContextChanged -= OnBindingContextChanged;

                // Dispose ViewModel if it implements IDisposable
                if (ViewModel is IDisposable disposableViewModel)
                {
                    disposableViewModel.Dispose();
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Releases all resources used by the BaseContentPage.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}