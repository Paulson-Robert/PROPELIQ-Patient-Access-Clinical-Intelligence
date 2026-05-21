import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { PasswordInput } from '../../components/auth/PasswordInput'
import { ApiError, authApi } from '../../services/authApi'
import { isPasswordComplexityValid } from '../../utils/passwordValidation'

export const PasswordResetPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [verificationCode, setVerificationCode] = useState('')
  const [isSubmittingRequest, setIsSubmittingRequest] = useState(false)
  const [isSubmittingReset, setIsSubmittingReset] = useState(false)
  const [requestSent, setRequestSent] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [requestCooldownSeconds, setRequestCooldownSeconds] = useState(0)
  const [attemptsRemaining, setAttemptsRemaining] = useState<number | null>(null)

  const hasResetToken = Boolean(searchParams.get('token'))
  const isResetStepActive = requestSent || hasResetToken
  const passwordIsValid = useMemo(
    () => isPasswordComplexityValid(password),
    [password],
  )

  useEffect(() => {
    if (requestCooldownSeconds <= 0) {
      return
    }

    const timeoutId = window.setTimeout(() => {
      setRequestCooldownSeconds((currentValue) => Math.max(0, currentValue - 1))
    }, 1000)

    return () => {
      window.clearTimeout(timeoutId)
    }
  }, [requestCooldownSeconds])

  const onRequestSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage(null)
    setSuccessMessage(null)
    setAttemptsRemaining(null)
    setIsSubmittingRequest(true)

    try {
      const response = await authApi.requestPasswordResetCode({ email })
      setRequestSent(true)
      setSuccessMessage('If an account exists, a verification code has been sent to your email.')
      setRequestCooldownSeconds(response.cooldownSeconds)
    } catch (error) {
      if (error instanceof ApiError && typeof error.cooldownSeconds === 'number') {
        setRequestCooldownSeconds(error.cooldownSeconds)
      }

      setErrorMessage('Unable to send verification code. Please try again.')
    } finally {
      setIsSubmittingRequest(false)
    }
  }

  const onResetSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage(null)
    setSuccessMessage(null)
    setAttemptsRemaining(null)

    if (!passwordIsValid) {
      setErrorMessage('Please satisfy all password requirements.')
      return
    }

    if (password !== confirmPassword) {
      setErrorMessage('Password and confirmation must match.')
      return
    }

    if (!/^[0-9]{6}$/.test(verificationCode.trim())) {
      setErrorMessage('Verification code must be a 6-digit number.')
      return
    }

    setIsSubmittingReset(true)

    try {
      await authApi.resetPassword({
        email,
        newPassword: password,
        verificationCode: verificationCode.trim(),
      })
      navigate('/auth/login?passwordReset=success')
    } catch (error) {
      if (error instanceof ApiError) {
        setAttemptsRemaining(typeof error.attemptsRemaining === 'number' ? error.attemptsRemaining : null)
      }

      setErrorMessage('Verification code is invalid or expired.')
    } finally {
      setIsSubmittingReset(false)
    }
  }

  return (
    <main className="flex min-h-screen bg-background text-foreground" id="main-content">
      <section className="hidden flex-1 bg-primary px-12 py-16 text-primary-foreground md:flex md:flex-col md:justify-center">
        <h1 className="text-4xl font-bold tracking-tight">HealthAccess</h1>
      </section>

      <section className="flex flex-1 items-center justify-center px-4 py-8 sm:px-8">
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-sm sm:p-8" data-uxr="UXR-004">
          <h2 className="text-2xl font-semibold tracking-tight">Reset your password</h2>
          <p className="mb-6 mt-2 text-sm text-muted-foreground">
            Enter your email address and we&apos;ll send you a reset link.
          </p>

          <form className="space-y-4" onSubmit={onRequestSubmit} aria-label="Request password reset link">
            <div className="space-y-1">
              <label htmlFor="reset-email" className="text-sm font-medium">
                Email address
              </label>
                <input
                id="reset-email"
                type="email"
                className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                placeholder="you@example.com"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                autoComplete="email"
                  disabled={isSubmittingRequest || isSubmittingReset}
                required
              />
            </div>

            <button
              type="submit"
              className="min-h-11 w-full rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={isSubmittingRequest || requestCooldownSeconds > 0}
              aria-describedby="reset-code-cooldown"
            >
              {isSubmittingRequest
                ? 'Sending verification code...'
                : requestSent
                  ? 'Resend verification code'
                  : 'Send verification code'}
            </button>

            <p id="reset-code-cooldown" className="text-xs text-muted-foreground">
              {requestCooldownSeconds > 0
                ? `You can request a new code in ${requestCooldownSeconds}s.`
                : 'You can request a new code if needed.'}
            </p>
          </form>

          {successMessage ? (
            <p className="mt-4 rounded-md border border-green-700/30 bg-green-700/10 px-3 py-2 text-sm text-green-700">
              {successMessage}
            </p>
          ) : null}

          {errorMessage ? (
            <p className="mt-4 rounded-md border border-destructive bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {errorMessage}
            </p>
          ) : null}

          <div className="mt-4 text-center">
            <Link to="/auth/login" className="text-sm font-medium text-primary underline-offset-4 hover:underline">
              ← Back to sign in
            </Link>
          </div>

          <hr className="my-6 border-border" />

          <section
            className={`space-y-4 ${isResetStepActive ? '' : 'pointer-events-none opacity-50'}`}
            aria-label="Create new password"
          >
            <h3 className="text-base font-medium">Create new password</h3>

            <form className="space-y-4" onSubmit={onResetSubmit}>
              <div className="space-y-1">
                <label htmlFor="verification-code" className="text-sm font-medium">
                  Verification code
                </label>
                <input
                  id="verification-code"
                  type="text"
                  inputMode="numeric"
                  className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  placeholder="Enter 6-digit code"
                  value={verificationCode}
                  onChange={(event) => setVerificationCode(event.target.value)}
                  autoComplete="one-time-code"
                  disabled={!isResetStepActive || isSubmittingReset}
                  required
                />
              </div>

              <PasswordInput
                id="new-password"
                label="New password"
                value={password}
                onChange={setPassword}
                placeholder="Enter new password"
                autoComplete="new-password"
                disabled={!isResetStepActive || isSubmittingReset}
                required
              />

              <div className="space-y-1">
                <label htmlFor="confirm-new-password" className="text-sm font-medium">
                  Confirm new password
                </label>
                <input
                  id="confirm-new-password"
                  type="password"
                  className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  placeholder="Re-enter password"
                  value={confirmPassword}
                  onChange={(event) => setConfirmPassword(event.target.value)}
                  autoComplete="new-password"
                  disabled={!isResetStepActive || isSubmittingReset}
                  required
                />
              </div>

              <button
                type="submit"
                className="min-h-11 w-full rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-60"
                disabled={isSubmittingReset || !isResetStepActive}
              >
                {isSubmittingReset ? 'Resetting password...' : 'Reset password'}
              </button>

              {attemptsRemaining !== null ? (
                <p className="text-xs text-muted-foreground" aria-live="polite">
                  Remaining verification attempts: {attemptsRemaining}
                </p>
              ) : null}
            </form>
          </section>
        </div>
      </section>
    </main>
  )
}
