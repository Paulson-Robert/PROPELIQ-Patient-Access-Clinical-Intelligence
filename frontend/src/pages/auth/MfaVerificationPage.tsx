import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { ArrowLeft, Clock3, RotateCcw, ShieldCheck } from 'lucide-react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { TotpInput } from '../../components/auth/TotpInput'
import { useAuth } from '../../hooks/useAuth'
import type { MfaMethod, UserRole } from '../../services/authApi'

const parseMethod = (value: string | null): MfaMethod => (value === 'sms' ? 'sms' : 'totp')

const parseRole = (value: string | null): UserRole => {
  if (value === 'admin' || value === 'staff' || value === 'patient') {
    return value
  }

  return 'staff'
}

const formatCountdown = (secondsRemaining: number): string => {
  const minutes = String(Math.floor(secondsRemaining / 60)).padStart(2, '0')
  const seconds = String(secondsRemaining % 60).padStart(2, '0')

  return `${minutes}:${seconds}`
}

export const MfaVerificationPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { verifyMfaCode, requestMfaCode, logout, isLoading, error, redirectPathForRole } = useAuth()
  const [code, setCode] = useState('')
  const [secondsRemaining, setSecondsRemaining] = useState(30)
  const [attemptsRemaining, setAttemptsRemaining] = useState(3)
  const [localMessage, setLocalMessage] = useState<string | null>(null)
  const [isExpired, setIsExpired] = useState(false)
  const [isResending, setIsResending] = useState(false)

  const email = searchParams.get('email') ?? ''
  const role = parseRole(searchParams.get('role'))
  const method = useMemo(() => parseMethod(searchParams.get('method')), [searchParams])

  useEffect(() => {
    if (secondsRemaining <= 0) {
      setIsExpired(true)
      return undefined
    }

    const timer = window.setTimeout(() => {
      setSecondsRemaining((current) => Math.max(0, current - 1))
    }, 1000)

    return () => window.clearTimeout(timer)
  }, [secondsRemaining])

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setLocalMessage(null)

    if (isExpired) {
      setLocalMessage('Code expired. Request a new code to continue.')
      return
    }

    if (code.length !== 6) {
      setLocalMessage('Enter the full 6-digit code.')
      return
    }

    const response = await verifyMfaCode({ email, code, method })

    if (response.status === 'success') {
      navigate(redirectPathForRole(role))
      return
    }

    if (response.status === 'expired') {
      setIsExpired(true)
      setLocalMessage('Code expired. Request a new code to continue.')
      return
    }

    if (response.status === 'locked' || response.attemptsRemaining <= 0) {
      logout()
      navigate('/auth/login?session=terminated', { replace: true })
      return
    }

    setAttemptsRemaining(response.attemptsRemaining)
    setLocalMessage(
      `Invalid code. ${response.attemptsRemaining} attempt${response.attemptsRemaining === 1 ? '' : 's'} remaining.`,
    )
  }

  const onRequestNewCode = async () => {
    setIsResending(true)

    try {
      await requestMfaCode({ email, method })
      setSecondsRemaining(30)
      setIsExpired(false)
      setAttemptsRemaining(3)
      setCode('')
      setLocalMessage('A new code was sent. Check your device and try again.')
    } finally {
      setIsResending(false)
    }
  }

  const description =
    method === 'sms'
      ? 'Enter the 6-digit code sent to your phone.'
      : 'Enter the 6-digit code from your authenticator app.'

  return (
    <main className="flex min-h-screen bg-background text-foreground">
      <section className="hidden flex-1 bg-primary px-12 py-16 text-primary-foreground md:flex md:flex-col md:justify-center">
        <div className="max-w-xl space-y-4">
          <ShieldCheck className="h-12 w-12" aria-hidden="true" />
          <h1 className="text-4xl font-bold tracking-tight">PropelIQ</h1>
          <p className="text-base/7 text-primary-foreground/85">
            Multi-factor verification protects staff and admin sessions before they reach the dashboard.
          </p>
        </div>
      </section>

      <section className="flex flex-1 items-center justify-center px-4 py-8 sm:px-8">
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-sm sm:p-8">
          <div className="mb-6 space-y-3">
            <p className="text-sm font-medium uppercase tracking-[0.2em] text-muted-foreground">
              Two-factor authentication
            </p>
            <h2 className="text-2xl font-semibold tracking-tight">Verify your identity</h2>
            <p className="text-sm text-muted-foreground">{description}</p>
            {email ? <p className="text-sm font-medium">{email}</p> : null}
          </div>

          {localMessage ? (
            <p className="mb-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
              {localMessage}
            </p>
          ) : null}

          {error ? (
            <p className="mb-4 rounded-md border border-destructive bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {error}
            </p>
          ) : null}

          <form className="space-y-5" onSubmit={onSubmit} aria-label="MFA verification form">
            <TotpInput value={code} onChange={setCode} disabled={isLoading || isResending} />

            <div className="flex items-center justify-between rounded-md border border-border bg-muted px-3 py-2 text-sm">
              <span className="inline-flex items-center gap-2 font-medium">
                <Clock3 className="h-4 w-4" aria-hidden="true" />
                Code expires in {formatCountdown(secondsRemaining)}
              </span>
              <span className="text-muted-foreground">{attemptsRemaining} attempts left</span>
            </div>

            <button
              type="submit"
              className="flex min-h-11 w-full items-center justify-center rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={isLoading || isResending}
            >
              {isLoading ? 'Verifying...' : 'Verify code'}
            </button>
          </form>

          <div className="mt-6 flex flex-col gap-3 border-t border-border pt-6 sm:flex-row sm:items-center sm:justify-between">
            <button
              type="button"
              className="inline-flex items-center gap-2 text-sm font-medium text-primary underline-offset-4 hover:underline disabled:cursor-not-allowed disabled:opacity-60"
              onClick={onRequestNewCode}
              disabled={isLoading || isResending}
            >
              <RotateCcw className="h-4 w-4" aria-hidden="true" />
              {isResending ? 'Requesting...' : 'Request new code'}
            </button>

            <Link
              to="/auth/login"
              className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
            >
              <ArrowLeft className="h-4 w-4" aria-hidden="true" />
              Use a different method
            </Link>
          </div>
        </div>
      </section>
    </main>
  )
}