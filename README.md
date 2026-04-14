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

The application follows a clean MVVM architecture pattern with:

- **Views**: XAML-based UI with platform-specific customizations
- **ViewModels**: Business logic and data binding
- **Services**: Azure AI integration and settings management
- **Models**: Data transfer objects for document analysis results

## Getting Started

1. Clone this repository
2. Open `check-yo-self-ai.sln` in Visual Studio
3. Restore NuGet packages
4. Set your target platform and run the application
5. Configure your Azure AI credentials in the settings screen
6. Upload a check or deposit slip image and start processing!

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