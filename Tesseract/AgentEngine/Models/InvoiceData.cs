
namespace AgentEngine.Models;

public class InvoiceData
{
    // Informations générales
    public string? SupplierName { get; set; }
    public string? SupplierSiret { get; set; }
    public string? SupplierVatNumber { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerSiret { get; set; }

    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PurchaseOrderNumber { get; set; }

    // Lignes de facture
    public List<InvoiceLine> Lines { get; set; } = [];

    // Totaux
    public decimal? SubtotalHT { get; set; }
    public decimal? VatRate { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal? TotalTTC { get; set; }

    // Contrôle et traçabilité
    public List<string> Warnings { get; set; } = [];
}

public class InvoiceLine
{
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? UnitPriceHT { get; set; }
    public decimal? TotalHT { get; set; }
}
