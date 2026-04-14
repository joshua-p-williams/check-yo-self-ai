# Feature 2: Check Processing Feature

## Overview

This feature implements the bank check processing capability using Azure AI Document Intelligence's prebuilt bank check model. It leverages the built-in `prebuilt-bankCheck` model to extract structured data from US bank check images, providing both raw JSON results and user-friendly formatted displays.

## Goals

- Integrate Azure AI Document Intelligence SDK for check processing
- Implement secure credential management and API authentication
- Process bank check images using the prebuilt model
- Display results in both JSON and formatted views
- Provide comprehensive error handling and user feedback

## User Stories

### US2.1: Azure AI Integration
**As a** developer  
**I want** to integrate Azure Document Intelligence SDK  
**So that** I can process bank check images using AI

**Acceptance Criteria:**
- Azure Document Intelligence client properly configured
- Secure credential storage and retrieval
- Connection testing and validation
- Proper SDK error handling and logging
- Support for different Azure regions

### US2.2: Check Image Processing
**As a** user  
**I want** to process bank check images  
**So that** I can extract check information automatically

**Acceptance Criteria:**
- Upload and process bank check images
- Use prebuilt-bankCheck model for analysis
- Display processing status with progress indicators
- Handle processing timeouts and errors gracefully
- Support common check image formats (JPEG, PNG)

### US2.3: Results Display
**As a** user  
**I want** to view extracted check data in multiple formats  
**So that** I can understand and use the processed information

**Acceptance Criteria:**
- Tabbed interface with JSON and formatted views
- JSON view with syntax highlighting and formatting
- Formatted view with labeled fields and confidence scores
- Copy/share functionality for results
- Visual overlay of extracted fields on original image

### US2.4: Error Handling and Validation
**As a** user  
**I want** clear feedback when processing fails  
**So that** I can understand and resolve issues

**Acceptance Criteria:**
- Detailed error messages for different failure scenarios
- Retry functionality for transient errors
- Image quality validation before processing
- Network connectivity error handling
- Azure service error interpretation and user guidance

### US2.5: Processing History
**As a** user  
**I want** to review recently processed checks  
**So that** I can reference previous results

**Acceptance Criteria:**
- Local storage of processing results (configurable)
- List view of recent processing sessions
- Quick access to previous results
- Delete functionality for privacy
- Export capabilities for processed data

## Technical Specifications

### Azure Integration

#### Document Intelligence Client Setup
```csharp
public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly DocumentIntelligenceClient _client;
    private readonly ILogger<DocumentIntelligenceService> _logger;
    private readonly ISettingsService _settingsService;

    public async Task<CheckResult> ProcessCheckAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.GetAsync<AzureSettings>("AzureSettings");
        
        using var content = new AnalyzeDocumentContent();
        content.Base64Source = BinaryData.FromStream(imageStream);
        
        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-bankCheck",
            content,
            cancellationToken: cancellationToken);
            
        return MapToCheckResult(operation.Value);
    }
}
```

#### Check Processing Models
```csharp
public class CheckResult
{
    public string DocumentId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public double OverallConfidence { get; set; }
    
    // Check-specific fields from Azure prebuilt model
    public string CheckNumber { get; set; }
    public decimal Amount { get; set; }
    public string PayTo { get; set; }
    public DateTime? Date { get; set; }
    public string Memo { get; set; }
    public string Signature { get; set; }
    
    // Bank information
    public string RoutingNumber { get; set; }
    public string AccountNumber { get; set; }
    public string BankName { get; set; }
    public string BankAddress { get; set; }
    
    // Field confidence scores
    public Dictionary<string, double> FieldConfidences { get; set; }
    
    // Raw Azure response
    public string RawJsonResponse { get; set; }
}

public class ProcessingStatus
{
    public string Id { get; set; }
    public ProcessingState State { get; set; }
    public string StatusMessage { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ErrorMessage { get; set; }
}

public enum ProcessingState
{
    NotStarted,
    Uploading,
    Processing,
    Completed,
    Failed,
    Cancelled
}
```

### UI Components

#### Processing Page Layout
```xml
<ContentPage x:Class="CheckYoSelfAI.Views.ProcessingPage"
             Title="Processing Check">
    <Grid RowDefinitions="Auto,*,Auto">
        
        <!-- Header with progress -->
        <StackLayout Grid.Row="0" Padding="20">
            <Label Text="{Binding StatusMessage}" 
                   FontSize="18" 
                   HorizontalOptions="Center"/>
            <ProgressBar Progress="{Binding ProgressPercentage, Converter={StaticResource PercentageConverter}}"
                         IsVisible="{Binding IsProcessing}"/>
        </StackLayout>
        
        <!-- Image preview -->
        <Image Grid.Row="1" 
               Source="{Binding ImageSource}"
               Aspect="AspectFit"
               HorizontalOptions="FillAndExpand"/>
        
        <!-- Action buttons -->
        <StackLayout Grid.Row="2" 
                     Orientation="Horizontal" 
                     HorizontalOptions="Center"
                     Padding="20">
            <Button Text="Cancel" 
                    Command="{Binding CancelCommand}"
                    IsVisible="{Binding CanCancel}"/>
            <Button Text="Retry" 
                    Command="{Binding RetryCommand}"
                    IsVisible="{Binding CanRetry}"/>
            <Button Text="View Results" 
                    Command="{Binding ViewResultsCommand}"
                    IsVisible="{Binding IsCompleted}"/>
        </StackLayout>
        
    </Grid>
</ContentPage>
```

#### Results Page with Tabbed Views
```xml
<ContentPage x:Class="CheckYoSelfAI.Views.ResultsPage"
             Title="Check Results">
    <Grid RowDefinitions="Auto,*,Auto">
        
        <!-- Header with confidence score -->
        <StackLayout Grid.Row="0" Padding="20" BackgroundColor="{DynamicResource PrimaryColor}">
            <Label Text="Check Processing Results" 
                   FontSize="20" 
                   TextColor="White"
                   HorizontalOptions="Center"/>
            <Label Text="{Binding ConfidenceMessage}" 
                   FontSize="14" 
                   TextColor="White"
                   HorizontalOptions="Center"/>
        </StackLayout>
        
        <!-- Tabbed results view -->
        <TabView Grid.Row="1">
            
            <!-- Formatted view -->
            <TabViewItem Text="Details">
                <ScrollView>
                    <StackLayout Padding="20" Spacing="15">
                        
                        <!-- Check information -->
                        <Frame BackgroundColor="{DynamicResource CardBackgroundColor}">
                            <StackLayout Spacing="10">
                                <Label Text="Check Information" FontAttributes="Bold" FontSize="16"/>
                                
                                <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto,Auto,Auto" ColumnSpacing="10" RowSpacing="5">
                                    <Label Grid.Row="0" Grid.Column="0" Text="Check Number:" FontAttributes="Bold"/>
                                    <Label Grid.Row="0" Grid.Column="1" Text="{Binding Result.CheckNumber}"/>
                                    
                                    <Label Grid.Row="1" Grid.Column="0" Text="Amount:" FontAttributes="Bold"/>
                                    <Label Grid.Row="1" Grid.Column="1" Text="{Binding Result.Amount, StringFormat='{0:C}'}"/>
                                    
                                    <Label Grid.Row="2" Grid.Column="0" Text="Date:" FontAttributes="Bold"/>
                                    <Label Grid.Row="2" Grid.Column="1" Text="{Binding Result.Date, StringFormat='{0:MM/dd/yyyy}'}"/>
                                    
                                    <Label Grid.Row="3" Grid.Column="0" Text="Pay To:" FontAttributes="Bold"/>
                                    <Label Grid.Row="3" Grid.Column="1" Text="{Binding Result.PayTo}"/>
                                </Grid>
                            </StackLayout>
                        </Frame>
                        
                        <!-- Bank information -->
                        <Frame BackgroundColor="{DynamicResource CardBackgroundColor}">
                            <StackLayout Spacing="10">
                                <Label Text="Bank Information" FontAttributes="Bold" FontSize="16"/>
                                
                                <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto,Auto" ColumnSpacing="10" RowSpacing="5">
                                    <Label Grid.Row="0" Grid.Column="0" Text="Bank Name:" FontAttributes="Bold"/>
                                    <Label Grid.Row="0" Grid.Column="1" Text="{Binding Result.BankName}"/>
                                    
                                    <Label Grid.Row="1" Grid.Column="0" Text="Routing Number:" FontAttributes="Bold"/>
                                    <Label Grid.Row="1" Grid.Column="1" Text="{Binding Result.RoutingNumber}"/>
                                    
                                    <Label Grid.Row="2" Grid.Column="0" Text="Account Number:" FontAttributes="Bold"/>
                                    <Label Grid.Row="2" Grid.Column="1" Text="{Binding Result.AccountNumber}"/>
                                </Grid>
                            </StackLayout>
                        </Frame>
                        
                        <!-- Confidence scores -->
                        <Frame BackgroundColor="{DynamicResource CardBackgroundColor}">
                            <StackLayout>
                                <Label Text="Field Confidence Scores" FontAttributes="Bold" FontSize="16"/>
                                <CollectionView ItemsSource="{Binding FieldConfidences}">
                                    <CollectionView.ItemTemplate>
                                        <DataTemplate>
                                            <Grid ColumnDefinitions="*,Auto" Padding="0,5">
                                                <Label Grid.Column="0" Text="{Binding Key}"/>
                                                <Label Grid.Column="1" Text="{Binding Value, StringFormat='{0:P1}'}"/>
                                            </Grid>
                                        </DataTemplate>
                                    </CollectionView.ItemTemplate>
                                </CollectionView>
                            </StackLayout>
                        </Frame>
                        
                    </StackLayout>
                </ScrollView>
            </TabViewItem>
            
            <!-- JSON view -->
            <TabViewItem Text="JSON">
                <ScrollView>
                    <StackLayout Padding="20">
                        <Label Text="Raw JSON Response" FontAttributes="Bold" FontSize="16"/>
                        <Frame BackgroundColor="Black" Padding="15">
                            <Label Text="{Binding Result.RawJsonResponse}" 
                                   FontFamily="Courier"
                                   TextColor="LightGreen"
                                   FontSize="12"/>
                        </Frame>
                    </StackLayout>
                </ScrollView>
            </TabViewItem>
            
        </TabView>
        
        <!-- Action buttons -->
        <StackLayout Grid.Row="2" 
                     Orientation="Horizontal" 
                     HorizontalOptions="Center"
                     Padding="20">
            <Button Text="Copy JSON" 
                    Command="{Binding CopyJsonCommand}"/>
            <Button Text="Share Results" 
                    Command="{Binding ShareResultsCommand}"/>
            <Button Text="Process Another" 
                    Command="{Binding ProcessAnotherCommand}"/>
        </StackLayout>
        
    </Grid>
</ContentPage>
```

### Service Interfaces

#### Document Intelligence Service
```csharp
public interface IDocumentIntelligenceService
{
    Task<bool> TestConnectionAsync(AzureSettings settings);
    Task<CheckResult> ProcessCheckAsync(Stream imageStream, CancellationToken cancellationToken = default);
    Task<ProcessingStatus> GetProcessingStatusAsync(string operationId);
    Task<bool> CancelProcessingAsync(string operationId);
}

public interface IResultsService
{
    Task<bool> SaveResultAsync(CheckResult result);
    Task<IEnumerable<CheckResult>> GetRecentResultsAsync(int count = 10);
    Task<CheckResult> GetResultByIdAsync(string id);
    Task<bool> DeleteResultAsync(string id);
    Task<bool> ExportResultAsync(CheckResult result, string format);
}
```

## Dependencies

### Additional NuGet Packages
- `Azure.AI.DocumentIntelligence` (primary SDK)
- `Microsoft.Toolkit.Mvvm` (command and observable support)
- `Newtonsoft.Json` (JSON formatting and syntax highlighting)

### Platform Capabilities
- Network access for Azure API calls
- File system access for temporary storage
- Clipboard access for copy functionality
- Share functionality for result export

## Implementation Considerations

### Error Handling Strategy
1. **Network Errors**: Retry with exponential backoff
2. **Azure Service Errors**: Parse and display user-friendly messages
3. **Invalid Images**: Pre-validation before sending to Azure
4. **Timeout Handling**: Configurable timeouts with cancellation support
5. **Rate Limiting**: Handle API rate limits gracefully

### Performance Optimization
1. **Image Compression**: Optimize image size before upload
2. **Async Processing**: Use async/await patterns throughout
3. **Caching**: Cache results locally for quick access
4. **Memory Management**: Proper disposal of streams and clients
5. **Progress Reporting**: Real-time progress updates for long operations

### Security Considerations
1. **Credential Storage**: Use SecureStorage for API keys
2. **Data Protection**: Don't persist sensitive check data
3. **Network Security**: Enforce HTTPS for all Azure communications
4. **Input Validation**: Validate all user inputs before processing
5. **Error Logging**: Log errors without exposing sensitive data

## Testing Strategy

### Unit Testing
- Service implementations with mocked Azure clients
- Result parsing and validation logic
- Error handling scenarios
- Data transformation and formatting

### Integration Testing
- End-to-end check processing workflow
- Azure API integration with test credentials
- Image upload and processing pipeline
- Results storage and retrieval

### UI Testing
- Processing flow navigation
- Results display in both formats
- Error message presentation
- Copy and share functionality

## Definition of Done

### Functional Criteria
- [ ] Bank checks can be processed using Azure prebuilt model
- [ ] Results display correctly in both JSON and formatted views
- [ ] Processing status provides real-time feedback
- [ ] Error handling covers all major failure scenarios
- [ ] Copy and share functionality works on all platforms

### Technical Criteria
- [ ] Unit tests cover >90% of service logic
- [ ] Integration tests validate Azure API integration
- [ ] Performance benchmarks met for processing operations
- [ ] Security review completed for credential handling
- [ ] Memory leak testing passed for image processing

### Quality Criteria
- [ ] Code follows established architecture patterns
- [ ] Error messages are user-friendly and actionable
- [ ] UI provides smooth and responsive experience
- [ ] Results accuracy validated with sample check images
- [ ] Cross-platform functionality verified