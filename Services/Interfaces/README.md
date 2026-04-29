# Document Pipeline Service Abstractions

This document provides an overview of the service interfaces used by the document processing pipeline and the concrete service implementations in `Services/`.

## Created Files

### Service Interfaces

1. **`IDocumentClassifierService.cs`** - Document classification service interface
   - Classifies documents into types (check, deposit slip, unknown)
   - Supports batch processing and model information queries
   - Includes confidence threshold configuration
   - Provides model validation capabilities

2. **`ICheckAnalyzerService.cs`** - US bank check analysis service interface
   - Uses Azure's prebuilt US bank check model (`prebuilt-check.us`)
   - Processes pre-classified check documents only
   - Provides mapping to normalized document format
   - Includes check-specific validation and quality assessment

3. **`IDepositSlipAnalyzerService.cs`** - Custom neural deposit slip analysis service interface
   - Uses custom trained neural models for variable layouts
   - Supports multiple models for different bank formats
   - Handles complex deposit slip validation (amounts, items)
   - Provides model management capabilities

4. **`IDocumentOrchestrationService.cs`** - Main pipeline coordination service interface
   - Orchestrates the complete document processing workflow
   - Provides end-to-end processing from input to normalized result
   - Supports batch processing with configurable concurrency
   - Includes event-driven progress tracking and health monitoring

5. **`DocumentProcessingExceptions.cs`** - Common exception types
   - Base `DocumentProcessingException` with stage tracking
   - Specific exceptions for classification, extraction, normalization
   - Azure AI service exceptions with error code mapping
   - Rich error details for debugging and handling

## Service Interface to Implementation Mapping

- `IDocumentClassifierService` → `DocumentClassifierService`
- `ICheckAnalyzerService` → `CheckAnalyzerService`
- `IDepositSlipAnalyzerService` → `DepositSlipAnalyzerService`
- `IDocumentOrchestrationService` → `DocumentOrchestrationService`
- `IImageService` → `ImageService`
- `ISettingsService` → `SettingsService`
- `INavigationService` → `NavigationService`

All services are registered in dependency injection via `MauiProgram.cs`.

## Key Features

### Service Design Patterns
- **Interface Segregation**: Each service has a focused responsibility
- **Async-First**: All operations return `Task` for non-blocking execution
- **Cancellation Support**: `CancellationToken` support throughout
- **Batch Processing**: Efficient multi-document processing capabilities
- **Event-Driven**: Progress tracking and completion notifications

### Error Handling Strategy
- **Structured Exceptions**: Specific exception types for different failure scenarios
- **Rich Error Context**: Document ID, stage, error codes, and details
- **Azure Integration**: Specific handling for Azure AI service errors
- **Debugging Support**: Error details dictionary for troubleshooting

### Configuration and Health
- **Model Management**: Information about AI models in use
- **Health Monitoring**: Service availability and performance checks
- **Configurable Thresholds**: Adjustable confidence and quality settings
- **Validation Framework**: Built-in quality assessment and recommendations

## Service Dependencies

### Document Processing Flow
```
IDocumentOrchestrationService
├── IDocumentClassifierService (classify document type)
├── ICheckAnalyzerService (extract check data if classified as check)
├── IDepositSlipAnalyzerService (extract deposit data if classified as deposit slip)
└── Quality Assessment & Normalization
```

### Data Flow Through Services
1. **Input**: `DocumentInput` (image with metadata)
2. **Classification**: `ClassificationResult` (document type + confidence)
3. **Routing**: Route to appropriate analyzer based on type
4. **Extraction**: `ExtractionResult` (raw field data + confidence)
5. **Normalization**: `NormalizedDocument` (unified business schema)
6. **Assessment**: `ConfidenceWarnings` (quality + recommendations)

## Implementation Guidelines

### Service Implementation Considerations
- **Azure SDK Integration**: Services will use Azure.AI.DocumentIntelligence SDK
- **Configuration Management**: Services need access to Azure credentials and endpoints
- **Logging**: Comprehensive logging for monitoring and debugging
- **Retry Logic**: Handle transient failures with exponential backoff
- **Resource Management**: Proper disposal of Azure clients and streams

### Testing Strategy
- **Unit Tests**: Mock service dependencies for isolated testing
- **Integration Tests**: End-to-end testing with Azure AI services
- **Performance Tests**: Batch processing and concurrent operation testing
- **Health Check Tests**: Service availability and configuration validation

## API Documentation

Service API surface is documented with XML comments in:
- `Services/Interfaces/*.cs`
- `Services/*.cs` (public service implementations)

For a high-level generated-style API index, see `docs/api-reference.md`.
