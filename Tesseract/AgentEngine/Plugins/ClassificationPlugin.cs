using Microsoft.SemanticKernel;
using AgentEngine.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace AgentEngine.Plugins;

public class ClassificationPlugin
{
    private readonly ILogger<ClassificationPlugin> _logger;

    public ClassificationPlugin(ILogger<ClassificationPlugin> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [KernelFunction("ClassifyInvoice")]
    [Description("Analyse le texte extrait d'une facture et retourne une décision structurée")]
    public async Task<AgentDecision> ClassifyInvoiceAsync(string extractedText)
    {
        _logger.LogInformation("Classification appelée sur {Length} caractères", extractedText.Length);

        // Version temporaire : on laisse le LLM faire le travail directement via InvokePromptAsync
        // On pourra plus tard transformer ça en vrai plugin qui appelle NormalizationValidationAgent

        return new AgentDecision
        {
            Supplier = "À déterminer",
            InvoiceNumber = "N/A",
            InvoiceDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            SuggestedAction = "REVIEW",
            ConfidenceScore = 0.4,
            Reasoning = "Classification non encore implémentée - en cours de développement",
            Issues = new List<string> { "Plugin temporaire" }
        };
    }
}