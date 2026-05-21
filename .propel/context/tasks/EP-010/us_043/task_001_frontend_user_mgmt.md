# Task - TASK_001

## Requirement Reference

- **User Story:** US_043
- **Story Location:** .propel/context/tasks/EP-010/us_043/us_043.md
- **Acceptance Criteria:**
  - AC-01: Admin can create, edit, deactivate user accounts
  - AC-02: Role assignment from dropdown (Patient, Staff, Admin)
  - AC-03: Deactivation prevents login without deleting data
- **Edge Cases:**
  - Admin deactivates self — prevented with error message

---

## Design References

| Reference Type         | Value                                                                   |
| ---------------------- | ----------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                     |
| **Wireframe Status**   | AVAILABLE                                                               |
| **Wireframe Type**     | HTML                                                                    |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-023-user-management.html |
| **Screen Spec**        | SCR-023, MOD-004                                                        |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification           |
| ----------- | ------------ | -------------------- | ----------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — admin UI       |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — tables, forms |

---

## Task Overview

Implement user management page (SCR-023) for admins with CRUD operations, role assignment, and deactivation dialog (MOD-004).

## Dependent Tasks

- US_001 task (Frontend Scaffold)

## Impacted Components

- frontend/src/pages/admin/UserManagementPage.tsx — user list
- frontend/src/components/admin/UserFormDialog.tsx — create/edit
- frontend/src/components/admin/DeactivateDialog.tsx — MOD-004

## Expected Changes

| Action | File Path                                          | Description     |
| ------ | -------------------------------------------------- | --------------- |
| CREATE | frontend/src/pages/admin/UserManagementPage.tsx    | User management |
| CREATE | frontend/src/components/admin/UserFormDialog.tsx   | User form       |
| CREATE | frontend/src/components/admin/DeactivateDialog.tsx | Deactivate      |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — CRUD operations, role assignment

## Implementation Checklist

- [x] Create user management table with create/edit/deactivate actions (AC-01)
- [x] Implement role assignment dropdown (AC-02)
- [x] Create deactivation dialog (MOD-004) preventing login (AC-03)
- [x] Prevent admin self-deactivation (Edge Cases)
