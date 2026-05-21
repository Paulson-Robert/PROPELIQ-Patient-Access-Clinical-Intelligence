interface IntakeProgressProps {
  completedCount: number
  totalCount: number
}

export const IntakeProgress = ({ completedCount, totalCount }: IntakeProgressProps) => {
  const percentage = totalCount === 0 ? 0 : Math.round((completedCount / totalCount) * 100)

  return (
    <div className="flex items-center gap-3" aria-label={`Intake ${percentage}% complete`} data-uxr="SCR-009">
      <div
        className="h-2 flex-1 rounded-full bg-muted overflow-hidden"
        role="progressbar"
        aria-valuenow={percentage}
        aria-valuemin={0}
        aria-valuemax={100}
      >
        <div
          className="h-full rounded-full bg-primary transition-all duration-500 ease-out"
          style={{ width: `${percentage}%` }}
        />
      </div>
      <span className="shrink-0 text-xs font-medium text-muted-foreground tabular-nums">
        {percentage}%
      </span>
    </div>
  )
}
