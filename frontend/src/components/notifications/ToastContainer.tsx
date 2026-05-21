import { useEffect, type ComponentType } from 'react'
import { X, CheckCircle, Info, AlertTriangle, XCircle } from 'lucide-react'
import { cn } from '../../lib/utils'
import {
  useNotifications,
  type Notification,
  type NotificationVariant,
} from '../../hooks/useNotifications'

/** Maximum toasts displayed simultaneously; excess shown as count badge. */
const STACK_LIMIT = 5

/** Milliseconds before a toast auto-dismisses. */
const AUTO_DISMISS_MS = 5000

const VARIANT_STYLES: Record<NotificationVariant, string> = {
  info: 'border-border bg-card text-foreground',
  success: 'border-emerald-500/30 bg-emerald-500/10 text-emerald-700',
  warning: 'border-amber-500/30 bg-amber-500/10 text-amber-700',
  error: 'border-destructive/30 bg-destructive/10 text-destructive',
}

const VARIANT_ICONS: Record<NotificationVariant, ComponentType<{ className?: string; 'aria-hidden'?: boolean | 'false' | 'true' }>> = {
  info: Info,
  success: CheckCircle,
  warning: AlertTriangle,
  error: XCircle,
}

interface ToastItemProps {
  notification: Notification
  onDismiss: (id: string) => void
}

const ToastItem = ({ notification, onDismiss }: ToastItemProps) => {
  const Icon = VARIANT_ICONS[notification.variant]

  useEffect(() => {
    const timer = setTimeout(() => onDismiss(notification.id), AUTO_DISMISS_MS)
    return () => clearTimeout(timer)
  }, [notification.id, onDismiss])

  return (
    <li
      role="alert"
      aria-live="assertive"
      aria-atomic="true"
      className={cn(
        'flex w-full items-start gap-3 rounded-lg border p-4 shadow-md',
        VARIANT_STYLES[notification.variant],
      )}
    >
      <Icon className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
      <div className="min-w-0 flex-1">
        <p className="text-sm font-medium leading-snug">{notification.title}</p>
        {notification.message && (
          <p className="mt-0.5 text-xs opacity-80">{notification.message}</p>
        )}
      </div>
      <button
        type="button"
        onClick={() => onDismiss(notification.id)}
        aria-label={`Dismiss: ${notification.title}`}
        className="shrink-0 rounded p-0.5 opacity-60 transition-opacity hover:opacity-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
      >
        <X className="h-3.5 w-3.5" aria-hidden="true" />
      </button>
    </li>
  )
}

export const ToastContainer = () => {
  const { active, dismiss } = useNotifications()

  if (active.length === 0) return null

  const visible = active.slice(-STACK_LIMIT)
  const overflowCount = active.length - STACK_LIMIT

  return (
    <div
      aria-label="Notifications"
      className="pointer-events-none fixed right-4 top-16 z-50 flex w-80 flex-col gap-2"
    >
      {overflowCount > 0 && (
        <p
          className="pointer-events-auto text-right text-xs text-muted-foreground"
          aria-live="polite"
        >
          +{overflowCount} more notification{overflowCount > 1 ? 's' : ''}
        </p>
      )}
      <ul className="pointer-events-auto flex flex-col gap-2" aria-label="Active notifications">
        {visible.map((notification) => (
          <ToastItem
            key={notification.id}
            notification={notification}
            onDismiss={dismiss}
          />
        ))}
      </ul>
    </div>
  )
}
