import type { SocialProvider } from '../../services/authApi'

interface SocialLoginButtonsProps {
  onProviderClick: (provider: SocialProvider) => void | Promise<void>
  isLoading?: boolean
  disabledProviders?: SocialProvider[]
}

const providerOutageText: Record<SocialProvider, string> = {
  google: 'Google login is temporarily unavailable. Please use email and password.',
  microsoft:
    'Microsoft login is temporarily unavailable. Please use email and password.',
}

export const SocialLoginButtons = ({
  onProviderClick,
  isLoading = false,
  disabledProviders = [],
}: SocialLoginButtonsProps) => {
  const isProviderDisabled = (provider: SocialProvider) =>
    isLoading || disabledProviders.includes(provider)

  return (
    <div className="space-y-3" aria-label="Social login options">
      <button
        type="button"
        className="flex min-h-11 w-full items-center justify-center gap-3 rounded-md border border-input bg-card px-3 py-2 text-sm font-medium text-card-foreground transition hover:bg-muted disabled:cursor-not-allowed disabled:opacity-60"
        onClick={() => onProviderClick('google')}
        disabled={isProviderDisabled('google')}
        title={
          disabledProviders.includes('google')
            ? providerOutageText.google
            : undefined
        }
      >
        Continue with Google
      </button>

      <button
        type="button"
        className="flex min-h-11 w-full items-center justify-center gap-3 rounded-md border border-input bg-card px-3 py-2 text-sm font-medium text-card-foreground transition hover:bg-muted disabled:cursor-not-allowed disabled:opacity-60"
        onClick={() => onProviderClick('microsoft')}
        disabled={isProviderDisabled('microsoft')}
        title={
          disabledProviders.includes('microsoft')
            ? providerOutageText.microsoft
            : undefined
        }
      >
        Continue with Microsoft
      </button>
    </div>
  )
}
