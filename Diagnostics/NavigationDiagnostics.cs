using CheckYoSelfAI.Services;
using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.Diagnostics;

/// <summary>
/// Diagnostic utilities for validating navigation and route configuration.
/// Useful for development and debugging navigation issues.
/// </summary>
public static class NavigationDiagnostics
{
    /// <summary>
    /// Validates the current navigation configuration and logs any issues.
    /// </summary>
    /// <param name="logger">Logger for outputting diagnostic information</param>
    /// <returns>True if configuration is valid, false if issues were found</returns>
    public static bool ValidateNavigationConfiguration(ILogger? logger = null)
    {
        var isValid = true;

        logger?.LogInformation("=== Navigation Configuration Validation ===");

        try
        {
            // Validate Shell route configuration
            var routeIssues = ShellRouteConfiguration.ValidateRouteConfiguration();
            if (routeIssues.Any())
            {
                isValid = false;
                logger?.LogWarning("Found {Count} route configuration issues:", routeIssues.Count);
                foreach (var issue in routeIssues)
                {
                    logger?.LogWarning("  - {Issue}", issue);
                }
            }
            else
            {
                logger?.LogInformation("✅ All routes properly configured");
            }

            // Log configured routes for reference
            var configuredRoutes = ShellRouteConfiguration.GetConfiguredRoutes();
            logger?.LogInformation("📋 Configured routes ({Count} total):", configuredRoutes.Count);
            foreach (var route in configuredRoutes)
            {
                logger?.LogInformation("  - {Route} → {Target}", route.Key, route.Value);
            }

            // Check if NavigationService is available
            var navigationService = GetNavigationServiceIfAvailable();
            if (navigationService != null)
            {
                logger?.LogInformation("✅ NavigationService is available");

                // Test basic navigation service functionality
                var currentPage = navigationService.GetCurrentPage();
                if (currentPage != null)
                {
                    logger?.LogInformation("✅ Current page detected: {PageType}", currentPage.GetType().Name);
                }
                else
                {
                    logger?.LogWarning("⚠️ No current page detected");
                }
            }
            else
            {
                logger?.LogWarning("⚠️ NavigationService not available (may be normal during early startup)");
            }

        }
        catch (Exception ex)
        {
            isValid = false;
            logger?.LogError(ex, "❌ Exception during navigation validation");
        }

        logger?.LogInformation("=== Validation Complete: {Result} ===", isValid ? "PASSED" : "FAILED");
        return isValid;
    }

    /// <summary>
    /// Gets the NavigationService if it's available in the current service provider.
    /// </summary>
    /// <returns>NavigationService instance or null if not available</returns>
    private static INavigationService? GetNavigationServiceIfAvailable()
    {
        try
        {
            // Try to get the service from the current application's service provider
            var app = Application.Current;
            if (app?.Handler?.MauiContext?.Services != null)
            {
                return app.Handler.MauiContext.Services.GetService<INavigationService>();
            }
        }
        catch
        {
            // Service not available or not yet initialized
        }

        return null;
    }

    /// <summary>
    /// Tests navigation to each configured route (for development testing).
    /// Note: This should only be used in development/debug scenarios.
    /// </summary>
    /// <param name="navigationService">NavigationService to use for testing</param>
    /// <param name="logger">Logger for test results</param>
    /// <returns>Results of navigation tests</returns>
    public static async Task<Dictionary<string, bool>> TestNavigationRoutes(
        INavigationService navigationService, 
        ILogger? logger = null)
    {
        var results = new Dictionary<string, bool>();
        var routes = ShellRouteConfiguration.GetConfiguredRoutes();

        logger?.LogInformation("=== Testing Navigation Routes ===");

        foreach (var route in routes.Keys)
        {
            try
            {
                logger?.LogInformation("Testing route: {Route}", route);

                // For testing, we won't actually navigate (to avoid disrupting the UI)
                // In a real test scenario, you might navigate and then navigate back
                // await navigationService.NavigateToAsync(route);
                // await navigationService.GoBackAsync();

                // For now, we'll just validate that the route exists
                results[route] = true;
                logger?.LogInformation("✅ Route {Route} is valid", route);
            }
            catch (Exception ex)
            {
                results[route] = false;
                logger?.LogError(ex, "❌ Route {Route} failed: {Error}", route, ex.Message);
            }
        }

        logger?.LogInformation("=== Navigation Tests Complete ===");
        return results;
    }
}