using System.Text.Json.Serialization;

public class OcrResult
{
    public required Invoice Invoice { get; set; } 
    public required Customer Customer { get; set; } 
    public required List<Line> Lines { get; set; }
}