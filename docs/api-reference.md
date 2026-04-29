# API Reference (Service Layer)

This document summarizes the primary public service APIs used by the app.

## Core Contracts

### `IImageService`
Image capture, selection, validation, compression, metadata, save/delete operations.

Primary members:
- `PickImageAsync(...)`
- `ValidateImageAsync(...)`
- `CompressImageAsync(...)`
- `GetImageInfoAsync(...)`
- `SaveImageAsync(...)`

### `ISettingsService`
Typed settings persistence with secure storage support for sensitive values.

Primary members:
- `GetAsync<T>(...)` / `SetAsync<T>(...)`
- `GetSecureAsync<T>(...)` / `SetSecureAsync<T>(...)`
- `ContainsKeyAsync(...)` / `RemoveAsync(...)`
- `ValidateSettingsAsync<T>(...)`

### `INavigationService`
Navigation abstraction over MAUI Shell.

Primary members:
- `NavigateToAsync(...)`
- `GoBackAsync(...)`
- `GoToRootAsync()`
- `DisplayAlertAsync(...)`
- `DisplayActionSheetAsync(...)`

### `IDocumentClassifierService`
Classifies document type and confidence to drive routing.

### `ICheckAnalyzerService`
Extracts check fields and maps to normalized output.

### `IDepositSlipAnalyzerService`
Extracts deposit slip fields and maps to normalized output.

### `IDocumentOrchestrationService`
Coordinates end-to-end processing:
1. Validate input
2. Classify
3. Route and extract
4. Normalize
5. Assess quality and report status/events

## Implementations

- `ImageService`
- `SettingsService`
- `NavigationService`
- `DocumentClassifierService`
- `CheckAnalyzerService`
- `DepositSlipAnalyzerService`
- `DocumentOrchestrationService`

## Notes

- Public APIs are documented with XML comments in `Services/Interfaces/*.cs` and service implementation files in `Services/*.cs`.
- For navigation constants and parameter keys, see `INavigationService.cs` (`NavigationRoutes`, `NavigationParameters`).
