import { useState } from 'react'
import { AlertTriangle, CheckCircle2 } from 'lucide-react'
import { cn } from '../../lib/utils'
import {
  ConflictResolutionDialog,
  type ConflictAuditEntry,
  type ConflictSource,
} from './ConflictResolutionDialog'

// Re-export shared types so consumers can import from a single entry point
export type { ConflictAuditEntry, ConflictSource }

type ConflictStatus = 'pending' | 'resolved'

interface ConflictHighlightProps {
  fieldLabel: string
  /** All conflicting sources — edge case: two or more sources on the same field */
  sources: ConflictSource[]
  status?: ConflictStatus
  /** Human-readable summary shown after resolution (e.g. "Resolved by … on … — accepted value: …") */
  resolvedSummary?: string
  /** Called with the audit entry when a value is accepted via inline button or the resolution dialog */
  onResolve?: (auditEntry: ConflictAuditEntry) => void
  /** Name of the staff member performing the resolution — forwarded to the dialog */
  resolvedBy?: string
  className?: string
}

export const ConflictHighlight = ({
  fieldLabel,
  sources,
  status = 'pending',
  resolvedSummary,
  onResolve,
  resolvedBy = 'Staff',
  className,
}: ConflictHighlightProps) => {
  const [dialogOpen, setDialogOpen] = useState(false)
  const isPending = status === 'pending'

  // AC-02: inline quick-accept builds and emits the audit entry immediately
  const handleAccept = (src: ConflictSource) => {
    const auditEntry: ConflictAuditEntry = {
      fieldLabel,
      resolvedAt: new Date().toISOString(),
      resolvedBy,
      acceptedSource: src.source,
      acceptedValue: src.value,
    }
    onResolve?.(auditEntry)
  }

  return (
    <>
      {/* AC-01: amber border + background when pending */}
      <div
        className={cn(
          'rounded-xl border p-4',
          isPending
            ? 'border-amber-500 bg-amber-500/5'
            : 'border-border bg-card opacity-70',
          className,
        )}
        data-conflict-status={status}
        data-uxr="SCR-020"
      >
        {/* Header row */}
        <div className="mb-3 flex items-center justify-between">
          <div className="flex items-center gap-2">
            {isPending && (
              <AlertTriangle
                className="h-4 w-4 text-amber-600"
                aria-hidden="true"
              />
            )}
            <span className="text-sm font-medium text-foreground">{fieldLabel}</span>
            <span
              className={cn(
                'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium',
                isPending
                  ? 'bg-amber-500/10 text-amber-700'
                  : 'bg-emerald-500/10 text-emerald-700',
              )}
              aria-label={
                isPending ? 'Conflict pending resolution' : 'Conflict resolved'
              }
            >
              {isPending ? (
                <>
                  <AlertTriangle className="h-3 w-3" aria-hidden="true" />
                  Pending
                </>
              ) : (
                <>
                  <CheckCircle2 className="h-3 w-3" aria-hidden="true" />
                  Resolved
                </>
              )}
            </span>
          </div>

          {isPending && (
            <button
              type="button"
              onClick={() => setDialogOpen(true)}
              className="rounded px-2 py-1 text-xs text-muted-foreground hover:bg-accent hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              aria-label={`Open details for ${fieldLabel} conflict`}
            >
              Details
            </button>
          )}
        </div>

        {isPending ? (
          <>
            {/*
             * AC-02: source grid — edge case: grid-cols-2 wraps naturally for 3+ sources
             * so all sources on the same field are always shown.
             */}
            <div
              className="grid grid-cols-2 gap-3"
              role="group"
              aria-label={`Conflicting values for ${fieldLabel}`}
            >
              {sources.map((src) => (
                <div
                  key={`${src.source}-${src.recordedAt}`}
                  className="rounded-lg border border-border bg-card p-3"
                >
                  <p className="mb-0.5 text-xs text-muted-foreground">
                    {src.source} — {src.recordedAt}
                  </p>
                  <p className="mb-2 text-sm font-medium text-foreground">{src.value}</p>
                  <button
                    type="button"
                    onClick={() => handleAccept(src)}
                    className="w-full rounded-lg border border-border bg-transparent py-1 text-xs font-medium text-foreground hover:bg-accent focus:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                    aria-label={`Accept value from ${src.source}: ${src.value}`}
                  >
                    Accept {src.source}
                  </button>
                </div>
              ))}
            </div>

            {/* Custom value — opens the full resolution dialog */}
            <button
              type="button"
              onClick={() => setDialogOpen(true)}
              className="mt-3 w-full rounded-lg py-1.5 text-xs text-muted-foreground hover:bg-accent hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              aria-label={`Enter custom value for ${fieldLabel}`}
            >
              Enter custom value
            </button>
          </>
        ) : (
          /* Resolved state — show audit summary when provided */
          resolvedSummary && (
            <p className="text-sm text-muted-foreground">{resolvedSummary}</p>
          )
        )}
      </div>

      {/* MOD-005 — full resolution dialog */}
      <ConflictResolutionDialog
        open={dialogOpen}
        fieldLabel={fieldLabel}
        sources={sources}
        resolvedBy={resolvedBy}
        onConfirm={(auditEntry) => {
          setDialogOpen(false)
          onResolve?.(auditEntry)
        }}
        onCancel={() => setDialogOpen(false)}
      />
    </>
  )
}
