using Microsoft.SemanticKernel;
using PDFtoImage;
using SkiaSharp;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace AgentEngine.Plugins;

public class InvoiceExtractionPlugin
{
    private readonly ILogger<InvoiceExtractionPlugin> _logger;
    private readonly string _tesseractLanguage = "fra+eng";

    public InvoiceExtractionPlugin(ILogger<InvoiceExtractionPlugin> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [KernelFunction("ExtractTextFromPdf")]
    [Description("Extrait tout le texte d'une facture PDF via Tesseract OCR")]
    public async Task<string> ExtractTextFromPdfAsync(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
        {
            _logger.LogError("Fichier PDF introuvable : {Path}", pdfPath);
            return string.Empty;
        }

        var sb = new StringBuilder();
        var tempImages = new List<string>();

        try
        {
            _logger.LogInformation("Début extraction OCR du PDF : {Path}", pdfPath);

            using var pdfStream = File.OpenRead(pdfPath);

#pragma warning disable CA1416
            var images = PDFtoImage.Conversion.ToImages(pdfStream).ToList();
#pragma warning restore CA1416

            _logger.LogInformation("{PageCount} pages détectées", images.Count);

            for (int i = 0; i < images.Count; i++)
            {
                var tempDirectory = Path.GetFullPath(Path.GetTempPath());
                Directory.CreateDirectory(tempDirectory);
                
                var tempImagePath = Path.Combine(
                    Path.GetTempPath(), 
                    $"invoice_page_{i}_{Guid.NewGuid():N}.png");

                tempImages.Add(tempImagePath);

                // Sauvegarde SkiaSharp
                using var image = images[i];
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                using (var fileStream = new FileStream(
                        tempImagePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None))
                    {
                        data.SaveTo(fileStream);
                    }


                var text = await RunTesseractAsync(tempImagePath);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine($"--- PAGE {i + 1} ---");
                    sb.AppendLine(text.Trim());
                    sb.AppendLine();
                }
            }

            var fullText = sb.ToString().Trim();
            _logger.LogInformation("Extraction OCR terminée - {Length} caractères", fullText.Length);

            return fullText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction OCR du PDF {Path}", pdfPath);
            return string.Empty;
        }
        finally
        {
            foreach (var file in tempImages)
            {
                try { File.Delete(file); } catch { }
            }
        }
    }

    // Méthode manquante
    
    private async Task<string> RunTesseractAsync(string imagePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = @"C:\Program Files\Tesseract-OCR\tesseract.exe",
            Arguments = $"\"{imagePath}\" stdout -l {_tesseractLanguage} --oem 3 --psm 6",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException(
                "Impossible de démarrer Tesseract.exe");

        _logger.LogInformation(
            "Tesseract démarré. PID={ProcessId}, Image={ImagePath}",
            process.Id, imagePath);

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        using var timeout = new CancellationTokenSource(
            TimeSpan.FromSeconds(60));

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Timeout Tesseract. PID={ProcessId}, Image={ImagePath}",
                process.Id, imagePath);

            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // Le processus a déjà terminé.
            }

            // Attente bornée de la terminaison.
            using var killTimeout =
                new CancellationTokenSource(TimeSpan.FromSeconds(10));

            try
            {
                await process.WaitForExitAsync(killTimeout.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException(
                    $"Impossible d'arrêter Tesseract (PID {process.Id}).");
            }

            await Task.WhenAll(outputTask, errorTask);

            throw new TimeoutException(
                "Tesseract a dépassé le délai maximal de 60 secondes.");
        }

        var output = await outputTask;
        var error = await errorTask;

        _logger.LogInformation(
            "Tesseract terminé. PID={ProcessId}, ExitCode={ExitCode}",
            process.Id, process.ExitCode);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Tesseract a échoué (code {process.ExitCode}) : {error}");
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("Tesseract : {Error}", error);
        }

        return output.Trim();
    }

}