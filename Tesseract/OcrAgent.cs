public class OcrAgent
{
    /*public OcrResult Process(string ocrText)
    {
        var invoice = ExtractInvoice(ocrText);
        var customer = ExtractCustomer(ocrText);
        var lines = ExtractLines(ocrText);

        ClassifyLines(lines);

        return new OcrResult
        {
            Invoice = invoice,
            Customer = customer,
            Lines = lines
        };
    }*/
     private void ClassifyLines(List<Line> lines)
    {
        foreach (var line in lines)
        {
            if (line.Description.Contains("Service"))
                line.Category = "service";
            else if (line.Description.Contains("Produit"))
                line.Category = "product";
            else
                line.Category = "other";
        }
    }
}
