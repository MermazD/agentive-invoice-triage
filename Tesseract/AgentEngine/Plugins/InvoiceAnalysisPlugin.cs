using System.Text.Json;
using AgentEngine.Models;
using Azure.AI.Inference;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace AgentEngine.Plugins;

public class InvoiceAnalysisPlugin
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly string _promptVersion;

    public InvoiceAnalysisPlugin(
    IChatCompletionService chatCompletionService,
    IConfiguration configuration)
    {
        _chatCompletionService = chatCompletionService;

        _promptVersion = configuration[
            "InvoiceAnalysis:PromptVersion"]
            ?? "hardened-v1";

        Console.WriteLine(
        $"[CONFIG] PromptVersion = {_promptVersion}");
    }

    
    public async Task<InvoiceData> AnalyzeAsync(
        string ocrText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ocrText))
            throw new ArgumentException(
                "Le texte OCR est vide.",
                nameof(ocrText));

        var history = new ChatHistory();

        var prompt = await LoadPromptAsync(
            _promptVersion,
            cancellationToken);

        history.AddSystemMessage(prompt);

        Console.WriteLine(
            $"===== PROMPT UTILISÉ : {_promptVersion} =====");

        Console.WriteLine(
            $"Prompt chargé : {prompt.Length} caractères");

        history.AddUserMessage($$"""
            Voici le texte OCR de la facture à analyser.

            <invoice_ocr>
            {{ocrText}}
            </invoice_ocr>
            """);

        var settings = new AzureAIInferencePromptExecutionSettings
        {
            Temperature = 0,
            MaxTokens = 2000,
            ResponseFormat =
                new ChatCompletionsResponseFormatJsonObject()
        };

        var response =
            await _chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: settings,
                cancellationToken: cancellationToken);

        var json = response.Content;

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException(
                "Le modèle n'a retourné aucun contenu.");

        Console.WriteLine("\n===== JSON BRUT DU LLM =====");
        Console.WriteLine(json);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        return JsonSerializer.Deserialize<InvoiceData>(
                json, options)
            ?? throw new InvalidOperationException(
                "Impossible de désérialiser InvoiceData.");
    }

    private static async Task<string> LoadPromptAsync(string promptVersion, CancellationToken cancellationToken)
    {
        if (promptVersion != "baseline-v1" &&
            promptVersion != "hardened-v1")
        {
            throw new ArgumentException(
                $"Version de prompt non autorisée : {promptVersion}");
        }

        var promptPath = Path.Combine(
            AppContext.BaseDirectory,
            "Prompts",
            "Experiments",
            "InvoiceAnalysis",
            $"{promptVersion}.txt");

        if (!File.Exists(promptPath))
        {
            throw new FileNotFoundException(
                $"Prompt introuvable : {promptPath}");
        }

        return await File.ReadAllTextAsync(
            promptPath,
            cancellationToken);
    }
}
