# Security

This is a personal research-oriented proof of concept exploring LLM reliability, structured extraction, and indirect prompt injection resistance. It is **not** a production system.

## Important notes

- **All sample invoices and documents included in this repository are fully synthetic.** They were generated for testing purposes and do not contain real personal or company data.
- Do not use this prototype to process real invoices or any sensitive documents without appropriate data-handling controls and approvals.
- OCR text and model responses may appear in console logs.
- Document content is sent to the configured Azure OpenAI endpoint (or any other LLM endpoint you configure). Review the terms of the service you use.
- The `baseline-v1` prompt is intentionally unprotected and provided only for experimental comparison. It should not be used outside of controlled tests.
- The `hardened-v1` prompt includes additional instructions to ignore document-level instructions, but it is not a complete defense against prompt injection.

## Reporting issues

If you discover a security or privacy concern in this repository, please open an issue or contact the repository owner.
