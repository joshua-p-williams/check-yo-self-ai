# Document Pipeline DTOs - Created Files Summary

This document provides an overview of the Data Transfer Objects (DTOs) created for the document processing pipeline.

## Created Files

### Core Pipeline DTOs

1. **`DocumentInput.cs`** - Input document representation
   - Contains document content, metadata, and validation
   - Supports cloning for processing safety
   - Includes file size and content type validation

2. **`DocumentTypes.cs`** - Core enums and supporting types
   - `DocumentType` enum (Unknown, BankCheck, DepositSlip, Other)
   - `AlternativeClassification` for secondary classification options
   - `FieldValue` for extracted field data with confidence
   - `BoundingBox` for field location coordinates
   - `ProcessingWarning` with severity levels

3. **`ClassificationResult.cs`** - Document classification results
   - Primary document type with confidence scoring
   - Alternative classification possibilities
   - Processing time and reasoning details
   - Methods for confidence assessment and manual review determination

4. **`ExtractionResult.cs`** - Raw extraction results from AI models
   - Field-level extraction data with confidence scores
   - Raw JSON response storage for debugging
   - Methods for filtering high/low confidence fields
   - Quality assessment for automatic processing

5. **`ProcessingTypes.cs`** - Supporting types for processing workflow
   - `DepositItem` for individual deposit slip items
   - `ProcessingResult` for complete pipeline results
   - `ProcessingStatus` enum and error handling types
   - End-to-end processing tracking

6. **`NormalizedDocument.cs`** - Unified business document representation
   - Common financial document fields (amount, dates, account info)
   - Check-specific fields (check number, payee, memo)
   - Deposit slip-specific fields (deposit items, cash/check amounts)
   - Processing metadata and confidence tracking
   - Validation methods and summary generation

7. **`ConfidenceWarnings.cs`** - Confidence assessment and recommendations
   - Overall and stage-specific confidence levels
   - Recommended actions based on processing quality
   - Configurable confidence thresholds
   - Manual review and auto-processing determination

## Key Features

### Validation and Quality Assurance
- All DTOs include comprehensive validation methods
- Confidence scoring at multiple levels (overall, stage, field)
- Warning and error classification with severity levels
- Automated quality assessment for processing decisions

### Business Logic Integration
- Document-type-specific field handling
- Financial calculation validation (totals, amounts)
- Processing stage tracking and timing
- Extensible metadata and configuration support

### Developer Experience
- Comprehensive XML documentation
- Fluent validation methods
- Helper methods for common operations
- Clear separation of concerns between processing stages

## Usage Patterns

### Document Processing Pipeline
1. **Input**: `DocumentInput` → Upload/capture document
2. **Classification**: `ClassificationResult` → Identify document type
3. **Extraction**: `ExtractionResult` → Extract field data
4. **Normalization**: `NormalizedDocument` → Unified business schema
5. **Assessment**: `ConfidenceWarnings` → Quality and recommendations

### Confidence-Based Routing
- High confidence → Automatic processing
- Medium confidence → Limited review
- Low confidence → Manual review required
- Critical issues → Reject or recapture

## Next Steps

The service abstractions will be created next to work with these DTOs:
- `IDocumentClassifierService`
- `ICheckAnalyzerService` 
- `IDepositSlipAnalyzerService`
- `IDocumentOrchestrationService`

These DTOs provide a solid foundation for the document processing pipeline with comprehensive validation, confidence assessment, and business logic integration.