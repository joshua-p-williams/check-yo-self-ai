using System.ComponentModel.DataAnnotations;

namespace CheckYoSelfAI.Models;

/// <summary>
/// Represents a document that has been processed through the complete pipeline and normalized
/// into a unified business schema regardless of the original extraction model used.
/// This provides a consistent interface for checks, deposit slips, and other financial documents.
/// </summary>
public class NormalizedDocument
{
    /// <summary>
    /// The unique identifier of the original document.
    /// </summary>
    [Required]
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// The type of document that was processed.
    /// </summary>
    public DocumentType Type { get; set; }

    /// <summary>
    /// Timestamp when the document processing was completed.
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    #region Common Financial Document Fields

    /// <summary>
    /// The primary monetary amount on the document (check amount, total deposit amount, etc.).
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// The date written or printed on the document.
    /// </summary>
    public DateTime? DocumentDate { get; set; }

    /// <summary>
    /// Bank account number associated with the document.
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// Bank routing number (ABA number) for the financial institution.
    /// </summary>
    public string? RoutingNumber { get; set; }

    /// <summary>
    /// Name of the bank or financial institution.
    /// </summary>
    public string? BankName { get; set; }

    #endregion

    #region Check-Specific Fields

    /// <summary>
    /// Check number (only populated for check documents).
    /// </summary>
    public string? CheckNumber { get; set; }

    /// <summary>
    /// Name of the person or entity the check is made out to (only for checks).
    /// </summary>
    public string? PayToName { get; set; }

    /// <summary>
    /// Memo or note field from the check (only for checks).
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    /// Name of the person or entity who wrote the check (only for checks).
    /// </summary>
    public string? PayerName { get; set; }

    /// <summary>
    /// Address of the check writer (only for checks).
    /// </summary>
    public string? PayerAddress { get; set; }

    #endregion

    #region Deposit Slip-Specific Fields

    /// <summary>
    /// Deposit slip number or reference (only populated for deposit slip documents).
    /// </summary>
    public string? DepositSlipNumber { get; set; }

    /// <summary>
    /// List of individual deposit items (checks and cash) on the deposit slip.
    /// </summary>
    public List<DepositItem> DepositItems { get; set; } = new();

    /// <summary>
    /// Total cash amount being deposited (only for deposit slips).
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Cash amount must be non-negative")]
    public decimal? CashAmount { get; set; }

    /// <summary>
    /// Total check amount being deposited (only for deposit slips).
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Check amount must be non-negative")]
    public decimal? CheckAmount { get; set; }

    /// <summary>
    /// Name of the account holder making the deposit (only for deposit slips).
    /// </summary>
    public string? DepositorName { get; set; }

    /// <summary>
    /// Date the deposit was made (only for deposit slips).
    /// </summary>
    public DateTime? DepositDate { get; set; }

    #endregion

    #region Processing Metadata

    /// <summary>
    /// The ID of the extraction model that was used to process this document.
    /// </summary>
    public string? ProcessingModelId { get; set; }

    /// <summary>
    /// Overall confidence score for the document processing (0.0 to 1.0).
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Processing confidence must be between 0.0 and 1.0")]
    public double ProcessingConfidence { get; set; }

    /// <summary>
    /// List of processing warnings or issues encountered during normalization.
    /// </summary>
    public List<string> ProcessingWarnings { get; set; } = new();

    /// <summary>
    /// Version of the normalization logic used to create this document.
    /// </summary>
    public string? NormalizationVersion { get; set; }

    #endregion

    #region Validation and Utility Methods

    /// <summary>
    /// Validates that the normalized document has consistent and reasonable data.
    /// </summary>
    public DocumentValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate common fields
        if (Amount.HasValue && Amount < 0)
            errors.Add("Amount cannot be negative");

        if (DocumentDate.HasValue && DocumentDate > DateTime.Now.AddDays(1))
            warnings.Add("Document date is in the future");

        // Validate check-specific fields
        if (Type == DocumentType.BankCheck)
        {
            if (string.IsNullOrEmpty(CheckNumber))
                warnings.Add("Check number is missing");

            if (string.IsNullOrEmpty(PayToName))
                warnings.Add("Pay-to name is missing");

            if (!Amount.HasValue || Amount <= 0)
                errors.Add("Check must have a positive amount");
        }

        // Validate deposit slip-specific fields
        if (Type == DocumentType.DepositSlip)
        {
            if (DepositItems.Count == 0 && !CashAmount.HasValue)
                warnings.Add("Deposit slip has no items and no cash amount");

            var calculatedTotal = (CashAmount ?? 0) + (CheckAmount ?? 0);
            if (Amount.HasValue && Math.Abs(Amount.Value - calculatedTotal) > 0.01m)
                warnings.Add($"Total amount ({Amount:C}) doesn't match sum of items ({calculatedTotal:C})");

            // Validate individual deposit items
            foreach (var item in DepositItems)
            {
                if (!item.IsValid())
                    errors.Add($"Invalid deposit item: {item.Id}");
            }
        }

        return new DocumentValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Gets a summary string describing the document content.
    /// </summary>
    public string GetSummary()
    {
        return Type switch
        {
            DocumentType.BankCheck => $"Check #{CheckNumber} for {Amount:C} to {PayToName}",
            DocumentType.DepositSlip => $"Deposit slip for {Amount:C} with {DepositItems.Count} items",
            DocumentType.Unknown => "Unknown document type",
            _ => $"Document of type {Type}"
        };
    }

    /// <summary>
    /// Determines if this document requires manual review based on confidence and validation.
    /// </summary>
    /// <param name="minimumConfidence">Minimum acceptable confidence (default: 0.8)</param>
    public bool RequiresReview(double minimumConfidence = 0.8)
    {
        var validation = Validate();
        return ProcessingConfidence < minimumConfidence || 
               !validation.IsValid || 
               ProcessingWarnings.Count > 0;
    }

    #endregion
}

/// <summary>
/// Result of document validation.
/// </summary>
public class DocumentValidationResult
{
    /// <summary>
    /// Indicates whether the document passed validation.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors that prevent processing.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings that don't prevent processing but indicate potential issues.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets all validation issues (errors and warnings) as a single list.
    /// </summary>
    public List<string> AllIssues => Errors.Concat(Warnings).ToList();
}