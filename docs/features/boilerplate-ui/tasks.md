# Tasks: Document Orchestration Foundation

## Phase 1: Document Pipeline Infrastructure (Days 1-2)

### 1.1 Project Setup
- [x] **Update .csproj file**
  - [x] Add Azure.AI.DocumentIntelligence NuGet package (v1.0.0+)
  - [x] Add CommunityToolkit.Mvvm NuGet package (v8.3.0+)
  - [x] Add Microsoft.Extensions.Logging.Debug package
  - [x] Configure platform-specific permissions (camera, photo library)
  - [x] Set appropriate minimum OS versions

- [x] **Create document pipeline folder structure**
  - [x] Create `Views/` folder for XAML pages
  - [x] Create `ViewModels/` folder for view models
  - [x] Create `Services/` folder with `Interfaces/` subfolder
  - [x] Create `Models/` folder with pipeline DTOs
  - [x] Create `Pipeline/` folder for orchestration logic
  - [x] Create `Controls/` folder for document UI components
  - [x] Create `Converters/` folder for value converters

### 1.2 Document Pipeline Contracts
- [x] **Create pipeline DTOs**
  - [x] Implement `DocumentInput` DTO with metadata and content
  - [x] Implement `ClassificationResult` DTO with type and confidence  
  - [x] Implement `ExtractionResult` DTO for raw model outputs
  - [x] Implement `NormalizedDocument` DTO for unified business data
  - [x] Implement `ConfidenceWarnings` DTO for exception handling

- [x] **Create service abstractions**
  - [x] Create `IDocumentClassifierService` abstraction for `ClassifyDocument`
  - [x] Create `ICheckAnalyzerService` abstraction for `AnalyzeCheck`
  - [x] Create `IDepositSlipAnalyzerService` abstraction for `AnalyzeDepositSlip`
  - [x] Create `IDocumentOrchestrationService` for pipeline coordination
  - [x] Define common exception types for pipeline errors

### 1.2 MVVM Architecture Setup
- [x] **Create base classes**
  - [x] Implement `BaseViewModel` with INotifyPropertyChanged
  - [x] Add common properties (IsBusy, Title, IsInitialized)
  - [x] Implement virtual InitializeAsync method
  - [x] Add error handling and logging support

- [x] **Create BaseContentPage**
  - [x] Generic base class with ViewModel binding
  - [x] Implement OnAppearing/OnDisappearing lifecycle
  - [x] Add loading state management
  - [x] Configure data binding context

- [x] **Configure dependency injection**
  - [x] Set up service registration in MauiProgram.cs
  - [x] Register ViewModels as transient services  
  - [x] Configure logging providers
  - [x] Set up service lifetime management

## Phase 2: Core Services (Days 3-4)

### 2.1 Settings Service Implementation
- [x] **Create ISettingsService interface**
  - [x] Define GetAsync<T> method with default value support
  - [x] Define SetAsync<T> method for storing values
  - [x] Define ContainsKeyAsync method for key existence check
  - [x] Define RemoveAsync method for value removal

- [x] **Implement SettingsService**
  - [x] Use MAUI Preferences API for simple settings
  - [x] Use SecureStorage for sensitive data (API keys)
  - [x] Add JSON serialization for complex objects
  - [x] Implement proper error handling and logging
  - [x] Add validation for setting values

- [x] **Create settings model classes**
  - [x] Create `AzureSettings` class with Endpoint and ApiKey
  - [x] Create `AppSettings` class for application preferences
  - [x] Add validation attributes and IsValid properties
  - [x] Implement IEquatable for change detection

### 2.2 Navigation Service Implementation
- [x] **Create INavigationService interface**
  - [x] Define NavigateToAsync with route and parameters
  - [x] Define GoBackAsync for navigation stack management
  - [x] Define DisplayAlertAsync for user notifications
  - [x] Define DisplayActionSheetAsync for menu options

- [x] **Implement NavigationService**
  - [x] Wrap MAUI Shell navigation functionality
  - [x] Add parameter serialization support
  - [x] Implement navigation stack tracking
  - [x] Add error handling for invalid routes

- [x] **Configure Shell routes**
  - [x] Register routes for all main pages
  - [x] Set up navigation parameters handling
  - [x] Configure page transitions and animations

### 2.3 Image Service Implementation
- [x] **Create IImageService interface**
  - [x] Define PickImageAsync with source parameter (camera/gallery)
  - [x] Define ValidateImageAsync for format and size checking
  - [x] Define CompressImageAsync for size optimization
  - [x] Define GetImageInfoAsync for metadata extraction

- [x] **Implement platform-specific image picking**
  - [x] Create Android implementation using MediaPicker
  - [x] Create iOS implementation using MediaPicker
  - [x] Create Windows implementation using MediaPicker
  - [x] Handle platform-specific permissions and capabilities

- [x] **Implement image validation**
  - [x] Check supported formats (JPEG, PNG)
  - [x] Validate file size limits (max 10MB)
  - [x] Verify image dimensions and quality
  - [x] Add error reporting for invalid images

## Phase 3: UI Foundation (Days 5-7)

### 3.1 Shell and Navigation Structure
- [x] **Create AppShell.xaml**
  - [x] Configure TabBar with Home and Settings tabs
  - [x] Add appropriate icons for each tab
  - [x] Set up Shell visual styling and colors
  - [x] Configure implicit and explicit route mapping

- [x] **Create navigation resources**
  - [x] Add tab icons to platform-specific resources
  - [x] Create shared color and style resources
  - [x] Set up platform-specific styling overrides
  - [x] Configure accessibility labels and hints

### 3.2 Document Processing Components
- [x] **Create upload component for image input**
  - [x] Build camera capture and file selection interface
  - [x] Support image formats (JPEG, PNG)
  - [x] Add file validation and size limits
  - [x] Implement preview functionality
  - [x] Add image metadata display

- [x] **Create processing timeline/status component**
  - [x] Design multi-stage progress indicator
  - [x] Show upload → classify → route → extract → normalize steps
  - [x] Add timing information for each stage
  - [x] Display current processing stage with animations
  - [x] Handle error states in timeline

- [x] **Create classification result card**
  - [x] Display identified document type with confidence
  - [x] Show alternative classification options
  - [x] Add confidence visualization (progress bars/colors)
  - [x] Include reasoning details when available
  - [x] Allow manual override for incorrect classification

- [x] **Create routed-model indicator**  
  - [x] Show which extraction model was selected
  - [x] Display model version and capabilities
  - [x] Indicate routing decision reasoning
  - [x] Show expected extraction fields for selected model
  - [x] Add link to model documentation

- [x] **Create normalized result view**
  - [x] Design model-agnostic result display
  - [x] Support both check and deposit slip fields
  - [x] Show extracted values with confidence scores
  - [x] Add raw vs. formatted result toggle
  - [x] Include export and sharing capabilities

- [x] **Create low-confidence / unsupported-document warning state**
  - [x] Design clear warning messaging
  - [x] Provide recovery options and suggestions
  - [x] Show confidence thresholds and explanations
  - [x] Add manual review workflow triggers
  - [x] Include fallback processing options

### 3.3 Settings Page Implementation
- [x] **Create SettingsPage.xaml layout**
  - [x] Azure AI Configuration section with grouped entries
  - [x] Endpoint URL entry with validation styling
  - [x] API Key entry with secure text entry
  - [x] Test Connection button with loading states
  - [x] App preferences section for future settings

- [x] **Create SettingsPageViewModel**
  - [x] Bind to AzureSettings model properties
  - [x] Implement SaveSettingsCommand with validation
  - [x] Implement TestConnectionCommand (placeholder)
  - [x] Add input validation with error messaging
  - [x] Handle navigation back after successful save

- [x] **Implement settings validation**
  - [x] URL format validation for Azure endpoint
  - [x] API key format basic validation
  - [x] Required field validation with visual feedback
  - [x] Real-time validation as user types

### 3.4 Shared UI Components
- [x] **Create LoadingIndicator control**
  - [x] Animated spinner with customizable size and color
  - [x] Text message support for loading states
  - [x] Overlay capability for full-screen loading
  - [x] Platform-appropriate animations

- [x] **Create ErrorMessage control**
  - [x] Consistent error styling and layout
  - [x] Icon support for different error types
  - [x] Dismissible and persistent message options
  - [x] Accessibility support for screen readers

- [x] **Create shared styles and themes**
  - [x] Define color palette and typography
  - [x] Create button styles for different actions
  - [x] Set up entry and label styling
  - [x] Configure platform-specific overrides

## Phase 4: Integration & Testing (Days 8-9)

### 4.1 Service Integration
- [x] **Complete dependency injection setup**
  - [x] Register all services in MauiProgram.cs
  - [x] Configure service lifetimes appropriately
  - [x] Set up logging and debugging services
  - [x] Validate service resolution and injection

- [x] **Implement end-to-end workflows**
  - [x] Test image upload to preview flow
  - [x] Validate settings save and retrieve flow
  - [x] Verify navigation between all pages
  - [x] Test error handling and recovery scenarios

### 4.2 Unit Testing Implementation
- [x] **Set up testing infrastructure**
  - [x] Add MSTest or xUnit testing framework
  - [x] Configure Moq for service mocking
  - [x] Set up test data and fixtures
  - [x] Configure code coverage reporting

- [x] **Create service unit tests**
  - [x] Test SettingsService with mocked storage
  - [x] Test NavigationService with mocked Shell
  - [x] Test ImageService validation logic
  - [x] Mock external dependencies and test edge cases

- [x] **Create ViewModel unit tests**
  - [x] Test command execution and property changes
  - [x] Mock service dependencies for isolation
  - [x] Test validation logic and error states
  - [x] Verify data binding and change notifications

### 4.3 UI Testing Implementation
- [x] **Set up UI testing framework**
  - [x] Configure Appium or platform-specific UI testing
  - [x] Create page object models for main screens
  - [x] Set up test data and automation helpers
  - [x] Configure cross-platform test execution

- [x] **Create navigation tests**
  - [x] Test tab navigation functionality
  - [x] Verify page transitions and back navigation
  - [x] Test deep linking and parameter passing
  - [x] Validate navigation state management

- [x] **Create interaction tests**
  - [x] Test image upload and preview functionality
  - [x] Validate settings form input and validation
  - [x] Test button states and loading indicators
  - [x] Verify error message display and dismissal

## Phase 5: Polish & Documentation (Day 10)

### 5.1 Performance Optimization
- [ ] **Optimize image handling**
  - [ ] Implement image compression and resizing
  - [ ] Add async loading with cancellation support
  - [ ] Monitor memory usage during image operations
  - [ ] Implement image caching if needed

- [ ] **Profile application performance**
  - [ ] Measure startup time and memory usage
  - [ ] Test navigation performance on all platforms
  - [ ] Monitor UI responsiveness during operations
  - [ ] Optimize any identified bottlenecks

### 5.2 Accessibility & Polish
- [ ] **Implement accessibility features**
  - [x] Add semantic descriptions for all UI elements
  - [x] Configure tab order and keyboard navigation
  - [ ] Test with screen readers on all platforms
  - [ ] Validate color contrast and font sizes

- [ ] **Add visual polish**
  - [x] Implement loading animations and transitions
  - [x] Add haptic feedback for interactions
  - [ ] Polish iconography and visual design
  - [ ] Test visual consistency across platforms

### 5.3 Documentation Updates
- [x] **Update code documentation**
  - [x] Add XML documentation for all public APIs
  - [x] Document service interfaces and implementations
  - [x] Create inline comments for complex logic
  - [x] Generate API documentation

- [x] **Update project documentation**
  - [x] Update README with current architecture
  - [x] Document setup and configuration steps
  - [x] Create troubleshooting guide for common issues
  - [x] Update feature status and next steps

## Phase 6: Optimized Wizard UI Flow (Timeline + Interactive Data Alignment)

### 6.1 Current Component Inventory (Baseline)
- [ ] **Confirm current UI workflow inventory and ownership**
  - [ ] Main host: `MainPage.xaml` embeds `DocumentUploadView`
  - [ ] Upload + action area: `Controls/DocumentUploadView.xaml`
  - [ ] Timeline rail: `Controls/ProcessingTimelineView.xaml`
  - [ ] Workflow orchestration state: `ViewModels/DocumentUploadViewModel.cs`
  - [ ] Timeline stage model: `ViewModels/ProcessingTimelineStage.cs`
  - [ ] Display item models: `ViewModels/ProcessingDisplayItems.cs`
  - [ ] Shared status controls: `Controls/LoadingIndicator.xaml`, `Controls/ErrorMessageView.xaml`

- [ ] **Map current content sections to pipeline stages**
  - [ ] Upload: image selection, preview, metadata
  - [ ] Classify: classification card, confidence, alternatives
  - [ ] Route: routed model, capabilities, expected fields, docs link
  - [ ] Extract: extracted field list with confidence
  - [ ] Normalize: normalized view + raw JSON/export/share
  - [ ] Warning lane: low confidence / unsupported / manual review actions

### 6.2 Wizard Experience Definition
- [ ] **Define wizard-first information architecture**
  - [ ] Co-locate processing timeline and interactive data flow in one shared stage workspace
  - [ ] Show only step-relevant data while the user is in each stage
  - [ ] Keep a compact persistent step header for small-screen constraints
  - [ ] Add end-state "Processing Summary" that consolidates all stage outputs

- [ ] **Define stage-by-stage UX behavior**
  - [ ] Stage 1 (Upload): show upload actions, preview, and image validation details
  - [ ] Stage 2 (Classify): show classification result and manual override options
  - [ ] Stage 3 (Route): show routing decision, model details, and expected fields
  - [ ] Stage 4 (Extract): show extracted fields and confidence guidance
  - [ ] Stage 5 (Normalize): show normalized output controls and output mode toggle
  - [ ] Stage 6 (Summary): show cross-stage recap + warnings + next actions

### 6.3 ViewModel and State Refactor Tasks
- [ ] **Introduce explicit wizard step state in `DocumentUploadViewModel`**
  - [ ] Add current wizard step enum and step index helpers
  - [ ] Add visible section flags per step (Upload/Classify/Route/Extract/Normalize/Summary)
  - [ ] Add Back/Next navigation commands independent from processing execution
  - [ ] Preserve existing `Run Next Step` and `Run All Remaining` processing behavior

- [ ] **Add summary projection model for final step**
  - [ ] Add summary display items for document, classification, routing, extraction, normalization
  - [ ] Add warning + confidence rollup in one consolidated summary panel
  - [ ] Add completeness indicators for unfinished/missing stage outputs

### 6.4 UI Composition Tasks (XAML)
- [ ] **Restructure `DocumentUploadView` into a wizard workspace layout**
  - [ ] Create a single stage workspace container that contains timeline + active-step panel
  - [ ] Replace always-visible stacked cards with step-conditional content panes
  - [ ] Keep status/alert messaging visible without breaking wizard continuity
  - [ ] Ensure layout works in narrow widths (phone portrait) without horizontal overflow

- [ ] **Refine `ProcessingTimelineView` for wizard alignment**
  - [ ] Make timeline act as a step navigator/indicator within the same workspace
  - [ ] Highlight current step and completed steps with clear visual hierarchy
  - [ ] Add compact mode behavior for small real estate

### 6.5 Interaction and Navigation Tasks
- [ ] **Implement wizard navigation controls**
  - [ ] Add Back/Next step buttons with disabled states by step validity
  - [ ] Keep processing actions contextual to the active step
  - [ ] Ensure manual overrides/fallback routing correctly reset downstream steps

- [ ] **Align progression logic with data reveal strategy**
  - [ ] Reveal new data only when corresponding stage completes
  - [ ] Prevent later-stage content from appearing before prerequisite stages
  - [ ] Keep summary step read-only by default with explicit actions to rerun/edit

### 6.6 Validation, Accessibility, and Testing
- [ ] **Update unit tests for wizard state behavior**
  - [ ] Add tests for step visibility flags and navigation commands
  - [ ] Add tests for summary model population across normal and warning paths
  - [ ] Add tests for downstream resets after overrides/fallbacks

- [ ] **Update UI and accessibility coverage**
  - [ ] Add/adjust UI tests for wizard progression and stage-specific rendering
  - [ ] Validate keyboard order and semantic descriptions in wizard mode
  - [ ] Validate screen reader flow for step transitions and summary content
  - [ ] Validate layout on small screens across iOS/Android/Windows

### 6.7 Rollout and Documentation
- [ ] **Document optimized flow behavior**
  - [ ] Update `docs/features/boilerplate-ui/feature.md` with wizard flow interaction model
  - [ ] Update `docs/features/boilerplate-ui/plan.md` with implementation sequence and risks
  - [ ] Add before/after screenshots or annotated mock references for handoff

## Verification Checklist

### Functional Verification
- [ ] Image can be selected from gallery and camera
- [ ] Image preview displays correctly with zoom/pan
- [ ] Settings can be saved and persist across app restarts
- [ ] Navigation works smoothly between all pages
- [ ] Error messages display appropriately for invalid inputs
- [ ] Loading states provide appropriate user feedback

### Technical Verification
- [ ] All unit tests pass with >90% code coverage
- [ ] UI tests cover critical user paths
- [ ] No memory leaks during normal usage
- [ ] Performance benchmarks met for image operations
- [ ] Code follows established style guidelines
- [ ] All NuGet packages are up to date

### Cross-Platform Verification
- [ ] Functionality identical across iOS, Android, Windows
- [ ] UI renders correctly on various screen sizes
- [ ] Platform-specific behaviors work as expected
- [ ] Permissions properly requested and handled
- [ ] App store/deployment requirements met

### Ready for Next Feature
- [ ] Settings service provides Azure configuration
- [ ] Image service delivers processed images for analysis
- [ ] Navigation can route to results display pages
- [ ] Error handling framework supports processing scenarios
- [ ] UI foundation supports additional workflows