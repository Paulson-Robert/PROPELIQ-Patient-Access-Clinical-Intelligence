# Design Tokens Applied — HealthAccess

## Aesthetic Direction

**Style**: Utilitarian — calm competence, institutional trust, information-dense, scannable

**Personality Traits**: Professional, clinical, efficient, trustworthy

**Design Principles**:

1. Information density over decorative whitespace
2. Neutral surfaces with semantic color reserved for status/action
3. Consistent component patterns across all three portals
4. Monospace typography for clinical codes and system identifiers

## Token Sources

| Source             | File                                                 | Description                                                                |
| ------------------ | ---------------------------------------------------- | -------------------------------------------------------------------------- |
| Design System Spec | `.propel/context/docs/designsystem.md`               | Canonical token definitions — colors, typography, spacing, radius, shadows |
| Shared CSS         | `.propel/context/wireframes/Hi-Fi/shared-tokens.css` | Implementation of all tokens as CSS custom properties                      |
| Figma Spec         | `.propel/context/docs/figma_spec.md`                 | Screen-level UX requirements referencing tokens                            |

## Token Application by Screen

### Color Tokens

| Token                | CSS Variable             | Value   | Screens Applied                                                             |
| -------------------- | ------------------------ | ------- | --------------------------------------------------------------------------- |
| Primary              | `--primary`              | #1e40af | All — buttons, links, active states, sidebar active                         |
| Primary Foreground   | `--primary-foreground`   | #ffffff | Auth brand panel, primary buttons                                           |
| Primary Muted        | `--primary-muted`        | #dbeafe | AI-extracted badges (SCR-010), selected conflict values (SCR-020)           |
| Success              | `--success`              | #16a34a | Completed badges, insurance verified (SCR-014), arrived status              |
| Warning              | `--warning`              | #d97706 | Pending conflicts (SCR-020), walk-in status, session timeout ring (MOD-001) |
| Destructive          | `--destructive`          | #dc2626 | Cancel buttons, failed status, delete actions, error alerts                 |
| Background           | `--background`           | #ffffff | Page surfaces, card backgrounds                                             |
| Background Secondary | `--background-secondary` | #f4f4f5 | Table headers, form sections, summary blocks                                |
| Background Tertiary  | `--background-tertiary`  | #e4e4e7 | Queue position circles, provider logos                                      |
| Foreground           | `--foreground`           | #18181b | Primary text                                                                |
| Foreground Secondary | `--foreground-secondary` | #52525b | Secondary text                                                              |
| Foreground Muted     | `--foreground-muted`     | #a1a1aa | Placeholder text, captions                                                  |
| Border               | `--border`               | #e4e4e7 | Card borders, table rules, input borders                                    |

### Risk Tier Colors

| Tier   | CSS Class      | Color              | Screens                |
| ------ | -------------- | ------------------ | ---------------------- |
| Low    | `.risk-low`    | Green (#16a34a bg) | SCR-003, 017, 022      |
| Medium | `.risk-medium` | Amber (#d97706 bg) | SCR-003, 017, 022      |
| High   | `.risk-high`   | Red (#dc2626 bg)   | SCR-003, 017, 019, 022 |

### Typography Tokens

| Token        | CSS Variable     | Value                     | Usage                                                                     |
| ------------ | ---------------- | ------------------------- | ------------------------------------------------------------------------- |
| Display Font | `--font-display` | Plus Jakarta Sans 600/700 | Auth brand heading (SCR-001), page section titles                         |
| Body Font    | `--font-body`    | Inter 400/500             | All body text, labels, descriptions                                       |
| Mono Font    | `--font-mono`    | JetBrains Mono 400        | ICD-10/CPT codes (SCR-021), timestamps (SCR-024), insurance IDs (SCR-014) |
| Text XS      | `--text-xs`      | 12px                      | Captions, pipeline labels, timestamps                                     |
| Text SM      | `--text-sm`      | 14px                      | Body text, table cells, form labels                                       |
| Text Base    | `--text-base`    | 16px                      | Headings, stat labels                                                     |
| Text LG      | `--text-lg`      | 18px                      | Auth brand subtitle                                                       |
| Text XL      | `--text-xl`      | 20px                      | Auth form titles, conflict values (MOD-005)                               |
| Text 2XL     | `--text-2xl`     | 24px                      | Dashboard greeting, auth brand name                                       |
| Text 4XL     | `--text-4xl`     | 36px                      | Auth brand heading (SCR-001)                                              |

### Spacing Tokens

| Token        | Value | Usage                                                   |
| ------------ | ----- | ------------------------------------------------------- |
| `--space-1`  | 4px   | Inline gaps, badge padding                              |
| `--space-2`  | 8px   | Calendar cell padding, compact gaps                     |
| `--space-3`  | 12px  | Form group gaps, card inner padding, table cell padding |
| `--space-4`  | 16px  | Card padding, section spacing, accordion padding        |
| `--space-6`  | 24px  | Section margins, grid gaps                              |
| `--space-8`  | 32px  | Auth form padding, page sections                        |
| `--space-16` | 64px  | Auth brand panel padding                                |

### Radius Tokens

| Token           | Value  | Usage                                                       |
| --------------- | ------ | ----------------------------------------------------------- |
| `--radius-sm`   | 4px    | Calendar blocks, small badges                               |
| `--radius-md`   | 8px    | Buttons, inputs, cards, tables                              |
| `--radius-lg`   | 12px   | Large cards, sidebar sections, provider logos               |
| `--radius-full` | 9999px | Avatars, badge pills, queue position circles, switch tracks |

### Shadow Tokens

| Token         | Usage                     |
| ------------- | ------------------------- |
| `--shadow-sm` | Cards, dropdowns          |
| `--shadow-md` | Modals, floating elements |
| `--shadow-lg` | Toast notifications       |

### Animation Tokens

| Animation        | Usage                                          |
| ---------------- | ---------------------------------------------- |
| `skeleton-pulse` | Loading skeletons (shared-tokens.css)          |
| `spinner`        | Button loading state                           |
| `typing-dots`    | AI chat typing indicator (SCR-009)             |
| `toast-enter`    | Toast notification entry                       |
| `countdown-ring` | Slot lock (MOD-007), session timeout (MOD-001) |

## Drift Notes

| Item                        | Expected Token                   | Actual                             | Severity | Notes                                                                                                                  |
| --------------------------- | -------------------------------- | ---------------------------------- | -------- | ---------------------------------------------------------------------------------------------------------------------- |
| SCR-001 auth styles         | Shared token classes             | Inline `<style>` block             | Low      | Auth layout is unique — inline styles are acceptable for split-panel layout not reused elsewhere                       |
| SCR-022 calendar colors     | Shared risk-badge classes        | Inline `color-mix()`               | Low      | Calendar block colors use `color-mix(in srgb, ...)` for 15% opacity tints — acceptable progressive enhancement         |
| SCR-020 conflict highlights | Shared card class                | Custom `.conflict-card`            | Low      | Conflict-specific border and background states require dedicated styles beyond base card                               |
| All screens                 | `min-height: 44px` touch targets | Not explicitly audited per element | Info     | Touch target compliance relies on button padding from shared-tokens.css; individual overrides not verified per-element |

No high-severity drift detected. All wireframes consistently reference `shared-tokens.css` for design token values (412 `var(--*)` references across 36 files).
