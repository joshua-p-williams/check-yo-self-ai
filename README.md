# Check Yo Self AI - Azure Document Intelligence Demo

A .NET MAUI application that demonstrates the power of Azure AI Document Intelligence by processing financial documents like bank checks and deposit slips.

## Overview

This application provides an intuitive interface for users to upload images of financial documents (checks and deposit slips), preview them, and process them using Azure AI Document Intelligence service. The app supports both built-in document types (US bank checks) and custom document types (various deposit slip formats).

## Features

- 🖼️ **Image Upload & Preview**: Upload and preview document images before processing
- ✅ **Check Processing**: Uses Azure's built-in bank check model for automated data extraction
- 📋 **Deposit Slip Processing**: Processes custom deposit slip formats using trained models
- ⚙️ **Settings Management**: Secure storage of Azure AI credentials with persistent settings
- 📊 **Dual Result Views**: View results in both JSON format and user-friendly display
- 📱 **Cross-Platform**: Built with .NET MAUI for iOS, Android, Windows, and macOS

## Prerequisites

- .NET 10 SDK
- Visual Studio 2026 or later with MAUI workload
- Azure AI Document Intelligence resource
- Valid Azure subscription

## Azure AI Document Intelligence Setup

1. Create an Azure AI Document Intelligence resource in the Azure portal
2. Note your endpoint URL (e.g., `https://your-resource.cognitiveservices.azure.com/`)
3. Copy your resource key from the Azure portal
4. Configure these values in the app's settings screen

## Architecture

The application follows a clean MVVM architecture pattern with dependency injection:

- **Views/Controls**: XAML pages and reusable controls for upload, timeline, and results
- **ViewModels**: Command-driven UI logic and workflow orchestration
- **Services**: Abstracted platform and domain services (`ImageService`, `SettingsService`, `NavigationService`, document pipeline services)
- **Models**: DTOs and normalized result types for check/deposit-slip processing

Service contracts are defined under `Services/Interfaces`, and concrete implementations are registered in `MauiProgram.cs`.

## Getting Started

1. Clone this repository
2. Open `check-yo-self-ai.sln` in Visual Studio
3. Restore NuGet packages
4. Set a target platform (`Windows`, `Android`, `iOS`, or `macOS`) and run the application
5. Open the **Settings** tab and configure:
   - Azure endpoint URL
   - Azure API key (stored in secure storage)
   - Azure region
6. Use the **Home** tab to capture/select an image, then run the guided document pipeline

## Troubleshooting

For common setup and runtime issues, see [docs/troubleshooting.md](docs/troubleshooting.md).

Common quick checks:
- Confirm the MAUI workload and `.NET 10` SDK are installed.
- Verify Azure endpoint/key/region values in `Settings`.
- Ensure camera/photo permissions are granted on device platforms.
- Run `dotnet test Tests/CheckYoSelfAI.Tests.csproj` to validate core flows.

## Testing

### Unit tests (current default in CI-friendly workflows)

The test project is located at `Tests/CheckYoSelfAI.Tests.csproj`.

Run all tests:

```powershell
dotnet test Tests/CheckYoSelfAI.Tests.csproj
```

Run with coverage collector settings:

```powershell
dotnet test Tests/CheckYoSelfAI.Tests.csproj --settings Tests/coverage.runsettings
```

### UI tests (Appium-based)

UI tests are scaffolded under `Tests/UI/` and currently marked to skip unless you intentionally enable and run them with an Appium environment.

#### 1) Install Appium 2 and driver

```powershell
npm install -g appium
appium driver install windows
```

#### 2) Start Appium server

```powershell
appium
```

#### 3) Set environment variables

At minimum (if running `dotnet test` directly):

```powershell
$env:UI_TEST_PLATFORM="Windows"
$env:UI_TEST_APPIUM_SERVER="http://127.0.0.1:4723"
$env:UI_TEST_APP_ID="<your app id or executable path>"
```

Optional:

```powershell
$env:UI_TEST_DEVICE="WindowsPC"
$env:UI_TEST_AUTOMATION="Windows"
```

#### 4) Run only UI-category tests

```powershell
dotnet test Tests/CheckYoSelfAI.Tests.csproj --filter "Category=UI"
```

or use helper script (recommended):

```powershell
./Tests/UI/run-ui-tests.ps1 -Platform Windows -AppiumServer "http://127.0.0.1:4723"
```

`run-ui-tests.ps1` can auto-detect `UI_TEST_APP_ID` by finding the latest built `check-yo-self-ai.exe` under `bin/Debug` or `bin/Release`.

If auto-detection fails, either build the app first or pass `-AppId` explicitly:

```powershell
./Tests/UI/run-ui-tests.ps1 -Platform Windows -AppId "C:\path\to\check-yo-self-ai.exe"
```

### CI/CD guidance

- Keep normal CI pipelines running `dotnet test` without requiring Appium.
- Run UI tests in a separate pipeline/job that provisions Appium + target device/emulator.
- This separation keeps automated unit-test validation stable while allowing UI automation expansion later.

## Project Structure

```
check-yo-self-ai/
├── docs/                          # Documentation
│   ├── architecture-definition.md  # System architecture details
│   ├── product-definition.md       # Product requirements and specifications
│   └── features/                   # Feature documentation
│       ├── boilerplate-ui/         # Core UI and infrastructure
│       ├── check-feature/          # Check processing functionality
│       └── deposit-slip-feature/   # Deposit slip processing functionality
├── Views/                          # XAML pages and user interface
├── ViewModels/                     # MVVM view models
├── Services/                       # Business logic and external integrations
├── Models/                         # Data models and DTOs
└── Platforms/                      # Platform-specific implementations
```

## Documentation

Comprehensive documentation is available in the `docs/` folder:

- [Architecture Definition](docs/architecture-definition.md)
- [Product Definition](docs/product-definition.md)
- [Feature Specifications](docs/features/)
- [Troubleshooting Guide](docs/troubleshooting.md)
- [Service/API Reference](docs/api-reference.md)
- [Boilerplate UI Feature Status](docs/features/boilerplate-ui/status.md)

## Technologies Used

- **.NET 10**: Latest .NET framework
- **.NET MAUI**: Cross-platform UI framework
- **Azure AI Document Intelligence**: Document processing and data extraction
- **MVVM Pattern**: Clean separation of concerns
- **Preferences API**: Secure settings storage

## Contributing

This project serves as a technical demonstration of Azure AI Document Intelligence capabilities. Feel free to explore the code and documentation to understand the implementation patterns.

## License

This project is for demonstration purposes.