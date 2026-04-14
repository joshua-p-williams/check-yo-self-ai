# Architecture Definition - Check Yo Self AI

## System Overview

The Check Yo Self AI application is built using .NET MAUI with a clean, maintainable architecture that separates concerns and enables efficient document processing using Azure AI Document Intelligence.

## Architecture Patterns

### MVVM (Model-View-ViewModel)
- **Views**: XAML-based user interface components
- **ViewModels**: Handle UI logic, data binding, and command execution
- **Models**: Data transfer objects and business entities

### Dependency Injection
- Service registration in `MauiProgram.cs`
- Constructor injection for loose coupling
- Abstraction through interfaces

### Repository Pattern
- Abstracted data access through interfaces
- Configurable storage implementations
- Clean separation between business logic and data persistence

## System Components

### Presentation Layer
```
Views/
├── MainPage.xaml              # Main navigation and document upload
├── SettingsPage.xaml          # Azure configuration settings
├── ProcessingPage.xaml        # Document processing status
├── ResultsPage.xaml           # Results display with tabs
└── Controls/
    ├── ImagePreview.xaml      # Image preview component
    └── ResultViewer.xaml      # Tabbed result viewer
```

### Business Logic Layer
```
ViewModels/
├── MainPageViewModel.cs       # Main page logic and navigation
├── SettingsViewModel.cs       # Settings management
├── ProcessingViewModel.cs     # Document processing orchestration
├── ResultsViewModel.cs        # Results formatting and display
└── Base/
    └── BaseViewModel.cs       # Common view model functionality
```

### Service Layer
```
Services/
├── Interfaces/
│   ├── IDocumentIntelligenceService.cs  # Document processing contract
│   ├── ISettingsService.cs              # Settings persistence contract
│   ├── IImageService.cs                 # Image handling contract
│   └── INavigationService.cs            # Navigation abstraction
├── DocumentIntelligenceService.cs       # Azure AI integration
├── SettingsService.cs                   # Preferences-based settings storage
├── ImageService.cs                      # Image upload and preview handling
└── NavigationService.cs                 # MAUI Shell navigation wrapper
```

### Data Layer
```
Models/
├── DocumentResult.cs          # Processed document results
├── CheckResult.cs            # Bank check specific data
├── DepositSlipResult.cs      # Deposit slip specific data
├── ProcessingStatus.cs       # Processing state information
└── Settings/
    └── AzureSettings.cs      # Azure AI configuration model
```

## Data Flow

### Document Processing Flow
1. **Upload**: User selects image through `MainPage`
2. **Preview**: Image displayed using `ImagePreview` control
3. **Processing**: Document sent to Azure AI through `DocumentIntelligenceService`
4. **Results**: Processed data formatted and displayed in `ResultsPage`

### Settings Flow
1. **Configuration**: User enters Azure credentials in `SettingsPage`
2. **Persistence**: Settings stored securely using `SettingsService`
3. **Retrieval**: Services access settings through dependency injection

## Azure AI Document Intelligence Integration

### Service Configuration
```csharp
var credential = new AzureKeyCredential(apiKey);
var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);
```

### Document Type Handling
- **Checks**: Uses `prebuilt-bankCheck` model
- **Deposit Slips**: Uses custom trained models (configurable)

### Processing Pipeline
1. Image validation and format conversion
2. Document type detection
3. Appropriate model selection
4. Azure AI processing
5. Result parsing and transformation

## Cross-Platform Considerations

### Platform-Specific Services
```
Platforms/
├── Android/
│   └── ImagePickerService.android.cs
├── iOS/
│   └── ImagePickerService.ios.cs
├── Windows/
│   └── ImagePickerService.windows.cs
└── MacCatalyst/
    └── ImagePickerService.maccatalyst.cs
```

### Storage Implementation
- Uses MAUI Preferences API for cross-platform settings
- Secure storage for sensitive Azure credentials
- File system access for temporary image storage

## Security Considerations

### Credential Management
- Azure keys stored using secure storage APIs
- No hardcoded credentials in source code
- Validation of endpoint URLs and key formats

### Data Protection
- Temporary image files cleaned after processing
- No persistent storage of processed documents
- Secure transmission to Azure services

## Performance Optimization

### Image Handling
- Image compression before transmission
- Async processing to maintain UI responsiveness
- Memory-efficient image loading and display

### Azure Integration
- Connection pooling and reuse
- Proper disposal of Azure client resources
- Error handling and retry logic

## Testing Strategy

### Unit Testing
- Service layer abstractions enable easy mocking
- ViewModels tested independently of UI
- Azure integration tested with mock services

### Integration Testing
- End-to-end processing workflows
- Platform-specific functionality validation
- Settings persistence verification

## Future Extensibility

### Additional Document Types
- Plugin architecture for new document processors
- Configurable model selection
- Custom training integration

### Enhanced Results
- Export functionality (PDF, CSV)
- Historical processing records
- Batch processing capabilities

### Advanced Features
- Offline processing capabilities
- Real-time camera capture
- OCR quality assessment and enhancement