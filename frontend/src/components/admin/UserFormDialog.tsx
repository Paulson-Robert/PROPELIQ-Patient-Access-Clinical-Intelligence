import { useEffect, useId, useRef, useState } from 'react'
import { cn } from '../../lib/utils'
import type { UserRole } from '../../services/authApi'
import type { ManagedUser } from '../../services/userManagementApi'

const ROLES: { value: UserRole; label: string }[] = [
  { value: 'patient', label: 'Patient' },
  { value: 'staff', label: 'Staff' },
  { value: 'admin', label: 'Admin' },
]

interface UserFormValues {
  fullName: string
  email: string
  role: UserRole
  password: string
}

interface UserFormDialogProps {
  /** undefined = create mode; defined = edit mode */
  user?: ManagedUser
  open: boolean
  isSubmitting: boolean
  error: string | null
  onSubmit: (values: UserFormValues) => void
  onCancel: () => void
}

const DEFAULT_FORM: UserFormValues = {
  fullName: '',
  email: '',
  role: 'patient',
  password: '',
}

export type { UserFormValues }

export const UserFormDialog = ({
  user,
  open,
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: UserFormDialogProps) => {
  const dialogRef = useRef<HTMLDivElement>(null)
  const titleId = useId()
  const fullNameId = useId()
  const emailId = useId()
  const roleId = useId()
  const passwordId = useId()

  const isEditMode = user !== undefined

  const [form, setForm] = useState<UserFormValues>(
    isEditMode
      ? { fullName: user.fullName, email: user.email, role: user.role, password: '' }
      : DEFAULT_FORM,
  )
  const [validationError, setValidationError] = useState<string | null>(null)

  // Reset form when dialog opens or user changes
  useEffect(() => {
    if (!open) return
    setValidationError(null)
    if (isEditMode && user) {
      setForm({ fullName: user.fullName, email: user.email, role: user.role, password: '' })
    } else {
      setForm(DEFAULT_FORM)
    }
  }, [open, user, isEditMode])

  // Focus trap + Escape key
  useEffect(() => {
    if (!open) return
    const dialog = dialogRef.current
    if (!dialog) return

    const focusable = dialog.querySelectorAll<HTMLElement>(
      'button, input, select, [tabindex]:not([tabindex="-1"])',
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

  const setField = <K extends keyof UserFormValues>(key: K, value: UserFormValues[K]) => {
    setForm((prev) => ({ ...prev, [key]: value }))
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setValidationError(null)

    if (!form.fullName.trim()) {
      setValidationError('Full name is required.')
      return
    }
    if (!form.email.trim() || !form.email.includes('@')) {
      setValidationError('A valid email address is required.')
      return
    }
    if (!isEditMode && form.password.length < 8) {
      setValidationError('Password must be at least 8 characters.')
      return
    }

    onSubmit(form)
  }

  const title = isEditMode ? 'Edit user' : 'Create user'
  const submitLabel = isEditMode ? 'Save changes' : 'Create user'

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center" role="presentation">
      <div
        className="absolute inset-0 bg-black/50"
        aria-hidden="true"
        onClick={onCancel}
      />

      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className="relative z-10 w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-xl"
      >
        <h2 id={titleId} className="text-lg font-semibold text-foreground">
          {title}
        </h2>

        <form onSubmit={handleSubmit} noValidate className="mt-4 flex flex-col gap-4">
          {/* Full name */}
          <div className="flex flex-col gap-1.5">
            <label htmlFor={fullNameId} className="text-sm font-medium text-foreground">
              Full name
            </label>
            <input
              id={fullNameId}
              type="text"
              autoComplete="name"
              value={form.fullName}
              onChange={(e) => setField('fullName', e.target.value)}
              disabled={isSubmitting}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
              placeholder="Jane Smith"
              aria-required="true"
            />
          </div>

          {/* Email */}
          <div className="flex flex-col gap-1.5">
            <label htmlFor={emailId} className="text-sm font-medium text-foreground">
              Email address
            </label>
            <input
              id={emailId}
              type="email"
              autoComplete="email"
              value={form.email}
              onChange={(e) => setField('email', e.target.value)}
              disabled={isSubmitting}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
              placeholder="jane@example.com"
              aria-required="true"
            />
          </div>

          {/* Role — AC-02: dropdown with Patient / Staff / Admin */}
          <div className="flex flex-col gap-1.5">
            <label htmlFor={roleId} className="text-sm font-medium text-foreground">
              Role
            </label>
            <select
              id={roleId}
              value={form.role}
              onChange={(e) => setField('role', e.target.value as UserRole)}
              disabled={isSubmitting}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
              aria-required="true"
            >
              {ROLES.map((r) => (
                <option key={r.value} value={r.value}>
                  {r.label}
                </option>
              ))}
            </select>
          </div>

          {/* Password — shown only in create mode */}
          {!isEditMode && (
            <div className="flex flex-col gap-1.5">
              <label htmlFor={passwordId} className="text-sm font-medium text-foreground">
                Password
              </label>
              <input
                id={passwordId}
                type="password"
                autoComplete="new-password"
                value={form.password}
                onChange={(e) => setField('password', e.target.value)}
                disabled={isSubmitting}
                className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
                placeholder="Min. 8 characters"
                aria-required="true"
              />
            </div>
          )}

          {/* Validation / API error */}
          {(validationError || error) && (
            <p role="alert" className="text-sm text-destructive">
              {validationError ?? error}
            </p>
          )}

          {/* Actions */}
          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={onCancel}
              disabled={isSubmitting}
              className={cn(
                'rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50',
              )}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className={cn(
                'rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50',
              )}
            >
              {isSubmitting ? 'Saving…' : submitLabel}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
