using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides common MVVM functionality including property change notification,
/// common properties, and error handling support.
/// </summary>
public abstract class BaseViewModel : ObservableObject, INotifyPropertyChanged
{
    protected readonly ILogger? _logger;

    #region Private Fields

    private bool _isBusy;
    private string _title = string.Empty;
    private bool _isInitialized;
    private string? _errorMessage;
    private bool _hasError;

    #endregion

    #region Constructor

    protected BaseViewModel(ILogger? logger = null)
    {
        _logger = logger;
    }

    #endregion

    #region Common Properties

    /// <summary>
    /// Indicates whether the ViewModel is currently performing a long-running operation.
    /// UI can use this to show loading indicators and disable user interactions.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnBusyChanged(value);
            }
        }
    }

    /// <summary>
    /// Inverse of IsBusy for convenient UI binding.
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// The title of the view or operation.
    /// Typically displayed in navigation bars or page headers.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Indicates whether the ViewModel has been initialized.
    /// Used to prevent multiple initialization calls and manage loading states.
    /// </summary>
    public bool IsInitialized
    {
        get => _isInitialized;
        protected set => SetProperty(ref _isInitialized, value);
    }

    /// <summary>
    /// Current error message to display to the user.
    /// Null or empty when there are no errors.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        protected set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                HasError = !string.IsNullOrEmpty(value);
            }
        }
    }

    /// <summary>
    /// Indicates whether there is currently an error to display.
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        protected set => SetProperty(ref _hasError, value);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Virtual method for ViewModel initialization.
    /// Override this method to perform async initialization logic.
    /// This method ensures initialization only happens once.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the initialization operation</param>
    /// <returns>Task representing the initialization operation</returns>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            _logger?.LogInformation("Initializing {ViewModelType}", GetType().Name);

            await OnInitializeAsync(cancellationToken);

            IsInitialized = true;
            _logger?.LogInformation("Completed initialization of {ViewModelType}", GetType().Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize {ViewModelType}", GetType().Name);
            SetError("Failed to initialize. Please try again.", ex);
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Override this method to provide custom initialization logic.
    /// This method is called once during the lifetime of the ViewModel.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the initialization operation</param>
    /// <returns>Task representing the initialization operation</returns>
    protected virtual Task OnInitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Sets an error message to display to the user.
    /// </summary>
    /// <param name="message">User-friendly error message</param>
    /// <param name="exception">Optional exception for logging purposes</param>
    protected virtual void SetError(string message, Exception? exception = null)
    {
        ErrorMessage = message;

        if (exception != null)
        {
            _logger?.LogError(exception, "Error in {ViewModelType}: {ErrorMessage}", GetType().Name, message);
        }
        else
        {
            _logger?.LogWarning("Error in {ViewModelType}: {ErrorMessage}", GetType().Name, message);
        }

        OnErrorSet(message, exception);
    }

    /// <summary>
    /// Clears the current error message.
    /// </summary>
    protected virtual void ClearError()
    {
        ErrorMessage = null;
    }

    /// <summary>
    /// Handles an exception by setting an appropriate error message.
    /// Override this method to provide custom exception handling logic.
    /// </summary>
    /// <param name="exception">The exception to handle</param>
    /// <param name="context">Optional context about where the exception occurred</param>
    protected virtual void HandleException(Exception exception, string? context = null)
    {
        var contextMessage = !string.IsNullOrEmpty(context) ? $" ({context})" : string.Empty;
        var userMessage = GetUserFriendlyErrorMessage(exception);
        SetError($"{userMessage}{contextMessage}", exception);
    }

    /// <summary>
    /// Converts an exception into a user-friendly error message.
    /// Override this method to customize error message generation.
    /// </summary>
    /// <param name="exception">The exception to convert</param>
    /// <returns>User-friendly error message</returns>
    protected virtual string GetUserFriendlyErrorMessage(Exception exception)
    {
        return exception switch
        {
            OperationCanceledException => "Operation was cancelled",
            TimeoutException => "The operation timed out. Please try again.",
            UnauthorizedAccessException => "You don't have permission to perform this action",
            ArgumentException => "Invalid input provided",
            InvalidOperationException => "Unable to complete the operation at this time",
            _ => "An unexpected error occurred. Please try again."
        };
    }

    /// <summary>
    /// Called when an error is set. Override to provide custom error handling.
    /// </summary>
    /// <param name="message">The error message that was set</param>
    /// <param name="exception">The exception that caused the error (if any)</param>
    protected virtual void OnErrorSet(string message, Exception? exception)
    {
        // Override in derived classes for custom error handling
    }

    #endregion

    #region Lifecycle Events

    /// <summary>
    /// Called when the busy state changes.
    /// Override this method to provide custom busy state handling.
    /// </summary>
    /// <param name="isBusy">The new busy state</param>
    protected virtual void OnBusyChanged(bool isBusy)
    {
        // Override in derived classes for custom busy state handling
        _logger?.LogDebug("{ViewModelType} busy state changed to {IsBusy}", GetType().Name, isBusy);
    }

    /// <summary>
    /// Called when the view associated with this ViewModel is appearing.
    /// Override this method to perform actions when the view becomes visible.
    /// </summary>
    public virtual Task OnAppearingAsync()
    {
        _logger?.LogDebug("{ViewModelType} view is appearing", GetType().Name);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the view associated with this ViewModel is disappearing.
    /// Override this method to perform cleanup when the view becomes hidden.
    /// </summary>
    public virtual Task OnDisappearingAsync()
    {
        _logger?.LogDebug("{ViewModelType} view is disappearing", GetType().Name);
        return Task.CompletedTask;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Executes an async operation with automatic busy state management and error handling.
    /// </summary>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="errorMessage">Custom error message if the operation fails</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if the operation completed successfully, false otherwise</returns>
    protected async Task<bool> ExecuteWithBusyAsync(
        Func<CancellationToken, Task> operation,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (IsBusy)
            return false;

        try
        {
            IsBusy = true;
            ClearError();

            await operation(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "An error occurred during the operation";
            SetError(message, ex);
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Executes an async operation with automatic busy state management and error handling.
    /// </summary>
    /// <typeparam name="T">The return type of the operation</typeparam>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="defaultValue">Default value to return if the operation fails</param>
    /// <param name="errorMessage">Custom error message if the operation fails</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The result of the operation or the default value if it fails</returns>
    protected async Task<T> ExecuteWithBusyAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        T defaultValue = default!,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (IsBusy)
            return defaultValue;

        try
        {
            IsBusy = true;
            ClearError();

            return await operation(cancellationToken);
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "An error occurred during the operation";
            SetError(message, ex);
            return defaultValue;
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to refresh or reload the ViewModel's data.
    /// Override OnRefreshAsync to provide custom refresh logic.
    /// </summary>
    public IAsyncRelayCommand RefreshCommand => new AsyncRelayCommand(
        OnRefreshAsync,
        () => !IsBusy);

    /// <summary>
    /// Called when the refresh command is executed.
    /// Override this method to provide custom refresh logic.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the refresh operation</param>
    /// <returns>Task representing the refresh operation</returns>
    protected virtual Task OnRefreshAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region IDisposable Support

    private bool _disposed;

    /// <summary>
    /// Releases the unmanaged resources used by the BaseViewModel and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _logger?.LogDebug("Disposing {ViewModelType}", GetType().Name);
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Releases all resources used by the BaseViewModel.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}