import { Clock, ShieldAlert, ShieldCheck, ShieldX } from 'lucide-react'
import type { ComponentType } from 'react'
import { cn } from '../../lib/utils'
import type { RiskLevel } from '../../services/queueApi'

// AC-01: Risk tier badge with Low/Medium/High color variants
// Edge Case: null/undefined tier shows pending grey badge
export type RiskTierValue = RiskLevel | null | undefined

interface RiskTierBadgeProps {
  tier: RiskTierValue
  className?: string
}

type TierConfig = {
  label: string
  className: string
  Icon: ComponentType<{ className?: string; 'aria-hidden'?: boolean | 'true' | 'false' }>
}

// AC-01 + AC-02: color + icon + text per tier
const TIER_CONFIG: Record<RiskLevel, TierConfig> = {
  Low: {
    label: 'Low',
    className: 'bg-emerald-500/10 text-emerald-700',
    Icon: ShieldCheck,
  },
  Medium: {
    label: 'Medium',
    className: 'bg-amber-500/10 text-amber-700',
    Icon: ShieldAlert,
  },
  High: {
    label: 'High',
    className: 'bg-red-500/10 text-red-700',
    Icon: ShieldX,
  },
}

const PENDING_CONFIG: TierConfig = {
  label: 'Calculating...',
  className: 'bg-muted text-muted-foreground',
  Icon: Clock,
}

export const RiskTierBadge = ({ tier, className }: RiskTierBadgeProps) => {
  const config = tier != null ? TIER_CONFIG[tier] : PENDING_CONFIG
  const { label, className: variantClass, Icon } = config

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium',
        variantClass,
        className,
      )}
      aria-label={tier != null ? `Risk tier: ${label}` : 'Risk score pending'}
    >
      <Icon className="h-3 w-3" aria-hidden="true" />
      {label}
    </span>
  )
}
