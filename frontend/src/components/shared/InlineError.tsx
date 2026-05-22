import { AlertCircle } from 'lucide-react'
import { cn } from '../../lib/utils'

interface InlineErrorProps {
  /** Unique ID linking this message to a form field via aria-describedby */
  id: string
  /** The validation error message to display */
  message: string
  /** Optional recovery suggestion shown below the error */
  suggestion?: string
  className?: string
}

export function InlineError({
  id,
  message,
  suggestion,
  className,
}: InlineErrorProps) {
  return (
    <div
      id={id}
      role="alert"
      aria-live="assertive"
      className={cn(
        'mt-1.5 flex items-start gap-1.5 text-sm text-destructive',
        className,
      )}
    >
      <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
      <div>
        <p>{message}</p>
        {suggestion && (
          <p className="mt-0.5 text-xs text-foreground-secondary">
            {suggestion}
          </p>
        )}
      </div>
    </div>
  )
}
