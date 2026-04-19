using Microsoft.Extensions.Logging;
using CheckYoSelfAI.Services.Interfaces;
using CheckYoSelfAI.Services;

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

            // Configure ViewModels
            ConfigureViewModels(builder);

            // Configure Views
            ConfigureViews(builder);

            return builder.Build();
        }

        private static void ConfigureLogging(MauiAppBuilder builder)
        {
            builder.Logging.ClearProviders();

#if DEBUG
            builder.Logging.AddDebug();
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
            // Note: These will be implemented in future phases
            // For now, we register the interfaces for dependency injection setup

            // Document orchestration service (main pipeline coordinator)
            // builder.Services.AddScoped<IDocumentOrchestrationService, DocumentOrchestrationService>();

            // Classification service (document type identification)
            // builder.Services.AddScoped<IDocumentClassifierService, DocumentClassifierService>();

            // Check analysis service (US bank check processing)
            // builder.Services.AddScoped<ICheckAnalyzerService, CheckAnalyzerService>();

            // Deposit slip analysis service (custom neural models)
            // builder.Services.AddScoped<IDepositSlipAnalyzerService, DepositSlipAnalyzerService>();

            // Settings and configuration services
            builder.Services.AddSingleton<ISettingsService, SettingsService>();

            // Navigation and image services (Phase 2)
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // Image service (Phase 2.3)
            // builder.Services.AddScoped<IImageService, ImageService>();

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

            // Main ViewModels (will be created in Phase 3)
            // builder.Services.AddTransient<MainPageViewModel>();
            // builder.Services.AddTransient<SettingsPageViewModel>();

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

            // Main Views (will be created in Phase 3)
            // builder.Services.AddTransient<MainPage>();
            // builder.Services.AddTransient<SettingsPage>();

            // Document processing Views (will be created in later phases)
            // builder.Services.AddTransient<DocumentUploadPage>();
            // builder.Services.AddTransient<DocumentResultsPage>();

            // Note: We'll add these registrations as we create the Views
        }
    }
}
