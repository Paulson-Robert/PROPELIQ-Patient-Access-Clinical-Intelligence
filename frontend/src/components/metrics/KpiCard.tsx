import { cn } from '../../lib/utils'

export interface KpiCardProps {
  label: string
  value: string
  trend: string
  trendUp?: boolean
}

export const KpiCard = ({ label, value, trend, trendUp }: KpiCardProps) => (
  <article className="rounded-xl border border-border bg-card p-5 shadow-sm">
    <p className="text-sm text-muted-foreground">{label}</p>
    <p className="mt-1 text-2xl font-semibold tracking-tight text-foreground">{value}</p>
    <p
      className={cn(
        'mt-1 text-xs font-medium',
        trendUp === true && 'text-green-600 dark:text-green-400',
        trendUp === false && 'text-red-600 dark:text-red-400',
        trendUp === undefined && 'text-muted-foreground',
      )}
      aria-label={`Trend: ${trend}`}
    >
      {trend}
    </p>
  </article>
)
