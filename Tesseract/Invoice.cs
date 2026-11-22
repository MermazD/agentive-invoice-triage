using System.Text.Json.Serialization;

public class Invoice
{
    [JsonPropertyName("invoice_number")]
    public required string InvoiceNumber { get; set; }
    [JsonPropertyName("invoice_date")]
    public DateTime InvoiceDate { get; set; }
    [JsonPropertyName("due_date")]
    public DateTime DueDate { get; set; }
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
    [JsonPropertyName("purchase_order_number")]
    public required string PurchaseOrderNumber { get; set; }
    [JsonPropertyName("amount_excl_tax")]
    public decimal AmountExclTax { get; set; }
    [JsonPropertyName("tax_amount")]
    public decimal TaxAmount { get; set; }
    [JsonPropertyName("amount_incl_tax")]
    public decimal AmountInclTax { get; set; }

    public Invoice ExtractInvoice(string ocrText)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = ocrText,
            InvoiceDate = DateTime.Now,
            DueDate = DateTime.Now,
            Currency = "USD",
            PurchaseOrderNumber = "",
            AmountExclTax = 0,
            TaxAmount = 0,
            AmountInclTax = 0
        };
        return invoice;
    }

}