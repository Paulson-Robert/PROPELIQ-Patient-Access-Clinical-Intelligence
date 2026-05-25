import { useMemo, useState, type FormEvent } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { RegistrationForm } from '../../components/auth/RegistrationForm'
import { useAuth } from '../../hooks/useAuth'

type AuthMode = 'login' | 'register'

export const LoginPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [mode, setMode] = useState<AuthMode>('login')
  const [loginEmail, setLoginEmail] = useState('')
  const [loginPassword, setLoginPassword] = useState('')

  const { loginWithPassword, registerWithEmail, isLoading, error, redirectPathForRole } =
    useAuth()

  const sessionMessage = useMemo(() => {
    if (searchParams.get('session') === 'terminated') {
      return 'Too many failed attempts - please log in again.'
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

  return (
    <main className="flex min-h-screen bg-background text-foreground" id="main-content">
      <section className="hidden flex-1 bg-primary px-12 py-16 text-primary-foreground md:flex md:flex-col md:justify-center">
        <h1 className="text-4xl font-bold tracking-tight">HealthAccess</h1>
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
                  onClick={() => navigate('/auth/password-reset')}
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
                  role: values.role,
                })

                navigate(redirectPathForRole(response.user.role))
              }}
            />
          )}
        </div>
      </section>
    </main>
  )
}
