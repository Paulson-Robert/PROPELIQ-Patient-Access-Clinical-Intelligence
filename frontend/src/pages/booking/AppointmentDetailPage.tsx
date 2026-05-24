import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { CancelConfirmDialog } from '../../components/booking/CancelConfirmDialog'
import { BookingError, bookingApi, type AppointmentRecord } from '../../services/bookingApi'

type LoadState = 'loading' | 'ready' | 'error'

const formatDate = (isoDate: string): string => {
  const [year, month, day] = isoDate.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  return date.toLocaleDateString('en-US', {
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  })
}

const formatTime = (time: string): string => {
  const [hourStr, minuteStr] = time.split(':')
  const hour = parseInt(hourStr, 10)
  const suffix = hour >= 12 ? 'PM' : 'AM'
  const displayHour = hour % 12 === 0 ? 12 : hour % 12
  return `${displayHour.toString().padStart(2, '0')}:${minuteStr} ${suffix}`
}

export const AppointmentDetailPage = () => {
  const { appointmentId = 'appt-001' } = useParams<{ appointmentId: string }>()
  const navigate = useNavigate()

  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [appointment, setAppointment] = useState<AppointmentRecord | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [isCancelling, setIsCancelling] = useState(false)
  const [cancelError, setCancelError] = useState<string | null>(null)
  const [calendarStatus, setCalendarStatus] = useState<'Synced' | 'Removed'>('Synced')
  const [statusMessage, setStatusMessage] = useState<string | null>(null)

  useEffect(() => {
    let ignore = false

    const run = async () => {
      setLoadState('loading')

      try {
        const record = await bookingApi.getAppointment(appointmentId)
        if (ignore) return

        setAppointment(record)
        if (record.status === 'Cancelled') {
          setCalendarStatus('Removed')
        }
        setLoadState('ready')
      } catch {
        if (ignore) return
        setLoadState('error')
      }
    }

    void run()

    return () => {
      ignore = true
    }
  }, [appointmentId])

  const canManageAppointment = appointment?.status === 'Scheduled'

  const handleReschedule = () => {
    if (!appointment) return

    const query = new URLSearchParams({
      provider: appointment.providerName,
      specialty: appointment.specialty,
      from: appointment.date,
    })

    navigate(`/booking/search?${query.toString()}`)
  }

  const handleCancelConfirm = async () => {
    if (!appointment || isCancelling) return

    setIsCancelling(true)
    setCancelError(null)

    try {
      const cancelled = await bookingApi.cancelAppointment({ appointmentId: appointment.id })
      setAppointment(cancelled)
      setCalendarStatus('Removed')
      setStatusMessage('Appointment cancelled. Calendar sync status updated.')
      setDialogOpen(false)
    } catch (error) {
      if (error instanceof BookingError && error.code === 'SLOT_LOCKED') {
        setCancelError('Cancellation is temporarily blocked while the slot is locked. Try again shortly.')
      } else {
        setCancelError('Unable to cancel the appointment right now. Please try again.')
      }
    } finally {
      setIsCancelling(false)
    }
  }

  const statusBadge = useMemo(() => {
    if (!appointment) return null

    if (appointment.status === 'Cancelled') {
      return <span className="inline-flex rounded-full bg-destructive/10 px-2.5 py-1 text-xs font-medium text-destructive">Cancelled</span>
    }

    return <span className="inline-flex rounded-full bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary">Scheduled</span>
  }, [appointment])

  if (loadState === 'loading') {
    return (
      <div className="flex items-center justify-center py-16 px-4">
        <p className="text-sm text-muted-foreground">Loading appointment details...</p>
      </div>
    )
  }

  if (loadState === 'error' || !appointment) {
    return (
      <div className="flex items-center justify-center py-16 px-4">
        <div className="text-center">
          <p className="text-sm text-muted-foreground">Could not load appointment details.</p>
          <button
            type="button"
            onClick={() => navigate('/booking/search')}
            className="mt-4 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring"
          >
            Back to search
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="text-foreground">
      <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6">
        <nav className="mb-4 flex items-center gap-2 text-sm text-muted-foreground" aria-label="Breadcrumb">
          <Link to="/dashboard/patient" className="transition-colors hover:text-foreground">
            Dashboard
          </Link>
          <span aria-hidden="true">/</span>
          <span aria-current="page" className="text-foreground">
            Appointment detail
          </span>
        </nav>

        <h1 className="mb-6 text-2xl font-semibold tracking-tight">Appointment detail</h1>

        {statusMessage ? (
          <div className="mb-4 rounded-md border border-green-500/40 bg-green-500/10 px-4 py-3 text-sm text-green-700" aria-live="polite">
            {statusMessage}
          </div>
        ) : null}

        <section className="rounded-xl border border-border bg-card p-6 shadow-sm">
          <header className="flex flex-wrap items-start justify-between gap-4">
            <div className="flex items-center gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                {appointment.providerInitials}
              </div>
              <div>
                <h2 className="text-lg font-semibold text-foreground">{appointment.providerName}</h2>
                <p className="text-sm text-muted-foreground">{appointment.specialty}</p>
              </div>
            </div>
            {statusBadge}
          </header>

          <dl className="mt-6 grid gap-4 text-sm sm:grid-cols-2">
            <div>
              <dt className="text-muted-foreground">Date</dt>
              <dd className="mt-1 font-medium text-foreground">{formatDate(appointment.date)}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Time</dt>
              <dd className="mt-1 font-medium text-foreground">{formatTime(appointment.startTime)}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Duration</dt>
              <dd className="mt-1 font-medium text-foreground">{appointment.durationMinutes} minutes</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Booking type</dt>
              <dd className="mt-1 font-medium text-foreground">{appointment.status}</dd>
            </div>
          </dl>

          <div className="mt-6 space-y-4 border-t border-border pt-4">
            <div className="flex items-center justify-between gap-4">
              <div>
                <h3 className="text-sm font-medium text-foreground">Pre-visit intake</h3>
                <p className="text-xs text-muted-foreground">Complete your intake before the appointment.</p>
              </div>
              <button
                type="button"
                className="rounded-md border border-border bg-card px-3 py-2 text-sm font-medium text-foreground transition hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring"
                onClick={() => navigate(`/intake/ai?appointmentId=${appointment.id}`)}
              >
                Start intake
              </button>
            </div>

            <div className="flex items-center justify-between gap-4 border-t border-border pt-4">
              <div>
                <h3 className="text-sm font-medium text-foreground">Calendar sync</h3>
                <p className="text-xs text-muted-foreground">
                  {calendarStatus === 'Synced' ? 'Added to Google Calendar' : 'Event removed from Google Calendar'}
                </p>
              </div>
              <span
                className={
                  calendarStatus === 'Synced'
                    ? 'inline-flex rounded-full bg-green-500/10 px-2.5 py-1 text-xs font-medium text-green-700'
                    : 'inline-flex rounded-full bg-muted px-2.5 py-1 text-xs font-medium text-foreground'
                }
              >
                {calendarStatus}
              </span>
            </div>
          </div>

          <footer className="mt-6 flex flex-wrap gap-3 border-t border-border pt-4">
            <button
              type="button"
              onClick={handleReschedule}
              disabled={!canManageAppointment}
              className="min-h-[44px] rounded-md border border-border bg-card px-4 py-2 text-sm font-medium text-foreground transition hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
            >
              Reschedule
            </button>
            <button
              type="button"
              onClick={() => {
                setDialogOpen(true)
                setCancelError(null)
              }}
              disabled={!canManageAppointment}
              className="min-h-[44px] rounded-md bg-destructive px-4 py-2 text-sm font-medium text-destructive-foreground transition hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
            >
              Cancel appointment
            </button>
          </footer>
        </section>
      </div>

      <CancelConfirmDialog
        open={dialogOpen}
        appointment={appointment}
        isSubmitting={isCancelling}
        errorMessage={cancelError}
        onConfirm={() => {
          void handleCancelConfirm()
        }}
        onKeep={() => setDialogOpen(false)}
      />
    </div>
  )
}
