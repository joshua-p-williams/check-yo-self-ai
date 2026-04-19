using CheckYoSelfAI.Services.Interfaces;

namespace CheckYoSelfAI.Services;

/// <summary>
/// Service for configuring Shell routes and navigation parameters.
/// Centralizes route configuration and provides navigation parameter handling.
/// </summary>
public static class ShellRouteConfiguration
{
    /// <summary>
    /// Configures all application routes and navigation parameter handling.
    /// This should be called during application startup.
    /// </summary>
    public static void ConfigureRoutes()
    {
        // Note: Primary routes are configured in AppShell.xaml
        // This method sets up additional navigation behaviors and parameter handling

        ConfigureNavigationParameters();
        ConfigurePageTransitions();
    }

    /// <summary>
    /// Sets up navigation parameter handling for the application.
    /// Configures parameter serialization and deserialization.
    /// </summary>
    private static void ConfigureNavigationParameters()
    {
        // MAUI Shell handles parameter passing through query parameters and navigation state
        // Parameters are automatically serialized/deserialized when using NavigationService

        // Example parameter handling could be added here for custom types
        // Shell.SetNavigation ParameterConverter for custom type serialization if needed

        // For now, we rely on the built-in parameter handling in MAUI Shell
        // which supports primitive types and implements IQueryAttributable for complex scenarios
    }

    /// <summary>
    /// Configures page transitions and animations for navigation.
    /// Sets up consistent navigation animations across the application.
    /// </summary>
    private static void ConfigurePageTransitions()
    {
        // MAUI Shell provides built-in navigation transitions
        // Custom transitions can be configured here if needed

        // For now, we use the default MAUI transitions which are:
        // - Slide from right on forward navigation
        // - Slide to right on back navigation  
        // - Fade for modal presentations

        // Custom animations would be implemented through:
        // - Shell.SetTransition() for specific routes
        // - Custom IPageTransition implementations
        // - Platform-specific transition configurations
    }

    /// <summary>
    /// Gets all configured routes in the application.
    /// Useful for debugging and route validation.
    /// </summary>
    /// <returns>Dictionary of route names and their associated types</returns>
    public static Dictionary<string, string> GetConfiguredRoutes()
    {
        var routes = new Dictionary<string, string>
        {
            // Primary Shell routes (from AppShell.xaml)
            { NavigationRoutes.Home, "MainPage (Home Tab)" },
            { NavigationRoutes.Settings, "MainPage (Settings Tab)" },

            // Secondary routes (registered programmatically)
            { NavigationRoutes.DocumentCapture, "DocumentCapturePage (placeholder: MainPage)" },
            { NavigationRoutes.DocumentPreview, "DocumentPreviewPage (placeholder: MainPage)" },
            { NavigationRoutes.DocumentResults, "DocumentResultsPage (placeholder: MainPage)" },
            { NavigationRoutes.ProcessingStatus, "ProcessingStatusPage (placeholder: MainPage)" },
            { NavigationRoutes.AzureSettings, "AzureSettingsPage (placeholder: MainPage)" },
            { NavigationRoutes.AzureSetup, "AzureSetupPage (placeholder: MainPage)" },
            { NavigationRoutes.Help, "HelpPage (placeholder: MainPage)" },
            { NavigationRoutes.About, "AboutPage (placeholder: MainPage)" },
            { NavigationRoutes.ErrorPage, "ErrorPage (placeholder: MainPage)" },
            { NavigationRoutes.DiagnosticsPage, "DiagnosticsPage (placeholder: MainPage)" }
        };

        return routes;
    }

    /// <summary>
    /// Validates that all expected routes are properly configured.
    /// </summary>
    /// <returns>List of any missing or misconfigured routes</returns>
    public static List<string> ValidateRouteConfiguration()
    {
        var issues = new List<string>();
        var configuredRoutes = GetConfiguredRoutes();

        // Check that all NavigationRoutes constants have corresponding route registrations
        var routeFields = typeof(NavigationRoutes).GetFields();
        foreach (var field in routeFields)
        {
            if (field.IsStatic && field.IsLiteral && field.FieldType == typeof(string))
            {
                var routeValue = field.GetValue(null)?.ToString();
                if (routeValue != null && !configuredRoutes.ContainsKey(routeValue))
                {
                    issues.Add($"Route '{routeValue}' defined in NavigationRoutes but not configured in Shell");
                }
            }
        }

        return issues;
    }
}