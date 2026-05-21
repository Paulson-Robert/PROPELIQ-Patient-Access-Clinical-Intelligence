import { CheckCircle2, Clock } from 'lucide-react'
import { cn } from '../../lib/utils'

// AC-02: VerificationBadge displays human-verified vs unverified status
export type VerificationStatus = 'verified' | 'unverified'

interface VerificationBadgeProps {
  status: VerificationStatus
  className?: string
}

const BADGE_CONFIG: Record<VerificationStatus, { label: string; className: string }> = {
  verified: {
    label: 'Verified',
    className: 'bg-emerald-500/10 text-emerald-700',
  },
  unverified: {
    label: 'Unverified',
    className: 'bg-amber-500/10 text-amber-700',
  },
}

export const VerificationBadge = ({ status, className }: VerificationBadgeProps) => {
  const config = BADGE_CONFIG[status]
  const isVerified = status === 'verified'

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium',
        config.className,
        className,
      )}
      aria-label={isVerified ? 'Human verified' : 'Not yet verified'}
    >
      {isVerified ? (
        <CheckCircle2 className="h-3 w-3" aria-hidden="true" />
      ) : (
        <Clock className="h-3 w-3" aria-hidden="true" />
      )}
      {config.label}
    </span>
  )
}
