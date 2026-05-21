# Task - TASK_001

## Requirement Reference

- **User Story:** US_032
- **Story Location:** .propel/context/tasks/EP-006/us_032/us_032.md
- **Acceptance Criteria:**
  - AC-01: Drag-and-drop upload zone with click fallback
  - AC-02: File format validation (PDF, DOCX, PNG, JPG, DICOM)
  - AC-03: File size validation (max 25MB per file)
  - AC-04: Upload progress indicator per file
- **Edge Cases:**
  - Invalid file type — clear error message with accepted formats

---

## Design References

| Reference Type         | Value                                                                   |
| ---------------------- | ----------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                     |
| **Wireframe Status**   | AVAILABLE                                                               |
| **Wireframe Type**     | HTML                                                                    |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-011-document-upload.html |
| **Screen Spec**        | SCR-011                                                                 |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification      |
| ----------- | ------------ | -------------------- | ------------------ |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — upload UI |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — dropzone |

---

## Task Overview

Implement document upload page (SCR-011) with drag-and-drop zone, format/size validation, and per-file upload progress indicators.

## Dependent Tasks

- US_001 task (Frontend Scaffold)

## Impacted Components

- frontend/src/pages/documents/DocumentUploadPage.tsx — upload page
- frontend/src/components/documents/DropZone.tsx — drag-and-drop area
- frontend/src/components/documents/UploadProgress.tsx — progress indicator

## Expected Changes

| Action | File Path                                            | Description |
| ------ | ---------------------------------------------------- | ----------- |
| CREATE | frontend/src/pages/documents/DocumentUploadPage.tsx  | Upload page |
| CREATE | frontend/src/components/documents/DropZone.tsx       | Drop zone   |
| CREATE | frontend/src/components/documents/UploadProgress.tsx | Progress    |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — validation, progress rendering

## Implementation Checklist

- [x] Create DropZone with drag-and-drop and click fallback (AC-01)
- [x] Implement file format validation with clear error messages (AC-02)
- [x] Implement 25MB file size validation (AC-03)
- [x] Create per-file UploadProgress indicator (AC-04)
