using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Benchmark suite for OCR extraction on invoice images.
/// Measures execution time and extraction quality (presence/validity of key fields).
/// </summary>
public class OcrBenchmark
{
    private readonly string _tesseractPath;
    private readonly string _samplesPath;
    
    // Metrics collected per image
    public class ImageResult
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; } = string.Empty;

        [JsonPropertyName("duration_ms")]
        public long DurationMs { get; set; }

        [JsonPropertyName("invoice_number_present")]
        public bool InvoiceNumberPresent { get; set; }

        [JsonPropertyName("amount_incl_tax_present")]
        public bool AmountInclTaxPresent { get; set; }

        [JsonPropertyName("postal_code_present")]
        public bool PostalCodePresent { get; set; }

        [JsonPropertyName("lines_count")]
        public int LinesCount { get; set; }

        [JsonPropertyName("extraction_score")]
        public double ExtractionScore { get; set; } // 0-100: % of key fields successfully extracted

        [JsonPropertyName("parse_success")]
        public bool ParseSuccess { get; set; }
    }

    public class BenchmarkSummary
    {
        [JsonPropertyName("total_images")]
        public int TotalImages { get; set; }

        [JsonPropertyName("successful_parses")]
        public int SuccessfulParses { get; set; }

        [JsonPropertyName("avg_duration_ms")]
        public double AvgDurationMs { get; set; }

        [JsonPropertyName("min_duration_ms")]
        public long MinDurationMs { get; set; }

        [JsonPropertyName("max_duration_ms")]
        public long MaxDurationMs { get; set; }

        [JsonPropertyName("avg_extraction_score")]
        public double AvgExtractionScore { get; set; }

        [JsonPropertyName("field_presence_rates")]
        public Dictionary<string, double> FieldPresenceRates { get; set; } = new();

        [JsonPropertyName("avg_lines_per_image")]
        public double AvgLinesPerImage { get; set; }
    }

    public OcrBenchmark(string tesseractPath, string samplesPath)
    {
        _tesseractPath = tesseractPath;
        _samplesPath = samplesPath;
    }

    /// <summary>
    /// Run benchmark on all invoice images in samples folder.
    /// Returns list of per-image results and writes CSV + JSON summaries.
    /// </summary>
    public (List<ImageResult> Results, BenchmarkSummary Summary) Run(Action<string>? logger = null)
    {
        logger?.Invoke("=== OCR Benchmark: Tesseract ===");
        logger?.Invoke($"Processing images from: {_samplesPath}\n");

        var results = new List<ImageResult>();

        // Find all invoice images (*.jpg, *.png, etc.)
        var imageFiles = Directory.GetFiles(_samplesPath, "*.*")
            .Where(f => new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff" }.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f)
            .ToList();

        logger?.Invoke($"Found {imageFiles.Count} images to process.\n");

        foreach (var imagePath in imageFiles)
        {
            var fileName = Path.GetFileName(imagePath);
            logger?.Invoke($"Processing: {fileName}...");

            var sw = Stopwatch.StartNew();
            try
            {
                var imageBytes = File.ReadAllBytes(imagePath);
                var ocrText = ParseTextTesseract(imageBytes);
                sw.Stop();

                var result = OcrParser.Parse(ocrText);
                var imageResult = EvaluateExtraction(fileName, result, sw.ElapsedMilliseconds);
                results.Add(imageResult);

                logger?.Invoke($"  Duration: {imageResult.DurationMs}ms | Score: {imageResult.ExtractionScore:F1}% | Lines: {imageResult.LinesCount}");
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger?.Invoke($"  ERROR: {ex.Message}");
                results.Add(new ImageResult
                {
                    Filename = fileName,
                    DurationMs = sw.ElapsedMilliseconds,
                    ParseSuccess = false,
                    ExtractionScore = 0
                });
            }
        }

        logger?.Invoke("\n");
        var summary = ComputeSummary(results);
        PrintSummary(summary, logger);

        return (results, summary);
    }

    /// <summary>
    /// Evaluate extraction quality: presence and validity of key fields.
    /// </summary>
    private ImageResult EvaluateExtraction(string filename, OcrResult result, long durationMs)
    {
        var fieldsValid = 0;
        const int totalFields = 4; // invoice_number, amount_incl_tax, postal_code, lines_count

        var invoiceNumberPresent = !string.IsNullOrEmpty(result.Invoice.InvoiceNumber);
        if (invoiceNumberPresent) fieldsValid++;

        var amountInclTaxPresent = result.Invoice.AmountInclTax > 0;
        if (amountInclTaxPresent) fieldsValid++;

        var postalCodePresent = !string.IsNullOrEmpty(result.Customer.CustomerAddress.PostalCode);
        if (postalCodePresent) fieldsValid++;

        var linesCountPresent = result.Lines.Count > 0;
        if (linesCountPresent) fieldsValid++;

        var extractionScore = (fieldsValid / (double)totalFields) * 100.0;

        return new ImageResult
        {
            Filename = filename,
            DurationMs = durationMs,
            InvoiceNumberPresent = invoiceNumberPresent,
            AmountInclTaxPresent = amountInclTaxPresent,
            PostalCodePresent = postalCodePresent,
            LinesCount = result.Lines.Count,
            ExtractionScore = extractionScore,
            ParseSuccess = fieldsValid == totalFields
        };
    }

    /// <summary>
    /// Compute aggregate statistics from per-image results.
    /// </summary>
    private BenchmarkSummary ComputeSummary(List<ImageResult> results)
    {
        var summary = new BenchmarkSummary
        {
            TotalImages = results.Count,
            SuccessfulParses = results.Count(r => r.ParseSuccess),
            AvgDurationMs = results.Any() ? results.Average(r => r.DurationMs) : 0,
            MinDurationMs = results.Any() ? results.Min(r => r.DurationMs) : 0,
            MaxDurationMs = results.Any() ? results.Max(r => r.DurationMs) : 0,
            AvgExtractionScore = results.Any() ? results.Average(r => r.ExtractionScore) : 0,
            AvgLinesPerImage = results.Any() ? results.Average(r => r.LinesCount) : 0
        };

        // Field presence rates
        summary.FieldPresenceRates["invoice_number"] = results.Any() ? (results.Count(r => r.InvoiceNumberPresent) / (double)results.Count) * 100 : 0;
        summary.FieldPresenceRates["amount_incl_tax"] = results.Any() ? (results.Count(r => r.AmountInclTaxPresent) / (double)results.Count) * 100 : 0;
        summary.FieldPresenceRates["postal_code"] = results.Any() ? (results.Count(r => r.PostalCodePresent) / (double)results.Count) * 100 : 0;
        summary.FieldPresenceRates["lines_extracted"] = results.Any() ? (results.Count(r => r.LinesCount > 0) / (double)results.Count) * 100 : 0;

        return summary;
    }

    private void PrintSummary(BenchmarkSummary summary, Action<string>? logger)
    {
        logger?.Invoke("=== Summary ===");
        logger?.Invoke($"Total images: {summary.TotalImages}");
        logger?.Invoke($"Successful parses: {summary.SuccessfulParses} ({(summary.TotalImages > 0 ? (summary.SuccessfulParses / (double)summary.TotalImages * 100) : 0):F1}%)");
        logger?.Invoke($"Avg duration: {summary.AvgDurationMs:F0}ms (min: {summary.MinDurationMs}ms, max: {summary.MaxDurationMs}ms)");
        logger?.Invoke($"Avg extraction score: {summary.AvgExtractionScore:F1}%");
        logger?.Invoke($"Avg lines per image: {summary.AvgLinesPerImage:F1}");
        logger?.Invoke("\nField presence rates:");
        foreach (var kvp in summary.FieldPresenceRates)
        {
            logger?.Invoke($"  {kvp.Key}: {kvp.Value:F1}%");
        }
    }

    /// <summary>
    /// Execute Tesseract OCR on an image file (in-memory).
    /// </summary>
    private string ParseTextTesseract(byte[] imageBytes)
    {
        string output = string.Empty;
        var tempOutputFile = Path.GetTempPath() + Guid.NewGuid();
        var tempImageFile = Path.GetTempFileName();

        try
        {
            File.WriteAllBytes(tempImageFile, imageBytes);

            var info = new System.Diagnostics.ProcessStartInfo
            {
                FileName = Path.Combine(_tesseractPath, "tesseract.exe"),
                Arguments = $"\"{tempImageFile}\" \"{tempOutputFile}\" -l eng+fra",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = System.Diagnostics.Process.Start(info))
            {
                if (process == null)
                    throw new Exception("Failed to start Tesseract process.");

                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    output = File.ReadAllText(tempOutputFile + ".txt");
                }
                else
                {
                    throw new Exception($"Tesseract failed with code {process.ExitCode}: {error}");
                }
            }
        }
        finally
        {
            if (File.Exists(tempImageFile)) File.Delete(tempImageFile);
            if (File.Exists(tempOutputFile + ".txt")) File.Delete(tempOutputFile + ".txt");
        }

        return output;
    }

    /// <summary>
    /// Write results to CSV file.
    /// </summary>
    public static void WriteCsv(string filePath, List<ImageResult> results)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Filename,Duration(ms),InvoiceNumberPresent,AmountInclTaxPresent,PostalCodePresent,LinesCount,ExtractionScore(%),ParseSuccess");
            foreach (var r in results)
            {
                writer.WriteLine($"{r.Filename},{r.DurationMs},{r.InvoiceNumberPresent},{r.AmountInclTaxPresent},{r.PostalCodePresent},{r.LinesCount},{r.ExtractionScore:F1},{r.ParseSuccess}");
            }
        }
    }

    /// <summary>
    /// Write summary to JSON file.
    /// </summary>
    public static void WriteSummaryJson(string filePath, BenchmarkSummary summary)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(summary, options);
        File.WriteAllText(filePath, json);
    }
}
