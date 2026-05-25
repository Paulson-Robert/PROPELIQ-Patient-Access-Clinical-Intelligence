import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { ArrowLeft, QrCode, Smartphone, ShieldCheck } from 'lucide-react'
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

export const MfaSetupPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { getMfaSetup, requestMfaCode, isLoading, error } = useAuth()
  const [setupCode, setSetupCode] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [setupMessage, setSetupMessage] = useState<string | null>(null)
  const [isSubmittingSms, setIsSubmittingSms] = useState(false)

  const email = searchParams.get('email') ?? ''
  const role = parseRole(searchParams.get('role'))
  const method = useMemo(() => parseMethod(searchParams.get('method')), [searchParams])

  const [setupData, setSetupData] = useState<{
    manualKey: string
    qrCodeDataUri: string
  } | null>(null)

  useEffect(() => {
    let isMounted = true

    void getMfaSetup(email, method)
      .then((response) => {
        if (!isMounted) {
          return
        }

        setSetupData({
          manualKey: response.manualKey,
          qrCodeDataUri: response.qrCodeDataUri,
        })
      })
      .catch(() => undefined)

    return () => {
      isMounted = false
    }
  }, [email, getMfaSetup, method])

  const onVerifyTotp = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (setupCode.length !== 6) {
      setSetupMessage('Enter the 6-digit code from your authenticator app.')
      return
    }

    navigate(
      `/auth/mfa/verify?email=${encodeURIComponent(email)}&role=${role}&method=totp`,
      { replace: true },
    )
  }

  const onSendSmsCode = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    setIsSubmittingSms(true)

    try {
      await requestMfaCode({ email, method: 'sms' })
      navigate(
        `/auth/mfa/verify?email=${encodeURIComponent(email)}&role=${role}&method=sms`,
        { replace: true },
      )
    } finally {
      setIsSubmittingSms(false)
    }
  }

  const onCopyManualKey = async () => {
    if (!setupData) {
      return
    }

    await navigator.clipboard.writeText(setupData.manualKey)
    setSetupMessage('Manual key copied to clipboard.')
  }

  return (
    <main className="flex min-h-screen bg-background text-foreground">
      <section className="hidden flex-1 bg-primary px-12 py-16 text-primary-foreground md:flex md:flex-col md:justify-center">
        <div className="max-w-xl space-y-4">
          <ShieldCheck className="h-12 w-12" aria-hidden="true" />
          <h1 className="text-4xl font-bold tracking-tight">HealthAccess</h1>
          <p className="text-base/7 text-primary-foreground/85">
            Set up your authenticator app or SMS code before you continue into the platform.
          </p>
        </div>
      </section>

      <section className="flex flex-1 items-center justify-center px-4 py-8 sm:px-8">
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-sm sm:p-8">
          <div className="mb-6 space-y-3">
            <p className="text-sm font-medium uppercase tracking-[0.2em] text-muted-foreground">
              MFA setup
            </p>
            <h2 className="text-2xl font-semibold tracking-tight">Set up two-factor authentication</h2>
            <p className="text-sm text-muted-foreground">Choose your preferred verification method.</p>
            {email ? <p className="text-sm font-medium">{email}</p> : null}
          </div>

          {setupMessage ? (
            <p className="mb-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
              {setupMessage}
            </p>
          ) : null}

          {error ? (
            <p className="mb-4 rounded-md border border-destructive bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {error}
            </p>
          ) : null}

          <div className="space-y-4">
            <div className="rounded-xl border border-border p-4">
              <h3 className="mb-2 flex items-center gap-2 text-sm font-semibold">
                <QrCode className="h-4 w-4" aria-hidden="true" />
                Authenticator app (recommended)
              </h3>
              <p className="mb-4 text-sm text-muted-foreground">
                Scan this QR code with Google Authenticator, Authy, or a similar app.
              </p>

              <div className="flex justify-center">
                {setupData ? (
                  <img
                    src={setupData.qrCodeDataUri}
                    alt="QR code for authenticator app setup"
                    className="h-40 w-40 rounded-xl border border-border bg-white p-2"
                  />
                ) : (
                  <div className="flex h-40 w-40 items-center justify-center rounded-xl border border-dashed border-border text-sm text-muted-foreground">
                    Loading QR code...
                  </div>
                )}
              </div>

              <div className="mt-4 rounded-md border border-border bg-muted px-3 py-2 text-center text-xs font-mono break-all text-muted-foreground">
                Manual key: {setupData?.manualKey ?? 'Loading...'}
              </div>

              <button
                type="button"
                onClick={onCopyManualKey}
                className="mt-3 w-full rounded-md border border-border px-3 py-2 text-sm font-medium text-foreground transition hover:bg-muted disabled:cursor-not-allowed disabled:opacity-60"
                disabled={isLoading || !setupData}
              >
                Copy manual key
              </button>

              <form className="mt-4 space-y-4" onSubmit={onVerifyTotp} aria-label="Verify authenticator setup">
                <TotpInput value={setupCode} onChange={setSetupCode} disabled={isLoading} label="Verification code" />

                <button
                  type="submit"
                  className="min-h-11 w-full rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={isLoading}
                >
                  {isLoading ? 'Verifying...' : 'Verify & enable'}
                </button>
              </form>
            </div>

            <div className="rounded-xl border border-border p-4">
              <h3 className="mb-2 flex items-center gap-2 text-sm font-semibold">
                <Smartphone className="h-4 w-4" aria-hidden="true" />
                SMS verification
              </h3>
              <p className="mb-4 text-sm text-muted-foreground">Receive a code via text message.</p>

              <form className="space-y-4" onSubmit={onSendSmsCode} aria-label="Request SMS verification">
                <div className="space-y-1">
                  <label htmlFor="sms-phone" className="text-sm font-medium">
                    Phone number
                  </label>
                  <input
                    id="sms-phone"
                    type="tel"
                    className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    placeholder="(555) 000-0000"
                    value={phoneNumber}
                    onChange={(event) => setPhoneNumber(event.target.value)}
                    autoComplete="tel"
                    required
                  />
                </div>

                <button
                  type="submit"
                  className="min-h-11 w-full rounded-md border border-input bg-card px-3 py-2 text-sm font-semibold text-card-foreground transition hover:bg-muted disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={isLoading || isSubmittingSms}
                >
                  {isSubmittingSms ? 'Sending code...' : 'Send code'}
                </button>
              </form>
            </div>
          </div>

          <div className="mt-6 flex items-center justify-between border-t border-border pt-6">
            <Link
              to="/auth/login"
              className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
            >
              <ArrowLeft className="h-4 w-4" aria-hidden="true" />
              Back to login
            </Link>
          </div>
        </div>
      </section>
    </main>
  )
}