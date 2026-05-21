import type { ButtonHTMLAttributes, ReactNode } from 'react'
import { Loader2 } from 'lucide-react'
import { cn } from '../../lib/utils'

interface SubmitButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  isSubmitting: boolean
  children: ReactNode
  loadingLabel?: string
  className?: string
}

export function SubmitButton({
  isSubmitting,
  children,
  loadingLabel,
  className,
  disabled,
  ...rest
}: SubmitButtonProps) {
  const isDisabled = isSubmitting || disabled

  return (
    <button
      type="submit"
      disabled={isDisabled}
      aria-disabled={isDisabled}
      aria-busy={isSubmitting}
      className={cn(
        'inline-flex w-full items-center justify-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground transition hover:opacity-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60',
        className,
      )}
      {...rest}
    >
      {isSubmitting && (
        <Loader2
          className="h-4 w-4 animate-spin"
          aria-hidden="true"
        />
      )}
      <span>
        {isSubmitting && loadingLabel ? loadingLabel : children}
      </span>
    </button>
  )
}
