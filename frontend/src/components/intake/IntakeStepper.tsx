import { Check } from 'lucide-react'
import { cn } from '../../lib/utils'

export interface IntakeStepperProps {
  steps: string[]
  currentStep: number
}

export const IntakeStepper = ({ steps, currentStep }: IntakeStepperProps) => (
  <nav aria-label="Intake form steps" data-uxr="SCR-010">
    <ol className="flex items-start">
      {steps.map((label, index) => {
        const isCompleted = index < currentStep
        const isActive = index === currentStep

        return (
          <li key={label} className="flex flex-1 flex-col items-center">
            <div className="flex w-full items-center">
              {index === 0 ? (
                <div className="flex-1" aria-hidden="true" />
              ) : (
                <div
                  className={cn(
                    'h-0.5 flex-1 transition-colors duration-300',
                    index <= currentStep ? 'bg-primary' : 'bg-border',
                  )}
                  aria-hidden="true"
                />
              )}

              <div
                className={cn(
                  'flex h-8 w-8 shrink-0 items-center justify-center rounded-full border-2 text-xs font-semibold transition-colors duration-300',
                  isCompleted
                    ? 'border-primary bg-primary text-primary-foreground'
                    : isActive
                      ? 'border-primary bg-background text-primary'
                      : 'border-border bg-background text-muted-foreground',
                )}
                aria-current={isActive ? 'step' : undefined}
              >
                {isCompleted ? (
                  <Check className="h-4 w-4" aria-hidden="true" />
                ) : (
                  <span aria-hidden="true">{index + 1}</span>
                )}
              </div>

              {index === steps.length - 1 ? (
                <div className="flex-1" aria-hidden="true" />
              ) : (
                <div
                  className={cn(
                    'h-0.5 flex-1 transition-colors duration-300',
                    index < currentStep ? 'bg-primary' : 'bg-border',
                  )}
                  aria-hidden="true"
                />
              )}
            </div>

            <span
              className={cn(
                'mt-2 max-w-[72px] text-center text-xs font-medium leading-tight',
                isActive ? 'text-foreground' : 'text-muted-foreground',
              )}
            >
              {label}
            </span>
          </li>
        )
      })}
    </ol>

    <p className="mt-3 text-center text-xs text-muted-foreground" aria-live="polite">
      Step {currentStep + 1} of {steps.length}
    </p>
  </nav>
)
