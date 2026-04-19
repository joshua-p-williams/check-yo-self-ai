# Feature 4: Custom Neural Extraction for Variable-Layout Deposit Slips (Post-Classification)

## Overview

This feature implements **custom neural extraction for variable-layout deposit slips after routing**. Deposit slips vary significantly by bank and branch, making them ideal candidates for Azure's **custom neural model** approach. This feature is invoked only after classification identifies the document as a deposit slip, and it uses custom trained models to extract common business fields across layout variants.

## Goals

- Process documents already classified as deposit slips
- Use Azure's **custom neural model** for variable-layout extraction
- Extract consistent business fields across different bank/branch formats  
- Handle layout diversity through neural model training
- Map custom model outputs to normalized teller-document schema
- Support retraining for new institution variants

## User Stories

### US4.1: Custom Neural Model Training
**As a** developer  
**I want** to train custom neural models for deposit slip variations  
**So that** I can extract consistent fields across different bank layouts

**Acceptance Criteria:**
- Collect representative deposit slips across multiple banks/branches
- Label the same conceptual fields consistently across layout variants
- Train and test as **custom neural** model (not template-based)
- Expect and handle significant layout diversity  
- Support model versioning and retraining workflows

### US4.2: Custom Model Integration (Post-Classification)
**As a** developer  
**I want** to integrate custom neural models into the document pipeline  
**So that** deposit slips are processed using specialized trained models after classification

**Acceptance Criteria:**
- Receive documents already classified as deposit slip type
- Use trained custom neural model for extraction
- Handle model confidence scoring and field validation
- Return structured extraction results with business field mapping
- Support model versioning and updates without code changes

### US4.3: Cross-Layout Field Extraction
**As a** user  
**I want** to extract consistent business fields from deposit slips regardless of bank layout  
**So that** I can process deposit slips from different institutions uniformly

**Acceptance Criteria:**
- Extract common deposit slip fields (account numbers, amounts, dates, deposit items)
- Handle cash vs. check deposit line items correctly
- Process totals and subtotals with validation
- Map varied field layouts to consistent business schema
- Provide confidence scoring for extracted business fields

### US4.4: Layout Variance Handling  
**As a** user  
**I want** appropriate confidence indicators when processing deposit slips from new layouts  
**So that** I can identify when retraining might be needed

**Acceptance Criteria:**
- Detect when extraction confidence is unusually low
- Identify field patterns that don't match training data
- Suggest when new layout samples should be collected
- Provide feedback for model improvement workflows
- Handle graceful degradation for unsupported layouts

## Technical Specifications

### Custom Neural Model Training Strategy

The deposit slip feature uses custom neural models with these design principles:

- **Layout diversity expectation**: Train across multiple bank/branch formats from the start
- **Consistent field labeling**: Label the same conceptual business fields across all layout variants  
- **Neural-first approach**: Use custom neural model capabilities, not template-first assumptions
- **Retraining capability**: Support sub-variants and retraining when institutions differ materially

### Deposit Slip Field Schema

Common business fields extracted across layout variants:

```csharp
public class DepositSlipFields
{
    // Account information
    public string AccountNumber { get; set; }
    public string AccountHolderName { get; set; }
    public DateTime DepositDate { get; set; }

    // Deposit items
    public List<DepositItem> CheckDeposits { get; set; }
    public decimal CashAmount { get; set; }
    public decimal TotalCheckAmount { get; set; }
    public decimal NetDepositAmount { get; set; }

    // Validation fields
    public string DepositSlipNumber { get; set; }
    public string BankName { get; set; }
    public string BranchLocation { get; set; }
}

public class DepositItem  
{
    public string CheckNumber { get; set; }
    public decimal Amount { get; set; }
    public string BankRoutingNumber { get; set; }
    public double Confidence { get; set; }
}
```

## Documentation References

### Custom Neural Model Training
- **Custom Neural**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/train/custom-neural?view=doc-intel-4.0.0
- **Custom Model Build Guide**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/how-to-guides/build-a-custom-model?view=doc-intel-4.0.0
- **Service Limits**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/service-limits?view=doc-intel-4.0.0

These matter because Microsoft positions custom neural for cases where documents contain similar information across different layouts, and the build/service-limit docs help plan the demo's training and testing work realistically.
- Comparison view for multiple deposit items

### US3.4: Model Training Integration
**As a** user  
**I want** guidance on training custom models  
**So that** I can create models for new deposit slip formats

**Acceptance Criteria:**
- Documentation and guidance for model training
- Integration with Azure AI Document Intelligence Studio
- Validation of trained models before deployment
- Best practices for deposit slip model training
- Sample deposit slip templates and labeling guidance

### US3.5: Document Type Detection
**As a** user  
**I want** automatic detection of document type (check vs deposit slip)  
**So that** I can process documents without manual selection

**Acceptance Criteria:**
- Automatic document classification before processing
- Confidence-based routing to appropriate processing pipeline
- Manual override option for misclassified documents
- Processing history showing document type detection accuracy
- Fallback options when detection confidence is low

## Technical Specifications

### Custom Model Integration

#### Custom Model Service
```csharp
public interface ICustomModelService
{
    Task<IEnumerable<CustomModel>> GetAvailableModelsAsync();
    Task<CustomModel> GetModelAsync(string modelId);
    Task<bool> ValidateModelAsync(string modelId);
    Task<CustomModel> RegisterModelAsync(string modelId, string name, string description);
    Task<bool> DeleteModelAsync(string modelId);
    Task<ModelCapabilities> GetModelCapabilitiesAsync(string modelId);
}

public class CustomModelService : ICustomModelService
{
    private readonly DocumentIntelligenceClient _client;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<CustomModelService> _logger;

    public async Task<DepositSlipResult> ProcessDepositSlipAsync(
        Stream imageStream, 
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            modelId, // Custom model ID instead of prebuilt
            new AnalyzeDocumentContent { Base64Source = BinaryData.FromStream(imageStream) },
            cancellationToken: cancellationToken);
            
        return MapToDepositSlipResult(operation.Value, modelId);
    }
}
```

#### Deposit Slip Models
```csharp
public class DepositSlipResult
{
    public string DocumentId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string ModelId { get; set; }
    public string ModelName { get; set; }
    public double OverallConfidence { get; set; }
    
    // Deposit slip specific fields
    public string DepositSlipNumber { get; set; }
    public DateTime? DepositDate { get; set; }
    public string AccountNumber { get; set; }
    public decimal TotalCashAmount { get; set; }
    public decimal TotalCheckAmount { get; set; }
    public decimal NetDepositAmount { get; set; }
    
    // Depositor information
    public string DepositorName { get; set; }
    public string DepositorSignature { get; set; }
    
    // Individual deposit items
    public List<DepositItem> DepositItems { get; set; } = new List<DepositItem>();
    
    // Field confidence scores
    public Dictionary<string, double> FieldConfidences { get; set; }
    
    // Raw model response
    public string RawJsonResponse { get; set; }
}

public class DepositItem
{
    public string ItemType { get; set; } // Cash, Check
    public decimal Amount { get; set; }
    public string CheckNumber { get; set; }
    public string BankRoutingNumber { get; set; }
    public string Description { get; set; }
    public double Confidence { get; set; }
}

public class CustomModel
{
    public string ModelId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public ModelStatus Status { get; set; }
    public ModelCapabilities Capabilities { get; set; }
    public List<string> SupportedLanguages { get; set; }
}

public enum ModelStatus
{
    Training,
    Ready,
    Failed,
    Cancelled
}

public class ModelCapabilities
{
    public List<string> SupportedFields { get; set; }
    public bool SupportsTableExtraction { get; set; }
    public bool SupportsSignatureDetection { get; set; }
    public int MaxDocumentSize { get; set; }
    public List<string> SupportedFormats { get; set; }
}
```

### Document Type Classification

#### Document Classifier Service
```csharp
public interface IDocumentClassifierService
{
    Task<DocumentClassification> ClassifyDocumentAsync(Stream imageStream);
    Task<bool> ValidateDocumentTypeAsync(Stream imageStream, DocumentType expectedType);
    Task<ClassificationConfidence> GetClassificationConfidenceAsync(Stream imageStream);
}

public class DocumentClassifierService : IDocumentClassifierService
{
    public async Task<DocumentClassification> ClassifyDocumentAsync(Stream imageStream)
    {
        // Use Azure prebuilt classifier or custom classifier model
        // Analyze image characteristics, text patterns, layout structure
        // Return classification with confidence score
        
        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-document", // Use general classifier first
            new AnalyzeDocumentContent { Base64Source = BinaryData.FromStream(imageStream) });
            
        return AnalyzeLayoutForDocumentType(operation.Value);
    }
    
    private DocumentClassification AnalyzeLayoutForDocumentType(AnalyzeResult result)
    {
        // Analyze layout patterns, text content, field positions
        // Look for check-specific patterns (MICR line, check number position)
        // Look for deposit slip patterns (deposit amounts, item lists)
        // Return classification with confidence
    }
}

public class DocumentClassification
{
    public DocumentType Type { get; set; }
    public double Confidence { get; set; }
    public string ReasoningDetails { get; set; }
    public List<DocumentTypeScore> AlternativeTypes { get; set; }
}

public enum DocumentType
{
    Unknown,
    BankCheck,
    DepositSlip,
    Other
}

public class DocumentTypeScore
{
    public DocumentType Type { get; set; }
    public double Score { get; set; }
    public string Evidence { get; set; }
}
```

### UI Extensions

#### Model Selection Interface
```xml
<!-- Enhanced Main Page with document type detection -->
<ContentPage x:Class="CheckYoSelfAI.Views.MainPage">
    <Grid RowDefinitions="Auto,*,Auto,Auto">
        
        <!-- Header with document type detection results -->
        <Frame Grid.Row="0" BackgroundColor="{DynamicResource SecondaryColor}" IsVisible="{Binding ShowClassificationResult}">
            <StackLayout>
                <Label Text="{Binding ClassificationResult.Message}" FontAttributes="Bold"/>
                <ProgressBar Progress="{Binding ClassificationResult.Confidence}" 
                           ProgressColor="{Binding ClassificationResult.ConfidenceColor}"/>
                <StackLayout Orientation="Horizontal" HorizontalOptions="Center">
                    <Button Text="Process as Check" 
                            Command="{Binding ProcessAsCheckCommand}"
                            IsVisible="{Binding CanProcessAsCheck}"/>
                    <Button Text="Process as Deposit Slip" 
                            Command="{Binding ProcessAsDepositSlipCommand}"
                            IsVisible="{Binding CanProcessAsDepositSlip}"/>
                    <Button Text="Choose Manually" 
                            Command="{Binding ChooseManuallyCommand}"/>
                </StackLayout>
            </StackLayout>
        </Frame>
        
        <!-- Image upload and preview -->
        <StackLayout Grid.Row="1">
            <!-- Existing image upload and preview content -->
        </StackLayout>
        
        <!-- Model selection for deposit slips -->
        <Frame Grid.Row="2" IsVisible="{Binding ShowModelSelection}">
            <StackLayout>
                <Label Text="Select Deposit Slip Model:" FontAttributes="Bold"/>
                <Picker ItemsSource="{Binding AvailableModels}"
                        SelectedItem="{Binding SelectedModel}"
                        ItemDisplayBinding="{Binding Name}"
                        Title="Choose a model..."/>
                <Label Text="{Binding SelectedModel.Description}" 
                       FontSize="12" 
                       TextColor="{DynamicResource SecondaryTextColor}"/>
            </StackLayout>
        </Frame>
        
        <!-- Processing actions -->
        <StackLayout Grid.Row="3" Orientation="Horizontal" HorizontalOptions="Center" Spacing="10">
            <Button Text="Classify Document" 
                    Command="{Binding ClassifyDocumentCommand}"
                    IsVisible="{Binding CanClassify}"/>
            <Button Text="Process Document" 
                    Command="{Binding ProcessDocumentCommand}"
                    IsEnabled="{Binding CanProcess}"/>
            <Button Text="Manage Models" 
                    Command="{Binding ManageModelsCommand}"/>
        </StackLayout>
        
    </Grid>
</ContentPage>
```

#### Deposit Slip Results Display
```xml
<!-- Enhanced Results Page for deposit slips -->
<ContentPage x:Class="CheckYoSelfAI.Views.DepositSlipResultsPage">
    <Grid RowDefinitions="Auto,*,Auto">
        
        <!-- Header with model information -->
        <StackLayout Grid.Row="0" Padding="20" BackgroundColor="{DynamicResource PrimaryColor}">
            <Label Text="Deposit Slip Processing Results" 
                   FontSize="20" 
                   TextColor="White"
                   HorizontalOptions="Center"/>
            <Label Text="{Binding ModelDisplayName}" 
                   FontSize="14" 
                   TextColor="White"
                   HorizontalOptions="Center"/>
            <Label Text="{Binding ConfidenceMessage}" 
                   FontSize="14" 
                   TextColor="White"
                   HorizontalOptions="Center"/>
        </StackLayout>
        
        <!-- Tabbed results view -->
        <TabView Grid.Row="1">
            
            <!-- Formatted deposit slip view -->
            <TabViewItem Text="Deposit Details">
                <ScrollView>
                    <StackLayout Padding="20" Spacing="15">
                        
                        <!-- Deposit summary -->
                        <Frame BackgroundColor="{DynamicResource CardBackgroundColor}">
                            <StackLayout>
                                <Label Text="Deposit Summary" FontAttributes="Bold" FontSize="16"/>
                                <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto,Auto,Auto" ColumnSpacing="10" RowSpacing="5">
                                    <Label Grid.Row="0" Grid.Column="0" Text="Deposit Date:" FontAttributes="Bold"/>
                                    <Label Grid.Row="0" Grid.Column="1" Text="{Binding Result.DepositDate, StringFormat='{0:MM/dd/yyyy}'}"/>
                                    
                                    <Label Grid.Row="1" Grid.Column="0" Text="Account Number:" FontAttributes="Bold"/>
                                    <Label Grid.Row="1" Grid.Column="1" Text="{Binding Result.AccountNumber}"/>
                                    
                                    <Label Grid.Row="2" Grid.Column="0" Text="Total Cash:" FontAttributes="Bold"/>
                                    <Label Grid.Row="2" Grid.Column="1" Text="{Binding Result.TotalCashAmount, StringFormat='{0:C}'}"/>
                                    
                                    <Label Grid.Row="3" Grid.Column="0" Text="Total Checks:" FontAttributes="Bold"/>
                                    <Label Grid.Row="3" Grid.Column="1" Text="{Binding Result.TotalCheckAmount, StringFormat='{0:C}'}"/>
                                </Grid>
                                
                                <Separator/>
                                
                                <StackLayout Orientation="Horizontal">
                                    <Label Text="Net Deposit Amount:" FontAttributes="Bold" FontSize="18"/>
                                    <Label Text="{Binding Result.NetDepositAmount, StringFormat='{0:C}'}" 
                                           FontAttributes="Bold" 
                                           FontSize="18"
                                           TextColor="{DynamicResource SuccessColor}"/>
                                </StackLayout>
                            </StackLayout>
                        </Frame>
                        
                        <!-- Deposit items -->
                        <Frame BackgroundColor="{DynamicResource CardBackgroundColor}">
                            <StackLayout>
                                <Label Text="Deposit Items" FontAttributes="Bold" FontSize="16"/>
                                <CollectionView ItemsSource="{Binding Result.DepositItems}">
                                    <CollectionView.ItemTemplate>
                                        <DataTemplate>
                                            <Grid ColumnDefinitions="Auto,*,Auto,Auto" Padding="0,10" ColumnSpacing="10">
                                                <Label Grid.Column="0" 
                                                       Text="{Binding ItemType}" 
                                                       FontAttributes="Bold"
                                                       VerticalOptions="Center"/>
                                                <Label Grid.Column="1" 
                                                       Text="{Binding Description}" 
                                                       VerticalOptions="Center"/>
                                                <Label Grid.Column="2" 
                                                       Text="{Binding Amount, StringFormat='{0:C}'}" 
                                                       FontAttributes="Bold"
                                                       VerticalOptions="Center"/>
                                                <ProgressBar Grid.Column="3" 
                                                           Progress="{Binding Confidence}"
                                                           VerticalOptions="Center"
                                                           WidthRequest="50"/>
                                            </Grid>
                                        </DataTemplate>
                                    </CollectionView.ItemTemplate>
                                </CollectionView>
                            </StackLayout>
                        </Frame>
                        
                        <!-- Depositor information -->
                        <Frame BackgroundColor="{DynamicResource CardBackgroundColor}">
                            <StackLayout>
                                <Label Text="Depositor Information" FontAttributes="Bold" FontSize="16"/>
                                <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto" ColumnSpacing="10" RowSpacing="5">
                                    <Label Grid.Row="0" Grid.Column="0" Text="Name:" FontAttributes="Bold"/>
                                    <Label Grid.Row="0" Grid.Column="1" Text="{Binding Result.DepositorName}"/>
                                    
                                    <Label Grid.Row="1" Grid.Column="0" Text="Signature:" FontAttributes="Bold"/>
                                    <Label Grid.Row="1" Grid.Column="1" Text="{Binding SignatureStatus}"/>
                                </Grid>
                            </StackLayout>
                        </Frame>
                        
                    </StackLayout>
                </ScrollView>
            </TabViewItem>
            
            <!-- JSON view (reused from check feature) -->
            <TabViewItem Text="JSON">
                <!-- Same JSON display as check feature -->
            </TabViewItem>
            
            <!-- Model information tab -->
            <TabViewItem Text="Model Info">
                <ScrollView>
                    <StackLayout Padding="20">
                        <Label Text="Processing Model Details" FontAttributes="Bold" FontSize="16"/>
                        <Frame BackgroundColor="{DynamicResource CardBackgroundColor}">
                            <StackLayout>
                                <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto,Auto,Auto" ColumnSpacing="10" RowSpacing="5">
                                    <Label Grid.Row="0" Grid.Column="0" Text="Model ID:" FontAttributes="Bold"/>
                                    <Label Grid.Row="0" Grid.Column="1" Text="{Binding ModelInfo.ModelId}"/>
                                    
                                    <Label Grid.Row="1" Grid.Column="0" Text="Model Name:" FontAttributes="Bold"/>
                                    <Label Grid.Row="1" Grid.Column="1" Text="{Binding ModelInfo.Name}"/>
                                    
                                    <Label Grid.Row="2" Grid.Column="0" Text="Created:" FontAttributes="Bold"/>
                                    <Label Grid.Row="2" Grid.Column="1" Text="{Binding ModelInfo.CreatedAt, StringFormat='{0:MM/dd/yyyy}'}"/>
                                    
                                    <Label Grid.Row="3" Grid.Column="0" Text="Capabilities:" FontAttributes="Bold"/>
                                    <Label Grid.Row="3" Grid.Column="1" Text="{Binding CapabilitiesSummary}"/>
                                </Grid>
                            </StackLayout>
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
            <Button Text="Export CSV" 
                    Command="{Binding ExportCsvCommand}"/>
            <Button Text="Share Results" 
                    Command="{Binding ShareResultsCommand}"/>
            <Button Text="Process Another" 
                    Command="{Binding ProcessAnotherCommand}"/>
        </StackLayout>
        
    </Grid>
</ContentPage>
```

## Implementation Strategy

### Phase 1: Model Management Infrastructure
- Extend settings service for custom model configuration
- Implement custom model discovery and validation
- Create model management UI components

### Phase 2: Document Classification
- Implement document type detection service
- Add classification UI and user feedback
- Integrate classification into processing workflow

### Phase 3: Deposit Slip Processing
- Extend processing service for custom models
- Implement deposit slip result parsing
- Create deposit-specific UI components

### Phase 4: Enhanced Results Display  
- Extend results framework for deposit slip data
- Add model information display
- Implement deposit-specific export formats

### Phase 5: Integration and Testing
- Comprehensive testing with multiple custom models
- Performance optimization for custom model processing
- Documentation and user guidance for model training

## Dependencies

### Custom Model Requirements
- Trained Azure AI Document Intelligence custom models
- Model deployment in same Azure region as service
- Model validation and testing data
- Documentation for supported deposit slip formats

### Additional NuGet Packages
- Enhanced JSON processing for complex custom model responses
- CSV export functionality for deposit item data
- Enhanced file system access for model configuration

## Risks and Mitigation

### Risk 1: Custom Model Availability and Quality
**Mitigation**: Provide comprehensive guidance for model training and validate models before deployment

### Risk 2: Deposit Slip Format Variations
**Mitigation**: Support multiple custom models and provide classification confidence feedback

### Risk 3: Custom Model Performance
**Mitigation**: Implement performance monitoring and fallback options for slow custom models

## Success Criteria

### Functional Success
- Deposit slips process successfully with configured custom models
- Document classification accurately routes to appropriate processing
- Results display clearly shows deposit-specific information
- Model management provides intuitive configuration interface

### Technical Success  
- Custom model integration follows Azure best practices
- Processing performance comparable to prebuilt model processing
- Results parsing handles various custom model response formats
- Classification accuracy >85% for supported document types

### User Experience Success
- Workflow feels consistent between check and deposit slip processing
- Model selection and management is intuitive
- Error messages provide helpful guidance for model issues
- Results presentation clearly communicates deposit information and confidence