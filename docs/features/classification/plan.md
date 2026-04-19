# Development Plan: Document Classification and Routing

## Overview

This plan outlines the implementation of document classification that serves as the **entry point for mixed document intake**. The system uses Azure AI Document Intelligence's custom classifier to identify document types before routing them to appropriate extraction models. This is the critical component that enables the **upload → classify → route → extract** pipeline.

## Classification Training Strategy

The classification feature requires a strategic approach to training:

- **Training document classes**: `check`, `deposit-slip`, and optionally `unknown`
- **Confidence thresholds**: Define minimum acceptable confidence for automatic routing
- **Routing rules**: Clear logic for classification → extractor selection  
- **Unknown/fallback path**: Handle low-confidence and unsupported documents gracefully
- **Demo-friendly observability**: Show users what the classifier chose and why

## Architecture Considerations

- This demo is a **mixed-document intake** scenario, not a single-model scenario
- **Classification happens before extraction**
- Routing belongs in the application layer
- Low-confidence outcomes should not silently pass as successful processing
- All outputs should be normalized into a shared teller-document result shape

## Implementation Strategy

### Phase 1: Classification Model Training (Days 1-3)
Establish the custom classifier with representative training data.

### Phase 2: Classification Service Implementation (Days 4-5)
Build the classifier service integration and result processing.

### Phase 3: Routing Logic Implementation (Days 6-7)
Create the routing engine that directs documents to appropriate extractors.

### Phase 4: Classification UI and Observability (Days 8-9)
Implement user-facing classification results and routing decision display.

### Phase 5: Integration and Testing (Days 10-11)
Complete pipeline integration and comprehensive testing.

## Detailed Implementation Phases

### Phase 1: Classification Model Training

#### 1.1 Training Data Collection and Preparation
- **Duration**: 1.5 days
- **Prerequisites**: Access to representative document samples
- **Deliverables**:
  - Minimum training set per class (check, deposit-slip samples)
  - Balanced training dataset across document types
  - Validation dataset for testing classifier accuracy
  - Training data organized for Azure Document Intelligence Studio

#### 1.2 Custom Classifier Training
- **Duration**: 1 day
- **Dependencies**: Training data prepared
- **Deliverables**:
  - Trained custom classifier model in Azure
  - Model validation results and accuracy metrics
  - Classifier model ID and deployment configuration
  - Performance benchmarks for confidence thresholds

#### 1.3 Model Validation and Tuning
- **Duration**: 0.5 days
- **Dependencies**: Classifier training complete
- **Deliverables**:
  - Classification accuracy testing on validation set
  - Confidence threshold optimization
  - Edge case testing and documentation
  - Model performance documentation

### Phase 2: Classification Service Implementation

#### 2.1 Classification Service Architecture
- **Duration**: 1 day
- **Prerequisites**: Trained classifier available
- **Deliverables**:
  - IDocumentClassifierService interface definition
  - DocumentClassifierService implementation
  - Classification result DTOs and models
  - Azure client configuration for classifier

#### 2.2 Classification Result Processing
- **Duration**: 1 day
- **Dependencies**: Classification service implemented
- **Deliverables**:
  - Classification confidence analysis
  - Alternative classification option handling
  - Classification metadata extraction and storage
  - Error handling for classification failures

### Phase 3: Routing Logic Implementation

#### 3.1 Routing Engine Development
- **Duration**: 1.5 days
- **Prerequisites**: Classification service operational
- **Deliverables**:
  - Document routing service implementation
  - Confidence-based routing rules engine
  - Integration with check and deposit slip analyzers
  - Routing decision tracking and logging

#### 3.2 Fallback and Exception Handling
- **Duration**: 0.5 days
- **Dependencies**: Core routing logic complete
- **Deliverables**:
  - Unknown document type handling
  - Low-confidence classification workflows
  - Manual override capabilities
  - User guidance for unsupported documents

### Phase 4: Classification UI and Observability

#### 4.1 Classification Results Display
- **Duration**: 1.5 days
- **Prerequisites**: Classification pipeline functional
- **Deliverables**:
  - Classification result cards with confidence visualization
  - Alternative classification options display
  - Routing decision indicators
  - Processing timeline with classification stage

#### 4.2 Demo Observability Features
- **Duration**: 0.5 days
- **Dependencies**: Results display implemented
- **Deliverables**:
  - Real-time classification progress tracking
  - Classification reasoning and evidence display
  - Model information and capabilities display
  - Performance metrics for demo purposes

### Phase 5: Integration and Testing

#### 5.1 Pipeline Integration
- **Duration**: 1 day
- **Prerequisites**: All classification components complete
- **Deliverables**:
  - Full document orchestration pipeline integration
  - End-to-end workflow testing
  - Performance optimization
  - Error handling validation

#### 5.2 Comprehensive Testing and Documentation
- **Duration**: 1 day
- **Dependencies**: Pipeline integration complete
- **Deliverables**:
  - Classification accuracy testing across document types
  - Edge case validation and documentation
  - Performance benchmarking
  - User documentation and troubleshooting guides