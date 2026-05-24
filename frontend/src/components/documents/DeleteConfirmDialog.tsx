import { useEffect, useId, useRef, useState } from 'react'
import { AlertTriangle, Loader2 } from 'lucide-react'
import { cn } from '../../lib/utils'

export type DeleteMode = 'single' | 'all'

interface DeleteConfirmDialogProps {
  open: boolean
  mode: DeleteMode
  /** Name of the document being deleted — required when mode is 'single'. */
  documentName?: string
  busy?: boolean
  errorMessage?: string | null
  onConfirm: () => void | Promise<void>
  onCancel: () => void
}

// AC-02: consequences shown for single-document deletion
const SINGLE_CONSEQUENCES = [
  'The document file will be permanently removed.',
  'All extracted data records from this document will be deleted.',
  'Related data conflict entries will be removed.',
  'Medical code mappings sourced from this document will be deleted.',
  'The PatientView will be re-aggregated from your remaining documents.',
]

// AC-03: consequences shown for delete-all
const ALL_CONSEQUENCES = [
  'All clinical documents will be permanently removed.',
  'All extracted data records will be deleted.',
  'All data conflicts and code mappings will be removed.',
  'Your entire PatientView aggregated health data will be deleted.',
  'This action is permanent and cannot be undone.',
]

// AC-03: user must type this phrase to unlock the "delete all" confirm button
const DELETE_ALL_KEYWORD = 'DELETE ALL'

export const DeleteConfirmDialog = ({
  open,
  mode,
  documentName,
  busy = false,
  errorMessage,
  onConfirm,
  onCancel,
}: DeleteConfirmDialogProps) => {
  const dialogRef = useRef<HTMLDivElement>(null)
  const confirmInputId = useId()
  const [confirmText, setConfirmText] = useState('')

  // Reset typed confirmation each time the dialog is opened
  useEffect(() => {
    if (open) setConfirmText('')
  }, [open])

  // Keyboard handling: Escape to close, Tab focus trap
  useEffect(() => {
    if (!open) return

    const dialog = dialogRef.current
    if (!dialog) return

    const focusable = dialog.querySelectorAll<HTMLElement>(
      'button, input, [tabindex]:not([tabindex="-1"])',
    )
    focusable[0]?.focus()

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && !busy) {
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
  }, [busy, open, onCancel])

  if (!open) return null

  const isAll = mode === 'all'
  const consequences = isAll ? ALL_CONSEQUENCES : SINGLE_CONSEQUENCES
  const canConfirm = isAll ? confirmText === DELETE_ALL_KEYWORD : true
  const title = isAll ? 'Delete all documents?' : `Delete "${documentName}"?`
  const description = isAll
    ? 'This will permanently delete all your clinical documents and associated data.'
    : 'This action cannot be undone.'

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center" role="presentation">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/50"
        aria-hidden="true"
        onClick={busy ? undefined : onCancel}
      />

      {/* Dialog panel */}
      <div
        ref={dialogRef}
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="delete-dialog-title"
        aria-describedby="delete-dialog-description"
        className="relative z-10 w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-xl"
      >
        {/* Header */}
        <div className="flex items-start gap-3">
          <div
            className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-destructive/10"
            aria-hidden="true"
          >
            <AlertTriangle className="h-5 w-5 text-destructive" />
          </div>
          <div>
            <h2 id="delete-dialog-title" className="text-base font-semibold text-foreground">
              {title}
            </h2>
            <p id="delete-dialog-description" className="mt-1 text-sm text-muted-foreground">
              {description}
            </p>
          </div>
        </div>

        {/* Consequences list — AC-02 / AC-03 */}
        <div className="mt-4 rounded-lg border border-destructive/20 bg-destructive/5 p-4">
          <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-destructive">
            What will be deleted
          </p>
          <ul className="space-y-1.5" aria-label="Deletion consequences">
            {consequences.map((item) => (
              <li key={item} className="flex items-start gap-2 text-sm text-foreground">
                <span
                  className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-destructive"
                  aria-hidden="true"
                />
                {item}
              </li>
            ))}
          </ul>
        </div>

        {/* AC-03: typed confirmation input for delete-all */}
        {isAll && (
          <div className="mt-4">
            <label htmlFor={confirmInputId} className="block text-sm font-medium text-foreground">
              Type <span className="font-mono font-bold">DELETE ALL</span> to confirm
            </label>
            <input
              id={confirmInputId}
              type="text"
              value={confirmText}
              onChange={(e) => setConfirmText(e.target.value)}
              className={cn(
                'mt-1.5 w-full rounded-md border bg-background px-3 py-2 text-sm font-mono text-foreground',
                'placeholder:text-muted-foreground',
                'focus:outline-none focus:ring-2 focus:ring-ring',
                confirmText.length > 0 && confirmText !== DELETE_ALL_KEYWORD
                  ? 'border-destructive'
                  : 'border-border',
              )}
              placeholder="DELETE ALL"
              autoComplete="off"
              spellCheck={false}
              aria-label="Type DELETE ALL to confirm deletion of all documents"
            />
          </div>
        )}

        {errorMessage ? (
          <p className="mt-4 rounded-md border border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive">
            {errorMessage}
          </p>
        ) : null}

        {/* Action buttons */}
        <div className="mt-6 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onCancel}
            disabled={busy}
            className="inline-flex items-center justify-center rounded-md border border-border bg-card px-4 py-2 text-sm font-medium text-foreground transition hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={canConfirm && !busy ? () => { void onConfirm() } : undefined}
            disabled={!canConfirm || busy}
            aria-disabled={!canConfirm || busy}
            className={cn(
              'inline-flex items-center justify-center gap-2 rounded-md px-4 py-2 text-sm font-medium text-white transition',
              'focus:outline-none focus:ring-2 focus:ring-ring',
              canConfirm && !busy
                ? 'bg-destructive hover:bg-destructive/90'
                : 'cursor-not-allowed bg-destructive/40',
            )}
          >
            {busy ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : null}
            {busy ? 'Deleting...' : isAll ? 'Delete all documents' : 'Delete document'}
          </button>
        </div>
      </div>
    </div>
  )
}
