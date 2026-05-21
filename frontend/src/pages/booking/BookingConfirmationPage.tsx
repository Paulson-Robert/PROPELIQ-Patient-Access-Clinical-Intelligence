import { useCallback, useEffect, useRef, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { CountdownTimer } from '../../components/booking/CountdownTimer'
import {
  BookingError,
  bookingApi,
  type AppointmentRecord,
  type AvailabilitySlot,
} from '../../services/bookingApi'

interface LocationState {
  slot: AvailabilitySlot
  lockToken: string
  expiresAtUtc: string
}

type PageState = 'confirming' | 'submitting' | 'confirmed' | 'lock_expired' | 'slot_unavailable' | 'error'

const formatTime = (time: string): string => {
  const [hourStr, minuteStr] = time.split(':')
  const hour = parseInt(hourStr, 10)
  const suffix = hour >= 12 ? 'PM' : 'AM'
  const displayHour = hour % 12 === 0 ? 12 : hour % 12
  return `${displayHour.toString().padStart(2, '0')}:${minuteStr} ${suffix}`
}

const formatDate = (isoDate: string): string => {
  const [year, month, day] = isoDate.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  return date.toLocaleDateString('en-US', {
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  })
}

const isValidLocationState = (value: unknown): value is LocationState => {
  if (!value || typeof value !== 'object') return false
  const s = value as Record<string, unknown>
  return (
    typeof s.slot === 'object' &&
    s.slot !== null &&
    typeof s.lockToken === 'string' &&
    typeof s.expiresAtUtc === 'string'
  )
}

const LOCK_TOTAL_SECONDS = 30

export const BookingConfirmationPage = () => {
  const navigate = useNavigate()
  const location = useLocation()

  const state = isValidLocationState(location.state) ? location.state : null

  const [pageState, setPageState] = useState<PageState>(state ? 'confirming' : 'error')
  const [lockSecondsRemaining, setLockSecondsRemaining] = useState(LOCK_TOTAL_SECONDS)
  const [appointment, setAppointment] = useState<AppointmentRecord | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const lockTickRef = useRef<number | null>(null)

  const clearTick = useCallback(() => {
    if (lockTickRef.current !== null) {
      window.clearInterval(lockTickRef.current)
      lockTickRef.current = null
    }
  }, [])

  useEffect(() => {
    if (!state) return

    const expiry = new Date(state.expiresAtUtc).getTime()

    lockTickRef.current = window.setInterval(() => {
      const remaining = Math.max(0, Math.round((expiry - Date.now()) / 1000))
      setLockSecondsRemaining(remaining)

      if (remaining <= 0) {
        clearTick()
        setPageState('lock_expired')
      }
    }, 500)

    return clearTick
  }, [state, clearTick])

  const handleConfirm = async () => {
    if (!state || pageState !== 'confirming') return

    clearTick()
    setPageState('submitting')

    try {
      const record = await bookingApi.confirmBooking({
        slotId: state.slot.id,
        lockToken: state.lockToken,
      })
      setAppointment(record)
      setPageState('confirmed')
    } catch (err) {
      if (err instanceof BookingError && err.code === 'LOCK_EXPIRED') {
        setPageState('lock_expired')
      } else if (err instanceof BookingError && err.code === 'SLOT_UNAVAILABLE') {
        setPageState('slot_unavailable')
      } else {
        setErrorMessage('Something went wrong. Please try again.')
        setPageState('error')
      }
    }
  }

  const handleCancel = () => {
    clearTick()
    navigate('/booking/search')
  }

  if (!state && pageState === 'error') {
    return (
      <main className="flex min-h-screen items-center justify-center bg-background px-4" id="main-content">
        <div className="text-center">
          <p className="text-muted-foreground text-sm">No booking session found.</p>
          <button
            type="button"
            onClick={() => navigate('/booking/search')}
            className="mt-4 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring"
          >
            Search for appointments
          </button>
        </div>
      </main>
    )
  }

  /* Lock expired state — AC-06 */
  if (pageState === 'lock_expired') {
    return (
      <main className="flex min-h-screen items-center justify-center bg-background px-4" id="main-content">
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-8 text-center shadow-sm">
          <div
            className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-amber-500/10 text-amber-600"
            aria-hidden="true"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <circle cx="12" cy="12" r="10" />
              <line x1="12" y1="8" x2="12" y2="12" />
              <line x1="12" y1="16" x2="12.01" y2="16" />
            </svg>
          </div>
          <h1 className="text-lg font-semibold text-foreground">Slot hold expired</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Slot hold expired — please select again.
          </p>
          <button
            type="button"
            onClick={() => navigate('/booking/search')}
            className="mt-6 rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring"
          >
            Search again
          </button>
        </div>
      </main>
    )
  }

  /* Slot unavailable between search and selection — Edge Case */
  if (pageState === 'slot_unavailable') {
    return (
      <main className="flex min-h-screen items-center justify-center bg-background px-4" id="main-content">
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-8 text-center shadow-sm">
          <h1 className="text-lg font-semibold text-foreground">Slot no longer available</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            This slot was booked by someone else. Please search for a different time.
          </p>
          <button
            type="button"
            onClick={() => navigate('/booking/search')}
            className="mt-6 rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring"
          >
            Search again
          </button>
        </div>
      </main>
    )
  }

  /* Error state */
  if (pageState === 'error') {
    return (
      <main className="flex min-h-screen items-center justify-center bg-background px-4" id="main-content">
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-8 text-center shadow-sm">
          <h1 className="text-lg font-semibold text-foreground">Booking failed</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            {errorMessage ?? 'An unexpected error occurred. Please try again.'}
          </p>
          <button
            type="button"
            onClick={() => navigate('/booking/search')}
            className="mt-6 rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring"
          >
            Search again
          </button>
        </div>
      </main>
    )
  }

  /* Confirmed state — SCR-006 success */
  if (pageState === 'confirmed' && appointment) {
    return (
      <main className="min-h-screen bg-background text-foreground" id="main-content">
        <div className="mx-auto max-w-2xl px-4 py-16 sm:px-6 text-center">
          <div
            className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-green-500/10 text-green-600"
            aria-hidden="true"
          >
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <polyline points="20 6 9 17 4 12" />
            </svg>
          </div>

          <h1 className="text-2xl font-semibold tracking-tight text-foreground">
            Appointment confirmed
          </h1>
          <p className="mt-2 text-sm text-muted-foreground">
            A confirmation PDF has been sent to {appointment.patientEmail}.
          </p>

          <div className="mx-auto mt-6 w-full max-w-sm rounded-xl border border-border bg-card p-6 text-left shadow-sm">
            <dl className="space-y-2">
              <div className="flex justify-between py-1 text-sm">
                <dt className="text-muted-foreground">Provider</dt>
                <dd className="font-medium text-foreground">{appointment.providerName}</dd>
              </div>
              <div className="flex justify-between py-1 text-sm">
                <dt className="text-muted-foreground">Specialty</dt>
                <dd className="text-foreground">{appointment.specialty}</dd>
              </div>
              <div className="flex justify-between py-1 text-sm">
                <dt className="text-muted-foreground">Date</dt>
                <dd className="font-medium text-foreground">{formatDate(appointment.date)}</dd>
              </div>
              <div className="flex justify-between py-1 text-sm">
                <dt className="text-muted-foreground">Time</dt>
                <dd className="font-medium text-foreground">{formatTime(appointment.startTime)}</dd>
              </div>
              <div className="flex justify-between py-1 text-sm">
                <dt className="text-muted-foreground">Duration</dt>
                <dd className="text-foreground">{appointment.durationMinutes} minutes</dd>
              </div>
              <div className="flex justify-between py-1 text-sm">
                <dt className="text-muted-foreground">Status</dt>
                <dd>
                  <span className="inline-flex items-center rounded-full bg-green-500/10 px-2 py-0.5 text-xs font-medium text-green-700">
                    Confirmed
                  </span>
                </dd>
              </div>
            </dl>
          </div>

          <div className="mt-6 flex flex-wrap justify-center gap-3">
            <button
              type="button"
              className="inline-flex items-center gap-2 rounded-md border border-border bg-card px-4 py-2 text-sm font-medium text-foreground hover:bg-muted transition focus:outline-none focus:ring-2 focus:ring-ring"
            >
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <rect x="3" y="4" width="18" height="18" rx="2" />
                <path d="M16 2v4M8 2v4M3 10h18" />
              </svg>
              Add to calendar
            </button>
            <button
              type="button"
              onClick={() => navigate('/booking/search')}
              className="rounded-md border border-border bg-card px-4 py-2 text-sm font-medium text-foreground hover:bg-muted transition focus:outline-none focus:ring-2 focus:ring-ring"
            >
              Book another
            </button>
          </div>

          <a
            href="/dashboard/patient"
            className="mt-6 inline-block text-sm text-muted-foreground hover:text-foreground underline underline-offset-4 transition"
          >
            Return to dashboard
          </a>
        </div>
      </main>
    )
  }

  /* Confirming / submitting state — slot details + countdown (UXR-502) */
  const slot = state!.slot

  return (
    <main className="min-h-screen bg-background text-foreground" id="main-content">
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
        <nav className="mb-4 flex items-center gap-2 text-sm text-muted-foreground" aria-label="Breadcrumb">
          <button
            type="button"
            onClick={handleCancel}
            className="hover:text-foreground transition-colors"
          >
            Search
          </button>
          <span aria-hidden="true">/</span>
          <span className="text-foreground" aria-current="page">
            Confirm booking
          </span>
        </nav>

        <h1 className="mb-6 text-2xl font-semibold tracking-tight">Confirm your booking</h1>

        {/* MOD-007 countdown — UXR-502 */}
        <div
          className="mb-6 flex flex-col items-center rounded-xl border border-border bg-card p-6 text-center shadow-sm"
          role="region"
          aria-label="Slot reservation timer"
          data-uxr="UXR-502"
        >
          <CountdownTimer
            totalSeconds={LOCK_TOTAL_SECONDS}
            secondsRemaining={lockSecondsRemaining}
            size="md"
          />
          <p className="mt-3 text-sm text-muted-foreground">This slot is held for you.</p>
          <div className="mt-3 rounded-md bg-muted px-4 py-3 text-sm">
            <div className="font-medium text-foreground">{slot.providerName}</div>
            <div className="text-muted-foreground">
              {formatDate(slot.date)} · {formatTime(slot.startTime)} · {slot.durationMinutes} min
            </div>
          </div>
        </div>

        {/* Slot detail card */}
        <div className="rounded-xl border border-border bg-card p-6 shadow-sm">
          <dl className="space-y-2">
            <div className="flex justify-between py-1 text-sm">
              <dt className="text-muted-foreground">Provider</dt>
              <dd className="font-medium text-foreground">{slot.providerName}</dd>
            </div>
            <div className="flex justify-between py-1 text-sm">
              <dt className="text-muted-foreground">Specialty</dt>
              <dd className="text-foreground">{slot.specialty}</dd>
            </div>
            <div className="flex justify-between py-1 text-sm">
              <dt className="text-muted-foreground">Date</dt>
              <dd className="font-medium text-foreground">{formatDate(slot.date)}</dd>
            </div>
            <div className="flex justify-between py-1 text-sm">
              <dt className="text-muted-foreground">Time</dt>
              <dd className="font-medium text-foreground">{formatTime(slot.startTime)}</dd>
            </div>
            <div className="flex justify-between py-1 text-sm">
              <dt className="text-muted-foreground">Duration</dt>
              <dd className="text-foreground">{slot.durationMinutes} minutes</dd>
            </div>
          </dl>
        </div>

        <div className="mt-6 flex gap-3">
          <button
            type="button"
            onClick={() => void handleConfirm()}
            disabled={pageState === 'submitting'}
            className="flex-1 min-h-[44px] rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
          >
            {pageState === 'submitting' ? 'Confirming…' : 'Confirm booking'}
          </button>
          <button
            type="button"
            onClick={handleCancel}
            disabled={pageState === 'submitting'}
            className="rounded-md border border-border bg-card px-6 py-2 text-sm font-medium text-foreground transition hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-60"
          >
            Cancel
          </button>
        </div>
      </div>
    </main>
  )
}
