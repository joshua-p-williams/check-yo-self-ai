using check_yo_self_ai.Models;

namespace check_yo_self_ai.Services.Interfaces;

/// <summary>
/// Base exception for all document processing pipeline errors.
/// </summary>
public abstract class DocumentProcessingException : Exception
{
    /// <summary>
    /// The processing stage where the error occurred.
    /// </summary>
    public ProcessingStage Stage { get; }

    /// <summary>
    /// The document ID associated with this error.
    /// </summary>
    public string? DocumentId { get; }

    /// <summary>
    /// Error code for programmatic handling.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Additional error details for debugging.
    /// </summary>
    public Dictionary<string, object>? ErrorDetails { get; protected set; }

    protected DocumentProcessingException(
        string message, 
        ProcessingStage stage, 
        string errorCode, 
        string? documentId = null, 
        Dictionary<string, object>? errorDetails = null, 
        Exception? innerException = null) 
        : base(message, innerException)
    {
        Stage = stage;
        DocumentId = documentId;
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }
}

/// <summary>
/// Exception thrown when document classification fails.
/// </summary>
public class DocumentClassificationException : DocumentProcessingException
{
    public DocumentClassificationException(
        string message, 
        string? documentId = null, 
        Dictionary<string, object>? errorDetails = null, 
        Exception? innerException = null)
        : base(message, ProcessingStage.Classification, "CLASSIFICATION_ERROR", documentId, errorDetails, innerException)
    {
    }

    public static DocumentClassificationException ModelNotAvailable(string? documentId = null, Exception? innerException = null)
    {
        return new DocumentClassificationException(
            "Classification model is not available or accessible", 
            documentId, 
            new Dictionary<string, object> { ["ErrorType"] = "ModelNotAvailable" }, 
            innerException);
    }

    public static DocumentClassificationException UnsupportedDocumentType(string? documentId = null, Exception? innerException = null)
    {
        return new DocumentClassificationException(
            "Document type is not supported by the classification model", 
            documentId, 
            new Dictionary<string, object> { ["ErrorType"] = "UnsupportedDocumentType" }, 
            innerException);
    }

    public static DocumentClassificationException LowConfidence(double confidence, string? documentId = null)
    {
        return new DocumentClassificationException(
            $"Classification confidence ({confidence:P1}) is below acceptable threshold", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "LowConfidence", 
                ["Confidence"] = confidence 
            });
    }
}

/// <summary>
/// Exception thrown when document extraction fails.
/// </summary>
public class DocumentExtractionException : DocumentProcessingException
{
    public DocumentExtractionException(
        string message, 
        string? documentId = null, 
        Dictionary<string, object>? errorDetails = null, 
        Exception? innerException = null)
        : base(message, ProcessingStage.Extraction, "EXTRACTION_ERROR", documentId, errorDetails, innerException)
    {
    }

    public static DocumentExtractionException ModelNotFound(string modelId, string? documentId = null, Exception? innerException = null)
    {
        return new DocumentExtractionException(
            $"Extraction model '{modelId}' was not found or is not accessible", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "ModelNotFound", 
                ["ModelId"] = modelId 
            }, 
            innerException);
    }

    public static DocumentExtractionException ExtractionTimeout(string modelId, string? documentId = null, Exception? innerException = null)
    {
        return new DocumentExtractionException(
            $"Extraction operation timed out for model '{modelId}'", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "ExtractionTimeout", 
                ["ModelId"] = modelId 
            }, 
            innerException);
    }

    public static DocumentExtractionException InvalidDocumentFormat(string? documentId = null, Exception? innerException = null)
    {
        return new DocumentExtractionException(
            "Document format is not supported for extraction", 
            documentId, 
            new Dictionary<string, object> { ["ErrorType"] = "InvalidDocumentFormat" }, 
            innerException);
    }
}

/// <summary>
/// Exception thrown when check analysis fails.
/// </summary>
public class CheckAnalysisException : DocumentExtractionException
{
    public CheckAnalysisException(
        string message, 
        string? documentId = null, 
        Dictionary<string, object>? errorDetails = null, 
        Exception? innerException = null)
        : base(message, documentId, errorDetails, innerException)
    {
        ErrorDetails ??= new Dictionary<string, object>();
        ErrorDetails["DocumentType"] = "Check";
        ErrorDetails["Model"] = "prebuilt-check.us";
    }

    public static CheckAnalysisException CheckNotDetected(string? documentId = null, Exception? innerException = null)
    {
        return new CheckAnalysisException(
            "No check was detected in the provided image", 
            documentId, 
            new Dictionary<string, object> { ["ErrorType"] = "CheckNotDetected" }, 
            innerException);
    }

    public static CheckAnalysisException MissingCriticalFields(List<string> missingFields, string? documentId = null)
    {
        return new CheckAnalysisException(
            $"Critical check fields are missing: {string.Join(", ", missingFields)}", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "MissingCriticalFields", 
                ["MissingFields"] = missingFields 
            });
    }

    public static CheckAnalysisException InvalidCheckAmount(string? documentId = null, Exception? innerException = null)
    {
        return new CheckAnalysisException(
            "Check amount could not be extracted or is invalid", 
            documentId, 
            new Dictionary<string, object> { ["ErrorType"] = "InvalidCheckAmount" }, 
            innerException);
    }
}

/// <summary>
/// Exception thrown when deposit slip analysis fails.
/// </summary>
public class DepositSlipAnalysisException : DocumentExtractionException
{
    public DepositSlipAnalysisException(
        string message, 
        string? documentId = null, 
        Dictionary<string, object>? errorDetails = null, 
        Exception? innerException = null)
        : base(message, documentId, errorDetails, innerException)
    {
        ErrorDetails ??= new Dictionary<string, object>();
        ErrorDetails["DocumentType"] = "DepositSlip";
    }

    public static DepositSlipAnalysisException CustomModelNotAvailable(string modelId, string? documentId = null, Exception? innerException = null)
    {
        return new DepositSlipAnalysisException(
            $"Custom model '{modelId}' is not available for deposit slip analysis", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "CustomModelNotAvailable", 
                ["ModelId"] = modelId 
            }, 
            innerException);
    }

    public static DepositSlipAnalysisException DepositSlipNotDetected(string? documentId = null, Exception? innerException = null)
    {
        return new DepositSlipAnalysisException(
            "No deposit slip was detected in the provided image", 
            documentId, 
            new Dictionary<string, object> { ["ErrorType"] = "DepositSlipNotDetected" }, 
            innerException);
    }

    public static DepositSlipAnalysisException AmountCalculationError(string details, string? documentId = null, Exception? innerException = null)
    {
        return new DepositSlipAnalysisException(
            $"Error calculating deposit slip amounts: {details}", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "AmountCalculationError", 
                ["Details"] = details 
            }, 
            innerException);
    }

    public static DepositSlipAnalysisException UnsupportedLayout(string? documentId = null, Exception? innerException = null)
    {
        return new DepositSlipAnalysisException(
            "Deposit slip layout is not supported by the available models", 
            documentId, 
            new Dictionary<string, object> { ["ErrorType"] = "UnsupportedLayout" }, 
            innerException);
    }
}

/// <summary>
/// Exception thrown when document normalization fails.
/// </summary>
public class DocumentNormalizationException : DocumentProcessingException
{
    public DocumentNormalizationException(
        string message, 
        string? documentId = null, 
        Dictionary<string, object>? errorDetails = null, 
        Exception? innerException = null)
        : base(message, ProcessingStage.Normalization, "NORMALIZATION_ERROR", documentId, errorDetails, innerException)
    {
    }

    public static DocumentNormalizationException MappingFailure(DocumentType documentType, string field, string? documentId = null, Exception? innerException = null)
    {
        return new DocumentNormalizationException(
            $"Failed to map field '{field}' for {documentType} document", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "MappingFailure", 
                ["DocumentType"] = documentType.ToString(), 
                ["Field"] = field 
            }, 
            innerException);
    }

    public static DocumentNormalizationException ValidationFailure(List<string> validationErrors, string? documentId = null)
    {
        return new DocumentNormalizationException(
            $"Document normalization validation failed: {string.Join(", ", validationErrors)}", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "ValidationFailure", 
                ["ValidationErrors"] = validationErrors 
            });
    }

    public static DocumentNormalizationException UnsupportedDocumentType(DocumentType documentType, string? documentId = null)
    {
        return new DocumentNormalizationException(
            $"Document type '{documentType}' is not supported for normalization", 
            documentId, 
            new Dictionary<string, object> 
            { 
                ["ErrorType"] = "UnsupportedDocumentType", 
                ["DocumentType"] = documentType.ToString() 
            });
    }
}

/// <summary>
/// Exception thrown when mapping between models fails.
/// </summary>
public class MappingException : Exception
{
    /// <summary>
    /// The source model or format being mapped from.
    /// </summary>
    public string SourceModel { get; }

    /// <summary>
    /// The target model or format being mapped to.
    /// </summary>
    public string TargetModel { get; }

    /// <summary>
    /// The specific field or property that failed to map.
    /// </summary>
    public string? FailedField { get; }

    public MappingException(
        string message, 
        string sourceModel, 
        string targetModel, 
        string? failedField = null, 
        Exception? innerException = null) 
        : base(message, innerException)
    {
        SourceModel = sourceModel;
        TargetModel = targetModel;
        FailedField = failedField;
    }

    public static MappingException FieldNotFound(string sourceModel, string targetModel, string fieldName, Exception? innerException = null)
    {
        return new MappingException(
            $"Field '{fieldName}' could not be found when mapping from {sourceModel} to {targetModel}", 
            sourceModel, 
            targetModel, 
            fieldName, 
            innerException);
    }

    public static MappingException TypeMismatch(string sourceModel, string targetModel, string fieldName, Type expectedType, Type actualType)
    {
        return new MappingException(
            $"Type mismatch for field '{fieldName}' when mapping from {sourceModel} to {targetModel}. Expected {expectedType.Name}, got {actualType.Name}", 
            sourceModel, 
            targetModel, 
            fieldName);
    }

    public static MappingException InvalidValue(string sourceModel, string targetModel, string fieldName, string value, string reason)
    {
        return new MappingException(
            $"Invalid value '{value}' for field '{fieldName}' when mapping from {sourceModel} to {targetModel}: {reason}", 
            sourceModel, 
            targetModel, 
            fieldName);
    }
}

/// <summary>
/// Exception thrown when Azure AI service operations fail.
/// </summary>
public class AzureAIException : Exception
{
    /// <summary>
    /// The Azure service that generated this error.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// The operation that failed.
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Azure-specific error code (if available).
    /// </summary>
    public string? AzureErrorCode { get; }

    /// <summary>
    /// Azure-specific error details.
    /// </summary>
    public Dictionary<string, object>? AzureErrorDetails { get; }

    public AzureAIException(
        string message, 
        string serviceName, 
        string operation, 
        string? azureErrorCode = null, 
        Dictionary<string, object>? azureErrorDetails = null, 
        Exception? innerException = null) 
        : base(message, innerException)
    {
        ServiceName = serviceName;
        Operation = operation;
        AzureErrorCode = azureErrorCode;
        AzureErrorDetails = azureErrorDetails;
    }

    public static AzureAIException AuthenticationFailed(string serviceName, string operation, Exception? innerException = null)
    {
        return new AzureAIException(
            $"Authentication failed for {serviceName} service during {operation}", 
            serviceName, 
            operation, 
            "AUTHENTICATION_FAILED", 
            null, 
            innerException);
    }

    public static AzureAIException QuotaExceeded(string serviceName, string operation, Exception? innerException = null)
    {
        return new AzureAIException(
            $"Quota exceeded for {serviceName} service during {operation}", 
            serviceName, 
            operation, 
            "QUOTA_EXCEEDED", 
            null, 
            innerException);
    }

    public static AzureAIException ServiceUnavailable(string serviceName, string operation, Exception? innerException = null)
    {
        return new AzureAIException(
            $"{serviceName} service is currently unavailable for {operation}", 
            serviceName, 
            operation, 
            "SERVICE_UNAVAILABLE", 
            null, 
            innerException);
    }

    public static AzureAIException RateLimitExceeded(string serviceName, string operation, TimeSpan retryAfter, Exception? innerException = null)
    {
        return new AzureAIException(
            $"Rate limit exceeded for {serviceName} service during {operation}. Retry after {retryAfter.TotalSeconds} seconds", 
            serviceName, 
            operation, 
            "RATE_LIMIT_EXCEEDED", 
            new Dictionary<string, object> { ["RetryAfter"] = retryAfter }, 
            innerException);
    }
}