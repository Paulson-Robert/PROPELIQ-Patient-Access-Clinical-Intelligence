# Task - TASK_002

## Requirement Reference

- **User Story:** US_035
- **Story Location:** .propel/context/tasks/EP-007/us_035/us_035.md
- **Acceptance Criteria:**
  - AC-01: PDF parsed via iText7
  - AC-02: DOCX parsed via DocumentFormat.OpenXml
  - AC-03: Images/scanned PDFs parsed via Tesseract OCR
  - AC-04: DICOM metadata extracted
  - AC-05: FHIR documents parsed to structured data
- **Edge Cases:**
  - Corrupt file — error logged, marked as unparseable
  - Multi-language document — default to English extraction

---

## Applicable Technology Stack

| Layer    | Technology             | Version | Justification           |
| -------- | ---------------------- | ------- | ----------------------- |
| Backend  | ASP.NET Core           | 9.0     | TR-002 — parser service |
| Document | iText7                 | Latest  | NFR-011 — PDF parsing   |
| Document | DocumentFormat.OpenXml | Latest  | NFR-011 — DOCX parsing  |
| Document | Tesseract.NET          | Latest  | NFR-011 — OCR           |

---

## Task Overview

Implement format-specific document parsers (PDF via iText7, DOCX via OpenXml, images via Tesseract OCR, DICOM metadata, FHIR) producing text for NER pipeline.

## Dependent Tasks

- US_033 task (Malware Pipeline) — clean files

## Impacted Components

- Infrastructure/Parsers/PdfDocumentParser.cs — iText7 PDF
- Infrastructure/Parsers/DocxDocumentParser.cs — OpenXml DOCX
- Infrastructure/Parsers/OcrDocumentParser.cs — Tesseract OCR
- Infrastructure/Parsers/DicomDocumentParser.cs — DICOM metadata
- Infrastructure/Parsers/FhirDocumentParser.cs — FHIR structured
- Infrastructure/Parsers/IDocumentParser.cs — parser interface

## Implementation Plan

1. Define IDocumentParser interface with Parse(stream) → text
2. Implement PdfDocumentParser using iText7
3. Implement DocxDocumentParser using DocumentFormat.OpenXml
4. Implement OcrDocumentParser using Tesseract.NET
5. Implement DicomDocumentParser for metadata extraction
6. Implement FhirDocumentParser for structured data
7. Create parser factory selecting parser by file type
8. Handle corrupt files with error logging

## Expected Changes

| Action | File Path                                         | Description |
| ------ | ------------------------------------------------- | ----------- |
| CREATE | src/Infrastructure/Parsers/IDocumentParser.cs     | Interface   |
| CREATE | src/Infrastructure/Parsers/PdfDocumentParser.cs   | PDF         |
| CREATE | src/Infrastructure/Parsers/DocxDocumentParser.cs  | DOCX        |
| CREATE | src/Infrastructure/Parsers/OcrDocumentParser.cs   | OCR         |
| CREATE | src/Infrastructure/Parsers/DicomDocumentParser.cs | DICOM       |
| CREATE | src/Infrastructure/Parsers/FhirDocumentParser.cs  | FHIR        |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — each parser extracts expected text

## Implementation Checklist

- [ ] Create IDocumentParser interface (AC-01-05)
- [ ] Implement PDF parser via iText7 (AC-01)
- [ ] Implement DOCX parser via DocumentFormat.OpenXml (AC-02)
- [ ] Implement OCR parser via Tesseract.NET (AC-03)
- [ ] Implement DICOM metadata extractor (AC-04)
- [ ] Implement FHIR document parser (AC-05)
- [ ] Handle corrupt files with error logging (Edge Cases)
