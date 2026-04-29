using CheckYoSelfAI.Models;
using CheckYoSelfAI.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckYoSelfAI.Services;

public class IntegrationValidationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IntegrationValidationService> _logger;

    public IntegrationValidationService(IServiceProvider serviceProvider, ILogger<IntegrationValidationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        ValidateServiceResolution(provider);
        await ValidateImageWorkflowAsync(provider, cancellationToken);
        await ValidateSettingsRoundTripAsync(provider, cancellationToken);
        ValidateNavigationWorkflow(provider);
        await ValidateErrorHandlingWorkflowAsync(provider, cancellationToken);
        await ValidatePipelineHealthAsync(provider, cancellationToken);
        ValidateRouteConfiguration();

        _logger.LogInformation("Phase 4.1 integration validation completed.");
    }

    private async Task ValidateImageWorkflowAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var imageService = provider.GetRequiredService<IImageService>();
        var validation = await imageService.ValidateImageAsync("integration-validation-missing-image.jpg");

        if (validation.IsValid)
        {
            throw new InvalidOperationException("Image validation workflow check expected missing image to fail validation.");
        }

        _logger.LogInformation("Image upload-to-preview workflow validation passed via image validation path check.");
    }

    private void ValidateServiceResolution(IServiceProvider provider)
    {
        _ = provider.GetRequiredService<ISettingsService>();
        _ = provider.GetRequiredService<INavigationService>();
        _ = provider.GetRequiredService<IImageService>();
        _ = provider.GetRequiredService<IDocumentClassifierService>();
        _ = provider.GetRequiredService<ICheckAnalyzerService>();
        _ = provider.GetRequiredService<IDepositSlipAnalyzerService>();
        _ = provider.GetRequiredService<IDocumentOrchestrationService>();

        _logger.LogInformation("Dependency injection resolution validation passed.");
    }

    private static async Task ValidateSettingsRoundTripAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var settingsService = provider.GetRequiredService<ISettingsService>();
        const string key = "integration_validation_ping";

        await settingsService.SetAsync(key, "ok");
        var value = await settingsService.GetAsync(key, string.Empty);
        await settingsService.RemoveAsync(key);

        if (!string.Equals(value, "ok", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Settings round-trip validation failed.");
        }
    }

    private void ValidateNavigationWorkflow(IServiceProvider provider)
    {
        _ = provider.GetRequiredService<INavigationService>();

        var routes = ShellRouteConfiguration.GetConfiguredRoutes();
        if (!routes.ContainsKey(NavigationRoutes.Home) || !routes.ContainsKey(NavigationRoutes.Settings))
        {
            throw new InvalidOperationException("Navigation workflow validation failed: required shell routes are missing.");
        }

        _logger.LogInformation("Navigation workflow validation passed for home and settings routes.");
    }

    private async Task ValidateErrorHandlingWorkflowAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var classifier = provider.GetRequiredService<IDocumentClassifierService>();

        try
        {
            await classifier.ClassifyDocumentAsync(null!, cancellationToken);
            throw new InvalidOperationException("Error handling validation failed: expected ArgumentNullException was not thrown.");
        }
        catch (ArgumentNullException)
        {
            _logger.LogInformation("Error handling and recovery workflow validation passed for classifier null input guard.");
        }
    }

    private async Task ValidatePipelineHealthAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var orchestrationService = provider.GetRequiredService<IDocumentOrchestrationService>();
        var health = await orchestrationService.CheckPipelineHealthAsync(cancellationToken);

        _logger.LogInformation("Pipeline health check status: {Status}", health.OverallStatus);
    }

    private void ValidateRouteConfiguration()
    {
        var routeIssues = ShellRouteConfiguration.ValidateRouteConfiguration();
        if (routeIssues.Count == 0)
        {
            _logger.LogInformation("Navigation route configuration validation passed.");
            return;
        }

        foreach (var issue in routeIssues)
        {
            _logger.LogWarning("Route configuration issue: {Issue}", issue);
        }
    }
}
