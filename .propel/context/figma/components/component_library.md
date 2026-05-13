# Component Library — HealthAccess

## Overview

- **App**: HealthAccess — Unified Patient Access & Clinical Intelligence Platform
- **Framework**: React 18+ with Vite
- **Component Library**: Shadcn UI (Radix primitives + TailwindCSS)
- **Icon Library**: Lucide React (outlined, 1.5px stroke, 16/20/24px)
- **Design System**: `.propel/context/docs/designsystem.md`
- **Total Components**: 40
- **Categories**: 6 (Actions, Inputs, Navigation, Content, Feedback, Data Display)

## Naming Convention

```
C/<Category>/<Name>
```

## Component Categories

### C/Actions

#### C/Actions/Button

| Property | Values                                           |
| -------- | ------------------------------------------------ |
| Source   | `shadcn/button`                                  |
| Type     | Primary, Secondary, Destructive, Ghost, Outline  |
| Size     | Small (28px), Medium (36px), Large (44px)        |
| State    | Default, Hover, Focus, Active, Disabled, Loading |
| Icon     | None, Leading, Trailing                          |

**Token Usage**:

| Token         | Value                                                |
| ------------- | ---------------------------------------------------- |
| Border Radius | `borderRadius.button` (6px)                          |
| Typography    | `typography.styles.label` (Inter 14px/500)           |
| Padding X     | `spacing.component.button-padding-x` (16px)          |
| Padding Y     | `spacing.component.button-padding-y` (8px)           |
| Focus Ring    | `colors.semantic.ring.DEFAULT` (#1e40af), 2px offset |

**Type Styles**:

| Type        | Background                             | Foreground                       | Hover BG                      |
| ----------- | -------------------------------------- | -------------------------------- | ----------------------------- |
| Primary     | `primary.DEFAULT` (#1e40af)            | `primary.foreground` (#fff)      | `primary.hover` (#1d4ed8)     |
| Secondary   | `secondary.DEFAULT` (#f4f4f5)          | `secondary.foreground` (#18181b) | `secondary.hover` (#e4e4e7)   |
| Destructive | `destructive.DEFAULT` (#dc2626)        | `destructive.foreground` (#fff)  | `destructive.hover` (#b91c1c) |
| Ghost       | transparent                            | `foreground.DEFAULT` (#18181b)   | `secondary.DEFAULT` (#f4f4f5) |
| Outline     | transparent (border: `border.DEFAULT`) | `foreground.DEFAULT`             | `secondary.DEFAULT`           |

**State Behavior**:

| State    | Visual                                                                             |
| -------- | ---------------------------------------------------------------------------------- |
| Default  | Base styling per type                                                              |
| Hover    | Background shifts to hover token (150ms ease)                                      |
| Focus    | 2px primary ring, 2px offset (≥3:1 contrast)                                       |
| Active   | Background shifts to `primary.active` (#1e3a8a)                                    |
| Disabled | 40% opacity, `cursor: not-allowed`, no pointer events                              |
| Loading  | Loader2 icon (spinning) + contextual text ("Signing in...", "Saving..."), disabled |

**Min Touch Target**: 44×44px on mobile viewports (UXR-205)

#### C/Actions/IconButton

| Property     | Values                                  |
| ------------ | --------------------------------------- |
| Source       | `shadcn/button` (icon variant)          |
| Type         | Ghost, Outline                          |
| Size         | Small (28×28px), Medium (36×36px)       |
| State        | Default, Hover, Focus, Active, Disabled |
| Aspect Ratio | 1:1 (square)                            |

#### C/Actions/Link

| Property | Values                                                     |
| -------- | ---------------------------------------------------------- |
| Source   | Custom (Tailwind)                                          |
| Type     | Default (`primary.DEFAULT`), Muted (`foreground.muted`)    |
| State    | Default, Hover (underline), Focus (ring), Active, Disabled |

#### C/Actions/FAB

| Property   | Values                        |
| ---------- | ----------------------------- |
| Source     | Custom (extends Button)       |
| Type       | Primary                       |
| Size       | 56×56px                       |
| Position   | Fixed, bottom-right           |
| Visibility | Mobile only (<768px)          |
| State      | Default, Hover, Focus, Active |

### C/Inputs

#### C/Inputs/Input

| Property | Values                                   |
| -------- | ---------------------------------------- |
| Source   | `shadcn/input`                           |
| Type     | Text, Email, Password, Tel, Search, Date |
| Size     | Small, Medium                            |
| State    | Default, Focus, Error, Disabled          |
| Content  | Empty (placeholder), Filled              |

**Token Usage**:

| Token             | Value                                        |
| ----------------- | -------------------------------------------- |
| Border            | `input.DEFAULT` (#e4e4e7)                    |
| Focus Border      | `border.focus` (#1e40af)                     |
| Error Border      | `border.error` (#dc2626)                     |
| Radius            | `borderRadius.input` (6px)                   |
| Padding X         | `spacing.component.input-padding-x` (12px)   |
| Padding Y         | `spacing.component.input-padding-y` (8px)    |
| Typography        | `typography.styles.body-sm` (Inter 14px/400) |
| Placeholder Color | `foreground.muted` (#a1a1aa)                 |

**Label**: Required above field (`typography.styles.label`). Error text below field in `destructive.DEFAULT` with AlertCircle icon.

#### C/Inputs/Textarea

| Property | Values                          |
| -------- | ------------------------------- |
| Source   | `shadcn/textarea`               |
| State    | Default, Focus, Error, Disabled |
| Feature  | Auto-resize option              |

#### C/Inputs/Select

| Property | Values                          |
| -------- | ------------------------------- |
| Source   | `shadcn/select`                 |
| State    | Default, Focus, Error, Disabled |
| Content  | Placeholder, Selected           |
| Icon     | ChevronDown (trailing)          |

#### C/Inputs/Checkbox

| Property | Values                                    |
| -------- | ----------------------------------------- |
| Source   | `shadcn/checkbox`                         |
| State    | Default, Checked, Indeterminate, Disabled |

#### C/Inputs/RadioGroup

| Property | Values                      |
| -------- | --------------------------- |
| Source   | `shadcn/radio-group`        |
| State    | Default, Selected, Disabled |

#### C/Inputs/Switch

| Property  | Values                                |
| --------- | ------------------------------------- |
| Source    | `shadcn/switch`                       |
| State     | Default (off), Checked (on), Disabled |
| On Color  | `primary.DEFAULT` track               |
| Off Color | `foreground.muted` track              |

#### C/Inputs/DatePicker

| Property | Values                                   |
| -------- | ---------------------------------------- |
| Source   | `shadcn/calendar` + `shadcn/popover`     |
| State    | Default, Focus, Error                    |
| Behavior | Calendar opens in popover on click/focus |

#### C/Inputs/TimePicker

| Property   | Values                  |
| ---------- | ----------------------- |
| Source     | Custom (extends Select) |
| State      | Default, Focus, Error   |
| Increments | 15-minute options       |

#### C/Inputs/FileUpload

| Property | Values                                                                                                                 |
| -------- | ---------------------------------------------------------------------------------------------------------------------- |
| Source   | Custom [CUSTOM]                                                                                                        |
| State    | Default (dashed border), Dragging (primary border), Uploading (progress), Complete (check), Error (destructive border) |
| Icon     | Upload (24px)                                                                                                          |
| Composes | Card + Progress                                                                                                        |

**Token Usage**:

| State    | Border                      | Background           |
| -------- | --------------------------- | -------------------- |
| Default  | Dashed `border.DEFAULT`     | `background.DEFAULT` |
| Dragging | Solid `primary.DEFAULT`     | `primary.muted`      |
| Error    | Solid `destructive.DEFAULT` | `destructive.muted`  |

#### C/Inputs/OTPInput

| Property   | Values                                               |
| ---------- | ---------------------------------------------------- |
| Source     | Custom [CUSTOM]                                      |
| Digits     | 6                                                    |
| State      | Default, Focus, Error, Complete                      |
| Behavior   | Auto-advance on input, auto-submit on 6 digits       |
| Typography | `typography.fontFamily.mono` (JetBrains Mono)        |
| A11y       | `aria-label="Verification code digit N"` per segment |

### C/Navigation

#### C/Navigation/Sidebar

| Property | Values                                     |
| -------- | ------------------------------------------ |
| Source   | `shadcn/sidebar`                           |
| State    | Expanded (256px), Collapsed (64px), Hidden |
| Role     | Patient, Staff, Admin                      |

**Menu Items**:

| Role    | Items                                                                                  |
| ------- | -------------------------------------------------------------------------------------- |
| Patient | Dashboard, Appointments, Intake, Documents, History, Insurance, Calendar Sync, Profile |
| Staff   | Dashboard, Queue, Walk-in, Schedule, Patient Lookup, Code Mapping                      |
| Admin   | Dashboard, Users, Audit Log, Metrics                                                   |

**Token Usage**:

| Token             | Value                                              |
| ----------------- | -------------------------------------------------- |
| Width (Expanded)  | `spacing.component.sidebar-width` (256px)          |
| Width (Collapsed) | `spacing.component.sidebar-collapsed-width` (64px) |
| Background        | `background.DEFAULT` (#fff)                        |
| Border Right      | `border.DEFAULT` (#e4e4e7)                         |
| Active Item BG    | `primary.muted` (#eff6ff)                          |
| Active Item Text  | `primary.DEFAULT` (#1e40af)                        |
| Active Indicator  | `aria-current="page"`                              |

**Responsive**:

- Desktop (≥1024px): Expanded
- Tablet (768–1023px): Collapsed (icon-only rail)
- Mobile (<768px): Hidden (replaced by BottomNav)

#### C/Navigation/BottomNav

| Property   | Values                                              |
| ---------- | --------------------------------------------------- |
| Source     | Custom [CUSTOM]                                     |
| Role       | Patient (5 items), Staff (5 items), Admin (4 items) |
| Height     | `spacing.component.bottom-nav-height` (64px)        |
| Visibility | Mobile only (<768px)                                |
| A11y       | Tab bar with `aria-current="page"` on active item   |

#### C/Navigation/Tabs

| Property | Values                                                             |
| -------- | ------------------------------------------------------------------ |
| Source   | `shadcn/tabs`                                                      |
| Style    | Underline variant                                                  |
| State    | Default, Active                                                    |
| A11y     | `role="tablist"`, `role="tab"`, `role="tabpanel"`, `aria-selected` |

#### C/Navigation/Breadcrumb

| Property  | Values                   |
| --------- | ------------------------ |
| Source    | `shadcn/breadcrumb`      |
| Separator | ChevronRight icon (16px) |

#### C/Navigation/Header

| Property | Values                                                                        |
| -------- | ----------------------------------------------------------------------------- |
| Source   | Custom (extends Shadcn)                                                       |
| Height   | `spacing.component.header-height` (64px)                                      |
| Content  | Logo (left), Search (center, Staff/Admin), Avatar dropdown (right), Bell icon |
| A11y     | Skip-to-content link before header                                            |

#### C/Navigation/CommandMenu

| Property | Values                            |
| -------- | --------------------------------- |
| Source   | `shadcn/command`                  |
| Trigger  | Cmd+K / Ctrl+K                    |
| Scope    | Staff/Admin only                  |
| Search   | Patients, appointments, documents |

### C/Content

#### C/Content/Card

| Property | Values                                                      |
| -------- | ----------------------------------------------------------- |
| Source   | `shadcn/card`                                               |
| Type     | Default, Interactive (hover shadow)                         |
| Content  | Header+Body, Body Only, Stat (large number + label + trend) |

**Token Usage**:

| Token      | Value                                   |
| ---------- | --------------------------------------- |
| Radius     | `borderRadius.card` (8px)               |
| Shadow     | `boxShadow.component.card` (sm)         |
| Background | `background.DEFAULT` (#fff)             |
| Border     | `border.DEFAULT` (#e4e4e7)              |
| Padding    | `spacing.component.card-padding` (24px) |
| Gap        | `spacing.component.card-gap` (16px)     |

#### C/Content/Table

| Property   | Values                                            |
| ---------- | ------------------------------------------------- |
| Source     | `shadcn/table`                                    |
| State      | Default, Loading (skeleton), Empty (EmptyState)   |
| Responsive | Transforms to stacked cards on mobile (<768px)    |
| A11y       | `role="table"`, sortable headers with `aria-sort` |

#### C/Content/DataTable

| Property   | Values                                         |
| ---------- | ---------------------------------------------- |
| Source     | Custom (extends Table + @tanstack/react-table) |
| Features   | Sortable, Filterable, Paginated                |
| Pagination | 20 items/page default                          |

#### C/Content/Avatar

| Property | Values                                    |
| -------- | ----------------------------------------- |
| Source   | `shadcn/avatar`                           |
| Size     | Small (24px), Medium (32px), Large (40px) |
| Content  | Image, Initials fallback                  |
| Radius   | `borderRadius.avatar` (9999px)            |

#### C/Content/Badge

| Property | Values                                                     |
| -------- | ---------------------------------------------------------- |
| Source   | `shadcn/badge`                                             |
| Type     | Default, Secondary, Destructive, Outline, Success, Warning |
| Radius   | `borderRadius.badge` (9999px)                              |

**Type Styles**:

| Type        | Background            | Foreground               |
| ----------- | --------------------- | ------------------------ |
| Default     | `primary.DEFAULT`     | `primary.foreground`     |
| Secondary   | `secondary.DEFAULT`   | `secondary.foreground`   |
| Destructive | `destructive.DEFAULT` | `destructive.foreground` |
| Success     | `success.DEFAULT`     | `success.foreground`     |
| Warning     | `warning.DEFAULT`     | `warning.foreground`     |
| Outline     | transparent (border)  | `foreground.DEFAULT`     |

#### C/Content/Separator

| Property  | Values                     |
| --------- | -------------------------- |
| Source    | `shadcn/separator`         |
| Direction | Horizontal, Vertical       |
| Color     | `border.DEFAULT` (#e4e4e7) |

#### C/Content/Skeleton

| Property  | Values                         |
| --------- | ------------------------------ |
| Source    | `shadcn/skeleton`              |
| Shape     | Rectangle, Circle, TextLine    |
| Animation | `skeleton-pulse` (2s infinite) |
| A11y      | `aria-hidden="true"`           |

#### C/Content/Progress

| Property    | Values                                                                  |
| ----------- | ----------------------------------------------------------------------- |
| Source      | `shadcn/progress`                                                       |
| State       | Default (animated fill), Complete                                       |
| Fill Color  | `primary.DEFAULT`                                                       |
| Track Color | `secondary.DEFAULT`                                                     |
| A11y        | `role="progressbar"`, `aria-valuenow`, `aria-valuemin`, `aria-valuemax` |

#### C/Content/StepIndicator

| Property   | Values                                                                                                   |
| ---------- | -------------------------------------------------------------------------------------------------------- |
| Source     | Custom [CUSTOM]                                                                                          |
| Steps      | 4 (Uploading → Scanning → Processing → Completed/Failed)                                                 |
| Step State | Completed (check icon, success bg), Active (primary ring), Pending (muted), Failed (X icon, destructive) |
| Layout     | Horizontal — circles connected by lines                                                                  |

#### C/Content/ChatBubble

| Property  | Values                                                                                                          |
| --------- | --------------------------------------------------------------------------------------------------------------- |
| Source    | Custom [CUSTOM]                                                                                                 |
| Sender    | User (right-aligned, `primary.DEFAULT` bg, white text), AI (left-aligned, `secondary.DEFAULT` bg, default text) |
| State     | Default, Typing (3-dot bounce animation, `typing-dots` keyframe)                                                |
| Max Width | 80%                                                                                                             |
| A11y      | `aria-live="polite"` on message container                                                                       |

#### C/Content/Timeline

| Property | Values                                    |
| -------- | ----------------------------------------- |
| Source   | Custom [CUSTOM]                           |
| Layout   | Vertical, circle markers + connector line |
| Usage    | Conflict resolution history (SCR-020)     |

#### C/Content/QRCode

| Property | Values                          |
| -------- | ------------------------------- |
| Source   | Custom [CUSTOM]                 |
| Usage    | TOTP provisioning URI (SCR-027) |

#### C/Content/EmptyState

| Property | Values                                                                                                         |
| -------- | -------------------------------------------------------------------------------------------------------------- |
| Source   | Custom [CUSTOM]                                                                                                |
| Layout   | Centered: Lucide icon (48px, `foreground.muted`) + Heading (H3) + Description (body-sm) + CTA Button (Primary) |

### C/Feedback

#### C/Feedback/Dialog

| Property  | Values                                                                            |
| --------- | --------------------------------------------------------------------------------- |
| Source    | `shadcn/dialog`                                                                   |
| Max Width | 425px                                                                             |
| Radius    | `borderRadius.dialog` (12px)                                                      |
| Shadow    | `boxShadow.component.dialog` (lg)                                                 |
| Z-Index   | `zIndex.modal` (70)                                                               |
| Overlay   | Semi-transparent black backdrop                                                   |
| A11y      | `role="dialog"`, `aria-modal="true"`, `aria-labelledby`, focus trap, ESC to close |

#### C/Feedback/AlertDialog

| Property | Values                                                         |
| -------- | -------------------------------------------------------------- |
| Source   | `shadcn/alert-dialog`                                          |
| Type     | Default, Destructive                                           |
| Buttons  | Cancel (secondary) + Continue/Confirm (primary or destructive) |
| A11y     | `role="alertdialog"`, non-dismissable backdrop                 |

#### C/Feedback/Drawer

| Property | Values                   |
| -------- | ------------------------ |
| Source   | `shadcn/drawer`          |
| Edge     | Bottom                   |
| Usage    | Mobile-only bottom sheet |

#### C/Feedback/Sheet

| Property | Values                                                               |
| -------- | -------------------------------------------------------------------- |
| Source   | `shadcn/sheet`                                                       |
| Edge     | Right, Bottom                                                        |
| Usage    | MOD-004 (Create/Edit User) on mobile; AI intake data panel on mobile |

#### C/Feedback/Toast

| Property     | Values                                         |
| ------------ | ---------------------------------------------- |
| Source       | `shadcn/sonner`                                |
| Type         | Info, Success, Warning, Destructive            |
| Position     | Bottom-right (desktop), Bottom-center (mobile) |
| Auto-Dismiss | 5 seconds                                      |
| Max Stack    | 3                                              |
| Radius       | `borderRadius.toast` (8px)                     |
| Shadow       | `boxShadow.component.toast` (md)               |
| Z-Index      | `zIndex.toast` (90)                            |
| Animation    | `toast-enter` (slide up + fade in, 200ms)      |
| A11y         | `role="status"`, `aria-live="polite"`          |

#### C/Feedback/Alert

| Property | Values                              |
| -------- | ----------------------------------- |
| Source   | `shadcn/alert`                      |
| Type     | Info, Success, Warning, Destructive |
| Content  | Icon + Title + Description          |

**Type Styles**:

| Type        | Border                | Background          | Icon          |
| ----------- | --------------------- | ------------------- | ------------- |
| Info        | `primary.DEFAULT`     | `primary.muted`     | AlertCircle   |
| Success     | `success.DEFAULT`     | `success.muted`     | Check         |
| Warning     | `warning.DEFAULT`     | `warning.muted`     | AlertTriangle |
| Destructive | `destructive.DEFAULT` | `destructive.muted` | AlertTriangle |

#### C/Feedback/Tooltip

| Property  | Values                 |
| --------- | ---------------------- |
| Source    | `shadcn/tooltip`       |
| Delay     | 200ms                  |
| Max Width | 200px                  |
| Z-Index   | `zIndex.tooltip` (100) |

#### C/Feedback/Popover

| Property | Values                                |
| -------- | ------------------------------------- |
| Source   | `shadcn/popover`                      |
| Z-Index  | `zIndex.popover` (80)                 |
| Usage    | DatePicker calendar, filter dropdowns |

### C/DataDisplay

#### C/DataDisplay/Chart

| Property      | Values                                                   |
| ------------- | -------------------------------------------------------- |
| Source        | Custom (Recharts)                                        |
| Type          | Bar, Line, Pie                                           |
| Color Palette | Semantic tokens (primary, success, warning, destructive) |
| A11y          | Accessible color palette, tooltip on hover               |

#### C/DataDisplay/Calendar

| Property          | Values                                      |
| ----------------- | ------------------------------------------- |
| Source            | Custom (extends Shadcn Calendar)            |
| View              | Day, Week                                   |
| Appointment Block | Patient name, time, status, risk tier badge |
| Color             | Primary for appointment blocks              |

## Risk Tier Component

Used across SCR-017, SCR-019, SCR-022. Composes Badge + Lucide icon.

| Tier   | Icon          | Color                              | Background                         | Border                         |
| ------ | ------------- | ---------------------------------- | ---------------------------------- | ------------------------------ |
| Low    | Shield        | `risk.low.foreground` (#15803d)    | `risk.low.background` (#f0fdf4)    | `risk.low.border` (#16a34a)    |
| Medium | AlertCircle   | `risk.medium.foreground` (#b45309) | `risk.medium.background` (#fffbeb) | `risk.medium.border` (#d97706) |
| High   | AlertTriangle | `risk.high.foreground` (#b91c1c)   | `risk.high.background` (#fef2f2)   | `risk.high.border` (#dc2626)   |

**A11y**: Icon + text label + color (UXR-206 — never color alone).

## Required State Matrix

Every interactive component MUST implement these states:

| State    | Visual Treatment                                      | Duration   |
| -------- | ----------------------------------------------------- | ---------- |
| Default  | Base styling per type tokens                          | —          |
| Hover    | Subtle background/elevation shift                     | 150ms ease |
| Focus    | 2px `ring.DEFAULT` ring, 2px offset, ≥3:1 contrast    | —          |
| Active   | Pressed/depressed visual (darker bg)                  | —          |
| Disabled | 40% opacity, `cursor: not-allowed`, no pointer events | —          |
| Loading  | Skeleton or Loader2 spinner, preserve dimensions      | —          |

## Anti-Pattern Checklist

| #   | Anti-Pattern                                             | Status   |
| --- | -------------------------------------------------------- | -------- |
| 1   | No `background-clip: text` usage                         | Enforced |
| 2   | No `backdrop-filter: blur` usage                         | Enforced |
| 3   | No thick side borders (>2px decorative)                  | Enforced |
| 4   | No sole-typeface-Inter (3 typefaces in use)              | Enforced |
| 5   | No purple-to-blue gradient                               | Enforced |
| 6   | No raw hex values outside primitive table                | Enforced |
| 7   | No "Lorem ipsum" or placeholder text                     | Enforced |
| 8   | No width/height transitions (use opacity/transform only) | Enforced |
