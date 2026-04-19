# Feature 2: Document Classification and Routing

## Overview

This feature implements the document classification system that serves as the entry point for mixed document intake. It uses Azure AI Document Intelligence's custom classifier to identify document types before routing them to appropriate extraction models. This feature is critical for enabling the **upload → classify → route → extract** pipeline.

## Goals

- Implement custom document classifier for teller-capture documents
- Classify incoming documents as `check`, `deposit-slip`, or `unknown`
- Provide confidence-based routing decisions to downstream extractors
- Handle low-confidence and unknown document scenarios gracefully
- Enable observable classification results for demo purposes

## User Stories

### US2.1: Document Classification Service
**As a** developer  
**I want** a trained custom classifier for financial documents  
**So that** I can automatically route documents to the correct extraction model

**Acceptance Criteria:**
- Custom classifier trained on check and deposit slip samples
- Minimum viable training set per document class
- Classification confidence scoring for routing decisions
- Integration with Azure Document Intelligence classifier API
- Model versioning and performance tracking

### US2.2: Classification Result Display
**As a** user  
**I want** to see how the system classified my document  
**So that** I can understand and verify the routing decision

**Acceptance Criteria:**
- Classification result panel showing document type and confidence
- Visual confidence indicators with color coding
- Alternative classification options displayed
- Reasoning/evidence display for classification decision
- Manual override capability for incorrect classifications

### US2.3: Routing Logic Implementation
**As a** developer  
**I want** confidence-based routing to extraction models  
**So that** documents go to the appropriate analyzer automatically

**Acceptance Criteria:**
- Routing rules based on classification confidence thresholds
- Route to check extractor for high-confidence check classification
- Route to deposit slip extractor for high-confidence deposit classification  
- Manual selection prompt for low-confidence classifications
- Fallback handling for unknown document types

### US2.4: Unknown Document Handling
**As a** user  
**I want** clear guidance when my document type isn't supported  
**So that** I can understand limitations and take appropriate action

**Acceptance Criteria:**
- Unknown document type detection and messaging
- Guidance on supported document types
- Option to proceed with manual model selection
- Fallback to general OCR for unsupported types
- User feedback collection for improving classification

### US2.5: Classification Observability
**As a** demo user  
**I want** to see the classification process in action  
**So that** I can understand how the AI routing works

**Acceptance Criteria:**
- Real-time classification progress indication
- Detailed classification results with evidence
- Classification history and accuracy tracking
- Model performance metrics display
- Confidence trend analysis across document sessions

## Technical Specifications

### Classification Architecture

#### Custom Classifier Service
```csharp
public interface IDocumentClassifierService
{
    Task<ClassificationResult> ClassifyDocumentAsync(DocumentInput document);
    Task<List<ClassificationResult>> BatchClassifyAsync(List<DocumentInput> documents);
    Task<ClassifierModelInfo> GetModelInfoAsync();
    Task<bool> ValidateModelAvailabilityAsync();
}

public class DocumentClassifierService : IDocumentClassifierService
{
    private readonly DocumentIntelligenceClient _client;
    private readonly ILogger<DocumentClassifierService> _logger;
    private readonly ISettingsService _settingsService;
    private readonly string _classifierModelId;

    public async Task<ClassificationResult> ClassifyDocumentAsync(DocumentInput document)
    {
        var operation = await _client.ClassifyDocumentAsync(
            WaitUntil.Completed,
            _classifierModelId,
            document.Content);

        return MapClassificationResult(operation.Value);
    }
}
```

### Routing Architecture
```csharp
public interface IDocumentRoutingService
{
    Task<RoutingDecision> RouteDocumentAsync(ClassificationResult classification);
    Task<ExtractionResult> ProcessDocumentAsync(DocumentInput document, RoutingDecision routing);
}

public class RoutingDecision
{
    public string ExtractorServiceType { get; set; }
    public string ModelId { get; set; }
    public double RequiredConfidence { get; set; }
    public bool RequiresManualConfirmation { get; set; }
    public string RoutingReason { get; set; }
}
```

## Documentation References

### Custom Classifier Training
- **Custom Classifier How-to**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/how-to-guides/build-a-custom-classifier?view=doc-intel-4.0.0
- **Custom Classifier Training**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/train/custom-classifier?view=doc-intel-4.0.0

This is the core documentation set for the routing architecture because Microsoft describes classifier models as identifying document types in incoming files, including multi-page and multi-document scenarios, and notes the minimum starting dataset guidance.
            WaitUntil.Completed,
            _classifierModelId,
            new ClassifyDocumentContent
            {
                Base64Source = BinaryData.FromStream(document.Content)
            });

        return MapToClassificationResult(operation.Value, document.Id);
    }
}
```

#### Classification Models
```csharp
public class ClassificationResult
{
    public string DocumentId { get; set; }
    public DocumentType DocumentType { get; set; }
    public double Confidence { get; set; }
    public string ModelVersion { get; set; }
    public List<AlternativeClassification> Alternatives { get; set; }
    public string ReasoningDetails { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public ClassificationEvidence Evidence { get; set; }
}

public class AlternativeClassification
{
    public DocumentType DocumentType { get; set; }
    public double Confidence { get; set; }
    public string Reasoning { get; set; }
}

public class ClassificationEvidence
{
    public List<string> KeyIndicators { get; set; }
    public List<BoundingRegion> EvidenceRegions { get; set; }
    public string LayoutAnalysis { get; set; }
    public List<string> TextPatterns { get; set; }
}

public enum DocumentType
{
    Unknown = 0,
    BankCheck = 1,
    DepositSlip = 2,
    Other = 99
}
```

#### Routing Service
```csharp
public interface IDocumentRoutingService
{
    Task<RoutingDecision> DetermineRoutingAsync(ClassificationResult classification);
    Task<ExtractionResult> RouteForExtractionAsync(DocumentInput document, RoutingDecision routing);
    ExtractionModel GetModelForDocumentType(DocumentType documentType);
}

public class DocumentRoutingService : IDocumentRoutingService
{
    private readonly ICheckAnalyzerService _checkAnalyzer;
    private readonly IDepositSlipAnalyzerService _depositSlipAnalyzer;
    private readonly ILogger<DocumentRoutingService> _logger;

    // Configurable confidence thresholds
    private const double HIGH_CONFIDENCE_THRESHOLD = 0.85;
    private const double LOW_CONFIDENCE_THRESHOLD = 0.60;

    public async Task<RoutingDecision> DetermineRoutingAsync(ClassificationResult classification)
    {
        return classification switch
        {
            { DocumentType: DocumentType.BankCheck, Confidence: >= HIGH_CONFIDENCE_THRESHOLD } 
                => new RoutingDecision 
                { 
                    TargetModel = ExtractionModel.PrebuiltCheck, 
                    RequiresUserConfirmation = false 
                },

            { DocumentType: DocumentType.DepositSlip, Confidence: >= HIGH_CONFIDENCE_THRESHOLD } 
                => new RoutingDecision 
                { 
                    TargetModel = ExtractionModel.CustomDepositSlip, 
                    RequiresUserConfirmation = false 
                },

            { Confidence: >= LOW_CONFIDENCE_THRESHOLD } 
                => new RoutingDecision 
                { 
                    TargetModel = GetModelForDocumentType(classification.DocumentType),
                    RequiresUserConfirmation = true,
                    ConfidenceWarning = $"Low confidence ({classification.Confidence:P1}). Please verify document type."
                },

            _ => new RoutingDecision 
                { 
                    TargetModel = ExtractionModel.Unknown, 
                    RequiresUserConfirmation = true,
                    ConfidenceWarning = "Unable to determine document type reliably."
                }
        };
    }
}

public class RoutingDecision
{
    public ExtractionModel TargetModel { get; set; }
    public bool RequiresUserConfirmation { get; set; }
    public string ConfidenceWarning { get; set; }
    public List<ExtractionModel> AlternativeModels { get; set; }
    public string RoutingReasoning { get; set; }
}

public enum ExtractionModel
{
    Unknown,
    PrebuiltCheck,
    CustomDepositSlip,
    GeneralOCR
}
```

### UI Components

#### Classification Result Display
```xml
<ContentView x:Class="CheckYoSelfAI.Controls.ClassificationResultControl">
    <Frame BackgroundColor="{DynamicResource CardBackgroundColor}" Padding="15" Margin="10">
        <StackLayout>

            <!-- Classification Header -->
            <StackLayout Orientation="Horizontal" Spacing="10">
                <Label Text="Document Classification:" FontAttributes="Bold" VerticalOptions="Center"/>
                <Label Text="{Binding Classification.DocumentType}" 
                       FontAttributes="Bold" 
                       TextColor="{Binding Classification.DocumentType, Converter={StaticResource DocumentTypeColorConverter}}"
                       VerticalOptions="Center"/>
                <ActivityIndicator IsRunning="{Binding IsClassifying}" IsVisible="{Binding IsClassifying}"/>
            </StackLayout>

            <!-- Confidence Indicator -->
            <StackLayout Orientation="Horizontal" Spacing="10" Margin="0,5">
                <Label Text="Confidence:" VerticalOptions="Center"/>
                <ProgressBar Progress="{Binding Classification.Confidence}" 
                           ProgressColor="{Binding Classification.Confidence, Converter={StaticResource ConfidenceColorConverter}}"
                           WidthRequest="100"/>
                <Label Text="{Binding Classification.Confidence, StringFormat='{0:P1}'}" 
                       VerticalOptions="Center"/>
            </StackLayout>

            <!-- Routing Decision -->
            <StackLayout IsVisible="{Binding ShowRoutingDecision}">
                <Label Text="Processing Route:" FontAttributes="Bold" Margin="0,10,0,0"/>
                <Label Text="{Binding RoutingDecision.TargetModel, Converter={StaticResource ModelDisplayNameConverter}}"/>
                <Label Text="{Binding RoutingDecision.ConfidenceWarning}" 
                       TextColor="{DynamicResource WarningColor}"
                       IsVisible="{Binding RoutingDecision.RequiresUserConfirmation}"/>
            </StackLayout>

            <!-- Alternative Options -->
            <StackLayout IsVisible="{Binding ShowAlternatives}" Margin="0,10,0,0">
                <Label Text="Alternative Classifications:" FontSize="12"/>
                <CollectionView ItemsSource="{Binding Classification.Alternatives}">
                    <CollectionView.ItemTemplate>
                        <DataTemplate>
                            <Grid ColumnDefinitions="*,Auto" Margin="0,2">
                                <Label Grid.Column="0" Text="{Binding DocumentType}" FontSize="12"/>
                                <Label Grid.Column="1" Text="{Binding Confidence, StringFormat='{0:P1}'}" FontSize="12"/>
                            </Grid>
                        </DataTemplate>
                    </CollectionView.ItemTemplate>
                </CollectionView>
            </StackLayout>

            <!-- Manual Override Button -->
            <Button Text="Choose Manually" 
                    Command="{Binding ManualOverrideCommand}"
                    IsVisible="{Binding AllowManualOverride}"
                    Margin="0,10,0,0"/>

        </StackLayout>
    </Frame>
</ContentView>
```

### Training Strategy

#### Document Classes Definition
- **check**: US bank checks with MICR line, standard check layout
- **deposit-slip**: Bank deposit slips with item lists and totals  
- **unknown**: Documents that don't fit supported categories

#### Minimum Training Set Requirements
- **Per Class**: Minimum 15 documents per class (Azure requirement)
- **Total Dataset**: Target 50+ documents across all classes
- **Diversity**: Multiple banks, formats, image qualities per class
- **Labeling**: Consistent class labeling across training set

#### Model Training Process
```csharp
// Training configuration for custom classifier
var trainingOptions = new BuildDocumentClassifierOptions
{
    ClassifierId = "check-deposit-classifier-v1",
    Description = "Classifier for bank checks and deposit slips",
    DocTypes = new Dictionary<string, ClassifierDocumentTypeDetails>
    {
        ["check"] = new ClassifierDocumentTypeDetails 
        { 
            AzureBlobSource = new AzureBlobContentSource(checkTrainingContainerUri)
        },
        ["deposit-slip"] = new ClassifierDocumentTypeDetails 
        { 
            AzureBlobSource = new AzureBlobContentSource(depositTrainingContainerUri) 
        }
    }
};
```

## Dependencies

### Azure Services
- Azure AI Document Intelligence with custom classifier capability
- Azure Blob Storage for training data storage
- Sufficient Azure credits for model training and testing

### Training Data
- Minimum 15 bank check samples across different banks
- Minimum 15 deposit slip samples with layout variations
- High-quality scanned or photographed documents
- Properly organized training data in Azure Blob containers

## Implementation Considerations

### Performance Optimization
- **Classification Caching**: Cache results for identical documents
- **Async Processing**: Non-blocking classification with progress updates
- **Batch Processing**: Support for multiple document classification
- **Model Warmup**: Keep classifier model ready for immediate use

### Error Handling Strategy
- **Network Failures**: Retry with exponential backoff
- **Low Confidence**: Graceful degradation to manual selection
- **Unknown Types**: Clear guidance and fallback options
- **Model Unavailable**: Offline mode with basic heuristics

### Confidence Thresholds
- **High Confidence**: ≥85% for automatic routing
- **Medium Confidence**: 60-85% with user confirmation
- **Low Confidence**: <60% requires manual classification
- **User Configurable**: Allow threshold adjustment in settings

## Testing Strategy

### Model Validation
- **Cross-validation**: Hold-out test set for accuracy measurement
- **Confusion Matrix**: Track classification accuracy per document type
- **Confidence Calibration**: Validate that confidence scores reflect accuracy
- **Edge Cases**: Test with poor quality, partial, or ambiguous documents

### Integration Testing
- **End-to-End Pipeline**: Validate classification → routing → extraction flow
- **Error Scenarios**: Test classification failures and fallbacks
- **Performance Testing**: Measure classification speed and resource usage
- **UI Testing**: Validate classification result display and user interactions

## Success Criteria

### Functional Success
- Classification accuracy >90% on held-out test set
- Automatic routing works correctly for high-confidence classifications
- Manual override functionality provides complete control
- Unknown document handling provides clear user guidance

### Technical Success
- Classification response time <5 seconds for standard documents
- Robust error handling covers network and model failures
- Integration with routing service works seamlessly
- Performance meets requirements for demo scenarios

### User Experience Success
- Classification results are clearly communicated to users
- Confidence indicators help users understand system certainty
- Manual override process is intuitive and effective
- Error messages provide actionable guidance for resolution

## Reference Documentation

### Azure Documentation Links

**Custom Classifier How-to Guide:**
```
https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/how-to-guides/build-a-custom-classifier?view=doc-intel-4.0.0
```

**Custom Classifier Training Guide:**
```
https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/train/custom-classifier?view=doc-intel-4.0.0
```

**Document Intelligence Overview:**
```
https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/overview?view=doc-intel-4.0.0
```

These resources provide the foundation for understanding Azure's classifier capabilities and training requirements for mixed-document scenarios.