using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;                    // ← AJOUT OBLIGATOIRE
using AgentEngine.Agents;
using AgentEngine.Plugins;
using AgentEngine.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using AgentEngine.Validation;

namespace AgentEngine.ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration;

                // Plugins
                services.AddSingleton<InvoiceExtractionPlugin>();
                services.AddSingleton<ClassificationPlugin>();

                // Kernel via Factory
                services.AddSingleton<Kernel>(sp => 
                    KernelFactory.Create(config, sp));

                // Agent principal
                services.AddSingleton<InvoiceTriageAgent>();

                services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            });

        using var host = builder.Build();

        Console.WriteLine($"Répertoire courant : {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"Content Root : {host.Services
            .GetRequiredService<IHostEnvironment>().ContentRootPath}");

        var config = host.Services.GetRequiredService<IConfiguration>();

        Console.WriteLine(
            $"[CONFIG] PromptVersion = " +
            $"{config["InvoiceAnalysis:PromptVersion"] ?? "NULL"}");

        //var triageAgent = host.Services.GetRequiredService<InvoiceTriageAgent>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("🚀 Agent InvoiceTriage démarré - Mode console");

        
        try
        {
            var ocr = host.Services
                .GetRequiredService<InvoiceExtractionPlugin>();

            var pdfPath = ResolvePdfPath(args);

            if (!File.Exists(pdfPath))
            {
                throw new FileNotFoundException(
                    $"Fichier PDF introuvable : {pdfPath}",
                    pdfPath);
            }

            logger.LogInformation("PDF sélectionné : {PdfPath}", pdfPath);

            var text = await ocr.ExtractTextFromPdfAsync(pdfPath);

            Console.WriteLine("\n===== TEXTE OCR COMPLET =====");
            Console.WriteLine(text);
            Console.WriteLine("=============================\n");

            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException(
                    "Aucun texte n'a été extrait du PDF.");

            logger.LogInformation(
                "Test OCR terminé : {Length} caractères extraits",
                text.Length);

            var kernel = host.Services.GetRequiredService<Kernel>();

            var chatService =
                kernel.GetRequiredService<IChatCompletionService>();

            var invoiceAnalysisPlugin = new InvoiceAnalysisPlugin(
                chatService,
                host.Services.GetRequiredService<IConfiguration>());

            logger.LogInformation(
                "Début de l'analyse de la facture avec GPT-4.1-mini");

            var invoice = await invoiceAnalysisPlugin.AnalyzeAsync(text);

            Console.WriteLine("\n===== FACTURE EXTRAITE =====");

            Console.WriteLine(
                JsonSerializer.Serialize(
                    invoice,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));

            var validator = new InvoiceValidator();
            var validationErrors = validator.Validate(invoice);

            Console.WriteLine("\n===== VALIDATION MÉTIER =====");

            if (validationErrors.Count == 0)
            {
                Console.WriteLine("Facture cohérente : aucun écart détecté.");
            }
            else
            {
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($"ANOMALIE : {error}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du traitement de la facture");
        }

        Console.WriteLine("\nAppuyez sur une touche pour quitter...");
        Console.ReadKey();
    }

    private static string ResolvePdfPath(string[] args)
    {
        // 1. Argument de ligne de commande
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            return Path.GetFullPath(args[0]);
        }

        // 2. Variable d'environnement
        var envPath = Environment.GetEnvironmentVariable("INVOICE_PDF_PATH");

        if (!string.IsNullOrWhiteSpace(envPath))
        {
            return Path.GetFullPath(envPath);
        }

        // 3. PDF fictif fourni avec le projet
        return Path.Combine(
            AppContext.BaseDirectory,
            "Samples",
            "INV-01.pdf");
    }
}