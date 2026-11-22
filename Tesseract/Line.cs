using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Line
{
    [JsonPropertyName("description")]
    public required string Description { get; set; } 
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; } 
    [JsonPropertyName("tax_rate")]
    public int TaxRate { get; set; } 
    [JsonPropertyName("Category")]
    public required string Category { get; set; }

    public Line ExtractLines(string ocrText)
    {
        var line = new Line
        {
            Description = Description,
            Category = Category,
            Quantity = Quantity,
            UnitPrice = UnitPrice,
            TaxRate = TaxRate
        };
        return line;
    }
}