# Tasks: Document Classification and Routing

## Phase 1: Classification Model Training (Days 1-3)

### 1.1 Define Document Classes: `check`, `deposit-slip`, `unknown`
- [ ] **Create document class taxonomy**
  - [ ] Define `check` class for US bank checks
  - [ ] Define `deposit-slip` class for bank deposit slips  
  - [ ] Define `unknown` class for unsupported document types
  - [ ] Document class characteristics and distinguishing features
  - [ ] Create classification guidelines for edge cases

- [ ] **Establish classification boundaries**
  - [ ] Define what constitutes a valid check vs. invalid check
  - [ ] Specify deposit slip variants included in training
  - [ ] Document exclusions and out-of-scope document types
  - [ ] Create decision tree for ambiguous documents
  - [ ] Plan for future class expansion

### 1.2 Gather at Least the Minimum Training Set Per Class
- [ ] **Collect check samples for training**
  - [ ] Gather diverse US bank check samples (at least 50 per class minimum)
  - [ ] Include checks from multiple banks and check designs
  - [ ] Collect checks with varying quality and scan conditions
  - [ ] Include handwritten and printed check variants
  - [ ] Document sample sources and characteristics

- [ ] **Collect deposit slip samples for training**  
  - [ ] Gather deposit slips from multiple banks (at least 50 samples)
  - [ ] Include different deposit slip layouts and formats
  - [ ] Collect both personal and business deposit slips
  - [ ] Include varying completeness and quality levels
  - [ ] Document layout variations and special characteristics

- [ ] **Collect unknown/negative samples**
  - [ ] Gather non-financial documents for negative training
  - [ ] Include receipts, invoices, and other common documents
  - [ ] Add random images and text documents
  - [ ] Include partially damaged or unclear documents
  - [ ] Document negative sample categories

### 1.3 Train Custom Classifier
- [ ] **Set up Azure Document Intelligence Studio**
  - [ ] Configure Azure blob storage for training data
  - [ ] Upload organized training samples by document class
  - [ ] Configure custom classifier project in Studio
  - [ ] Set up training and validation data splits
  - [ ] Configure classifier training parameters

- [ ] **Execute classifier training**
  - [ ] Start custom classifier training process
  - [ ] Monitor training progress and metrics
  - [ ] Evaluate training results and accuracy
  - [ ] Review classification confidence distributions
  - [ ] Document training results and model performance

- [ ] **Validate classifier performance**
  - [ ] Test classifier on held-out validation set
  - [ ] Measure classification accuracy per document class
  - [ ] Analyze false positive and false negative rates
  - [ ] Test edge cases and ambiguous documents
  - [ ] Document classifier limitations and edge cases

## Phase 2: Classification Service Implementation (Days 4-5)

### 2.1 Implement Classifier Service Wrapper
- [ ] **Create IDocumentClassifierService interface**
  - [ ] Define ClassifyDocumentAsync method accepting DocumentInput
  - [ ] Return ClassificationResult DTO with type and confidence
  - [ ] Add batch classification support for multiple documents
  - [ ] Include model information and capability queries
  - [ ] Define error handling for classification failures

- [ ] **Implement DocumentClassifierService class**
  - [ ] Configure Azure DocumentIntelligenceClient for classifier
  - [ ] Implement document classification using trained custom model
  - [ ] Process classification results and confidence scores
  - [ ] Map Azure classification response to ClassificationResult DTO
  - [ ] Add proper error handling and logging

- [ ] **Create classification result DTOs**
  - [ ] Implement ClassificationResult with document type and confidence
  - [ ] Add AlternativeClassification for secondary options
  - [ ] Include classification reasoning and evidence fields
  - [ ] Add processing metadata (timing, model version)
  - [ ] Implement confidence visualization helpers

### 2.2 Display Classifier Result and Confidence
- [ ] **Design classification result UI components**
  - [ ] Create classification result card showing document type
  - [ ] Add confidence visualization with progress bars/colors
  - [ ] Display alternative classification options
  - [ ] Show classification reasoning when available
  - [ ] Add manual override options for incorrect results

- [ ] **Implement ClassificationResultViewModel**
  - [ ] Bind to ClassificationResult data with confidence indicators
  - [ ] Handle alternative classification display and selection
  - [ ] Implement manual classification override commands
  - [ ] Add confidence threshold warnings and messaging
  - [ ] Support classification result export and sharing

### 2.3 Implement Routing Logic from Class → Extractor
- [ ] **Create document routing service**
  - [ ] Design IDocumentRoutingService interface
  - [ ] Implement routing rules based on classification confidence
  - [ ] Route high-confidence checks to ICheckAnalyzerService
  - [ ] Route high-confidence deposit slips to IDepositSlipAnalyzerService
  - [ ] Handle low-confidence classifications with user prompts

- [ ] **Configure routing thresholds and rules**
  - [ ] Define minimum confidence thresholds for automatic routing
  - [ ] Create routing decision logic with fallback options
  - [ ] Implement routing decision tracking and logging
  - [ ] Add routing override capabilities for manual selection
  - [ ] Document routing rules and threshold rationale

### 2.4 Add Fallback Path for Unknown or Low-Confidence Class
- [ ] **Design unknown document handling**
  - [ ] Create unknown document detection and messaging
  - [ ] Provide clear guidance on supported document types
  - [ ] Add option to proceed with manual model selection
  - [ ] Implement fallback to general OCR for unsupported types
  - [ ] Create user feedback collection workflows

- [ ] **Implement low-confidence handling**
  - [ ] Detect classifications below confidence thresholds
  - [ ] Present manual selection options to users
  - [ ] Show confidence explanations and guidance
  - [ ] Allow users to proceed with low-confidence routing
  - [ ] Collect feedback for model improvement

## Phase 3: Classification Integration and Testing (Days 6-7)

### 3.1 Integration with Document Pipeline
- [ ] **Integrate classification into document orchestration**
  - [ ] Connect classification service to upload workflow
  - [ ] Implement classification as first stage in processing pipeline
  - [ ] Add classification status tracking to processing timeline
  - [ ] Connect routing service to downstream extractors
  - [ ] Test end-to-end document processing pipeline

- [ ] **Add classification observability**
  - [ ] Display real-time classification progress
  - [ ] Show classification results in processing timeline
  - [ ] Add routing decision indicators to UI
  - [ ] Include classification metadata in result displays
  - [ ] Implement classification performance tracking

### 3.2 Comprehensive Testing and Validation
- [ ] **Test classification accuracy**
  - [ ] Validate classification performance on diverse test set
  - [ ] Test edge cases and ambiguous documents
  - [ ] Measure and document classification accuracy metrics
  - [ ] Test classification confidence calibration
  - [ ] Validate routing decisions and threshold settings

- [ ] **Test pipeline integration**
  - [ ] Test complete upload → classify → route → extract workflow
  - [ ] Validate error handling throughout classification pipeline
  - [ ] Test performance with various document types and qualities
  - [ ] Verify classification result display and user interactions
  - [ ] Document integration issues and resolutions