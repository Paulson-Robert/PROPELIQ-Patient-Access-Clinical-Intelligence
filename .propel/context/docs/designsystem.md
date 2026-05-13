# Design System — Unified Patient Access & Clinical Intelligence Platform

## 1. UI Impact Assessment

### Change Summary

This is a greenfield design system for a healthcare platform supporting three roles (Patient, Staff, Admin) across 29 screens, 7 modals, and 11 prototype flows.

### Impact Classification

| Dimension          | Impact Level | Details                                                                                                |
| ------------------ | ------------ | ------------------------------------------------------------------------------------------------------ |
| Visual Consistency | High         | Full design token system required for colors, typography, spacing, shadows                             |
| Component Coverage | High         | 40+ component types across 6 categories (Actions, Inputs, Navigation, Content, Feedback, Data Display) |
| Accessibility      | High         | WCAG 2.2 Level AA compliance across all components                                                     |
| Responsiveness     | High         | 4 breakpoints (375px, 768px, 1024px, 1440px) with layout adaptation                                    |

---

## 2. User Story Design Context

### Role-Specific Design Needs

| Role    | Key Design Considerations                                                                                                          |
| ------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Patient | Mobile-first forms, minimal cognitive load, clear CTAs, reassuring feedback during AI intake, simple document upload               |
| Staff   | Information-dense layouts, scannable tables/queues, efficient keyboard navigation, risk tier visual indicators, drag-reorder queue |
| Admin   | Data tables with search/filter, audit log readability, metrics dashboard with accessible charts                                    |

---

## 3. Design Source References

| Source                     | Path                                 | Usage                                        |
| -------------------------- | ------------------------------------ | -------------------------------------------- |
| Requirements Specification | `.propel/context/docs/spec.md`       | Functional requirements, use cases, personas |
| Architecture Design        | `.propel/context/docs/design.md`     | Technology stack, NFR constraints            |
| Figma Specification        | `.propel/context/docs/figma_spec.md` | Screen inventory, UXRs, prototype flows      |

---

## 4. Screen-to-Design Mappings

| Screen Group   | Screens              | Design System Sections Applied                                              |
| -------------- | -------------------- | --------------------------------------------------------------------------- |
| Auth & Shared  | SCR-001, SCR-026–029 | Typography, Form Inputs, Buttons, Alerts, Cards                             |
| Patient Portal | SCR-002, SCR-005–016 | All tokens; ChatBubble, StepIndicator, FileUpload, Calendar sync components |
| Staff Portal   | SCR-003, SCR-017–022 | Data Tables, Badges, Drag handles, Risk tier indicators, Calendar, Tabs     |
| Admin Portal   | SCR-004, SCR-023–025 | Data Tables, Charts, Badges, Export controls                                |
| Modals         | MOD-001–007          | Dialog, AlertDialog, Sheet, Overlay, Toast                                  |

---

## 5. Design Tokens

### Color Primitives

```yaml
colors:
  primitive:
    blue:
      50: "#eff6ff"
      100: "#dbeafe"
      200: "#bfdbfe"
      300: "#93c5fd"
      400: "#60a5fa"
      500: "#3b82f6"
      600: "#2563eb"
      700: "#1d4ed8"
      800: "#1e40af"
      900: "#1e3a8a"
      950: "#172554"
    zinc:
      50: "#fafafa"
      100: "#f4f4f5"
      200: "#e4e4e7"
      300: "#d4d4d8"
      400: "#a1a1aa"
      500: "#71717a"
      600: "#52525b"
      700: "#3f3f46"
      800: "#27272a"
      900: "#18181b"
      950: "#09090b"
    green:
      50: "#f0fdf4"
      100: "#dcfce7"
      500: "#22c55e"
      600: "#16a34a"
      700: "#15803d"
    amber:
      50: "#fffbeb"
      100: "#fef3c7"
      500: "#f59e0b"
      600: "#d97706"
      700: "#b45309"
    red:
      50: "#fef2f2"
      100: "#fee2e2"
      500: "#ef4444"
      600: "#dc2626"
      700: "#b91c1c"
    white: "#ffffff"
    black: "#000000"
```

### Semantic Color Tokens

```yaml
colors:
  semantic:
    primary:
      DEFAULT: "{colors.primitive.blue.800}" # #1e40af — primary actions, active nav
      foreground: "{colors.primitive.white}"
      hover: "{colors.primitive.blue.700}" # #1d4ed8
      active: "{colors.primitive.blue.900}" # #1e3a8a
      muted: "{colors.primitive.blue.50}" # #eff6ff — selected row, active tab bg
    secondary:
      DEFAULT: "{colors.primitive.zinc.100}" # #f4f4f5
      foreground: "{colors.primitive.zinc.900}"
      hover: "{colors.primitive.zinc.200}"
    destructive:
      DEFAULT: "{colors.primitive.red.600}" # #dc2626
      foreground: "{colors.primitive.white}"
      hover: "{colors.primitive.red.700}"
      muted: "{colors.primitive.red.50}" # #fef2f2
    success:
      DEFAULT: "{colors.primitive.green.600}" # #16a34a
      foreground: "{colors.primitive.white}"
      muted: "{colors.primitive.green.50}" # #f0fdf4
    warning:
      DEFAULT: "{colors.primitive.amber.600}" # #d97706
      foreground: "{colors.primitive.white}"
      muted: "{colors.primitive.amber.50}" # #fffbeb
    background:
      DEFAULT: "{colors.primitive.white}" # #ffffff — page background
      secondary: "{colors.primitive.zinc.50}" # #fafafa — section/card background
      tertiary: "{colors.primitive.zinc.100}" # #f4f4f5 — nested element bg
    foreground:
      DEFAULT: "{colors.primitive.zinc.900}" # #18181b — primary text
      secondary: "{colors.primitive.zinc.500}" # #71717a — secondary text
      muted: "{colors.primitive.zinc.400}" # #a1a1aa — placeholder text
    border:
      DEFAULT: "{colors.primitive.zinc.200}" # #e4e4e7 — default borders
      focus: "{colors.primitive.blue.800}" # #1e40af — focus ring
      error: "{colors.primitive.red.600}" # #dc2626 — error field border
      conflict: "{colors.primitive.amber.500}" # #f59e0b — conflict highlight border
    input:
      DEFAULT: "{colors.primitive.zinc.200}" # #e4e4e7 — input border
      background: "{colors.primitive.white}"
    ring:
      DEFAULT: "{colors.primitive.blue.800}" # #1e40af — focus ring
```

### Risk Tier Tokens

```yaml
colors:
  risk:
    low:
      background: "{colors.primitive.green.50}"
      border: "{colors.primitive.green.600}"
      foreground: "{colors.primitive.green.700}"
      icon: "Shield"
      label: "Low"
    medium:
      background: "{colors.primitive.amber.50}"
      border: "{colors.primitive.amber.600}"
      foreground: "{colors.primitive.amber.700}"
      icon: "AlertCircle"
      label: "Medium"
    high:
      background: "{colors.primitive.red.50}"
      border: "{colors.primitive.red.600}"
      foreground: "{colors.primitive.red.700}"
      icon: "AlertTriangle"
      label: "High"
```

### Typography Tokens

```yaml
typography:
  fontFamily:
    display: "'Plus Jakarta Sans', system-ui, sans-serif"
    body: "'Inter', system-ui, sans-serif"
    mono: "'JetBrains Mono', ui-monospace, monospace"
  fontSize:
    xs: "0.75rem" # 12px
    sm: "0.875rem" # 14px
    base: "1rem" # 16px
    lg: "1.125rem" # 18px
    xl: "1.25rem" # 20px
    2xl: "1.5rem" # 24px
    3xl: "1.875rem" # 30px
    4xl: "2.25rem" # 36px
  fontWeight:
    normal: 400
    medium: 500
    semibold: 600
    bold: 700
  lineHeight:
    tight: 1.25
    normal: 1.5
    relaxed: 1.625
  letterSpacing:
    tight: "-0.025em"
    normal: "0em"
    wide: "0.025em"
  styles:
    h1:
      fontFamily: "{typography.fontFamily.display}"
      fontSize: "{typography.fontSize.4xl}"
      fontWeight: "{typography.fontWeight.bold}"
      lineHeight: "{typography.lineHeight.tight}"
      letterSpacing: "{typography.letterSpacing.tight}"
    h2:
      fontFamily: "{typography.fontFamily.display}"
      fontSize: "{typography.fontSize.3xl}"
      fontWeight: "{typography.fontWeight.semibold}"
      lineHeight: "{typography.lineHeight.tight}"
    h3:
      fontFamily: "{typography.fontFamily.display}"
      fontSize: "{typography.fontSize.2xl}"
      fontWeight: "{typography.fontWeight.semibold}"
      lineHeight: "{typography.lineHeight.tight}"
    h4:
      fontFamily: "{typography.fontFamily.display}"
      fontSize: "{typography.fontSize.xl}"
      fontWeight: "{typography.fontWeight.semibold}"
      lineHeight: "{typography.lineHeight.normal}"
    body:
      fontFamily: "{typography.fontFamily.body}"
      fontSize: "{typography.fontSize.base}"
      fontWeight: "{typography.fontWeight.normal}"
      lineHeight: "{typography.lineHeight.normal}"
    body-sm:
      fontFamily: "{typography.fontFamily.body}"
      fontSize: "{typography.fontSize.sm}"
      fontWeight: "{typography.fontWeight.normal}"
      lineHeight: "{typography.lineHeight.normal}"
    label:
      fontFamily: "{typography.fontFamily.body}"
      fontSize: "{typography.fontSize.sm}"
      fontWeight: "{typography.fontWeight.medium}"
      lineHeight: "{typography.lineHeight.normal}"
    caption:
      fontFamily: "{typography.fontFamily.body}"
      fontSize: "{typography.fontSize.xs}"
      fontWeight: "{typography.fontWeight.normal}"
      lineHeight: "{typography.lineHeight.normal}"
    code:
      fontFamily: "{typography.fontFamily.mono}"
      fontSize: "{typography.fontSize.sm}"
      fontWeight: "{typography.fontWeight.normal}"
      lineHeight: "{typography.lineHeight.normal}"
```

### Spacing Tokens

```yaml
spacing:
  unit: "4px"
  scale:
    0: "0px"
    0.5: "2px"
    1: "4px"
    1.5: "6px"
    2: "8px"
    2.5: "10px"
    3: "12px"
    4: "16px"
    5: "20px"
    6: "24px"
    8: "32px"
    10: "40px"
    12: "48px"
    16: "64px"
    20: "80px"
    24: "96px"
  component:
    card-padding: "{spacing.scale.6}" # 24px
    card-gap: "{spacing.scale.4}" # 16px
    input-padding-x: "{spacing.scale.3}" # 12px
    input-padding-y: "{spacing.scale.2}" # 8px
    button-padding-x: "{spacing.scale.4}" # 16px
    button-padding-y: "{spacing.scale.2}" # 8px
    section-gap: "{spacing.scale.8}" # 32px
    page-padding-x: "{spacing.scale.6}" # 24px (mobile: 16px)
    page-padding-y: "{spacing.scale.6}" # 24px
    sidebar-width: "256px"
    sidebar-collapsed-width: "64px"
    bottom-nav-height: "64px"
    header-height: "64px"
```

### Border Radius Tokens

```yaml
borderRadius:
  none: "0px"
  sm: "2px"
  DEFAULT: "6px" # Shadcn default
  md: "6px"
  lg: "8px"
  xl: "12px"
  2xl: "16px"
  full: "9999px"
  component:
    button: "{borderRadius.md}"
    input: "{borderRadius.md}"
    card: "{borderRadius.lg}"
    dialog: "{borderRadius.xl}"
    badge: "{borderRadius.full}"
    avatar: "{borderRadius.full}"
    toast: "{borderRadius.lg}"
```

### Shadow Tokens

```yaml
boxShadow:
  none: "none"
  sm: "0 1px 2px 0 rgb(0 0 0 / 0.05)"
  DEFAULT: "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)"
  md: "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)"
  lg: "0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)"
  component:
    card: "{boxShadow.sm}"
    dropdown: "{boxShadow.md}"
    dialog: "{boxShadow.lg}"
    toast: "{boxShadow.md}"
```

### Breakpoint Tokens

```yaml
breakpoints:
  sm: "375px" # mobile
  md: "768px" # tablet
  lg: "1024px" # desktop
  xl: "1440px" # wide desktop
```

### Z-Index Tokens

```yaml
zIndex:
  dropdown: 50
  sticky: 40
  overlay: 60
  modal: 70
  popover: 80
  toast: 90
  tooltip: 100
```

### Animation Tokens

```yaml
animation:
  duration:
    fast: "100ms"
    normal: "200ms"
    slow: "300ms"
  easing:
    DEFAULT: "cubic-bezier(0.4, 0, 0.2, 1)"
    in: "cubic-bezier(0.4, 0, 1, 1)"
    out: "cubic-bezier(0, 0, 0.2, 1)"
    in-out: "cubic-bezier(0.4, 0, 0.2, 1)"
  keyframes:
    skeleton-pulse:
      description: "Pulsing opacity for skeleton loaders"
      steps: "0%: opacity 1 → 50%: opacity 0.5 → 100%: opacity 1"
      duration: "2s"
      iteration: "infinite"
    spinner:
      description: "Continuous rotation for Loader2 icon"
      steps: "0%: rotate(0deg) → 100%: rotate(360deg)"
      duration: "1s"
      iteration: "infinite"
    typing-dots:
      description: "Sequential bounce for AI typing indicator"
      steps: "3 dots with staggered 0.2s delay, translateY -4px bounce"
      duration: "1.4s"
      iteration: "infinite"
    toast-enter:
      description: "Slide up + fade in for toast notifications"
      steps: "translateY(16px) opacity(0) → translateY(0) opacity(1)"
      duration: "{animation.duration.normal}"
    countdown-ring:
      description: "SVG stroke-dashoffset for slot lock countdown"
      steps: "dashoffset 0 → dashoffset circumference"
      duration: "30s"
      timing: "linear"
```

---

## 6. Component References

### Shadcn UI Component Mapping

All components below are sourced from [Shadcn UI](https://ui.shadcn.com/) unless marked `[CUSTOM]`.

| Component     | Shadcn Source                        | Variants                                                   | Custom Notes                                                                                   |
| ------------- | ------------------------------------ | ---------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| Button        | `shadcn/button`                      | Primary, Secondary, Destructive, Ghost, Outline × S/M/L    | Loading state adds Loader2 spinner + disabled                                                  |
| IconButton    | `shadcn/button` (icon variant)       | Ghost, Outline × S/M                                       | Icon-only, square aspect ratio                                                                 |
| Link          | Custom (Tailwind)                    | Default, Muted                                             | Underline on hover, primary color                                                              |
| FAB           | Custom (extends Button)              | Primary                                                    | Mobile-only, fixed bottom-right, 56x56px                                                       |
| Input         | `shadcn/input`                       | Default, Focus, Error, Disabled × S/M                      | Error border uses `border.error` token                                                         |
| Textarea      | `shadcn/textarea`                    | Default, Focus, Error, Disabled                            | Auto-resize option                                                                             |
| Select        | `shadcn/select`                      | Default, Focus, Error, Disabled                            | Chevron down icon                                                                              |
| Checkbox      | `shadcn/checkbox`                    | Default, Checked, Indeterminate, Disabled                  |                                                                                                |
| RadioGroup    | `shadcn/radio-group`                 | Default, Selected, Disabled                                |                                                                                                |
| Switch        | `shadcn/switch`                      | Default, Checked, Disabled                                 |                                                                                                |
| DatePicker    | `shadcn/calendar` + `shadcn/popover` | Default, Focus, Error                                      | Calendar opens in popover                                                                      |
| TimePicker    | Custom (extends Select)              | Default, Focus, Error                                      | 15-min increment options                                                                       |
| FileUpload    | Custom `[CUSTOM]`                    | Default (dropzone), Dragging, Uploading, Complete, Error   | Dashed border dropzone; composes Card + Progress                                               |
| OTPInput      | Custom `[CUSTOM]`                    | Default, Focus, Error, Complete                            | 6-digit segmented input; auto-advance; auto-submit                                             |
| Sidebar       | `shadcn/sidebar`                     | Expanded, Collapsed, Mobile (hidden)                       | Role-filtered menu items                                                                       |
| BottomNav     | Custom `[CUSTOM]`                    | Default                                                    | Mobile-only; 4-5 Lucide icon tabs; 64px height                                                 |
| Tabs          | `shadcn/tabs`                        | Default, Active                                            | Underline variant for horizontal sections                                                      |
| Breadcrumb    | `shadcn/breadcrumb`                  | Default                                                    | ChevronRight separator                                                                         |
| Header        | Custom (extends Shadcn)              | Default                                                    | 64px height; logo + search + avatar dropdown                                                   |
| CommandMenu   | `shadcn/command`                     | Default                                                    | Cmd+K global search; Staff/Admin only                                                          |
| Card          | `shadcn/card`                        | Default, Interactive (hover shadow)                        | Uses `card` radius and shadow tokens                                                           |
| Table         | `shadcn/table`                       | Default                                                    | Transforms to stacked cards on mobile                                                          |
| DataTable     | Custom (extends Table)               | Sortable, Filterable, Paginated                            | Column header sort indicators; row-level actions                                               |
| Avatar        | `shadcn/avatar`                      | S (24px), M (32px), L (40px)                               | Initials fallback; uses `avatar` radius                                                        |
| Badge         | `shadcn/badge`                       | Default, Secondary, Destructive, Outline, Success, Warning | Custom success/warning variants added                                                          |
| Separator     | `shadcn/separator`                   | Horizontal, Vertical                                       |                                                                                                |
| Skeleton      | `shadcn/skeleton`                    | Rectangle, Circle, Text line                               | Uses `skeleton-pulse` animation                                                                |
| Progress      | `shadcn/progress`                    | Default                                                    | Primary color fill                                                                             |
| StepIndicator | Custom `[CUSTOM]`                    | Horizontal (4 steps)                                       | Pipeline: circles + connector lines; Completed (check), Active (primary ring), Pending (muted) |
| ChatBubble    | Custom `[CUSTOM]`                    | User (right, primary bg), AI (left, muted bg)              | Max-width 80%; rounded corners; timestamp below                                                |
| Timeline      | Custom `[CUSTOM]`                    | Default                                                    | Vertical timeline; circle markers; resolution history                                          |
| QRCode        | Custom `[CUSTOM]`                    | Default                                                    | Renders TOTP provisioning URI                                                                  |
| Dialog        | `shadcn/dialog`                      | Default                                                    | Uses `dialog` radius and shadow; max-width 425px                                               |
| AlertDialog   | `shadcn/alert-dialog`                | Default, Destructive                                       | Cancel/Continue buttons; destructive uses red CTA                                              |
| Drawer        | `shadcn/drawer`                      | Bottom                                                     | Mobile-only bottom sheet                                                                       |
| Sheet         | `shadcn/sheet`                       | Right, Bottom                                              | Used for MOD-004 on mobile                                                                     |
| Toast         | `shadcn/sonner`                      | Info, Success, Warning, Destructive                        | Bottom-right (desktop), bottom-center (mobile); 5s auto-dismiss; max stack 3                   |
| Alert         | `shadcn/alert`                       | Info, Success, Warning, Destructive                        | Icon + title + description                                                                     |
| Tooltip       | `shadcn/tooltip`                     | Default                                                    | 200ms delay; max-width 200px                                                                   |
| Popover       | `shadcn/popover`                     | Default                                                    | For DatePicker, filter dropdowns                                                               |
| Chart         | Custom (Recharts)                    | Bar, Line, Pie                                             | Accessible color palette from semantic tokens                                                  |
| Calendar      | Custom (extends Shadcn Calendar)     | Day view, Week view                                        | Staff schedule view; uses primary color for appointments                                       |
| EmptyState    | Custom `[CUSTOM]`                    | Default                                                    | Centered: Lucide icon (48px muted) + heading + description + CTA button                        |

---

## 7. New Visual Assets

### Icon Requirements

| Source   | Library | Subset Used                              |
| -------- | ------- | ---------------------------------------- |
| UI Icons | Lucide  | Outlined, 1.5px stroke, 16/20/24px sizes |

#### Required Lucide Icons

| Icon Name     | Size  | Usage                              |
| ------------- | ----- | ---------------------------------- |
| Shield        | 16/20 | Low risk tier indicator            |
| AlertCircle   | 16/20 | Medium risk tier, form errors      |
| AlertTriangle | 16/20 | High risk tier, conflict indicator |
| Loader2       | 16/20 | Loading spinner (animated rotate)  |
| Upload        | 24    | File upload dropzone               |
| Search        | 20    | Search inputs                      |
| Calendar      | 20    | Appointment date references        |
| Clock         | 16    | Time references                    |
| User          | 20    | Patient references                 |
| Users         | 20    | User management                    |
| ChevronRight  | 16    | Breadcrumb separator               |
| ChevronDown   | 16    | Select dropdown indicator          |
| Check         | 16    | Completed step, verified status    |
| X             | 16    | Close buttons, dismiss actions     |
| Plus          | 16    | Create/add actions                 |
| GripVertical  | 16    | Drag handle for queue reorder      |
| WifiOff       | 20    | Offline connectivity banner        |
| FileText      | 20    | Document references                |
| MessageCircle | 20    | AI chat intake                     |
| Settings      | 20    | Settings navigation                |
| LogOut        | 20    | Logout action                      |
| Bell          | 20    | Notification indicator             |
| Eye           | 16    | View action                        |
| Pencil        | 16    | Edit action                        |
| Trash2        | 16    | Delete action                      |
| ExternalLink  | 16    | External link indicator            |
| Copy          | 16    | Copy to clipboard                  |
| QrCode        | 24    | MFA TOTP setup                     |

### Typeface Assets

| Typeface          | Source       | Weights  | Usage                                        |
| ----------------- | ------------ | -------- | -------------------------------------------- |
| Plus Jakarta Sans | Google Fonts | 600, 700 | Display headings (h1–h4)                     |
| Inter             | Google Fonts | 400, 500 | Body text, labels, UI elements               |
| JetBrains Mono    | Google Fonts | 400      | ICD-10/CPT codes, OTP inputs, technical data |

---

## 8. Task Design Mapping

### Use Case to Design Element Mapping

| Use Case                    | Screens                   | Key Components                           | Design Considerations                               |
| --------------------------- | ------------------------- | ---------------------------------------- | --------------------------------------------------- |
| UC-001 Patient Registration | SCR-001, SCR-028, SCR-029 | Input, Button, Tabs, Alert               | Progressive disclosure, password strength indicator |
| UC-002 Staff MFA            | SCR-001, SCR-026, SCR-027 | OTPInput, Button, Card, QRCode           | Auto-submit on complete, resend code flow           |
| UC-003 Appointment Booking  | SCR-005, SCR-006          | DatePicker, Select, Card, Button, Dialog | Slot lock countdown, 409 conflict recovery          |
| UC-004 Preferred Slot       | SCR-005                   | Switch, Card, Badge                      | Toggle within booking flow                          |
| UC-005 Walk-in Booking      | SCR-018                   | Input, Card, Button, Select              | Patient search + create flow                        |
| UC-006 Queue Management     | SCR-017                   | Table, Badge, Button, DragHandle         | Drag reorder, arrival marking                       |
| UC-007 Calendar Sync        | SCR-013                   | Card, Switch, Button, Badge              | OAuth connection status                             |
| UC-008 Cancel/Reschedule    | SCR-007, SCR-008          | Button, AlertDialog, Card                | Destructive confirmation, slot picker               |
| UC-009 AI Intake            | SCR-009                   | ChatBubble, Input, Card, Badge, Tabs     | Typing indicator, confidence badges                 |
| UC-010 Manual Intake        | SCR-010                   | Input, Textarea, Select, Button, Tabs    | Inline validation, section collapsing               |
| UC-011 Document Upload      | SCR-011                   | FileUpload, Progress, Alert              | Drag-and-drop, per-file progress                    |
| UC-012 Document Processing  | SCR-012                   | Table, StepIndicator, Badge              | Status pipeline animation                           |
| UC-013 360-View & Conflicts | SCR-019, SCR-020          | Tabs, Card, Badge, Alert, Button         | Conflict indicators, resolution actions             |
| UC-014 Code Mapping         | SCR-021                   | Table, Badge, Button, Input              | Confidence indicators, accept/reject actions        |
| UC-016 Schedule View        | SCR-022                   | Calendar, Card, Badge, Tabs              | Day/week toggle, risk tier badges                   |
| UC-017 Insurance Check      | SCR-014                   | Input, Button, Alert                     | Soft validation, skip option                        |
| UC-018 User Management      | SCR-023, MOD-004          | DataTable, Dialog, Input, Select, Button | CRUD with last-admin protection                     |
| UC-019 Audit Logs           | SCR-024                   | DataTable, DatePicker, Select, Button    | Immutable, export, expandable rows                  |
| UC-020 Platform Metrics     | SCR-025                   | Card, Chart, DatePicker                  | Metric cards, trend visualization                   |

---

## 9. Visual Validation Criteria

### Contrast Validation

| Element                                        | Minimum Ratio | Standard                                                                |
| ---------------------------------------------- | ------------- | ----------------------------------------------------------------------- |
| Normal text (body, label, caption)             | 4.5:1         | WCAG 2.2 AA 1.4.3                                                       |
| Large text (h1–h3, ≥18px bold / ≥24px regular) | 3:1           | WCAG 2.2 AA 1.4.3                                                       |
| UI components (borders, icons, focus rings)    | 3:1           | WCAG 2.2 AA 1.4.11                                                      |
| Placeholder text against input background      | 4.5:1         | Using zinc-400 on white = 3.9:1 — use zinc-500 (#71717a) for compliance |

### Token Compliance

| Rule                                            | Validation                                      |
| ----------------------------------------------- | ----------------------------------------------- |
| No raw hex values outside primitive color table | All component styles reference semantic tokens  |
| No arbitrary spacing values                     | All padding/margin/gap use spacing scale tokens |
| No inline font definitions                      | All text uses typography style tokens           |
| No hard-coded shadow values                     | All shadows use shadow scale tokens             |

---

## 10. Implementation Scenarios

### Responsive Behavior

| Breakpoint            | Layout Changes                                                                                       |
| --------------------- | ---------------------------------------------------------------------------------------------------- |
| <768px (Mobile)       | Sidebar hidden → BottomNav visible; tables → stacked cards; Dialog → bottom Sheet; page padding 16px |
| 768–1023px (Tablet)   | Sidebar collapsed (icons only); tables remain; dialogs centered; page padding 24px                   |
| 1024–1439px (Desktop) | Sidebar expanded; full tables; dialogs centered; page padding 24px                                   |
| ≥1440px (Wide)        | Sidebar expanded; max-width 1280px content area; centered layout                                     |

### Loading State Patterns

| Pattern          | Components                         | Behavior                                                                |
| ---------------- | ---------------------------------- | ----------------------------------------------------------------------- |
| Skeleton Screen  | Skeleton (Rectangle, Text, Circle) | Replaces content areas during data fetch; uses skeleton-pulse animation |
| Button Loading   | Button (Loading variant)           | Loader2 spinner + "Processing..." text; button disabled                 |
| Inline Progress  | Progress bar                       | Per-file upload progress (0–100%)                                       |
| Countdown        | Custom SVG ring or Progress bar    | Slot lock timer (30s linear countdown)                                  |
| Typing Indicator | Custom (3 dots)                    | AI chat response generation; sequential bounce animation                |

### State Management Patterns

| State    | Visual Treatment                                               |
| -------- | -------------------------------------------------------------- |
| Default  | Standard component rendering with populated data               |
| Hover    | Subtle background shift (secondary bg) on interactive elements |
| Focus    | 2px primary-colored ring offset 2px from element               |
| Active   | Slightly darkened primary for pressed state                    |
| Disabled | 50% opacity; cursor not-allowed; no pointer events             |
| Error    | Destructive border + inline error text below element           |
| Selected | Primary muted background + primary border                      |

---

## 11. Accessibility Requirements

### WCAG 2.2 Level AA Compliance Matrix

| Criterion | Guideline              | Implementation                                                  |
| --------- | ---------------------- | --------------------------------------------------------------- |
| 1.3.1     | Info and Relationships | Semantic HTML (headings, lists, tables, form labels)            |
| 1.4.1     | Use of Color           | All color-coded indicators include icon + text label (UXR-206)  |
| 1.4.3     | Contrast (Minimum)     | 4.5:1 normal text, 3:1 large text (UXR-201)                     |
| 1.4.11    | Non-text Contrast      | 3:1 for UI components and graphical objects                     |
| 2.1.1     | Keyboard               | Full keyboard navigation for all interactive elements (UXR-202) |
| 2.1.2     | No Keyboard Trap       | No focus traps; Escape closes modals, Tab moves forward         |
| 2.4.3     | Focus Order            | Logical tab order following DOM order                           |
| 2.4.7     | Focus Visible          | 2px primary ring visible on all interactive elements (UXR-203)  |
| 2.5.8     | Target Size            | ≥44x44px touch targets on mobile (UXR-205)                      |
| 4.1.2     | Name, Role, Value      | ARIA labels on all complex widgets (UXR-204)                    |

### ARIA Implementation

| Widget                   | ARIA Pattern                                                            |
| ------------------------ | ----------------------------------------------------------------------- |
| Dialog / AlertDialog     | `role="dialog"`, `aria-modal="true"`, `aria-labelledby`                 |
| Toast Notifications      | `role="status"`, `aria-live="polite"`                                   |
| AI Chat Messages         | `aria-live="polite"` on message container                               |
| Tab Navigation           | `role="tablist"`, `role="tab"`, `role="tabpanel"`                       |
| Data Tables              | `role="table"`, sortable headers with `aria-sort`                       |
| Queue Drag Reorder       | `aria-grabbed`, `aria-dropeffect`; keyboard alternative (move up/down)  |
| OTP Input                | `aria-label="Verification code digit N"` per segment                    |
| Progress / StepIndicator | `role="progressbar"`, `aria-valuenow`, `aria-valuemin`, `aria-valuemax` |
| Skeleton Loaders         | `aria-hidden="true"` (screen readers skip)                              |

---

## 12. Design Review Checklist

### Token Compliance

| #   | Check                                                                        | Status |
| --- | ---------------------------------------------------------------------------- | ------ |
| 1   | All colors reference semantic tokens (no raw hex in component styles)        | ☐      |
| 2   | All typography uses defined style tokens (h1–h4, body, label, caption, code) | ☐      |
| 3   | All spacing uses scale tokens (no arbitrary pixel values)                    | ☐      |
| 4   | All border-radius uses radius tokens                                         | ☐      |
| 5   | All shadows use shadow scale tokens                                          | ☐      |
| 6   | All z-index values use z-index tokens                                        | ☐      |

### Component Compliance

| #   | Check                                                                                       | Status |
| --- | ------------------------------------------------------------------------------------------- | ------ |
| 1   | All interactive components have 6 states (Default, Hover, Focus, Active, Disabled, Loading) | ☐      |
| 2   | All custom components compose from Shadcn primitives                                        | ☐      |
| 3   | Component naming follows `C/<Category>/<Name>` convention                                   | ☐      |
| 4   | All form inputs have label, placeholder, error, and disabled states                         | ☐      |
| 5   | All buttons have loading state with Loader2 spinner                                         | ☐      |

### Accessibility Compliance

| #   | Check                                                                     | Status |
| --- | ------------------------------------------------------------------------- | ------ |
| 1   | Color contrast ratios verified (4.5:1 normal text, 3:1 large text and UI) | ☐      |
| 2   | All interactive elements keyboard-accessible                              | ☐      |
| 3   | Focus indicators visible (2px primary ring) on all interactive elements   | ☐      |
| 4   | ARIA roles and labels on all complex widgets                              | ☐      |
| 5   | Touch targets ≥44x44px on mobile viewports                                | ☐      |
| 6   | No information conveyed by color alone                                    | ☐      |
| 7   | Skip-to-content link present                                              | ☐      |

### Anti-Pattern Avoidance

| #   | Anti-Pattern                                                                           | Status |
| --- | -------------------------------------------------------------------------------------- | ------ |
| 1   | No `background-clip: text` usage                                                       | ☐      |
| 2   | No `backdrop-filter: blur` usage                                                       | ☐      |
| 3   | No thick side borders (>2px decorative borders)                                        | ☐      |
| 4   | No sole-typeface-Inter (3 typefaces defined: Plus Jakarta Sans, Inter, JetBrains Mono) | ☐      |
| 5   | No purple-to-blue gradient usage                                                       | ☐      |
| 6   | No raw hex values outside primitive color table                                        | ☐      |
| 7   | No "Lorem ipsum" or placeholder text in any component                                  | ☐      |
| 8   | No width/height transitions (use opacity/transform only)                               | ☐      |
