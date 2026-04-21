namespace CheckYoSelfAI.ViewModels;

public sealed class ExtractedFieldDisplayItem
{
    public required string FieldName { get; init; }

    public required string FieldValue { get; init; }

    public required double Confidence { get; init; }

    public string ConfidenceDisplay => $"{Confidence:P1}";

    public bool IsLowConfidence => Confidence < 0.7;
}

public sealed class NormalizedFieldDisplayItem
{
    public required string Label { get; init; }

    public required string Value { get; init; }
}
