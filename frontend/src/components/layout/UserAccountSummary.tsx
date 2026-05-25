import { cn } from '../../lib/utils'
import type { AuthUser, UserRole } from '../../services/authApi'

const ROLE_LABELS: Record<UserRole, string> = {
  patient: 'Patient',
  staff: 'Staff',
  admin: 'Admin',
}

const ROLE_BADGE_CLASSES: Record<UserRole, string> = {
  patient: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  staff: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
  admin: 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400',
}

interface UserAccountSummaryProps {
  user: AuthUser | null | undefined
  className?: string
}

const fallbackNameFromEmail = (email: string): string => {
  const localPart = email.split('@')[0] ?? ''
  const words = localPart
    .replace(/[._-]+/g, ' ')
    .split(' ')
    .map((word) => word.trim())
    .filter(Boolean)

  if (words.length === 0) {
    return 'Signed in user'
  }

  return words
    .map((word) =>
      word.length === 1
        ? word.toUpperCase()
        : `${word.charAt(0).toUpperCase()}${word.slice(1).toLowerCase()}`,
    )
    .join(' ')
}

const getDisplayName = (user: AuthUser): string => {
  const fullName = user.fullName?.trim()
  return fullName ? fullName : fallbackNameFromEmail(user.email)
}

const getInitials = (displayName: string, email: string): string => {
  const source = displayName === 'Signed in user' ? email.split('@')[0] ?? '' : displayName
  const parts = source
    .replace(/[._-]+/g, ' ')
    .split(' ')
    .map((part) => part.trim())
    .filter(Boolean)

  if (parts.length === 0) {
    return 'U'
  }

  if (parts.length === 1) {
    return parts[0].slice(0, 2).toUpperCase()
  }

  return `${parts[0].charAt(0)}${parts[parts.length - 1].charAt(0)}`.toUpperCase()
}

export const UserAccountSummary = ({ user, className }: UserAccountSummaryProps) => {
  if (!user) {
    return null
  }

  const displayName = getDisplayName(user)
  const initials = getInitials(displayName, user.email)
  const roleLabel = ROLE_LABELS[user.role]

  return (
    <div
      className={cn('rounded-md border border-border bg-muted/40 p-3', className)}
      aria-label={`Signed in as ${displayName}, ${roleLabel}`}
    >
      <div className="flex min-w-0 items-start gap-3">
        <div
          className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-semibold text-primary-foreground"
          aria-hidden="true"
        >
          {initials}
        </div>
        <div className="min-w-0 flex-1">
          <div className="flex min-w-0 items-center gap-2">
            <p className="truncate text-sm font-semibold text-foreground">{displayName}</p>
            <span
              className={cn(
                'shrink-0 rounded-full px-2 py-0.5 text-[11px] font-medium leading-4',
                ROLE_BADGE_CLASSES[user.role],
              )}
            >
              {roleLabel}
            </span>
          </div>
          <p className="mt-0.5 truncate text-xs text-muted-foreground">{user.email}</p>
        </div>
      </div>
    </div>
  )
}
