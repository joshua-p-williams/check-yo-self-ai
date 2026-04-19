# Tasks: US Bank Check Extraction (Post-Classification)

## Phase 1: Check Analyzer Service (Days 1-2)

### 1.1 Implement Check Analyzer Service using `prebuilt-check.us`
- [ ] **Create ICheckAnalyzerService interface**
  - [ ] Define AnalyzeCheckAsync method that accepts DocumentInput
  - [ ] Return ExtractionResult DTO with raw Azure response
  - [ ] Add confidence scoring and field-level validation
  - [ ] Include error handling for extraction failures
  - [ ] Support cancellation tokens for long-running operations

- [ ] **Implement CheckAnalyzerService class**
  - [ ] Configure Azure DocumentIntelligenceClient for `prebuilt-check.us`
  - [ ] Process pre-classified check documents only
  - [ ] Extract check fields: amount, date, payer/payee, routing/account/check numbers
  - [ ] Map Azure response to ExtractionResult DTO
  - [ ] Implement proper error handling and logging

### 1.2 Create Raw-to-Normalized Check Mapping  
- [ ] **Design normalized check fields mapping**
  - [ ] Map Azure check fields to NormalizedDocument schema
  - [ ] Handle amount formatting and currency conversion
  - [ ] Parse and validate dates from various check formats  
  - [ ] Extract and validate routing/account number formats
  - [ ] Process payee/payer name fields with confidence

- [ ] **Implement CheckMappingService**
  - [ ] Create mapping logic from raw Azure response to normalized fields
  - [ ] Add field-level confidence preservation
  - [ ] Handle missing or low-confidence fields gracefully  
  - [ ] Validate extracted data against business rules
  - [ ] Generate processing warnings for data quality issues

### 1.3 Add Sample Teller-Capture Check Test Files
- [ ] **Collect representative check samples**
  - [ ] Gather various US bank check formats and layouts
  - [ ] Include skewed and imperfect teller-capture scenarios
  - [ ] Add checks with different handwriting and print qualities
  - [ ] Collect checks with various amounts and date formats
  - [ ] Include edge cases: damaged, partially obscured checks

- [ ] **Create test data structure**
  - [ ] Organize sample files by quality and complexity  
  - [ ] Document expected extraction results for each sample
  - [ ] Add metadata about scan quality and expected confidence
  - [ ] Create test scenarios for validation workflows
  ## Phase 2: Check-Specific Result Display (Days 3-4)

  ### 2.1 Create Confidence-Aware Result Display
  - [ ] **Design check result UI components**
    - [ ] Create check-specific field layout (amount, date, payee, etc.)
    - [ ] Add confidence visualization for each extracted field
    - [ ] Implement color coding for confidence levels (high/medium/low)
    - [ ] Show field-level warnings for low-confidence extractions
    - [ ] Add tooltips explaining confidence scores and warnings

  - [ ] **Implement CheckResultsViewModel**
    - [ ] Bind to NormalizedDocument check fields
    - [ ] Display confidence scores with visual indicators
    - [ ] Handle missing or failed field extractions
    - [ ] Implement raw JSON toggle for technical view
    - [ ] Add export functionality for check results

  ### 2.2 Add Fallback Warning for Weak or Partial Extraction
  - [ ] **Design extraction quality validation**
    - [ ] Define critical check fields that must be present (amount, date)
    - [ ] Set confidence thresholds for acceptable extraction quality
    - [ ] Create validation rules for check field formats
    - [ ] Implement overall extraction confidence scoring
    - [ ] Add business logic validation (reasonable amounts, valid dates)

  - [ ] **Create warning and fallback UI**
    - [ ] Design warning cards for low-confidence results
    - [ ] Add explanatory text for extraction failures
    - [ ] Provide suggestions for re-scanning or manual entry
    - [ ] Include feedback options for misclassified documents
    - [ ] Add manual review workflow triggers

  ### 2.3 Validate Result Rendering for Skewed / Imperfect Scans
  - [ ] **Test with challenging check images**
    - [ ] Validate extraction accuracy with skewed check images
    - [ ] Test with low-quality teller-capture scenarios
    - [ ] Verify confidence scoring for imperfect scans
    - [ ] Document extraction limitations and edge cases
    - [ ] Create user guidance for optimal check capture

  - [ ] **Optimize result display for edge cases**
    - [ ] Handle partial or corrupted field extractions gracefully
    - [ ] Show appropriate messages for unreadable areas
    - [ ] Provide visual feedback for extraction confidence
    - [ ] Add zoom and enhancement options for unclear results
    - [ ] Include retry options with capture guidance
- [ ] **Update Settings page for Azure configuration**
  - [ ] Add test connection button with loading state
  - [ ] Implement connection status indicator (success/failure/testing)
  - [ ] Add Azure region selection if needed
  - [ ] Include processing timeout configuration
  - [ ] Add model selection options for future extensibility

- [ ] **Implement connection testing**
  - [ ] Create TestConnectionCommand in SettingsPageViewModel
  - [ ] Call DocumentIntelligenceService.TestConnectionAsync
  - [ ] Display connection result with appropriate messaging
  - [ ] Handle various connection failure scenarios
  - [ ] Save successful configuration automatically

- [ ] **Add validation and error handling**
  - [ ] Validate Azure endpoint URL format
  - [ ] Check API key format and length requirements
  - [ ] Test network connectivity before Azure calls
  - [ ] Display validation errors in real-time
  - [ ] Provide troubleshooting guidance for common issues

## Phase 2: Check Processing Logic (Days 3-4)

### 2.1 Document Processing Pipeline
- [ ] **Implement ProcessCheckAsync method**
  - [ ] Validate input image stream (format, size, content)
  - [ ] Convert image to Azure-compatible format if needed
  - [ ] Create AnalyzeDocumentContent with base64 image data
  - [ ] Call Azure API with "prebuilt-bankCheck" model ID
  - [ ] Handle long-running operation with status polling

- [ ] **Add progress tracking and cancellation**
  - [ ] Implement CancellationToken support throughout pipeline
  - [ ] Track processing progress with status updates
  - [ ] Handle operation cancellation gracefully
  - [ ] Provide cancellation feedback to UI
  - [ ] Clean up resources on cancellation

- [ ] **Implement image validation**
  - [ ] Check supported formats (JPEG, PNG, PDF)
  - [ ] Validate file size limits (under 50MB for Azure)
  - [ ] Verify image dimensions and resolution
  - [ ] Check for corrupted or invalid image data
  - [ ] Provide specific validation error messages

### 2.2 Result Mapping and Transformation
- [ ] **Map Azure response to CheckResult**
  - [ ] Parse AnalyzeResult to extract check fields
  - [ ] Map prebuilt model field names to CheckResult properties
  - [ ] Handle missing or null field values gracefully
  - [ ] Extract confidence scores for each field
  - [ ] Preserve raw JSON response for display

- [ ] **Implement data type conversion**
  - [ ] Convert string amounts to decimal with proper formatting
  - [ ] Parse date strings to DateTime objects with validation
  - [ ] Handle various date formats from different banks
  - [ ] Validate routing and account number formats
  - [ ] Clean and normalize text fields (trim, case correction)

- [ ] **Extract confidence and quality metrics**
  - [ ] Calculate overall document confidence score
  - [ ] Extract individual field confidence scores
  - [ ] Identify low-confidence fields for user attention
  - [ ] Map Azure confidence values to user-friendly indicators
  - [ ] Store confidence data for results display

### 2.3 Error Handling and Retry Logic
- [ ] **Implement comprehensive error handling**
  - [ ] Map Azure service errors to user-friendly messages
  - [ ] Handle network connectivity errors with retry logic
  - [ ] Process timeout scenarios with appropriate feedback
  - [ ] Handle rate limiting with exponential backoff
  - [ ] Log errors for debugging without exposing sensitive data

- [ ] **Add retry logic with exponential backoff**
  - [ ] Implement configurable retry policies
  - [ ] Add exponential backoff for transient failures
  - [ ] Distinguish between retryable and non-retryable errors
  - [ ] Limit maximum retry attempts to prevent infinite loops
  - [ ] Provide retry status feedback to users

- [ ] **Create error recovery mechanisms**
  - [ ] Allow manual retry after error resolution
  - [ ] Provide troubleshooting guidance for specific errors
  - [ ] Cache processed results to prevent re-processing on retry
  - [ ] Handle partial processing results appropriately
  - [ ] Implement graceful degradation for non-critical errors

## Phase 3: Processing UI and UX (Days 5-6)

### 3.1 Processing Page Implementation
- [ ] **Create ProcessingPage.xaml layout**
  - [ ] Add header with processing status message
  - [ ] Include progress bar with percentage display
  - [ ] Show image preview during processing
  - [ ] Add cancel button with confirmation dialog
  - [ ] Include retry button for failed operations

- [ ] **Create ProcessingPageViewModel**
  - [ ] Implement navigation parameter handling for image data
  - [ ] Create ProcessCommand to start check processing
  - [ ] Add CancelCommand with cancellation token handling
  - [ ] Implement RetryCommand for failed operations
  - [ ] Handle navigation to results page on completion

- [ ] **Implement progress tracking**
  - [ ] Bind progress percentage to UI progress bar
  - [ ] Update status message based on processing stage
  - [ ] Show estimated time remaining if available
  - [ ] Handle indeterminate progress for Azure operations
  - [ ] Animate progress updates smoothly

### 3.2 Processing Workflow Integration
- [ ] **Update MainPage for check processing**
  - [ ] Add "Process as Check" button to MainPageViewModel
  - [ ] Navigate to ProcessingPage with image parameter
  - [ ] Pass processing type information to processing page
  - [ ] Handle back navigation from processing workflow
  - [ ] Clear previous processing state on new upload

- [ ] **Implement navigation parameter passing**
  - [ ] Create processing parameters model for navigation
  - [ ] Serialize image data for navigation (or use shared service)
  - [ ] Pass processing configuration (model type, options)
  - [ ] Handle navigation parameter validation
  - [ ] Implement proper parameter cleanup

- [ ] **Add processing state management**
  - [ ] Track current processing operation across navigation
  - [ ] Handle app backgrounding during processing
  - [ ] Restore processing state on app activation
  - [ ] Clean up processing resources on completion
  - [ ] Handle navigation interruptions gracefully

### 3.3 Status and Progress Feedback
- [ ] **Implement real-time progress updates**
  - [ ] Connect to Azure operation status polling
  - [ ] Update UI with processing milestones (uploaded, analyzing, complete)
  - [ ] Show detailed status messages for each processing stage
  - [ ] Handle connection issues during status polling
  - [ ] Provide fallback status when polling fails

- [ ] **Add visual feedback and animations**
  - [ ] Implement loading animations during processing
  - [ ] Add success/error animations for completion states
  - [ ] Use platform-appropriate progress indicators
  - [ ] Add haptic feedback for processing completion
  - [ ] Ensure smooth UI updates without blocking

- [ ] **Create platform-specific implementations**
  - [ ] Use native progress indicators on each platform
  - [ ] Implement platform-appropriate status notifications
  - [ ] Handle platform-specific backgrounding behavior
  - [ ] Add platform-specific error dialogs and alerts
  - [ ] Ensure consistent behavior across platforms

## Phase 4: Results Display (Days 7-8)

### 4.1 Results Page Foundation
- [ ] **Create ResultsPage.xaml with tabbed layout**
  - [ ] Set up TabView with "Details" and "JSON" tabs
  - [ ] Add header with confidence score display
  - [ ] Include action buttons (copy, share, process another)
  - [ ] Configure consistent styling and spacing
  - [ ] Ensure accessibility support for all elements

- [ ] **Create ResultsPageViewModel**
  - [ ] Receive CheckResult via navigation parameters
  - [ ] Set up data binding for all result properties
  - [ ] Implement CopyJsonCommand for clipboard operations
  - [ ] Add ShareResultsCommand for platform sharing
  - [ ] Create ProcessAnotherCommand for workflow continuation

- [ ] **Implement navigation parameter handling**
  - [ ] Accept CheckResult through navigation parameters
  - [ ] Validate received result data
  - [ ] Handle missing or corrupt result data
  - [ ] Set up proper data context for binding
  - [ ] Initialize tab selection and display state

### 4.2 Formatted Results View
- [ ] **Design check information display**
  - [ ] Create card-based layout for different data sections
  - [ ] Group related fields (check info, bank info, amounts)
  - [ ] Add confidence indicators for each field
  - [ ] Use appropriate formatting for currencies and dates
  - [ ] Include visual icons for different data types

- [ ] **Implement confidence score indicators**
  - [ ] Create visual confidence indicators (colors, icons, progress bars)
  - [ ] Add confidence percentage display for each field
  - [ ] Highlight low-confidence fields for user attention
  - [ ] Provide overall document confidence summary
  - [ ] Add tooltips or help text for confidence meanings

- [ ] **Add field validation and formatting**
  - [ ] Format currency amounts with proper locale settings
  - [ ] Display dates in user-friendly formats
  - [ ] Validate and format routing/account numbers
  - [ ] Truncate or wrap long text fields appropriately
  - [ ] Handle null or missing field values gracefully

### 4.3 JSON Results View
- [ ] **Implement JSON syntax highlighting**
  - [ ] Format JSON with proper indentation and structure
  - [ ] Add syntax highlighting for better readability
  - [ ] Use monospace font for consistent formatting
  - [ ] Add line numbers if helpful for debugging
  - [ ] Ensure JSON is properly escaped and valid

- [ ] **Create scrollable JSON viewer**
  - [ ] Use ScrollView for large JSON responses
  - [ ] Implement horizontal scrolling for long lines
  - [ ] Add zoom functionality for detailed inspection
  - [ ] Include search functionality within JSON
  - [ ] Optimize performance for large JSON documents

- [ ] **Add JSON interaction features**
  - [ ] Implement select-all functionality for copying
  - [ ] Add copy-to-clipboard for entire JSON
  - [ ] Include formatted vs. minified JSON options
  - [ ] Add expand/collapse for nested JSON objects
  - [ ] Provide JSON validation and error highlighting

### 4.4 Results Actions and Export
- [ ] **Implement copy to clipboard functionality**
  - [ ] Add platform-specific clipboard implementation
  - [ ] Copy JSON data with proper formatting
  - [ ] Copy formatted results as text
  - [ ] Provide user feedback for successful copy operations
  - [ ] Handle clipboard permissions and errors

- [ ] **Add share functionality**
  - [ ] Use platform share dialog for result sharing
  - [ ] Format results appropriately for sharing (text, JSON)
  - [ ] Include image attachment if relevant
  - [ ] Add email/message templates for sharing
  - [ ] Handle share cancellation and errors gracefully

- [ ] **Create export functionality**
  - [ ] Export results as formatted text file
  - [ ] Export raw JSON to file
  - [ ] Add PDF export for formatted results
  - [ ] Include file picker for save location
  - [ ] Handle export permissions and file system access

## Phase 5: Testing and Polish (Days 9-10)

### 5.1 Comprehensive Testing
- [ ] **Create unit tests for service logic**
  - [ ] Test DocumentIntelligenceService with mocked Azure client
  - [ ] Test result parsing with sample Azure responses
  - [ ] Test error handling with various failure scenarios
  - [ ] Mock dependencies for isolated testing
  - [ ] Achieve >90% code coverage for service layer

- [ ] **Implement integration tests**
  - [ ] Test end-to-end processing with real Azure service
  - [ ] Validate processing with various check image formats
  - [ ] Test error scenarios with invalid configurations
  - [ ] Verify results accuracy with known test data
  - [ ] Test network resilience and recovery

- [ ] **Create UI automation tests**
  - [ ] Test complete processing workflow navigation
  - [ ] Validate results display in both tab views
  - [ ] Test copy and share functionality
  - [ ] Verify error message display and handling
  - [ ] Test cancellation and retry operations

### 5.2 Performance Optimization
- [ ] **Optimize image handling performance**
  - [ ] Implement image compression before Azure submission
  - [ ] Optimize memory usage during image processing
  - [ ] Add image caching for repeated operations
  - [ ] Monitor and optimize garbage collection impact
  - [ ] Test with various image sizes and formats

- [ ] **Optimize Azure API interactions**
  - [ ] Implement connection pooling and reuse
  - [ ] Optimize JSON parsing and object creation
  - [ ] Add response caching where appropriate
  - [ ] Monitor API call performance and timing
  - [ ] Implement timeout optimization based on image size

- [ ] **Profile and optimize UI performance**
  - [ ] Monitor UI thread usage during processing
  - [ ] Optimize data binding performance in results view
  - [ ] Test scrolling performance with large JSON
  - [ ] Profile memory usage during navigation
  - [ ] Optimize startup time and navigation speed

### 5.3 Documentation and Polish
- [ ] **Update code documentation**
  - [ ] Add XML documentation for all public APIs
  - [ ] Document service interfaces and implementations
  - [ ] Create inline comments for complex processing logic
  - [ ] Update architecture documentation with new components
  - [ ] Generate API reference documentation

- [ ] **Create user documentation**
  - [ ] Write user guide for check processing workflow
  - [ ] Create troubleshooting guide for common errors
  - [ ] Document Azure setup and configuration requirements
  - [ ] Add FAQ for processing issues and limitations
  - [ ] Include sample images and expected results

- [ ] **Final polish and validation**
  - [ ] Review and refine error messages for clarity
  - [ ] Ensure consistent UI styling and behavior
  - [ ] Validate accessibility compliance
  - [ ] Test with various Azure regions and configurations
  - [ ] Perform final cross-platform validation

## Verification Checklist

### Functional Verification
- [ ] Check images process successfully with Azure prebuilt model
- [ ] Results display accurately in both formatted and JSON views
- [ ] Processing status provides real-time feedback and progress
- [ ] Error handling covers network, Azure, and validation failures
- [ ] Copy and share functionality works on all target platforms
- [ ] Cancellation and retry operations work correctly

### Technical Verification
- [ ] Azure integration follows security best practices
- [ ] Performance benchmarks met for processing operations
- [ ] Memory usage remains within acceptable bounds
- [ ] Unit tests achieve >90% coverage with passing status
- [ ] Integration tests validate end-to-end functionality
- [ ] Code follows established architecture patterns

### User Experience Verification
- [ ] Processing workflow feels intuitive and responsive
- [ ] Status messages and progress indicators are helpful
- [ ] Results presentation is clear and well-organized
- [ ] Error messages provide actionable guidance
- [ ] Cross-platform experience is consistent and native

### Integration Verification
- [ ] Successfully integrates with Feature 1 foundation services
- [ ] Provides reusable patterns for Feature 3 (deposit slips)
- [ ] Settings service properly manages Azure credentials
- [ ] Navigation service handles processing workflow correctly
- [ ] Image service provides compatible image processing

## Ready for Feature 3 Handoff
- [ ] Processing infrastructure documented and extensible
- [ ] Results display framework supports additional document types
- [ ] Azure integration patterns established and reusable
- [ ] Error handling framework ready for custom models
- [ ] UI patterns established for document processing workflows