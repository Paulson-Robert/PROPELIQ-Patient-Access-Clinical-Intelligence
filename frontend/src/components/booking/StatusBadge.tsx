type AppointmentStatus =
  | 'Confirmed'
  | 'Scheduled'
  | 'Completed'
  | 'Cancelled'
  | 'No-Show'
  | 'Arrived'

interface StatusBadgeProps {
  status: AppointmentStatus
}

const STATUS_STYLES: Record<AppointmentStatus, string> = {
  Confirmed: 'bg-primary/10 text-primary',
  Scheduled: 'bg-primary/10 text-primary',
  Completed: 'bg-emerald-500/10 text-emerald-700',
  Cancelled: 'bg-secondary text-secondary-foreground',
  'No-Show': 'bg-destructive/10 text-destructive',
  Arrived: 'bg-amber-500/10 text-amber-700',
}

export const StatusBadge = ({ status }: StatusBadgeProps) => {
  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${STATUS_STYLES[status]}`}
      aria-label={`Status: ${status}`}
    >
      {status}
    </span>
  )
}

export type { AppointmentStatus }
