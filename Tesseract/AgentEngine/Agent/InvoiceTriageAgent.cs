using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using AgentEngine.Models;

#pragma warning disable SKEXP0070

namespace AgentEngine.Agents;

public class InvoiceTriageAgent
{
    private readonly Kernel _kernel;
    private readonly IConfiguration _configuration;

    public InvoiceTriageAgent(
    IConfiguration config,
    Kernel kernel)
    {
        _configuration = config;
        _kernel = kernel;
    }

    public async Task<AgentDecision> ProcessInvoiceAsync(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
            throw new FileNotFoundException("PDF introuvable", pdfPath);

        // 1. Extraction OCR
        var extractionFunction = _kernel.Plugins["InvoiceExtractionPlugin"]["ExtractTextFromPdf"];

        var extractionResult = await _kernel.InvokeAsync(extractionFunction, new KernelArguments
        {
            ["pdfPath"] = pdfPath
        });

        string rawText = extractionResult?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(rawText))
        {
            return new AgentDecision 
            { 
                Supplier = "Erreur", 
                SuggestedAction = "REVIEW",
                Reasoning = "Échec de l'extraction OCR",
                ConfidenceScore = 0.0 
            };
        }

        // 2. Analyse avec LLM
        var prompt = await LoadPromptAsync();

        // Correction du warning nullable
        var decision = await _kernel.InvokePromptAsync<AgentDecision>(
            prompt,
            new KernelArguments
            {
                ["rawText"] = rawText
            });

        // On garantit un objet non-null
        return decision ?? new AgentDecision
        {
            Supplier = "Erreur de parsing",
            SuggestedAction = "REVIEW",
            Reasoning = "Le LLM n'a pas retourné de JSON valide",
            ConfidenceScore = 0.0
        };
    }

    
    private async Task<string> LoadPromptAsync()
    {
        var promptVersion = _configuration[
            "InvoiceAnalysis:PromptVersion"];

        if (string.IsNullOrWhiteSpace(promptVersion))
        {
            throw new InvalidOperationException(
                "La version du prompt n'est pas configurée.");
        }

        // Liste blanche : seules les versions déclarées sont autorisées.
        var fileName = promptVersion switch
        {
            "baseline-v1" => "baseline-v1.txt",
            "hardened-v1" => "hardened-v1.txt",
            _ => throw new InvalidOperationException(
                $"Version de prompt inconnue : {promptVersion}")
        };

        var promptPath = Path.Combine(
            AppContext.BaseDirectory,
            "Prompts",
            "Experiments",
            "InvoiceAnalysis",
            fileName);

        if (!File.Exists(promptPath))
        {
            throw new FileNotFoundException(
                $"Fichier de prompt introuvable : {promptPath}");
        }

        return await File.ReadAllTextAsync(promptPath);
    }

}