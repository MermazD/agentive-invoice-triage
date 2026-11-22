# Prompt Experiments

This folder contains the prompts used for comparing baseline and hardened configurations in the context of structured invoice extraction and indirect prompt injection resistance.

## Files

- `baseline-v1.txt` / `baseline-v1.en.txt`  
  Standard extraction prompt with no additional security instructions.  
  **Intentionally unprotected** — provided for comparison only.  
  Do not use it in production or with real documents.

- `hardened-v1.txt` / `hardened-v1.en.txt`  
  Same extraction prompt with additional instructions to ignore document-level instructions and treat the OCR text as untrusted data.

## Purpose

These prompts are part of an experimental setup to observe how different prompt-level instructions affect model behavior when processing untrusted document content (e.g., indirect prompt injection attempts embedded in invoices).

They are research artifacts and are not claimed to provide complete protection.
