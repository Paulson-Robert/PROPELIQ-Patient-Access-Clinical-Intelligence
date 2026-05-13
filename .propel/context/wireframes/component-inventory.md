# Component Inventory — HealthAccess

## Component Specification

- **Fidelity Level**: High
- **Screen Type**: Web (Responsive)
- **Viewport**: 1440 × 900 (primary), 768 px, 375 px

## Component Summary

| Component         | Type        | Screens Used                               | Priority | Status  |
| ----------------- | ----------- | ------------------------------------------ | -------- | ------- |
| App Shell         | Layout      | SCR-002–025                                | High     | Pending |
| Sidebar           | Navigation  | SCR-002–025 (role-specific)                | High     | Pending |
| Header Bar        | Layout      | SCR-002–025                                | High     | Pending |
| Bottom Nav        | Navigation  | SCR-002–025 (mobile)                       | High     | Pending |
| Auth Layout       | Layout      | SCR-001, 026–029                           | High     | Pending |
| Card              | Content     | SCR-002–025                                | High     | Pending |
| Stat Card         | Content     | SCR-003, 004, 025                          | High     | Pending |
| DataTable         | Content     | SCR-012, 016, 017, 023, 024                | High     | Pending |
| Button            | Interactive | All screens                                | High     | Pending |
| Form Input        | Interactive | SCR-001, 005, 010, 014, 015, 018, 023, 029 | High     | Pending |
| Form Select       | Interactive | SCR-005, 010, 016, 023, 024, 025           | High     | Pending |
| Badge             | Feedback    | SCR-003, 012, 016, 017, 019, 021, 023, 024 | High     | Pending |
| Risk Badge        | Feedback    | SCR-003, 017, 019, 022                     | High     | Pending |
| Alert             | Feedback    | SCR-003, 014                               | Medium   | Pending |
| Modal / Dialog    | Feedback    | MOD-001–007                                | High     | Pending |
| Tabs              | Navigation  | SCR-009, 010, 019, 022                     | Medium   | Pending |
| Pagination        | Navigation  | SCR-016, 023, 024                          | Medium   | Pending |
| Avatar            | Content     | All authenticated screens                  | Medium   | Pending |
| Progress Bar      | Feedback    | SCR-011                                    | Medium   | Pending |
| Step Indicator    | Feedback    | SCR-012                                    | Medium   | Pending |
| Dropzone          | Interactive | SCR-011                                    | Medium   | Pending |
| OTP Input         | Interactive | SCR-026                                    | Medium   | Pending |
| Switch / Toggle   | Interactive | SCR-013, 015                               | Medium   | Pending |
| Chart Placeholder | Content     | SCR-025                                    | Low      | Pending |
| QR Placeholder    | Content     | SCR-027                                    | Low      | Pending |
| Countdown Ring    | Feedback    | MOD-001, MOD-007                           | Medium   | Pending |
| Chat Bubble       | Content     | SCR-009                                    | Medium   | Pending |
| Accordion         | Layout      | SCR-010                                    | Medium   | Pending |
| Breadcrumb        | Navigation  | SCR-005                                    | Low      | Pending |
| Calendar Grid     | Content     | SCR-022                                    | Medium   | Pending |
| Conflict Card     | Content     | SCR-020                                    | Medium   | Pending |

## Detailed Component Specifications

### Layout Components

#### App Shell

- **Type**: Layout
- **Used In**: SCR-002 through SCR-025 (all authenticated screens)
- **Description**: Flex container with sidebar (240 px) + main content area. Skip link, ARIA landmarks.
- **Variants**: Patient shell (9 nav items), Staff shell (6 nav items), Admin shell (4 nav items)
- **Responsive**:
  - Desktop (1440 px): Sidebar visible, main content fluid
  - Tablet (768 px): Sidebar collapsed to icon-only rail
  - Mobile (375 px): Sidebar hidden, bottom nav appears

#### Auth Layout

- **Type**: Layout
- **Used In**: SCR-001, SCR-026, SCR-027, SCR-028, SCR-029
- **Description**: Two-panel split — brand panel (left, primary bg) + form panel (right, white bg)
- **Responsive**:
  - Desktop (1440 px): 50/50 split
  - Mobile (375 px): Brand panel hidden, form centered

#### Accordion

- **Type**: Layout
- **Used In**: SCR-010
- **Description**: Collapsible sections with header + body, chevron indicator
- **States**: Expanded (default), Collapsed

### Navigation Components

#### Sidebar

- **Type**: Navigation
- **Used In**: All authenticated screens
- **Description**: Persistent left nav with logo, nav items (icon + label), logout at bottom
- **Variants**: Patient (Dashboard, Appointments, Intake, Documents, History, Insurance, Calendar, Profile), Staff (Dashboard, Queue, Walk-in, Schedule, Patient lookup, Codes), Admin (Dashboard, Users, Audit, Metrics)
- **States**: Default, Active (`aria-current="page"`), Hover
- **Responsive**:
  - Desktop: Full sidebar with labels
  - Tablet: Icon-only rail
  - Mobile: Hidden (replaced by bottom nav)

#### Bottom Nav

- **Type**: Navigation
- **Used In**: All authenticated screens (mobile only)
- **Description**: Fixed bottom bar with 4 icon+label items per role
- **Responsive**: Visible only below 768 px

#### Tabs

- **Type**: Navigation
- **Used In**: SCR-009/010 (AI/Manual toggle), SCR-019 (Vitals/Meds/Allergies/Dx/Procedures), SCR-022 (Day/Week)
- **States**: Active (`aria-selected="true"`), Inactive

#### Pagination

- **Type**: Navigation
- **Used In**: SCR-016, SCR-023, SCR-024
- **Description**: Page number buttons with prev/next, "Showing X–Y of Z" label
- **States**: Active page, Disabled prev/next

#### Breadcrumb

- **Type**: Navigation
- **Used In**: SCR-005
- **Description**: Hierarchical path text links

### Content Components

#### Card

- **Type**: Content
- **Used In**: SCR-002–025 (widespread)
- **Description**: White surface with padding, border-radius, optional title and actions
- **Variants**: Default, With header (title + action link), Stat card

#### Stat Card

- **Type**: Content
- **Used In**: SCR-003, SCR-004, SCR-025
- **Description**: Label, large numeric value, optional trend indicator
- **Variants**: Default, With trend (up/down arrow + percentage)

#### DataTable

- **Type**: Content
- **Used In**: SCR-012, SCR-016, SCR-017, SCR-021, SCR-023, SCR-024
- **Description**: Bordered table with thead/tbody, sortable headers, row hover
- **Responsive**: Horizontal scroll on mobile

#### Avatar

- **Type**: Content
- **Used In**: All authenticated screens (header), SCR-003 queue rows
- **Description**: Circular element with initials, background color
- **Variants**: Default (32 px), Small (24 px), Large (56 px)

#### Chat Bubble

- **Type**: Content
- **Used In**: SCR-009
- **Description**: Rounded message container with tail, different alignment for AI vs user
- **Variants**: AI (left-aligned, secondary bg), User (right-aligned, primary bg), Typing indicator (3 dots animation)

#### Calendar Grid

- **Type**: Content
- **Used In**: SCR-022
- **Description**: CSS grid with time column + provider columns, appointment blocks
- **Variants**: Day view, Week view (toggle via tabs)

#### Conflict Card

- **Type**: Content
- **Used In**: SCR-020
- **Description**: Bordered card with two conflict value panels, accept/custom actions
- **Variants**: Active (amber border), Resolved (muted opacity)

### Interactive Components

#### Button

- **Type**: Interactive
- **Used In**: All screens
- **Description**: Action trigger with label, optional leading icon
- **Variants**: Primary, Secondary, Outline, Ghost, Destructive
- **States**: Default, Hover, Active, Focus (2 px ring), Disabled
- **Sizes**: Default (36 px), Small (28 px)
- **Min touch target**: 44 × 44 px

#### Form Input

- **Type**: Interactive
- **Used In**: SCR-001, 005, 010, 014, 015, 018, 023, 027, 029
- **Description**: Text input with label, placeholder, optional validation
- **Variants**: Text, Email, Password, Tel, Date, Search
- **States**: Default, Focus (primary ring), Error (destructive border), Disabled

#### Form Select

- **Type**: Interactive
- **Used In**: SCR-005, 010, 016, 023, 024, 025
- **Description**: Native select dropdown with label
- **States**: Default, Focus, Disabled

#### Dropzone

- **Type**: Interactive
- **Used In**: SCR-011
- **Description**: Dashed border area with upload icon, drag-and-drop support
- **States**: Default (dashed border), Drag-over (primary border), Error (destructive)

#### OTP Input

- **Type**: Interactive
- **Used In**: SCR-026
- **Description**: 6 individual single-character inputs in a row
- **States**: Empty, Filled, Focus (auto-advance)

#### Switch / Toggle

- **Type**: Interactive
- **Used In**: SCR-013, SCR-015
- **Description**: Binary on/off toggle with track and thumb
- **States**: Off (muted track), On (primary track)

### Feedback Components

#### Badge

- **Type**: Feedback
- **Used In**: SCR-003, 012, 016, 017, 021, 023, 024
- **Description**: Small inline status label
- **Variants**: Primary, Secondary, Success, Warning, Destructive

#### Risk Badge

- **Type**: Feedback
- **Used In**: SCR-003, 017, 019, 022
- **Description**: Risk tier indicator with icon
- **Variants**: Low (green shield), Medium (amber info), High (red triangle)

#### Alert

- **Type**: Feedback
- **Used In**: SCR-003, 014
- **Description**: Bordered message block with icon, title, description
- **Variants**: Info, Success, Warning, Destructive

#### Modal / Dialog

- **Type**: Feedback
- **Used In**: MOD-001–007
- **Description**: Centered overlay with backdrop, header, body, footer actions
- **Variants**: Dialog (dismissable), AlertDialog (non-dismissable backdrop)
- **Responsive**: Full-screen sheet on mobile
- **Focus**: Trapped within modal, returns to trigger on close

#### Progress Bar

- **Type**: Feedback
- **Used In**: SCR-011
- **Description**: Horizontal fill bar with percentage
- **States**: In-progress (animated), Complete

#### Step Indicator

- **Type**: Feedback
- **Used In**: SCR-012
- **Description**: Horizontal connected dots showing pipeline stage
- **States**: Completed (check), Active (numbered), Pending, Failed (X)

#### Countdown Ring

- **Type**: Feedback
- **Used In**: MOD-001, MOD-007
- **Description**: SVG circular progress with center text timer
- **States**: Active (animating), Expired

## Component States Matrix

| Component   | Default | Hover | Active | Focus | Disabled | Error | Loading | Empty |
| ----------- | :-----: | :---: | :----: | :---: | :------: | :---: | :-----: | :---: |
| Button      |    ✓    |   ✓   |   ✓    |   ✓   |    ✓     |   —   |    ✓    |   —   |
| Form Input  |    ✓    |   ✓   |   ✓    |   ✓   |    ✓     |   ✓   |    —    |   ✓   |
| Form Select |    ✓    |   ✓   |   ✓    |   ✓   |    ✓     |   —   |    —    |   ✓   |
| Card        |    ✓    |   —   |   —    |   —   |    —     |   —   |    ✓    |   ✓   |
| DataTable   |    ✓    |   ✓   |   —    |   —   |    —     |   —   |    ✓    |   ✓   |
| Modal       |    ✓    |   —   |   —    |   ✓   |    —     |   —   |    —    |   —   |
| Badge       |    ✓    |   —   |   —    |   —   |    —     |   —   |    —    |   —   |
| Dropzone    |    ✓    |   —   |   ✓    |   ✓   |    —     |   ✓   |    —    |   —   |
| Switch      |    ✓    |   ✓   |   ✓    |   ✓   |    ✓     |   —   |    —    |   —   |
| OTP Input   |    ✓    |   —   |   —    |   ✓   |    —     |   ✓   |    —    |   ✓   |

## Reusability Analysis

| Component  | Reuse Count | Screens                        | Recommendation          |
| ---------- | :---------: | ------------------------------ | ----------------------- |
| Button     |     36      | All                            | Shared component        |
| Card       |     28      | All authenticated              | Shared component        |
| Sidebar    |     25      | All authenticated              | Shared, 3 role variants |
| Avatar     |     25      | All authenticated              | Shared component        |
| Badge      |     12      | Queue, tables, status displays | Shared component        |
| Form Input |     11      | Auth + forms                   | Shared component        |
| DataTable  |      6      | List/log screens               | Shared component        |
| Modal      |      7      | All modal screens              | Shared component        |
| Alert      |      4      | Dashboard, forms               | Shared component        |
| Tabs       |      4      | Intake, patient view, schedule | Shared component        |
| Pagination |      3      | History, users, audit          | Shared component        |

## Responsive Breakpoints Summary

| Breakpoint | Width   | Components Affected                                      | Key Adaptations                                                                                                    |
| ---------- | ------- | -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| Mobile     | 375 px  | Sidebar, Bottom Nav, Modal, DataTable, Auth Layout, Grid | Sidebar hidden, bottom nav visible, modals → sheets, tables scroll horizontally, single column, brand panel hidden |
| Tablet     | 768 px  | Sidebar, Grid                                            | Sidebar collapsed to icons, 2-column grids where applicable                                                        |
| Desktop    | 1440 px | All                                                      | Full sidebar, multi-column layouts, expanded tables                                                                |

## Implementation Priority Matrix

### High Priority (Core Components)

- [ ] App Shell — Foundation for all authenticated screens
- [ ] Sidebar — Primary navigation for all roles
- [ ] Button — Used in every screen
- [ ] Card — Primary content container
- [ ] Form Input / Select — All form interactions
- [ ] Modal / Dialog — Required for booking, confirmation, user management

### Medium Priority (Feature Components)

- [ ] DataTable — Queue, history, user management, audit log
- [ ] Badge / Risk Badge — Status communication
- [ ] Tabs — Intake mode toggle, patient 360° sections
- [ ] Alert — Dashboard notifications, validation feedback
- [ ] Chat Bubble — AI intake interface
- [ ] Countdown Ring — Slot lock, session timeout

### Low Priority (Enhancement Components)

- [ ] Chart Placeholder — Metrics dashboard (replaced by real chart library)
- [ ] QR Placeholder — MFA setup (replaced by actual QR generation)
- [ ] Breadcrumb — Only used in booking flow
- [ ] Calendar Grid — Staff schedule (may use third-party calendar)

## Framework-Specific Notes

- **Framework**: React 18+ with Vite
- **Component Library**: Shadcn UI (Radix primitives + Tailwind CSS)
- **Icon Library**: Lucide React (outlined, 1.5 px stroke)

### Component Library Mappings

| Wireframe Component | Shadcn Component                          | Customization             |
| ------------------- | ----------------------------------------- | ------------------------- |
| Button              | `@shadcn/button`                          | Destructive variant added |
| Card                | `@shadcn/card`                            | Stat card variant         |
| Form Input          | `@shadcn/input`                           | Standard                  |
| Form Select         | `@shadcn/select`                          | Standard                  |
| Badge               | `@shadcn/badge`                           | Risk tier variants        |
| Modal / Dialog      | `@shadcn/dialog` + `@shadcn/alert-dialog` | Sheet on mobile           |
| Tabs                | `@shadcn/tabs`                            | Standard                  |
| Avatar              | `@shadcn/avatar`                          | Initials fallback         |
| Switch              | `@shadcn/switch`                          | Standard                  |
| Progress            | `@shadcn/progress`                        | Standard                  |
| Pagination          | Custom (Shadcn pattern)                   | Page numbers + prev/next  |
| DataTable           | `@shadcn/table` + `@tanstack/react-table` | Sortable, filterable      |
| Dropzone            | Custom (Radix + react-dropzone)           | Drag state feedback       |

## Accessibility Considerations

| Component | ARIA Attributes                                                                | Keyboard                        | Screen Reader              |
| --------- | ------------------------------------------------------------------------------ | ------------------------------- | -------------------------- |
| Sidebar   | `aria-label="Main navigation"`, `aria-current="page"`                          | Tab through items               | Announces current page     |
| Modal     | `role="dialog"` or `role="alertdialog"`, `aria-labelledby`, `aria-describedby` | Tab trap, ESC to close          | Title announced on open    |
| Tabs      | `role="tablist"`, `role="tab"`, `aria-selected`                                | Arrow keys switch tabs          | Tab label announced        |
| OTP Input | `aria-label="Digit N"`                                                         | Auto-advance on input           | Per-digit label            |
| DataTable | `aria-label` on table                                                          | Tab to rows                     | Row content announced      |
| Button    | Type + label                                                                   | Enter/Space to activate         | Label announced            |
| Dropzone  | `role="button"`, `aria-label`                                                  | Enter/Space to open file picker | Drop instruction announced |

## Design System Integration

### Components Matching Design System

All wireframe components use semantic CSS custom properties from `shared-tokens.css` that map directly to the HealthAccess design system documented in `designsystem.md`:

- [x] Color tokens (`--primary`, `--success`, `--warning`, `--destructive`)
- [x] Typography (`--font-display`, `--font-body`, `--font-mono`)
- [x] Spacing scale (`--space-1` through `--space-16`)
- [x] Border radius (`--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-full`)
- [x] Shadow system (`--shadow-sm`, `--shadow-md`, `--shadow-lg`)
