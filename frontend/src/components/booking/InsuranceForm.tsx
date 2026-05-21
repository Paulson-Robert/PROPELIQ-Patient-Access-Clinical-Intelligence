import { useMemo } from 'react'

const INSURANCE_PROVIDERS = [
  'Blue Cross Blue Shield',
  'Aetna',
  'UnitedHealthcare',
  'Cigna',
  'Humana',
  'Kaiser Permanente',
  'Medicare',
  'Medicaid',
] as const

export interface InsuranceFormValue {
  provider: string
  policyNumber: string
}

interface InsuranceFormProps {
  value: InsuranceFormValue
  onChange: (nextValue: InsuranceFormValue) => void
}

const POLICY_NUMBER_PATTERN = /^[A-Za-z0-9]{2,8}-?[A-Za-z0-9]{4,12}$/

const getWarningMessage = (value: InsuranceFormValue): string | null => {
  const provider = value.provider.trim()
  const policyNumber = value.policyNumber.trim()

  if (!provider && !policyNumber) {
    return null
  }

  if (provider && !policyNumber) {
    return 'Policy number is missing. You can still continue without insurance details.'
  }

  if (!provider && policyNumber) {
    return 'Select an insurance provider for better matching. This check is optional.'
  }

  if (!POLICY_NUMBER_PATTERN.test(policyNumber)) {
    return 'Policy number format looks unusual. Example format: BCBS-882341.'
  }

  return null
}

export const InsuranceForm = ({ value, onChange }: InsuranceFormProps) => {
  const warningMessage = useMemo(() => getWarningMessage(value), [value])

  return (
    <section
      className="mt-6 rounded-xl border border-border bg-card p-6 shadow-sm"
      aria-labelledby="insurance-form-heading"
      data-uxr="SCR-014"
    >
      <h2 id="insurance-form-heading" className="text-base font-semibold text-foreground">
        Insurance pre-check (optional)
      </h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Add provider and policy number for a soft check. Booking continues even if details are missing or invalid.
      </p>

      <div className="mt-4 grid gap-4 sm:grid-cols-2">
        <div className="flex flex-col gap-1">
          <label htmlFor="insurance-provider" className="text-xs font-medium text-muted-foreground">
            Insurance provider
          </label>
          <select
            id="insurance-provider"
            value={value.provider}
            onChange={(event) =>
              onChange({
                ...value,
                provider: event.target.value,
              })
            }
            className="min-h-[44px] rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          >
            <option value="">Prefer not to provide</option>
            {INSURANCE_PROVIDERS.map((provider) => (
              <option key={provider} value={provider}>
                {provider}
              </option>
            ))}
          </select>
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="insurance-policy-number" className="text-xs font-medium text-muted-foreground">
            Policy number
          </label>
          <input
            id="insurance-policy-number"
            type="text"
            autoComplete="off"
            placeholder="e.g., BCBS-882341"
            value={value.policyNumber}
            onChange={(event) =>
              onChange({
                ...value,
                policyNumber: event.target.value,
              })
            }
            aria-describedby={warningMessage ? 'insurance-policy-warning' : undefined}
            className="min-h-[44px] rounded-md border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
      </div>

      {warningMessage ? (
        <p
          id="insurance-policy-warning"
          role="status"
          aria-live="polite"
          className="mt-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700"
        >
          {warningMessage}
        </p>
      ) : null}
    </section>
  )
}
