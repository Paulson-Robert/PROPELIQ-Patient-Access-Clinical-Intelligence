import { useMemo, useState, type FormEvent } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { RegistrationForm } from '../../components/auth/RegistrationForm'
import { SocialLoginButtons } from '../../components/auth/SocialLoginButtons'
import { useAuth } from '../../hooks/useAuth'
import type { SocialProvider } from '../../services/authApi'

type AuthMode = 'login' | 'register'

const parseDisabledProviders = (value: string | null): SocialProvider[] => {
  if (!value) {
    return []
  }

  return value
    .split(',')
    .map((provider) => provider.trim().toLowerCase())
    .filter((provider): provider is SocialProvider =>
      provider === 'google' || provider === 'microsoft',
    )
}

export const LoginPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [mode, setMode] = useState<AuthMode>('login')
  const [loginEmail, setLoginEmail] = useState('')
  const [loginPassword, setLoginPassword] = useState('')
  const [registrationHint, setRegistrationHint] = useState<string | null>(null)

  const { loginWithPassword, registerWithEmail, startSocialLogin, isLoading, error, redirectPathForRole } =
    useAuth()

  const oauthError = searchParams.get('oauthError')
  const disabledProviders = parseDisabledProviders(searchParams.get('providerOutage'))

  const oauthGuidance = useMemo(() => {
    if (oauthError === 'consent_denied') {
      return 'Consent was denied. Use email registration, or retry a social provider.'
    }

    if (oauthError === 'email_mismatch') {
      return 'Your social account email does not match an existing record. Sign in with email to link accounts.'
    }

    return null
  }, [oauthError])

  const sessionMessage = useMemo(() => {
    if (searchParams.get('session') === 'terminated') {
      return 'Too many failed attempts — please log in again.'
    }

    return null
  }, [searchParams])

  const onLoginSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const authResponse = await loginWithPassword(loginEmail, loginPassword)

    if (authResponse.mfaChallenge?.state === 'setup') {
      navigate(
        `/auth/mfa/setup?email=${encodeURIComponent(authResponse.user.email)}&role=${authResponse.user.role}&method=${authResponse.mfaChallenge.method}`,
      )
      return
    }

    if (authResponse.mfaChallenge?.state === 'verify') {
      navigate(
        `/auth/mfa/verify?email=${encodeURIComponent(authResponse.user.email)}&role=${authResponse.user.role}&method=${authResponse.mfaChallenge.method}`,
      )
      return
    }

    navigate(redirectPathForRole(authResponse.user.role))
  }

  const onSocialProviderClick = async (provider: SocialProvider) => {
    const redirectPath = await startSocialLogin(provider)

    if (redirectPath.startsWith('/')) {
      navigate(redirectPath)
      return
    }

    window.location.assign(redirectPath)
  }

  return (
    <main className="flex min-h-screen bg-background text-foreground" id="main-content">
      <section className="hidden flex-1 bg-primary px-12 py-16 text-primary-foreground md:flex md:flex-col md:justify-center">
        <h1 className="text-4xl font-bold tracking-tight">PropelIQ</h1>
        <p className="mt-4 max-w-xl text-base/7 text-primary-foreground/85">
          Patient Access &amp; Clinical Intelligence Platform. Streamline appointments,
          intake, and care workflows from one secure place.
        </p>
      </section>

      <section className="flex flex-1 items-center justify-center px-4 py-8 sm:px-8">
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-sm sm:p-8" data-uxr="UXR-605">
          <div className="mb-6 space-y-3">
            <h2 className="text-2xl font-semibold tracking-tight">
              {mode === 'login' ? 'Welcome back' : 'Create your account'}
            </h2>
            <div className="grid grid-cols-2 gap-2 rounded-md border border-border bg-muted p-1" role="tablist" aria-label="Authentication mode">
              <button
                type="button"
                role="tab"
                aria-selected={mode === 'login'}
                className={`rounded px-3 py-2 text-sm font-medium ${
                  mode === 'login'
                    ? 'bg-background text-foreground shadow-sm'
                    : 'text-muted-foreground'
                }`}
                onClick={() => setMode('login')}
              >
                Log in
              </button>
              <button
                type="button"
                role="tab"
                aria-selected={mode === 'register'}
                className={`rounded px-3 py-2 text-sm font-medium ${
                  mode === 'register'
                    ? 'bg-background text-foreground shadow-sm'
                    : 'text-muted-foreground'
                }`}
                onClick={() => setMode('register')}
              >
                Create account
              </button>
            </div>
          </div>

          {oauthGuidance ? (
            <p className="mb-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
              {oauthGuidance}
            </p>
          ) : null}

          {registrationHint ? (
            <p className="mb-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
              {registrationHint}
            </p>
          ) : null}

          {sessionMessage ? (
            <p className="mb-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
              {sessionMessage}
            </p>
          ) : null}

          {error ? (
            <p className="mb-4 rounded-md border border-destructive bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {error}
            </p>
          ) : null}

          <SocialLoginButtons
            onProviderClick={onSocialProviderClick}
            isLoading={isLoading}
            disabledProviders={disabledProviders}
          />

          <div className="my-6 flex items-center gap-3 text-xs uppercase tracking-wide text-muted-foreground">
            <span className="h-px flex-1 bg-border" aria-hidden="true" />
            <span>or</span>
            <span className="h-px flex-1 bg-border" aria-hidden="true" />
          </div>

          {mode === 'login' ? (
            <form className="space-y-4" onSubmit={onLoginSubmit} aria-label="Log in to your account">
              <div className="space-y-1">
                <label htmlFor="login-email" className="text-sm font-medium">
                  Email address
                </label>
                <input
                  id="login-email"
                  type="email"
                  className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  placeholder="e.g., maria.santos@email.com"
                  value={loginEmail}
                  onChange={(event) => setLoginEmail(event.target.value)}
                  autoComplete="email"
                  required
                />
              </div>

              <div className="space-y-1">
                <label htmlFor="login-password" className="text-sm font-medium">
                  Password
                </label>
                <input
                  id="login-password"
                  type="password"
                  className="min-h-11 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  placeholder="Enter your password"
                  value={loginPassword}
                  onChange={(event) => setLoginPassword(event.target.value)}
                  autoComplete="current-password"
                  required
                />
              </div>

              <div className="flex items-center justify-between text-sm">
                <label className="flex items-center gap-2">
                  <input type="checkbox" />
                  <span>Remember me</span>
                </label>
                <button
                  type="button"
                  className="font-medium text-primary underline-offset-4 hover:underline"
                >
                  Forgot password?
                </button>
              </div>

              <button
                type="submit"
                className="min-h-11 w-full rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-60"
                disabled={isLoading}
              >
                {isLoading ? 'Logging in...' : 'Log in'}
              </button>
            </form>
          ) : (
            <RegistrationForm
              isSubmitting={isLoading}
              onSubmit={async (values) => {
                const response = await registerWithEmail({
                  email: values.email,
                  password: values.password,
                })

                if (response.duplicateEmail) {
                  return response
                }

                navigate(`/auth/verify?status=pending&email=${encodeURIComponent(values.email)}`)
                return response
              }}
              onDuplicateEmail={() => {
                setRegistrationHint(
                  'This email already has an account. Try logging in or reset your password.',
                )
                setMode('login')
              }}
            />
          )}
        </div>
      </section>
    </main>
  )
}
