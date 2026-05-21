import { useState, type FormEvent } from 'react'
import { isPasswordComplexityValid } from '../../utils/passwordValidation'
import { PasswordInput } from './PasswordInput'

interface RegistrationFormValues {
  email: string
  password: string
  confirmPassword: string
  acceptedTerms: boolean
}

interface RegistrationFormProps {
  isSubmitting: boolean
  onSubmit: (values: RegistrationFormValues) => Promise<void>
}

export const RegistrationForm = ({
  isSubmitting,
  onSubmit,
}: RegistrationFormProps) => {
  const [values, setValues] = useState<RegistrationFormValues>({
    email: '',
    password: '',
    confirmPassword: '',
    acceptedTerms: false,
  })
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const isPasswordValid = isPasswordComplexityValid(values.password)

  const onFormSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage(null)

    if (!isPasswordValid) {
      setErrorMessage('Please satisfy all password requirements.')
      return
    }

    if (values.password !== values.confirmPassword) {
      setErrorMessage('Password and confirmation must match.')
      return
    }

    if (!values.acceptedTerms) {
      setErrorMessage('Accept terms and privacy policy before continuing.')
      return
    }

    try {
      await onSubmit(values)
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : 'Unable to create account. Please try again.',
      )
    }
  }

  return (
    <form className="space-y-4" onSubmit={onFormSubmit} aria-label="Create account">
      <div className="space-y-1">
        <label htmlFor="register-email" className="text-sm font-medium">
          Email address
        </label>
        <input
          id="register-email"
          type="email"
          className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
          placeholder="e.g., your.name@email.com"
          value={values.email}
          onChange={(event) =>
            setValues((previous) => ({ ...previous, email: event.target.value }))
          }
          autoComplete="email"
          required
        />
      </div>

      <PasswordInput
        id="register-password"
        label="Password"
        value={values.password}
        onChange={(nextValue) =>
          setValues((previous) => ({ ...previous, password: nextValue }))
        }
        placeholder="Create a strong password"
        autoComplete="new-password"
        required
      />

      <div className="space-y-1">
        <label htmlFor="register-confirm-password" className="text-sm font-medium">
          Confirm password
        </label>
        <input
          id="register-confirm-password"
          type="password"
          className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
          placeholder="Re-enter your password"
          value={values.confirmPassword}
          onChange={(event) =>
            setValues((previous) => ({
              ...previous,
              confirmPassword: event.target.value,
            }))
          }
          autoComplete="new-password"
          required
        />
      </div>

      <label className="flex items-start gap-2 text-sm">
        <input
          type="checkbox"
          checked={values.acceptedTerms}
          onChange={(event) =>
            setValues((previous) => ({
              ...previous,
              acceptedTerms: event.target.checked,
            }))
          }
          className="mt-1"
          required
        />
        <span>
          I agree to the terms of service and privacy policy.
        </span>
      </label>

      {errorMessage ? (
        <p className="rounded-md border border-destructive bg-destructive/10 px-3 py-2 text-sm text-destructive">
          {errorMessage}
        </p>
      ) : null}

      <button
        type="submit"
        className="min-h-11 w-full rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-60"
        disabled={isSubmitting}
      >
        {isSubmitting ? 'Creating account...' : 'Create account'}
      </button>
    </form>
  )
}
