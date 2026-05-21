export interface ExtractedField {
  label: string
  value: string | null
  confidence: 'high' | 'medium' | 'low' | 'pending'
}

interface IntakeSummaryProps {
  fields: ExtractedField[]
  isSubmitting: boolean
  onConfirm: () => void
  onBack: () => void
}

const CONFIDENCE_STYLES: Record<ExtractedField['confidence'], string> = {
  high: 'text-xs font-medium text-green-700 bg-green-50 border border-green-200 rounded px-1.5 py-0.5',
  medium: 'text-xs font-medium text-amber-700 bg-amber-50 border border-amber-200 rounded px-1.5 py-0.5',
  low: 'text-xs font-medium text-destructive bg-red-50 border border-red-200 rounded px-1.5 py-0.5',
  pending: 'text-xs font-medium text-muted-foreground bg-muted border border-border rounded px-1.5 py-0.5',
}

const CONFIDENCE_LABELS: Record<ExtractedField['confidence'], string> = {
  high: 'High confidence',
  medium: 'Medium confidence',
  low: 'Low confidence',
  pending: 'Pending',
}

export const IntakeSummary = ({ fields, isSubmitting, onConfirm, onBack }: IntakeSummaryProps) => (
  <main
    className="flex flex-col gap-6 p-6 max-w-xl mx-auto"
    aria-labelledby="summary-heading"
    data-uxr="SCR-009"
  >
    <div>
      <h2 id="summary-heading" className="text-lg font-semibold text-foreground">
        Review your intake information
      </h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Please confirm the information extracted from your conversation before submitting.
      </p>
    </div>

    <ul className="flex flex-col gap-3" aria-label="Extracted intake data">
      {fields.map((field) => (
        <li
          key={field.label}
          className="rounded-lg border border-border bg-card p-4 shadow-sm"
        >
          <div className="mb-1 text-xs text-muted-foreground">{field.label}</div>
          <div className="text-sm font-medium text-foreground">
            {field.value ?? <span className="italic text-muted-foreground">Not captured</span>}
          </div>
          <div className="mt-2">
            <span className={CONFIDENCE_STYLES[field.confidence]}>
              {CONFIDENCE_LABELS[field.confidence]}
            </span>
          </div>
        </li>
      ))}
    </ul>

    <div className="flex gap-3">
      <button
        type="button"
        className="flex-1 rounded-lg border border-border bg-background px-4 py-2.5 text-sm font-medium text-foreground transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
        onClick={onBack}
        disabled={isSubmitting}
      >
        Back
      </button>
      <button
        type="button"
        className="flex-1 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
        onClick={onConfirm}
        disabled={isSubmitting}
        aria-busy={isSubmitting}
      >
        {isSubmitting ? 'Submitting…' : 'Submit intake'}
      </button>
    </div>
  </main>
)
