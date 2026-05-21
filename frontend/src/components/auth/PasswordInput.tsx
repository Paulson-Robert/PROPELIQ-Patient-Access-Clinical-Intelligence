import { useMemo } from 'react'
import {
  validatePasswordComplexity,
  type PasswordRequirementResult,
} from '../../utils/passwordValidation'

interface PasswordInputProps {
  id: string
  label: string
  value: string
  onChange: (nextValue: string) => void
  placeholder: string
  autoComplete: string
  required?: boolean
  disabled?: boolean
  describedBy?: string
  requirementsLabel?: string
}

const requirementTextClass = (result: PasswordRequirementResult): string =>
  result.passed ? 'text-green-700' : 'text-muted-foreground'

export const PasswordInput = ({
  id,
  label,
  value,
  onChange,
  placeholder,
  autoComplete,
  required = false,
  disabled = false,
  describedBy,
  requirementsLabel = 'Password requirements',
}: PasswordInputProps) => {
  const requirementResults = useMemo(
    () => validatePasswordComplexity(value),
    [value],
  )
  const requirementsId = `${id}-requirements`
  const describedByIds = [describedBy, requirementsId].filter(Boolean).join(' ')

  return (
    <div className="space-y-1" data-uxr="UXR-004">
      <label htmlFor={id} className="text-sm font-medium">
        {label}
      </label>
      <input
        id={id}
        type="password"
        className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
        placeholder={placeholder}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        autoComplete={autoComplete}
        aria-describedby={describedByIds}
        required={required}
        disabled={disabled}
      />

      <ul className="space-y-1" id={requirementsId} aria-label={requirementsLabel}>
        {requirementResults.map((requirement) => (
          <li
            key={requirement.id}
            className={`text-xs ${requirementTextClass(requirement)}`}
          >
            {requirement.label}
          </li>
        ))}
      </ul>
    </div>
  )
}
