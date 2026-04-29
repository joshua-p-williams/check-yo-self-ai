# Copilot Instructions for Check Yo Self AI

## Project Overview
- **Type**: .NET MAUI cross-platform app (iOS, Android, Windows, macOS)
- **Purpose**: Demo Azure AI Document Intelligence for processing financial documents (checks, deposit slips)
- **Architecture**: Clean MVVM (Views/XAML, ViewModels, Services, Models)
- **Key Integration**: Azure AI Document Intelligence (prebuilt and custom models)

## Architecture & Patterns
- **MVVM**: Strict separation of UI (Views), logic (ViewModels), and data (Models)
- **Dependency Injection**: All services registered in `MauiProgram.cs` and injected via constructors
- **Repository Pattern**: Used for data access and settings persistence
- **Platform Services**: Platform-specific code in `Platforms/` (e.g., image picking)

## Developer Workflows
- **Build**: Use Visual Studio 2026+ with .NET 10 and MAUI workload
- **Run**: Open `check-yo-self-ai.sln`, restore NuGet, set platform, run
- **Configure**: Enter Azure endpoint/key in app Settings page (stored securely)
- **Test**: 
  - Unit tests mock Azure services; integration tests validate end-to-end flows
  - For Appium UI tests, ensure the Appium server is running and configured correctly. Use the provided test scripts in the `tests/appium` directory to execute UI tests without breaking CI/CD pipelines.

## Key Conventions
- **No hardcoded credentials**: Use secure storage APIs for Azure keys
- **All document processing**: Routed through `DocumentIntelligenceService` (see `Services/`)
- **Result display**: Unified, model-agnostic viewer (toggle JSON/business view)
- **Error handling**: User-facing errors must be actionable and clear
- **Settings**: Persisted via `SettingsService` using MAUI Preferences API
- **Image handling**: Use `ImageService` for all image operations
- **Navigation**: Use `NavigationService` abstraction, not direct Shell calls
- **Asset Workflow**: Prefer a MAUI-first asset workflow; avoid platform-specific asset-pack integration approaches.

## Data Flow Example
1. User uploads image (MainPage → ImageService)
2. Image previewed (ImagePreview control)
3. User triggers processing (calls DocumentIntelligenceService)
4. Results parsed, normalized, and shown (ResultsPage)

## Security & Privacy
- Azure credentials encrypted in storage
- No persistent storage of processed documents
- Temporary files cleaned after use

## Extensibility
- Add new document types via plugin-style processors
- All outputs normalized to shared result shape
- UI foundation supports new workflows with minimal changes

## Documentation
- See `docs/architecture-definition.md` for system details
- See `docs/features/` for feature specs and plans
- See `README.md` for quickstart and structure

---
**When in doubt, follow the MVVM pattern, use dependency injection, and keep all Azure integration in the service layer.**
