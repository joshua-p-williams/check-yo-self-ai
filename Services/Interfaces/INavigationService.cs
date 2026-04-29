namespace CheckYoSelfAI.Services.Interfaces;

/// <summary>
/// Service interface for navigation operations throughout the application.
/// Provides a clean abstraction over MAUI Shell navigation with error handling.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to a page using the specified route with optional parameters.
    /// </summary>
    /// <param name="route">The navigation route (Shell route or page name)</param>
    /// <param name="parameters">Optional dictionary of parameters to pass to the destination page</param>
    /// <returns>Task representing the async navigation operation</returns>
    Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Navigates to a page using a strongly-typed route with optional parameters.
    /// </summary>
    /// <typeparam name="TPage">The target page type</typeparam>
    /// <param name="parameters">Optional dictionary of parameters to pass to the destination page</param>
    /// <returns>Task representing the async navigation operation</returns>
    Task NavigateToAsync<TPage>(IDictionary<string, object>? parameters = null) where TPage : class;

    /// <summary>
    /// Navigates back in the navigation stack.
    /// </summary>
    /// <param name="parameters">Optional dictionary of parameters to pass back to the previous page</param>
    /// <returns>Task representing the async navigation operation</returns>
    Task GoBackAsync(IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Navigates to the root page of the current shell section.
    /// </summary>
    /// <returns>Task representing the async navigation operation</returns>
    Task GoToRootAsync();

    /// <summary>
    /// Displays an alert dialog to the user.
    /// </summary>
    /// <param name="title">The title of the alert</param>
    /// <param name="message">The message content</param>
    /// <param name="cancel">The cancel button text (default: "OK")</param>
    /// <returns>Task representing the async operation</returns>
    Task DisplayAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>
    /// Displays an alert dialog with accept and cancel options.
    /// </summary>
    /// <param name="title">The title of the alert</param>
    /// <param name="message">The message content</param>
    /// <param name="accept">The accept button text</param>
    /// <param name="cancel">The cancel button text</param>
    /// <returns>True if accept was selected, false if cancel was selected</returns>
    Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel);

    /// <summary>
    /// Displays an action sheet with multiple options for the user to choose from.
    /// </summary>
    /// <param name="title">The title of the action sheet</param>
    /// <param name="cancel">The cancel button text</param>
    /// <param name="destruction">Optional destruction button text (typically "Delete" style)</param>
    /// <param name="buttons">Array of button options</param>
    /// <returns>The text of the selected button, or null if cancelled</returns>
    Task<string?> DisplayActionSheetAsync(string title, string cancel, string? destruction = null, params string[] buttons);

    /// <summary>
    /// Displays a prompt dialog for user input.
    /// </summary>
    /// <param name="title">The title of the prompt</param>
    /// <param name="message">The message content</param>
    /// <param name="accept">The accept button text (default: "OK")</param>
    /// <param name="cancel">The cancel button text (default: "Cancel")</param>
    /// <param name="placeholder">Optional placeholder text for the input field</param>
    /// <param name="maxLength">Maximum length of input (default: -1 for no limit)</param>
    /// <param name="keyboard">Keyboard type for input (default: Default)</param>
    /// <param name="initialValue">Initial value for the input field</param>
    /// <returns>The user's input string, or null if cancelled</returns>
    Task<string?> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", 
        string? placeholder = null, int maxLength = -1, Keyboard? keyboard = null, string initialValue = "");

    /// <summary>
    /// Gets the current navigation stack count.
    /// </summary>
    /// <returns>The number of pages in the current navigation stack</returns>
    Task<int> GetNavigationStackCountAsync();

    /// <summary>
    /// Checks if navigation back is possible.
    /// </summary>
    /// <returns>True if can navigate back, false otherwise</returns>
    Task<bool> CanGoBackAsync();

    /// <summary>
    /// Gets the current page being displayed.
    /// </summary>
    /// <returns>The current page instance, or null if not available</returns>
    Page? GetCurrentPage();

    /// <summary>
    /// Registers a route for Shell navigation.
    /// </summary>
    /// <param name="route">The route name</param>
    /// <param name="pageType">The page type to associate with the route</param>
    void RegisterRoute(string route, Type pageType);

    /// <summary>
    /// Registers a route for Shell navigation using a generic type.
    /// </summary>
    /// <typeparam name="TPage">The page type to associate with the route</typeparam>
    /// <param name="route">The route name</param>
    void RegisterRoute<TPage>(string route) where TPage : Page;

    /// <summary>
    /// Unregisters a previously registered route.
    /// </summary>
    /// <param name="route">The route name to unregister</param>
    void UnregisterRoute(string route);
}

/// <summary>
/// Navigation routes used throughout the application.
/// Centralizes route definitions to prevent typos and ensure consistency.
/// </summary>
public static class NavigationRoutes
{
    // Main application routes
    public const string Home = "home";
    public const string Settings = "settings";

    // Document processing routes
    public const string DocumentCapture = "document-capture";
    public const string DocumentPreview = "document-preview";
    public const string DocumentResults = "document-results";
    public const string ProcessingStatus = "processing-status";

    // Azure configuration routes
    public const string AzureSettings = "azure-settings";
    public const string AzureSetup = "azure-setup";

    // Help and support routes
    public const string Help = "help";
    public const string About = "about";

    // Error and diagnostic routes
    public const string ErrorPage = "error";
    public const string DiagnosticsPage = "diagnostics";
}

/// <summary>
/// Navigation parameter keys used throughout the application.
/// Centralizes parameter key definitions to prevent typos and ensure consistency.
/// </summary>
public static class NavigationParameters
{
    // Document processing parameters
    public const string DocumentId = "document_id";
    public const string DocumentType = "document_type";
    public const string ImagePath = "image_path";
    public const string ImageData = "image_data";
    public const string ProcessingResults = "processing_results";

    // Navigation behavior parameters  
    public const string ShouldClearStack = "clear_stack";
    public const string AnimationType = "animation_type";
    public const string IsModal = "is_modal";

    // Error handling parameters
    public const string ErrorMessage = "error_message";
    public const string ErrorDetails = "error_details";
    public const string RetryAction = "retry_action";

    // Settings parameters
    public const string SettingsSection = "settings_section";
    public const string HighlightField = "highlight_field";

    // User interaction parameters
    public const string ConfirmationRequired = "confirmation_required";
    public const string SuccessMessage = "success_message";
    public const string CompletionCallback = "completion_callback";
}

/// <summary>
/// Exception thrown when a navigation operation fails.
/// </summary>
public class NavigationException : Exception
{
    public string? Route { get; }
    public IDictionary<string, object>? Parameters { get; }
    public string? Operation { get; }

    public NavigationException(string message) : base(message) { }

    public NavigationException(string message, string? route, string? operation) : base(message)
    {
        Route = route;
        Operation = operation;
    }

    public NavigationException(string message, string? route, IDictionary<string, object>? parameters, string? operation) 
        : base(message)
    {
        Route = route;
        Parameters = parameters;
        Operation = operation;
    }

    public NavigationException(string message, Exception innerException) : base(message, innerException) { }

    public NavigationException(string message, Exception innerException, string? route, string? operation) 
        : base(message, innerException)
    {
        Route = route;
        Operation = operation;
    }

    public NavigationException(string message, Exception innerException, string? route, 
        IDictionary<string, object>? parameters, string? operation) : base(message, innerException)
    {
        Route = route;
        Parameters = parameters;
        Operation = operation;
    }
}