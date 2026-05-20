import { useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'

const verificationContent = {
  success: {
    title: 'Email verified',
    description:
      'Your email address has been confirmed. You can now sign in to your account.',
    tone: 'success',
    ctaLabel: 'Sign in',
    ctaPath: '/auth/login',
  },
  pending: {
    title: 'Check your inbox',
    description:
      'We sent a verification link to your email. Open the message and confirm your account.',
    tone: 'neutral',
    ctaLabel: 'Back to login',
    ctaPath: '/auth/login',
  },
  expired: {
    title: 'Verification link expired',
    description:
      'This link is no longer valid. Request a new verification email to continue.',
    tone: 'warning',
    ctaLabel: 'Back to login',
    ctaPath: '/auth/login',
  },
  invalid: {
    title: 'Invalid verification link',
    description:
      'We could not validate this link. Request a new verification email and try again.',
    tone: 'warning',
    ctaLabel: 'Back to login',
    ctaPath: '/auth/login',
  },
} as const

export const EmailVerificationPage = () => {
  const [searchParams] = useSearchParams()
  const { resendVerificationEmail } = useAuth()
  const [resendMessage, setResendMessage] = useState<string | null>(null)

  const email = searchParams.get('email') ?? ''
  const rawStatus = searchParams.get('status')

  const status = useMemo(() => {
    if (
      rawStatus === 'success' ||
      rawStatus === 'pending' ||
      rawStatus === 'expired' ||
      rawStatus === 'invalid'
    ) {
      return rawStatus
    }

    return 'pending'
  }, [rawStatus])

  const content = verificationContent[status]

  const onResendClick = async () => {
    await resendVerificationEmail(email)
    setResendMessage('A new verification email was sent. Please check your inbox.')
  }

  return (
    <main className="flex min-h-screen bg-background text-foreground">
      <section className="hidden flex-1 bg-primary px-12 py-16 text-primary-foreground md:flex md:flex-col md:justify-center">
        <h1 className="text-4xl font-bold tracking-tight">PropelIQ</h1>
        <p className="mt-4 max-w-xl text-base/7 text-primary-foreground/85">
          Secure account verification helps keep patient and care-team data protected.
        </p>
      </section>

      <section className="flex flex-1 items-center justify-center px-4 py-8 sm:px-8">
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 text-center shadow-sm sm:p-8" data-uxr="UXR-605">
          <div
            className={`mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full ${
              content.tone === 'success'
                ? 'bg-green-600/15 text-green-700'
                : content.tone === 'warning'
                  ? 'bg-amber-500/15 text-amber-700'
                  : 'bg-muted text-foreground'
            }`}
            aria-hidden="true"
          >
            {content.tone === 'success' ? '✓' : 'i'}
          </div>

          <h2 className="text-2xl font-semibold tracking-tight">{content.title}</h2>
          <p className="mt-2 text-sm text-muted-foreground">{content.description}</p>
          {email ? <p className="mt-2 text-sm font-medium">{email}</p> : null}

          <Link
            to={content.ctaPath}
            className="mt-6 inline-flex min-h-11 w-full items-center justify-center rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground"
            id="btn-login"
          >
            {content.ctaLabel}
          </Link>

          <div className="mt-6 border-t border-border pt-6">
            <h3 className="text-sm font-medium">Still waiting?</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              Check your spam folder or request a new verification link.
            </p>
            <button
              type="button"
              onClick={onResendClick}
              className="mt-3 text-sm font-medium text-primary underline-offset-4 hover:underline"
              id="link-resend"
            >
              Resend verification email
            </button>
            {resendMessage ? (
              <p className="mt-3 rounded-md border border-green-600/30 bg-green-600/10 px-3 py-2 text-sm text-green-700">
                {resendMessage}
              </p>
            ) : null}
          </div>
        </div>
      </section>
    </main>
  )
}
