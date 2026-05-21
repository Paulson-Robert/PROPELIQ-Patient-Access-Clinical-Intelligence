import { Check } from 'lucide-react'
import { cn } from '../../lib/utils'

interface ProgressIndicatorProps {
  steps: string[]
  currentStep: number
  className?: string
}

export function ProgressIndicator({
  steps,
  currentStep,
  className,
}: ProgressIndicatorProps) {
  return (
    <nav
      aria-label="Progress"
      className={cn('w-full', className)}
    >
      <ol className="flex items-center" role="list">
        {steps.map((label, index) => {
          const isCompleted = index < currentStep
          const isCurrent = index === currentStep
          const isLast = index === steps.length - 1

          return (
            <li
              key={label}
              className={cn(
                'flex items-center',
                !isLast && 'flex-1',
              )}
            >
              <div className="flex flex-col items-center gap-1">
                <div
                  aria-current={isCurrent ? 'step' : undefined}
                  className={cn(
                    'flex h-8 w-8 items-center justify-center rounded-full border-2 text-sm font-medium transition-colors',
                    isCompleted
                      ? 'border-primary bg-primary text-primary-foreground'
                      : isCurrent
                        ? 'border-primary bg-background text-primary'
                        : 'border-border bg-background text-foreground-muted',
                  )}
                >
                  {isCompleted ? (
                    <Check className="h-4 w-4" aria-hidden="true" />
                  ) : (
                    <span aria-hidden="true">{index + 1}</span>
                  )}
                  <span className="sr-only">
                    {isCompleted
                      ? `${label} — completed`
                      : isCurrent
                        ? `${label} — current step`
                        : label}
                  </span>
                </div>
                <span
                  className={cn(
                    'text-xs',
                    isCurrent
                      ? 'font-medium text-foreground'
                      : 'text-foreground-secondary',
                  )}
                  aria-hidden="true"
                >
                  {label}
                </span>
              </div>

              {!isLast && (
                <div
                  aria-hidden="true"
                  className={cn(
                    'mx-2 mb-5 h-0.5 flex-1 transition-colors',
                    isCompleted ? 'bg-primary' : 'bg-border',
                  )}
                />
              )}
            </li>
          )
        })}
      </ol>

      <p className="sr-only" aria-live="polite">
        Step {currentStep + 1} of {steps.length}: {steps[currentStep]}
      </p>
    </nav>
  )
}
