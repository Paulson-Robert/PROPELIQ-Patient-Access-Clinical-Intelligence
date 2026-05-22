import { useCallback, useState } from 'react'
import { AlertTriangle, RefreshCw, X } from 'lucide-react'
import { cn } from '../../lib/utils'

type ErrorSeverity = 'critical' | 'warning' | 'info'

export interface NetworkError {
  id: string
  message: string
  severity: ErrorSeverity
  retryable: boolean
}

interface ErrorBannerProps {
  errors: NetworkError[]
  onRetry?: (errorId: string) => void
  onDismiss?: (errorId: string) => void
  className?: string
}

const severityOrder: Record<ErrorSeverity, number> = {
  critical: 0,
  warning: 1,
  info: 2,
}

const severityStyles: Record<ErrorSeverity, string> = {
  critical:
    'border-destructive bg-destructive-muted text-destructive',
  warning:
    'border-warning bg-warning-muted text-warning',
  info:
    'border-border bg-background-secondary text-foreground-secondary',
}

export function ErrorBanner({
  errors,
  onRetry,
  onDismiss,
  className,
}: ErrorBannerProps) {
  const [retryingIds, setRetryingIds] = useState<Set<string>>(new Set())

  const handleRetry = useCallback(
    (errorId: string) => {
      if (!onRetry) return
      setRetryingIds((prev) => new Set(prev).add(errorId))
      onRetry(errorId)
      // Allow parent to remove the error; clean up local state after delay
      setTimeout(() => {
        setRetryingIds((prev) => {
          const next = new Set(prev)
          next.delete(errorId)
          return next
        })
      }, 3000)
    },
    [onRetry],
  )

  if (errors.length === 0) return null

  // Prioritize errors: critical > warning > info
  const sorted = [...errors].sort(
    (a, b) => severityOrder[a.severity] - severityOrder[b.severity],
  )

  return (
    <div
      role="status"
      aria-live="polite"
      aria-atomic="true"
      className={cn('flex flex-col gap-2', className)}
    >
      {sorted.map((error) => {
        const isRetrying = retryingIds.has(error.id)

        return (
          <div
            key={error.id}
            className={cn(
              'flex items-center gap-3 rounded-lg border px-4 py-3',
              severityStyles[error.severity],
            )}
          >
            <AlertTriangle className="h-5 w-5 shrink-0" aria-hidden="true" />

            <p className="flex-1 text-sm font-medium">{error.message}</p>

            <div className="flex items-center gap-2">
              {error.retryable && onRetry && (
                <button
                  type="button"
                  onClick={() => handleRetry(error.id)}
                  disabled={isRetrying}
                  aria-label={`Retry: ${error.message}`}
                  className="inline-flex items-center gap-1.5 rounded-md border border-current/20 px-3 py-1.5 text-xs font-medium transition hover:opacity-80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <RefreshCw
                    className={cn('h-3.5 w-3.5', isRetrying && 'animate-spin')}
                    aria-hidden="true"
                  />
                  {isRetrying ? 'Retrying…' : 'Retry'}
                </button>
              )}

              {onDismiss && (
                <button
                  type="button"
                  onClick={() => onDismiss(error.id)}
                  aria-label={`Dismiss: ${error.message}`}
                  className="rounded-md p-1 transition hover:opacity-70 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                >
                  <X className="h-4 w-4" aria-hidden="true" />
                </button>
              )}
            </div>
          </div>
        )
      })}
    </div>
  )
}
