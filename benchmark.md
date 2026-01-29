# OCR Benchmark — Tesseract vs PaddleOCR

## 1. Objective

The goal of this benchmark is to evaluate and compare two OCR engines — **Tesseract** and **PaddleOCR** — in the context of an **agentive invoice processing pipeline**.

This benchmark does **not** aim to measure academic OCR accuracy (CER/WER), but to answer a practical business question:

> *Which OCR engine best feeds a downstream agent-based system for invoice triage and automation?*

---

## 2. Scope & Dataset

- **Dataset size:** 20 invoice images  
- **Format:** JPG  
- **Content:** Real-world invoices with heterogeneous layouts  
- **Language:** English  
- **Preprocessing:** Minimal (grayscale + resize when required)

Sensitive data has been anonymized where applicable.

---

## 3. OCR Pipelines

### 3.1 Tesseract Pipeline
- Engine: Tesseract OCR
- Layout detection: basic
- Post-processing: regex-based field extraction
- Strengths: speed, simplicity
- Limitations: weak table and layout handling

### 3.2 PaddleOCR Pipeline
- Engine: PaddleOCR
- Layout detection: advanced (text blocks, tables)
- Post-processing: structured text regions
- Strengths: robustness to complex layouts
- Limitations: slightly higher processing time

Both pipelines output a **normalized JSON structure** aligned with the data model defined in **Ticket #1**.

---

## 4. Evaluation Metrics

The following **business-oriented metrics** were used.

### 4.1 Field Extraction Success Rate (FESR)

Percentage of expected invoice fields correctly extracted.


### 4.2 Missing Field Rate (MFR)

Percentage of expected fields not detected at all.


### 4.3 Critical Field Accuracy (CFA)

Accuracy on **business-critical fields**:
- Invoice number
- Invoice date
- Supplier
- Exclusive of taxes
- VAT
- Amount including tax


### 4.4 Noise / Hallucination Rate (NHR)

Qualitative assessment of OCR noise:
- irrelevant text
- malformed values
- unusable output for downstream agents

### 4.5 Time-to-Extract (TTE)

Average processing time to produce a usable JSON output per invoice.

---

## 5. Results Summary

| Metric | Tesseract | PaddleOCR |
|------|----------|-----------|
| Field Extraction Success Rate (FESR) | ~68% | ~84% |
| Missing Field Rate (MFR) | ~22% | ~8% |
| Critical Field Accuracy (CFA) | ~71% | ~91% |
| Noise / Hallucination Rate | High | Low |
| Time-to-Extract (avg) | ~1.2s | ~1.6s |

---

## 6. Observations

### Tesseract
- Performs adequately on simple invoices
- Struggles with:
  - tables
  - multi-column layouts
  - alignment-sensitive fields
- Generates significant noise, increasing downstream agent error risk

### PaddleOCR
- Robust across diverse layouts
- Superior table and block detection
- Produces cleaner, more structured text
- Slightly slower, but acceptable for batch processing

---

## 7. Business Impact Analysis

From an agentic automation perspective:

- Missing or malformed fields significantly increase:
  - agent retries
  - LLM hallucinations
  - human validation cost

- Cleaner OCR output reduces:
  - normalization complexity
  - error handling logic
  - operational friction

The **Critical Field Accuracy (CFA)** metric proved to be the most decisive for downstream reliability.

---

## 8. Decision & Recommendation

**PaddleOCR is selected as the OCR engine for the rest of the POC.**

Justification:
- Higher reliability on business-critical fields
- Lower noise rate
- Better alignment with agent-based reasoning and validation steps

Tesseract remains a viable fallback for:
- simple documents
- constrained environments
- lightweight deployments

---

## 9. Next Steps

- Integrate PaddleOCR output into:
  - normalization and validation agent (Ticket #3)
- Use OCR confidence and missing-field signals to:
  - trigger human-in-the-loop review
  - guide agent decision paths

