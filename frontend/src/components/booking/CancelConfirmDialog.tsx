import type { AppointmentRecord } from '../../services/bookingApi'

interface CancelConfirmDialogProps {
  open: boolean
  appointment: AppointmentRecord
  isSubmitting: boolean
  errorMessage: string | null
  onConfirm: () => void
  onKeep: () => void
}

const formatDateTime = (isoDate: string, startTime: string): string => {
  const [year, month, day] = isoDate.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  const dateText = date.toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })

  const [hourStr, minuteStr] = startTime.split(':')
  const hour = parseInt(hourStr, 10)
  const suffix = hour >= 12 ? 'PM' : 'AM'
  const displayHour = hour % 12 === 0 ? 12 : hour % 12
  const timeText = `${displayHour.toString().padStart(2, '0')}:${minuteStr} ${suffix}`

  return `${dateText} · ${timeText}`
}

export const CancelConfirmDialog = ({
  open,
  appointment,
  isSubmitting,
  errorMessage,
  onConfirm,
  onKeep,
}: CancelConfirmDialogProps) => {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4" role="presentation">
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="cancel-dialog-title"
        aria-describedby="cancel-dialog-description"
        className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-xl"
      >
        <h2 id="cancel-dialog-title" className="text-lg font-semibold text-destructive">
          Cancel appointment
        </h2>

        <p id="cancel-dialog-description" className="mt-3 text-sm text-foreground">
          Are you sure you want to cancel your appointment?
        </p>

        <div
          role="status"
          className="mt-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700"
        >
          This action cannot be undone. You will need to rebook if you change your mind.
        </div>

        <dl className="mt-4 space-y-2 rounded-md bg-muted/40 p-3">
          <div className="flex justify-between gap-3 text-sm">
            <dt className="text-muted-foreground">Provider</dt>
            <dd className="font-medium text-foreground">{appointment.providerName}</dd>
          </div>
          <div className="flex justify-between gap-3 text-sm">
            <dt className="text-muted-foreground">Date</dt>
            <dd className="font-medium text-foreground">
              {formatDateTime(appointment.date, appointment.startTime)}
            </dd>
          </div>
        </dl>

        {errorMessage ? (
          <p className="mt-3 rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive" role="alert">
            {errorMessage}
          </p>
        ) : null}

        <div className="mt-5 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onKeep}
            disabled={isSubmitting}
            className="min-h-[44px] rounded-md border border-border bg-card px-4 py-2 text-sm font-medium text-foreground transition hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-60"
          >
            Keep appointment
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isSubmitting}
            className="min-h-[44px] rounded-md bg-destructive px-4 py-2 text-sm font-medium text-destructive-foreground transition hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-60"
          >
            {isSubmitting ? 'Cancelling...' : 'Yes, cancel appointment'}
          </button>
        </div>
      </div>
    </div>
  )
}
