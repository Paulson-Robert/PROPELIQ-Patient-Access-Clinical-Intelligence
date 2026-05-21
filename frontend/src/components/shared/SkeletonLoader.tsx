import { RefreshCw } from 'lucide-react'
import { cn } from '../../lib/utils'

type SkeletonVariant = 'text' | 'avatar' | 'card' | 'button' | 'custom'

interface SkeletonLoaderProps {
  variant?: SkeletonVariant
  className?: string
  timedOut?: boolean
  onRetry?: () => void
}

const variantClasses: Record<SkeletonVariant, string> = {
  text: 'h-4 w-full rounded',
  avatar: 'h-10 w-10 rounded-full',
  card: 'h-24 w-full rounded-xl',
  button: 'h-10 w-32 rounded-md',
  custom: '',
}

export function SkeletonLoader({
  variant = 'text',
  className,
  timedOut = false,
  onRetry,
}: SkeletonLoaderProps) {
  if (timedOut) {
    return (
      <div
        role="status"
        aria-live="polite"
        className={cn(
          'flex flex-col items-center justify-center gap-3 rounded-xl border border-border bg-background-secondary p-6 text-center',
          className,
        )}
      >
        <p className="text-sm text-foreground-secondary">
          Content took too long to load.
        </p>
        {onRetry && (
          <button
            type="button"
            onClick={onRetry}
            className="inline-flex items-center gap-2 rounded-md border border-border bg-background px-3 py-2 text-sm font-medium text-foreground transition hover:bg-background-secondary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
          >
            <RefreshCw className="h-4 w-4" aria-hidden="true" />
            Retry
          </button>
        )}
      </div>
    )
  }

  return (
    <div
      role="status"
      aria-label="Loading content"
      aria-live="polite"
      className={cn(
        'animate-pulse bg-muted',
        variantClasses[variant],
        className,
      )}
    >
      <span className="sr-only">Loading…</span>
    </div>
  )
}
