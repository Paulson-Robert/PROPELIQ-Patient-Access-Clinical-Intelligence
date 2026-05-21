import { useEffect, useId, useRef } from 'react'
import { AlertTriangle } from 'lucide-react'
import { cn } from '../../lib/utils'
import type { ManagedUser } from '../../services/userManagementApi'

interface DeactivateDialogProps {
  user: ManagedUser | null
  open: boolean
  isSubmitting: boolean
  error: string | null
  /** True when the currently-authenticated user is the same as `user` */
  isSelf: boolean
  onConfirm: () => void
  onCancel: () => void
}

export const DeactivateDialog = ({
  user,
  open,
  isSubmitting,
  error,
  isSelf,
  onConfirm,
  onCancel,
}: DeactivateDialogProps) => {
  const dialogRef = useRef<HTMLDivElement>(null)
  const titleId = useId()

  // Focus trap + Escape key
  useEffect(() => {
    if (!open) return
    const dialog = dialogRef.current
    if (!dialog) return

    const focusable = dialog.querySelectorAll<HTMLElement>(
      'button, [tabindex]:not([tabindex="-1"])',
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

  if (!open || !user) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center" role="presentation">
      <div
        className="absolute inset-0 bg-black/50"
        aria-hidden="true"
        onClick={onCancel}
      />

      <div
        ref={dialogRef}
        role="alertdialog"
        aria-modal="true"
        aria-labelledby={titleId}
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
            <h2 id={titleId} className="text-base font-semibold text-foreground">
              {isSelf ? 'Cannot deactivate your own account' : `Deactivate "${user.fullName}"?`}
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              {isSelf
                ? 'Admins cannot deactivate their own account. Ask another admin to perform this action.'
                : 'The user will no longer be able to log in. Their data will be preserved and can be restored by reactivating the account.'}
            </p>
          </div>
        </div>

        {/* API error */}
        {error && (
          <p role="alert" className="mt-4 text-sm text-destructive">
            {error}
          </p>
        )}

        {/* Actions */}
        <div className={cn('mt-6 flex justify-end gap-2', isSelf && 'justify-end')}>
          <button
            type="button"
            onClick={onCancel}
            disabled={isSubmitting}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
          >
            {isSelf ? 'Close' : 'Cancel'}
          </button>
          {!isSelf && (
            <button
              type="button"
              onClick={onConfirm}
              disabled={isSubmitting}
              className="rounded-md bg-destructive px-4 py-2 text-sm font-medium text-destructive-foreground hover:bg-destructive/90 focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
            >
              {isSubmitting ? 'Deactivating…' : 'Deactivate'}
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
