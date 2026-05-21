import { cn } from '../../lib/utils'

// AC-01: Confidence level tiers derived from wireframe SCR-021 color samples
// 96%/94% → green (success), 72% → amber (warning), 45% → red (destructive)
// Inferred decision: thresholds set at ≥80% = high, ≥60% = medium, <60% = low
export type ConfidenceLevel = 'high' | 'medium' | 'low'

export function getConfidenceLevel(score: number): ConfidenceLevel {
  if (score >= 80) return 'high'
  if (score >= 60) return 'medium'
  return 'low'
}

const DOT_STYLES: Record<ConfidenceLevel, string> = {
  high: 'bg-emerald-500',
  medium: 'bg-amber-500',
  low: 'bg-destructive',
}

const LEVEL_LABELS: Record<ConfidenceLevel, string> = {
  high: 'High confidence',
  medium: 'Medium confidence',
  low: 'Low confidence',
}

interface ConfidenceIndicatorProps {
  score: number
  className?: string
}

export const ConfidenceIndicator = ({ score, className }: ConfidenceIndicatorProps) => {
  const level = getConfidenceLevel(score)

  return (
    <div
      className={cn('inline-flex items-center gap-1.5', className)}
      aria-label={`${LEVEL_LABELS[level]}: ${score}%`}
      data-uxr="SCR-021"
    >
      <span
        className={cn('h-2 w-2 shrink-0 rounded-full', DOT_STYLES[level])}
        aria-hidden="true"
      />
      <span className="text-sm font-medium">{score}%</span>
    </div>
  )
}
