import { useEffect, useRef, useState } from 'react'
import { Bell, Trash2 } from 'lucide-react'
import { cn } from '../../lib/utils'
import {
  useNotifications,
  type Notification,
  type NotificationVariant,
} from '../../hooks/useNotifications'

const BADGE_CAP = 99

const VARIANT_DOT_STYLES: Record<NotificationVariant, string> = {
  info: 'bg-primary',
  success: 'bg-emerald-500',
  warning: 'bg-amber-500',
  error: 'bg-destructive',
}

const formatTime = (timestamp: number): string =>
  new Date(timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })

interface HistoryItemProps {
  notification: Notification
}

const HistoryItem = ({ notification }: HistoryItemProps) => (
  <li className="flex items-start gap-3 px-4 py-3">
    <span
      className={cn(
        'mt-1.5 h-2 w-2 shrink-0 rounded-full',
        VARIANT_DOT_STYLES[notification.variant],
      )}
      aria-hidden="true"
    />
    <div className="min-w-0 flex-1">
      <span className="sr-only">Notification type: {notification.variant}. </span>
      <p className="text-sm font-medium leading-snug">{notification.title}</p>
      {notification.message && (
        <p className="mt-0.5 text-xs text-muted-foreground">{notification.message}</p>
      )}
      <time
        className="mt-1 block text-xs text-muted-foreground"
        dateTime={new Date(notification.createdAt).toISOString()}
      >
        {formatTime(notification.createdAt)}
      </time>
    </div>
  </li>
)

export const NotificationHistory = () => {
  const [open, setOpen] = useState(false)
  const { history, clearHistory } = useNotifications()
  const lastCountRef = useRef(history.length)
  const [announcement, setAnnouncement] = useState('')

  useEffect(() => {
    if (history.length === lastCountRef.current) return

    if (history.length === 0 && lastCountRef.current > 0) {
      setAnnouncement('Notification history cleared.')
    } else if (history.length > lastCountRef.current) {
      const delta = history.length - lastCountRef.current
      setAnnouncement(`${delta} new notification${delta > 1 ? 's' : ''} added. ${history.length} total.`)
    } else {
      setAnnouncement(`Notification count updated. ${history.length} total.`)
    }

    lastCountRef.current = history.length
  }, [history.length])

  const badgeCount = history.length > BADGE_CAP ? `${BADGE_CAP}+` : String(history.length)
  const ariaLabel = history.length > 0
    ? `Notification history, ${history.length} item${history.length > 1 ? 's' : ''}`
    : 'Notification history, no items'

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setOpen((prev) => !prev)}
        aria-label={ariaLabel}
        aria-expanded={open}
        aria-haspopup="dialog"
        className="relative rounded-md p-2 hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
      >
        <Bell className="h-5 w-5" aria-hidden="true" />
        {history.length > 0 && (
          <span
            className="absolute right-1 top-1 flex h-4 min-w-[1rem] items-center justify-center rounded-full bg-destructive px-0.5 text-[10px] font-medium text-primary-foreground"
            aria-hidden="true"
          >
            {badgeCount}
          </span>
        )}
      </button>

      {open && (
        <>
          {/* Backdrop — closes panel on outside click */}
          <div
            className="fixed inset-0 z-40"
            onClick={() => setOpen(false)}
            aria-hidden="true"
          />
          <div
            role="dialog"
            aria-labelledby="notification-history-title"
            aria-modal="true"
            className="absolute right-0 top-full z-50 mt-2 w-80 rounded-lg border border-border bg-card shadow-lg"
          >
            <header className="flex items-center justify-between border-b border-border px-4 py-3">
              <h2 id="notification-history-title" className="text-sm font-semibold">Notification History</h2>
              {history.length > 0 && (
                <button
                  type="button"
                  onClick={clearHistory}
                  aria-label="Clear all notifications from history"
                  className="flex items-center gap-1 rounded p-1 text-xs text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <Trash2 className="h-3.5 w-3.5" aria-hidden="true" />
                  Clear all
                </button>
              )}
            </header>

            {history.length === 0 ? (
              <p className="px-4 py-8 text-center text-sm text-muted-foreground">
                No notifications yet
              </p>
            ) : (
              <ul
                className="max-h-80 divide-y divide-border overflow-y-auto"
                aria-label="Notification history items"
              >
                {history.map((notification) => (
                  <HistoryItem key={notification.id} notification={notification} />
                ))}
              </ul>
            )}
          </div>
        </>
      )}

      <p aria-live="polite" className="sr-only">
        {announcement}
      </p>
    </div>
  )
}
