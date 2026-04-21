using Microsoft.Extensions.Logging;
using CheckYoSelfAI.Services.Interfaces;
using CheckYoSelfAI.Services;
using CheckYoSelfAI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CheckYoSelfAI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configure logging
            ConfigureLogging(builder);

            // Configure services
            ConfigureServices(builder);

            // Configure Shell routes and navigation
            ConfigureShellRoutes();

            // Configure ViewModels
            ConfigureViewModels(builder);

            // Configure Views
            ConfigureViews(builder);

            var app = builder.Build();
            ValidateServiceIntegration(app);
            return app;
        }

        private static void ConfigureLogging(MauiAppBuilder builder)
        {
            builder.Logging.ClearProviders();

#if DEBUG
            builder.Logging.AddDebug();
            builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
            builder.Logging.AddFilter("System", LogLevel.Warning);
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
            builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

            // Note: Console logging may not be available on all MAUI platforms
            // For comprehensive logging, consider using a cross-platform logging solution
        }

        private static void ConfigureServices(MauiAppBuilder builder)
        {
            // Core infrastructure services
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
            builder.Services.AddSingleton<IPreferences>(Preferences.Default);

            // Document processing pipeline services
            builder.Services.AddSingleton<IDocumentClassifierService, DocumentClassifierService>();
            builder.Services.AddSingleton<ICheckAnalyzerService, CheckAnalyzerService>();
            builder.Services.AddSingleton<IDepositSlipAnalyzerService, DepositSlipAnalyzerService>();
            builder.Services.AddSingleton<IDocumentOrchestrationService, DocumentOrchestrationService>();

            // Settings and configuration services
            builder.Services.AddSingleton<ISettingsService, SettingsService>();

            // Navigation and image services
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IImageService, ImageService>();

            // Integration validation service
            builder.Services.AddSingleton<IntegrationValidationService>();

            // Azure AI Document Intelligence client
            // This will be configured when we implement the actual services
            // builder.Services.AddHttpClient();
            // builder.Services.AddSingleton<DocumentIntelligenceClient>(provider =>
            // {
            //     var settingsService = provider.GetRequiredService<ISettingsService>();
            //     var settings = settingsService.GetAzureSettingsAsync().Result;
            //     return new DocumentIntelligenceClient(
            //         new Uri(settings.Endpoint), 
            //         new AzureKeyCredential(settings.ApiKey));
            // });
        }

        private static void ConfigureViewModels(MauiAppBuilder builder)
        {
            // Register all ViewModels as transient services
            // ViewModels should be transient because they are tied to specific view instances

            // Main ViewModels (Phase 3)
            builder.Services.AddTransient<ViewModels.DocumentUploadViewModel>();
            builder.Services.AddTransient<ViewModels.SettingsPageViewModel>();

            // Document processing ViewModels (will be created in later phases)
            // builder.Services.AddTransient<DocumentUploadViewModel>();
            // builder.Services.AddTransient<DocumentResultsViewModel>();
            // builder.Services.AddTransient<ClassificationResultViewModel>();

            // Note: We'll add these registrations as we create the ViewModels
        }

        private static void ConfigureViews(MauiAppBuilder builder)
        {
            // Register Views as transient services
            // Views should be transient because they are tied to specific navigation instances

            // Main Views
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SettingsPage>();

            // Document processing Views (will be created in later phases)
            // builder.Services.AddTransient<DocumentUploadPage>();
            // builder.Services.AddTransient<DocumentResultsPage>();

        }

        private static void ConfigureShellRoutes()
        {
            // Configure Shell navigation routes and parameters
            // This sets up additional navigation behaviors beyond what's defined in AppShell.xaml
            ShellRouteConfiguration.ConfigureRoutes();
        }

        private static void ValidateServiceIntegration(MauiApp app)
        {
#if DEBUG
            using var scope = app.Services.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IntegrationValidationService>();
            validator.ValidateAsync().GetAwaiter().GetResult();
#endif
        }
            }
}
