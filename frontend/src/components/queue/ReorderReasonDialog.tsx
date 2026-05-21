import { useRef, type FormEvent } from 'react'

interface ReorderReasonDialogProps {
  open: boolean
  patientName: string
  isSubmitting: boolean
  errorMessage: string | null
  onConfirm: (reason: string) => void
  onCancel: () => void
}

export const ReorderReasonDialog = ({
  open,
  patientName,
  isSubmitting,
  errorMessage,
  onConfirm,
  onCancel,
}: ReorderReasonDialogProps) => {
  const reasonRef = useRef<HTMLTextAreaElement>(null)

  if (!open) return null

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const reason = reasonRef.current?.value.trim() ?? ''
    if (!reason) return
    onConfirm(reason)
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
      role="presentation"
    >
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="reorder-dialog-title"
        aria-describedby="reorder-dialog-description"
        className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-xl"
      >
        <h2 id="reorder-dialog-title" className="text-lg font-semibold">
          Reorder queue entry
        </h2>

        <p id="reorder-dialog-description" className="mt-2 text-sm text-muted-foreground">
          You moved <span className="font-medium text-foreground">{patientName}</span> to a new
          position. Please provide a brief reason for this change.
        </p>

        <form onSubmit={handleSubmit} className="mt-4 flex flex-col gap-3" noValidate>
          <label htmlFor="reorder-reason" className="sr-only">
            Reason for reordering
          </label>
          <textarea
            id="reorder-reason"
            ref={reasonRef}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
            placeholder="e.g. Patient has mobility limitations and needs earlier slot"
            rows={3}
            required
            aria-required="true"
            autoFocus
            disabled={isSubmitting}
          />

          {errorMessage && (
            <p role="alert" className="text-sm text-destructive">
              {errorMessage}
            </p>
          )}

          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={onCancel}
              disabled={isSubmitting}
              className="rounded-md px-4 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-50"
            >
              {isSubmitting ? 'Saving…' : 'Confirm reorder'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
