namespace AgentEngine.Models;

public record AgentDecision
{
    public string Supplier { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public string InvoiceDate { get; init; } = string.Empty;
    public string DueDate { get; init; } = string.Empty;
    public string Currency { get; init; } = "EUR";
    public decimal AmountExclTax { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal AmountInclTax { get; init; }
    public string Type { get; init; } = string.Empty;           // ex: "service", "office_equipment"
    public double ConfidenceScore { get; init; } = 0.0;
    public string SuggestedAction { get; init; } = "REVIEW";    // APPROVE | REVIEW | REJECT
    public string Reasoning { get; init; } = string.Empty;
    public List<string> Issues { get; init; } = new();
}