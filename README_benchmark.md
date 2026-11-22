# OCR Benchmark: Tesseract

## Overview

This benchmark evaluates the execution performance and basic field extraction coverage of the Tesseract OCR engine on a set of synthetic invoice images.

It measures whether selected invoice fields can be detected in the OCR output. It does not assess the semantic accuracy or correctness of the extracted values.

### Metrics

For each image, the benchmark collects:

- **Duration (ms)**: Execution time of Tesseract OCR
- **Invoice Number Present**: Whether an invoice number was extracted
- **Amount Incl Tax Present**: Whether the total amount including tax was extracted
- **Postal Code Present**: Whether the customer postal code was extracted
- **Lines Count**: Number of line items extracted
- **Extraction Score (%)**: Composite score based on the presence of the 4 key fields (0-100%)
- **Parse Success**: Boolean indicating whether all 4 key fields were extracted

### Aggregated Results

The summary includes:

- **Total images**: Number of images processed
- **Successful parses**: Number and percentage of images where all key fields were extracted
- **Avg/Min/Max duration**: Execution time statistics
- **Avg extraction score**: Average extraction score across all images
- **Field presence rates**: Percentage of presence for each key field
- **Avg lines per image**: Average number of line items extracted per invoice

## Running the Benchmark

### Prerequisites

- **.NET 10.0** or later
- **Tesseract OCR** installed and available on the system PATH  
  (or configured via the application settings)
- **Invoice images are JPG** placed in the `samples/` folder

### Command

From the repository root:

```bash
dotnet run --project Tesseract/Tesseract.csproj -- --benchmark
```

### Output

The benchmark generates two files in the project root:

1. **`bench_results.csv`** — Detailed results per image
   Columns: Filename, Duration, InvoiceNumberPresent, AmountInclTaxPresent, PostalCodePresent, LinesCount, ExtractionScore, ParseSuccess
2. **`bench_summary.json`** — Aggregated statistics

The following JSON is an illustrative example of the benchmark output, not a measured result:

```json
{
  "total_images": 20,
  "successful_parses": 18,
  "avg_duration_ms": 845.5,
  "min_duration_ms": 620,
  "max_duration_ms": 1250,
  "avg_extraction_score": 87.5,
  "field_presence_rates": {
    "invoice_number": 95.0,
    "amount_incl_tax": 90.0,
    "postal_code": 85.0,
    "lines_extracted": 90.0
  },
  "avg_lines_per_image": 4.8
}
```

## Interpretation

- **Extraction Score > 90%**: Excellent — nearly all key fields are detected in the OCR output.
- **Extraction Score 70–90%**: Good — most key fields are detected; some edge cases may remain.
- **Extraction Score < 70%**: Needs improvement — extraction heuristics or OCR raw text processing should be refined.
- **Field presence rates < 80%**: This field is frequently missing; consider adjusting the extraction heuristics or Tesseract configuration.

## Limitations

- The benchmark uses a small, synthetic dataset and is not representative of real-world invoice diversity.
- Field presence does not guarantee that the extracted values are correct.
- The extraction score is a custom coverage indicator, not a standard OCR accuracy metric.
- Results may vary depending on the Tesseract version, configuration, image quality and execution environment.
- This benchmark evaluates OCR output and field detection only; it does not evaluate LLM extraction or prompt-injection resistance.

## Notes

- This benchmark evaluates field presence and structural validity, not the accuracy of extracted values (e.g., whether the total amount is correct).
- For more rigorous OCR evaluation metrics such as Character Error Rate (CER) and Word Error Rate (WER), refer to the [Tesseract Documentation](https://github.com/UB-Mannheim/tesseract/wiki) and [Tesseract training and benchmarking tools](https://github.com/tesseract-ocr/tesstrain).

### Main changes

- Personal path removed
- Execution instructions made generic
- Explicit mention that the invoices are synthetic (consistent with your cleanup)
- Markdown fixed
