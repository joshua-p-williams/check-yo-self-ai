using CheckYoSelfAI.Services.Interfaces;

namespace CheckYoSelfAI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Register routes for pages that aren't directly in the Shell hierarchy
            // These routes allow navigation via the NavigationService

            // Document processing workflow routes
            Routing.RegisterRoute(NavigationRoutes.DocumentCapture, typeof(MainPage)); // Placeholder - will be replaced with actual DocumentCapturePage
            Routing.RegisterRoute(NavigationRoutes.DocumentPreview, typeof(MainPage)); // Placeholder - will be replaced with actual DocumentPreviewPage  
            Routing.RegisterRoute(NavigationRoutes.DocumentResults, typeof(MainPage)); // Placeholder - will be replaced with actual DocumentResultsPage
            Routing.RegisterRoute(NavigationRoutes.ProcessingStatus, typeof(MainPage)); // Placeholder - will be replaced with actual ProcessingStatusPage

            // Azure configuration routes
            Routing.RegisterRoute(NavigationRoutes.AzureSettings, typeof(MainPage)); // Placeholder - will be replaced with actual AzureSettingsPage
            Routing.RegisterRoute(NavigationRoutes.AzureSetup, typeof(MainPage)); // Placeholder - will be replaced with actual AzureSetupPage

            // Help and support routes
            Routing.RegisterRoute(NavigationRoutes.Help, typeof(MainPage)); // Placeholder - will be replaced with actual HelpPage
            Routing.RegisterRoute(NavigationRoutes.About, typeof(MainPage)); // Placeholder - will be replaced with actual AboutPage

            // Error and diagnostic routes
            Routing.RegisterRoute(NavigationRoutes.ErrorPage, typeof(MainPage)); // Placeholder - will be replaced with actual ErrorPage  
            Routing.RegisterRoute(NavigationRoutes.DiagnosticsPage, typeof(MainPage)); // Placeholder - will be replaced with actual DiagnosticsPage
        }
    }
}
