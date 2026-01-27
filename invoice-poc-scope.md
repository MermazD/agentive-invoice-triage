📄 Documentation (POC – Scope & Decisions)

Purpose
This ticket defines the functional and data scope of the Invoice Triage POC. It serves as the reference for all downstream tickets (OCR, Agent, UI).

Key Decisions
- Target document: standard B2B invoices
- Address kept as a single string (no granular parsing in POC)
- due_date (échéance) is a mandatory business field
- No predefined invoice categories (free-text + line items only)
- Line items are extracted as a list (description, quantity, price, tax)

Extracted Fields
- Invoice number
- Client: first name, last name, address, country
- Invoice date
- Due date (échéance)
- Purchase order number
- Line items (description, quantity, price, tax %)
- Subtotal, tax amount (€), total excl. tax (HT), total incl. tax (TTC)

Output Contract
- Single normalized JSON schema (v1) used by OCR → Agent → UI

Constraints & Assumptions
- RGPD-compliant usage (POC, internal, limited retention)
- OCR quality may vary; errors must be surfaced, not hidden
- Not intended for production or legal-grade accounting

Out of Scope
- Advanced tax rules
- Multi-country fiscal compliance
- ERP-level categorization