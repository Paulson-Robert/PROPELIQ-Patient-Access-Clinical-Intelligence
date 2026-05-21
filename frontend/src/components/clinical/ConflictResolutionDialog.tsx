import { useEffect, useId, useRef, useState } from 'react'
import { AlertTriangle } from 'lucide-react'
import { cn } from '../../lib/utils'

// Shared types exported for use by ConflictHighlight and consumers

export interface ConflictSource {
  source: string
  value: string
  recordedAt: string
}

// AC-03: Audit trail entry shape — captured on every resolution
export interface ConflictAuditEntry {
  fieldLabel: string
  resolvedAt: string
  resolvedBy: string
  acceptedSource: string
  acceptedValue: string
}

interface ConflictResolutionDialogProps {
  open: boolean
  fieldLabel: string
  /** All conflicting sources — edge case: supports 2+ sources on the same field */
  sources: ConflictSource[]
  /** Name of the staff member resolving the conflict */
  resolvedBy?: string
  onConfirm: (auditEntry: ConflictAuditEntry) => void
  onCancel: () => void
}

export const ConflictResolutionDialog = ({
  open,
  fieldLabel,
  sources,
  resolvedBy = 'Staff',
  onConfirm,
  onCancel,
}: ConflictResolutionDialogProps) => {
  const dialogRef = useRef<HTMLDivElement>(null)
  const customValueId = useId()
  const [selectedKey, setSelectedKey] = useState<string | null>(null)
  const [customValue, setCustomValue] = useState('')
  const [useCustom, setUseCustom] = useState(false)

  // Reset state each time the dialog is opened
  useEffect(() => {
    if (open) {
      setSelectedKey(null)
      setCustomValue('')
      setUseCustom(false)
    }
  }, [open])

  // Focus trap + Escape handling — matches DeleteConfirmDialog pattern
  useEffect(() => {
    if (!open) return

    const dialog = dialogRef.current
    if (!dialog) return

    const focusable = dialog.querySelectorAll<HTMLElement>(
      'button, input, [tabindex]:not([tabindex="-1"])',
    )
    focusable[0]?.focus()

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onCancel()
        return
      }
      if (e.key !== 'Tab' || focusable.length === 0) return
      const first = focusable[0]
      const last = focusable[focusable.length - 1]
      if (e.shiftKey && document.activeElement === first) {
        e.preventDefault()
        last.focus()
      } else if (!e.shiftKey && document.activeElement === last) {
        e.preventDefault()
        first.focus()
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [open, onCancel])

  if (!open) return null

  const canConfirm = useCustom ? customValue.trim().length > 0 : selectedKey !== null

  const handleConfirm = () => {
    const match = sources.find((s) => `${s.source}-${s.recordedAt}` === selectedKey)
    const acceptedSource = useCustom ? 'Custom' : (match?.source ?? '')
    const acceptedValue = useCustom ? customValue.trim() : (match?.value ?? '')

    // AC-03: Audit trail captured at the moment of confirmation
    const auditEntry: ConflictAuditEntry = {
      fieldLabel,
      resolvedAt: new Date().toISOString(),
      resolvedBy,
      acceptedSource,
      acceptedValue,
    }
    onConfirm(auditEntry)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center" role="presentation">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/50" aria-hidden="true" onClick={onCancel} />

      {/* Dialog panel — MOD-005 */}
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="conflict-dialog-title"
        aria-describedby="conflict-dialog-description"
        className="relative z-10 w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-xl"
        data-uxr="MOD-005"
      >
        {/* Header */}
        <div className="mb-4 flex items-start gap-3">
          <div
            className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-amber-500/10"
            aria-hidden="true"
          >
            <AlertTriangle className="h-5 w-5 text-amber-600" />
          </div>
          <div>
            <h2
              id="conflict-dialog-title"
              className="text-base font-semibold text-foreground"
            >
              Resolve conflict — {fieldLabel}
            </h2>
            <p
              id="conflict-dialog-description"
              className="mt-0.5 text-sm text-muted-foreground"
            >
              Select the preferred value to resolve this conflict.
            </p>
          </div>
        </div>

        {/* Source options — AC-02: selecting preferred value; edge case: 2+ sources rendered */}
        <fieldset className="mb-4 space-y-2">
          <legend className="sr-only">Select preferred value for {fieldLabel}</legend>

          {sources.map((src) => {
            const key = `${src.source}-${src.recordedAt}`
            const isSelected = !useCustom && selectedKey === key
            return (
              <label
                key={key}
                className={cn(
                  'flex cursor-pointer items-start gap-3 rounded-lg border p-3 transition-colors',
                  isSelected
                    ? 'border-primary bg-primary/5'
                    : 'border-border bg-card hover:bg-accent',
                )}
              >
                <input
                  type="radio"
                  name="conflict-source"
                  value={key}
                  checked={isSelected}
                  onChange={() => {
                    setSelectedKey(key)
                    setUseCustom(false)
                  }}
                  className="mt-0.5 accent-primary"
                  aria-label={`Accept value from ${src.source}: ${src.value}`}
                />
                <div className="min-w-0 flex-1">
                  <p className="text-xs text-muted-foreground">
                    {src.source} — {src.recordedAt}
                  </p>
                  <p className="mt-0.5 text-sm font-medium text-foreground">{src.value}</p>
                </div>
              </label>
            )
          })}

          {/* Custom value option */}
          <label
            className={cn(
              'flex cursor-pointer items-start gap-3 rounded-lg border p-3 transition-colors',
              useCustom ? 'border-primary bg-primary/5' : 'border-border bg-card hover:bg-accent',
            )}
          >
            <input
              type="radio"
              name="conflict-source"
              value="custom"
              checked={useCustom}
              onChange={() => {
                setUseCustom(true)
                setSelectedKey(null)
              }}
              className="mt-0.5 accent-primary"
              aria-label="Enter a custom value"
            />
            <div className="min-w-0 flex-1">
              <p className="text-xs text-muted-foreground">Custom value</p>
              {useCustom && (
                <input
                  id={customValueId}
                  type="text"
                  value={customValue}
                  onChange={(e) => setCustomValue(e.target.value)}
                  placeholder="Enter value…"
                  autoComplete="off"
                  className="mt-1 w-full rounded-md border border-input bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  aria-label={`Custom value for ${fieldLabel}`}
                />
              )}
            </div>
          </label>
        </fieldset>

        {/* Actions */}
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={onCancel}
            className="rounded-lg border border-border bg-transparent px-4 py-2 text-sm font-medium text-foreground hover:bg-accent focus:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handleConfirm}
            disabled={!canConfirm}
            aria-disabled={!canConfirm}
            className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 focus:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
          >
            Confirm resolution
          </button>
        </div>
      </div>
    </div>
  )
}
