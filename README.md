# agentive-invoice-triage

Experimental pipeline exploring LLM reliability for structured extraction from untrusted documents, with a focus on indirect prompt injection resistance and deterministic validation.

This POC investigates how prompt-level safeguards, platform protections, and deterministic business rules interact when processing invoice PDFs that may contain adversarial content.

---

### 🎯 **Objective**

Replace manual invoice triage with a modular pipeline while systematically studying:

- Consistency of structured JSON extraction
- Behavior under indirect prompt injection attempts embedded in documents
- Effectiveness of hardened vs baseline prompts
- Value of deterministic post-validation as a safety layer

The goal is empirical observation of current LLM limitations rather than a production-ready product.

---

### 🏗️ **Architecture**

**Pipeline:**  
PDF Upload → OCR (Tesseract) → LLM Extraction (Semantic Kernel + Azure OpenAI) → Deterministic Validation → Decision / Logging

**Key components:**

- **.NET console app** — orchestration, ingestion, exposure of results
- **OCR layer** — Tesseract for text extraction from PDFs
- **LLM extraction** — structured output via Semantic Kernel
- **Validation layer** — deterministic business rules independent of the LLM
- **Experiment harness** — baseline vs hardened prompt configurations + injection test cases

---

### ✨ **Experimental Focus**

- Comparative evaluation of prompt configurations (baseline vs hardened)
- Injection of adversarial instructions inside invoice documents (indirect prompt injection)
- Measurement of extraction consistency (valid JSON, correct field values, refusal behavior)
- Analysis of the interplay between prompt safeguards, platform-level protections, and deterministic validation
- Documentation of current methodology limitations

---

### 🧩 **Tech Stack**

- **C# / .NET**
- **Microsoft Semantic Kernel**
- **Azure OpenAI**
- **Tesseract OCR**
- **Structured JSON generation + schema validation**

---

# 🛠️ Installation & execution

### **Cloner the project**

```bash
git clone https://github.com/mermazd/agentive-invoice-triage
cd Tesseract

# Default invoice: INV-01.pdf
dotnet run

# Specific invoice
dotnet run -- Samples/INV-04.pdf

# Prompt injection scenario
dotnet run -- Samples/INV-PROMPT-INJECTION.pdf
```

---

# 📈 Current Limitations & Next Experiments

- Limited test set size
- Evaluation still partially manual
- No systematic red-teaming framework yet
- Planned: automated injection suite, consistency metrics over multiple runs, comparison with local models

---

# 🔄 Status

Personal research-oriented POC (2026).

Work in progress — commits continue as new experiments are added.
