using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

public static class OcrParser
{
    public static OcrResult Parse(string text)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = string.Empty,
            Currency = string.Empty,
            PurchaseOrderNumber = string.Empty
        };
        var customer = new Customer
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            CustomerAddress = new Address
            {
                Street = string.Empty,
                PostalCode = string.Empty,
                City = string.Empty,
                Country = string.Empty
            }
        };
        var lines = new List<Line>();

        // Invoice number (line starting with "Facture")
        var m = Regex.Match(text, @"Facture\s+([^\r\n]+)", RegexOptions.IgnoreCase);
        if (m.Success)
            invoice.InvoiceNumber = m.Groups[1].Value.Trim();

        // Purchase order / BC
        m = Regex.Match(text, @"N°\s*BC\s*\r?\n\s*([A-Z0-9-]+)", RegexOptions.IgnoreCase);
        if (m.Success)
            invoice.PurchaseOrderNumber = m.Groups[1].Value.Trim();
        else
        {
            m = Regex.Match(text, @"BC[0-9A-Z]+", RegexOptions.IgnoreCase);
            if (m.Success) invoice.PurchaseOrderNumber = m.Value.Trim();
        }

        // Dates (ISO-like yyyy-MM-dd)
        var dateMatches = Regex.Matches(text, @"\d{4}-\d{2}-\d{2}");
        if (dateMatches.Count >= 1 && DateTime.TryParse(dateMatches[0].Value, out var d1))
            invoice.InvoiceDate = d1;
        if (dateMatches.Count >= 2 && DateTime.TryParse(dateMatches[1].Value, out var d2))
            invoice.DueDate = d2;

        // Currency detection
        invoice.Currency = text.Contains("€") ? "EUR" : "";

        // Amounts: find euro values and choose heuristics
        var euroMatches = Regex.Matches(text, @"[€]?\s*([0-9\s]+,[0-9]{2})");
        var amounts = new List<decimal>();
        foreach (Match mm in euroMatches)
        {
            var s = mm.Groups[1].Value.Replace(" ", "");
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"), out var v))
                amounts.Add(v);
        }
        if (amounts.Count > 0)
        {
            var max = amounts.Max();
            invoice.AmountInclTax = max;
            var second = amounts.Where(a => a < max).OrderByDescending(a => a).FirstOrDefault();
            if (second > 0)
            {
                invoice.AmountExclTax = second;
                invoice.TaxAmount = invoice.AmountInclTax - invoice.AmountExclTax;
            }
        }

        // Customer name and address: use AddressExtractor for robust multi-line handling
        var linesText = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        
        // Extract address using new AddressExtractor
        customer.CustomerAddress = AddressExtractor.ExtractAddress(linesText);

        // Extract customer name from line before street (if street was found)
        if (!string.IsNullOrEmpty(customer.CustomerAddress.Street))
        {
            var streetLineIndex = linesText.FindIndex(l => l.Equals(customer.CustomerAddress.Street, StringComparison.OrdinalIgnoreCase));
            if (streetLineIndex > 0)
            {
                var nameLine = linesText[streetLineIndex - 1];
                var nameParts = nameLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 2)
                {
                    customer.FirstName = nameParts[0];
                    customer.LastName = string.Join(' ', nameParts.Skip(1));
                }
                else
                {
                    customer.FirstName = nameLine;
                    customer.LastName = string.Empty;
                }
            }
        }

        // Lines detection (simplified: pair descriptions between headers with quantities and subtotals)
        var descHeader = linesText.FindIndex(l => l.IndexOf("Description", StringComparison.OrdinalIgnoreCase) >= 0);
        var qtyHeader = linesText.FindIndex(l => l.IndexOf("Quantit", StringComparison.OrdinalIgnoreCase) >= 0);

        List<string> descCandidates = new List<string>();
        if (descHeader >= 0 && qtyHeader > descHeader)
        {
            for (int i = descHeader + 1; i < qtyHeader; i++)
            {
                if (!string.IsNullOrWhiteSpace(linesText[i]) && !Regex.IsMatch(linesText[i], @"^\d+$"))
                    descCandidates.Add(linesText[i]);
            }
        }

        // stricter filtering
        string[] streetKeywords = new[] { "rue", "avenue", "av", "boulevard", "bd", "allee", "place", "street", "road", "ave", "st", "lane", "route" };
        descCandidates = descCandidates.Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).Where(d =>
        {
            var lower = d.ToLowerInvariant();
            if (d.Length < 3 || d.Length > 200) return false;
            if (Regex.IsMatch(d, @"\b\d{5}\b")) return false;
            if (Regex.IsMatch(d, @"^\d")) return false;
            if (Regex.IsMatch(lower, @"\b(tva|montant|sous total|prix|date|quantit|n\u00B0|n°|bc|facture)\b")) return false;
            if (lower.Contains("€") || lower.Contains("%")) return false;
            if (streetKeywords.Any(k => lower.Contains(" " + k) || lower.Contains(k + " "))) return false;
            var parts = d.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && parts.All(p => p.Length <= 20 && char.IsUpper(p[0]) && p.Skip(1).All(c => char.IsLower(c) || c == '.'))) return false;
            if (!d.Any(c => char.IsLower(c))) return false;
            if (Regex.IsMatch(d, @"\d{1,3}[ \u00A0]?\w+") && d.Split(' ').Length <= 4 && d.Any(char.IsDigit)) return false;
            return true;
        }).ToList();

        var quantitiesFound = new List<decimal>();
        if (qtyHeader >= 0)
        {
            for (int i = qtyHeader + 1; i < linesText.Count; i++)
            {
                var ln = linesText[i];
                if (ln.IndexOf("Date", StringComparison.OrdinalIgnoreCase) >= 0) break;
                var qm2 = Regex.Match(ln, @"(\d{1,3}(?:[ \u00A0]\d{3})*|\d+)[,\.]\d{2}");
                if (qm2.Success)
                {
                    var s = qm2.Groups[1].Value.Replace(" ", "").Replace('\u00A0', ' ');
                    if (decimal.TryParse(s.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        quantitiesFound.Add(v);
                }
            }
        }

        var subtotalsFound = new List<decimal>();
        var subtotalHeader = linesText.FindIndex(l => l.IndexOf("Sous total", StringComparison.OrdinalIgnoreCase) >= 0 || l.IndexOf("Montant TTC", StringComparison.OrdinalIgnoreCase) >= 0);
        var searchFrom = subtotalHeader >= 0 ? subtotalHeader : Math.Max(0, linesText.Count - 20);
        for (int i = searchFrom; i < linesText.Count; i++)
        {
            var sm = Regex.Match(linesText[i], "([0-9\\s]+,[0-9]{2})");
            if (sm.Success)
            {
                var sv = sm.Groups[1].Value.Replace(" ", "");
                if (decimal.TryParse(sv, NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"), out var svd))
                    subtotalsFound.Add(svd);
            }
        }

        var itemCount = Math.Max(descCandidates.Count, Math.Max(quantitiesFound.Count, subtotalsFound.Count));
        if (itemCount > 0 && descCandidates.Count > 0)
        {
            for (int i = 0; i < descCandidates.Count; i++)
            {
                var line = new Line { Description = descCandidates[i], Category = "other" };
                if (i < quantitiesFound.Count)
                    line.Quantity = (int)Math.Round(quantitiesFound[i]);
                else
                    line.Quantity = 0;

                if (line.Quantity > 0 && i < subtotalsFound.Count)
                    line.UnitPrice = Math.Round(subtotalsFound[i] / line.Quantity, 2);
                else if (line.Quantity > 0 && invoice.AmountExclTax > 0)
                    line.UnitPrice = Math.Round(invoice.AmountExclTax / line.Quantity, 2);
                else
                    line.UnitPrice = 0m;

                line.TaxRate = 20;
                line.Category = line.Description.IndexOf("Service", StringComparison.OrdinalIgnoreCase) >= 0 ? "service" : "other";
                lines.Add(line);
            }
        }
        else
        {
            var descIndex = linesText.FindIndex(l => l.IndexOf("Service", StringComparison.OrdinalIgnoreCase) >= 0);
            if (descIndex >= 0)
            {
                var line = new Line { Description = linesText[descIndex], Category = "service" };
                if (quantitiesFound.Count > 0) line.Quantity = (int)Math.Round(quantitiesFound[0]);
                if (subtotalsFound.Count > 0 && line.Quantity > 0) line.UnitPrice = Math.Round(subtotalsFound[0] / line.Quantity, 2);
                lines.Add(line);
            }
        }

        invoice.InvoiceNumber ??= string.Empty;
        invoice.Currency ??= string.Empty;
        invoice.PurchaseOrderNumber ??= string.Empty;
        customer.FirstName ??= string.Empty;
        customer.LastName ??= string.Empty;
        customer.CustomerAddress.Street ??= string.Empty;
        customer.CustomerAddress.PostalCode ??= string.Empty;
        customer.CustomerAddress.City ??= string.Empty;
        customer.CustomerAddress.Country ??= string.Empty;

        return new OcrResult
        {
            Invoice = invoice,
            Customer = customer,
            Lines = lines.Count > 0 ? lines : new List<Line> { new Line { Description = string.Empty, Quantity = 0, UnitPrice = 0m, TaxRate = 0, Category = "other" } }
        };
    }
}
