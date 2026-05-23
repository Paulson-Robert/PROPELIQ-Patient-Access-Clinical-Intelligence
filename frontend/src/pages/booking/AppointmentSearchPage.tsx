import { useCallback, useEffect, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { SlotCard } from '../../components/booking/SlotCard'
import {
  BookingError,
  bookingApi,
  type AvailabilitySlot,
  type LockSlotResponse,
} from '../../services/bookingApi'

const SPECIALTIES = [
  'Internal Medicine',
  'Cardiology',
  'Dermatology',
  'Orthopedics',
  'Family Medicine',
]

const LOCK_DURATION_SECONDS = 30

type SearchState = 'idle' | 'loading' | 'done'

const groupSlotsByDate = (slots: AvailabilitySlot[]): Map<string, AvailabilitySlot[]> => {
  const grouped = new Map<string, AvailabilitySlot[]>()

  for (const slot of slots) {
    if (!slot.date) continue
    const existing = grouped.get(slot.date) ?? []
    existing.push(slot)
    grouped.set(slot.date, existing)
  }

  return grouped
}

const formatDateHeader = (isoDate: string): string => {
  const [year, month, day] = isoDate.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}

const formatSlotSummary = (slot: AvailabilitySlot): string => {
  const [hourStr, minuteStr] = slot.startTime.split(':')
  const hour = parseInt(hourStr, 10)
  const suffix = hour >= 12 ? 'PM' : 'AM'
  const displayHour = hour % 12 === 0 ? 12 : hour % 12
  const time = `${displayHour.toString().padStart(2, '0')}:${minuteStr} ${suffix}`

  const [year, month, day] = slot.date.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  const shortDate = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })

  return `${slot.providerName} — ${shortDate}, ${time}`
}

export const AppointmentSearchPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()

  const [provider, setProvider] = useState(() => searchParams.get('provider') ?? '')
  const [specialty, setSpecialty] = useState(() => searchParams.get('specialty') ?? '')
  const [from, setFrom] = useState(() => searchParams.get('from') ?? '')
  const [to, setTo] = useState(() => searchParams.get('to') ?? '')

  const [searchState, setSearchState] = useState<SearchState>('idle')
  const [slots, setSlots] = useState<AvailabilitySlot[]>([])
  const [searchError, setSearchError] = useState<string | null>(null)

  const [selectedSlot, setSelectedSlot] = useState<AvailabilitySlot | null>(null)
  const [lockResponse, setLockResponse] = useState<LockSlotResponse | null>(null)
  const [lockSecondsRemaining, setLockSecondsRemaining] = useState(LOCK_DURATION_SECONDS)
  const [lockError, setLockError] = useState<string | null>(null)
  const [isLocking, setIsLocking] = useState(false)
  const [lockExpired, setLockExpired] = useState(false)

  const lockTickRef = useRef<number | null>(null)

  const clearLockTick = useCallback(() => {
    if (lockTickRef.current !== null) {
      window.clearInterval(lockTickRef.current)
      lockTickRef.current = null
    }
  }, [])

  const startLockCountdown = useCallback(
    (lockResp: LockSlotResponse) => {
      clearLockTick()

      const expiry = new Date(lockResp.expiresAtUtc).getTime()

      lockTickRef.current = window.setInterval(() => {
        const remaining = Math.max(0, Math.round((expiry - Date.now()) / 1000))
        setLockSecondsRemaining(remaining)

        if (remaining <= 0) {
          clearLockTick()
          setLockExpired(true)
          setSelectedSlot(null)
          setLockResponse(null)
        }
      }, 500)
    },
    [clearLockTick],
  )

  useEffect(() => clearLockTick, [clearLockTick])

  const handleSearch = async () => {
    setSearchState('loading')
    setSearchError(null)
    setSelectedSlot(null)
    setLockResponse(null)
    setLockExpired(false)
    clearLockTick()

    try {
      const results = await bookingApi.searchSlots({ provider, specialty, from, to })
      const sorted = [...results].sort((a, b) =>
        `${a.date}T${a.startTime}`.localeCompare(`${b.date}T${b.startTime}`),
      )
      setSlots(sorted)
    } catch {
      setSearchError('Unable to load available slots. Please try again.')
    } finally {
      setSearchState('done')
    }
  }

  const handleSelectSlot = async (slot: AvailabilitySlot) => {
    if (selectedSlot?.id === slot.id) return

    setIsLocking(true)
    setLockError(null)
    clearLockTick()

    try {
      const lockResp = await bookingApi.lockSlot(slot.id)
      setSelectedSlot(slot)
      setLockResponse(lockResp)
      setLockExpired(false)
      setLockSecondsRemaining(lockResp.lockDurationSeconds)
      startLockCountdown(lockResp)
    } catch (err) {
      if (err instanceof BookingError) {
        if (err.code === 'SLOT_LOCKED') {
          setLockError(err.message)
          setSlots((prev) =>
            prev.map((s) => (s.id === slot.id ? { ...s, isLocked: true } : s)),
          )
        } else if (err.code === 'SLOT_UNAVAILABLE') {
          setLockError('This slot is no longer available. Results have been refreshed.')
          setSlots((prev) => prev.filter((s) => s.id !== slot.id))
        } else {
          setLockError('Could not reserve the slot. Please try again.')
        }
      } else {
        setLockError('Could not reserve the slot. Please try again.')
      }
    } finally {
      setIsLocking(false)
    }
  }

  const handleLockExpire = useCallback(() => {
    setLockExpired(true)
    setSelectedSlot(null)
    setLockResponse(null)
    clearLockTick()
  }, [clearLockTick])

  const handleConfirm = () => {
    if (!selectedSlot || !lockResponse) return

    navigate('/booking/confirm', {
      state: {
        slot: selectedSlot,
        lockToken: lockResponse.lockToken,
        expiresAtUtc: lockResponse.expiresAtUtc,
      },
    })
  }

  const groupedSlots = groupSlotsByDate(slots)

  return (
    <div
      className="text-foreground"
    >
      <a
        href="#search-form"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
      >
        Skip to search
      </a>

      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <nav className="mb-4 flex items-center gap-2 text-sm text-muted-foreground" aria-label="Breadcrumb">
          <a href="/dashboard/patient" className="hover:text-foreground transition-colors">
            Dashboard
          </a>
          <span aria-hidden="true">/</span>
          <span className="text-foreground" aria-current="page">
            Book appointment
          </span>
        </nav>

        <h1 className="mb-6 text-2xl font-semibold tracking-tight">Book an appointment</h1>

        {/* Filter Bar */}
        <form
          id="search-form"
          role="search"
          aria-label="Search available appointments"
          className="mb-6 flex flex-wrap items-end gap-3 rounded-lg border border-border bg-muted/40 p-4"
          onSubmit={(e) => {
            e.preventDefault()
            void handleSearch()
          }}
        >
          <div className="flex min-w-[180px] flex-1 flex-col gap-1">
            <label htmlFor="search-provider" className="text-xs font-medium text-muted-foreground">
              Provider
            </label>
            <input
              id="search-provider"
              type="text"
              placeholder="e.g., Dr. Sarah Chen"
              value={provider}
              onChange={(e) => setProvider(e.target.value)}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </div>

          <div className="flex min-w-[160px] flex-1 flex-col gap-1">
            <label htmlFor="search-specialty" className="text-xs font-medium text-muted-foreground">
              Specialty
            </label>
            <select
              id="search-specialty"
              value={specialty}
              onChange={(e) => setSpecialty(e.target.value)}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            >
              <option value="">All specialties</option>
              {SPECIALTIES.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </div>

          <div className="flex min-w-[140px] flex-col gap-1">
            <label htmlFor="search-from" className="text-xs font-medium text-muted-foreground">
              From
            </label>
            <input
              id="search-from"
              type="date"
              value={from}
              onChange={(e) => setFrom(e.target.value)}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </div>

          <div className="flex min-w-[140px] flex-col gap-1">
            <label htmlFor="search-to" className="text-xs font-medium text-muted-foreground">
              To
            </label>
            <input
              id="search-to"
              type="date"
              value={to}
              onChange={(e) => setTo(e.target.value)}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </div>

          <button
            type="submit"
            disabled={searchState === 'loading'}
            className="min-h-[44px] rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Search for available slots"
          >
            {searchState === 'loading' ? 'Searching…' : 'Search'}
          </button>
        </form>

        {/* Alerts */}
        {lockExpired ? (
          <div
            role="alert"
            className="mb-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-4 py-3 text-sm text-amber-700"
          >
            Slot hold expired — please select again.
          </div>
        ) : null}

        {lockError ? (
          <div
            role="alert"
            className="mb-4 rounded-md border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive"
          >
            {lockError}
          </div>
        ) : null}

        {searchError ? (
          <div
            role="alert"
            className="mb-4 rounded-md border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive"
          >
            {searchError}
          </div>
        ) : null}

        {/* Results */}
        <div aria-live="polite" aria-busy={searchState === 'loading'}>
          {searchState === 'loading' ? (
            <p className="py-12 text-center text-sm text-muted-foreground">
              Searching for available slots…
            </p>
          ) : searchState === 'done' && slots.length === 0 ? (
            /* AC-07: No results empty state */
            <div className="rounded-lg border border-border bg-card px-6 py-12 text-center">
              <h2 className="text-base font-semibold text-foreground">No available slots found</h2>
              <p className="mt-2 text-sm text-muted-foreground">
                Try adjusting your filters — a different date range, specialty, or provider may have
                availability.
              </p>
            </div>
          ) : searchState === 'done' ? (
            Array.from(groupedSlots.entries()).map(([date, dateSlots]) => (
              <section key={date} className="mb-6">
                <h2 className="mb-3 text-sm font-semibold text-foreground">
                  {formatDateHeader(date)}
                </h2>
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
                  {dateSlots.map((slot) => (
                    <SlotCard
                      key={slot.id}
                      slot={slot}
                      isSelected={selectedSlot?.id === slot.id}
                      lockSecondsRemaining={
                        selectedSlot?.id === slot.id ? lockSecondsRemaining : undefined
                      }
                      onSelect={handleSelectSlot}
                      onLockExpire={handleLockExpire}
                    />
                  ))}
                </div>
              </section>
            ))
          ) : (
            <p className="py-12 text-center text-sm text-muted-foreground">
              Enter your search criteria above and press Search.
            </p>
          )}
        </div>
      </div>

      {/* Sticky Confirm Bar — shown when slot is selected */}
      {selectedSlot && lockResponse ? (
        <div className="sticky bottom-0 border-t border-border bg-background px-4 py-4 sm:px-6">
          <div className="mx-auto flex max-w-5xl items-center justify-between gap-4">
            <div>
              <div className="text-sm font-medium text-foreground">
                Selected: {formatSlotSummary(selectedSlot)}
              </div>
              <div className="text-xs text-muted-foreground">
                {selectedSlot.specialty} · {selectedSlot.durationMinutes} min
              </div>
            </div>
            <button
              type="button"
              onClick={handleConfirm}
              disabled={isLocking}
              className="min-h-[44px] rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
            >
              Confirm booking
            </button>
          </div>
        </div>
      ) : null}
    </div>
  )
}
