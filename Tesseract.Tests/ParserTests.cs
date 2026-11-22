using Xunit;

public class ParserTests
{
    [Fact]
    public void ParseSampleInvoice()
    {
        var sample = @"Facture FAQ2/2015/020059

Description

Service Client (Heures Prépayées)
Flipover

Combinaison de bureau

Boite de rangement

Tiroir noir

Marc Demo

3575 Buena Vista Avenue
Eugene COR 97401

Etats Unis

Quantité
64,00 Heures
35,00 Unités
7,00 Unités
41,00 Unités
82,00 Unités

Date Echéacnce
2015-02-02 2015-02-02

Prix Taxes
190,00 TVA 20%
1700,00 TVA5,5%
300,00 TVA 10%
14,00 TVA 20%
20,00

Montant HT
Taxes

Montant TTC

N° BC
BC06263
Sous total
12 160,00€
59 500,00 €
2 100,00 €
574,00 €
1640,00€
€75 974,00
€6029,30

€82 003,30";

        var result = OcrParser.Parse(sample);
        Assert.Equal("FAQ2/2015/020059", result.Invoice.InvoiceNumber);
        Assert.Equal(82003.30m, result.Invoice.AmountInclTax);
        Assert.Equal("Marc", result.Customer.FirstName);
        Assert.Equal("97401", result.Customer.CustomerAddress.PostalCode);
        Assert.True(result.Lines.Count >= 1);
    }
}
