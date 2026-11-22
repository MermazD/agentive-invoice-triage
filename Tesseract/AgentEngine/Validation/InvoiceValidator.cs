
using AgentEngine.Models;

namespace AgentEngine.Validation;

public class InvoiceValidator
{
    private const decimal Tolerance = 0.01m;

    public List<string> Validate(InvoiceData invoice)
    {
        var errors = new List<string>();

        if (invoice.Lines.Count == 0)
        {
            errors.Add("La facture ne contient aucune ligne.");
        }

        decimal calculatedSubtotal = 0m;
        bool allLineTotalsAvailable = invoice.Lines.Count > 0;

        foreach (var line in invoice.Lines)
        {
            // Contrôle du calcul individuel de la ligne
            if (line.Quantity is decimal quantity &&
                line.UnitPriceHT is decimal unitPrice &&
                line.TotalHT is decimal lineTotal)
            {
                var calculatedLineTotal = quantity * unitPrice;

                if (Math.Abs(calculatedLineTotal - lineTotal) > Tolerance)
                {
                    errors.Add(
                        $"Écart sur la ligne '{line.Description}' : " +
                        $"attendu {calculatedLineTotal:F2}, " +
                        $"déclaré {lineTotal:F2}.");
                }
            }
            else
            {
                errors.Add(
                    $"Données incomplètes pour le contrôle arithmétique : {line.Description}");
            }

            // Contrôle indépendant de la présence du montant total de ligne
            if (line.TotalHT is decimal total)
            {
                calculatedSubtotal += total;
            }
            else
            {
                allLineTotalsAvailable = false;
                errors.Add(
                    $"Montant HT manquant pour la ligne : {line.Description}");
            }
        }

        // Contrôle du total HT à partir des montants extraits
        if (invoice.SubtotalHT is decimal subtotal)
        {
            if (allLineTotalsAvailable &&
                Math.Abs(calculatedSubtotal - subtotal) > Tolerance)
            {
                errors.Add(
                    $"Écart sur le total HT : " +
                    $"somme des lignes {calculatedSubtotal:F2}, " +
                    $"total déclaré {subtotal:F2}.");
            }
        }
        else
        {
            errors.Add("Le total HT est absent.");
        }

        return errors;
    }
}
