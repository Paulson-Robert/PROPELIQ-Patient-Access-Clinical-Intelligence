import { type FormEvent, useMemo, useState } from 'react'
import { PatientSearchInput } from '../../components/walkin/PatientSearchInput'
import {
  bookingApi,
  type PatientSearchResult,
  type WalkInBookingResponse,
} from '../../services/bookingApi'

const PROVIDERS = [
  { id: 'auto', label: 'Auto-assign (next available)' },
  { id: 'prov-001', label: 'Dr. Sarah Chen - Internal Medicine' },
  { id: 'prov-004', label: 'Dr. Lisa Nakamura - Family Medicine' },
  { id: 'prov-002', label: 'Dr. Michael Okafor - Cardiology' },
]

type WalkInMode = 'existing' | 'guest'

export const WalkInBookingPage = () => {
  const [mode, setMode] = useState<WalkInMode>('existing')
  const [selectedPatient, setSelectedPatient] = useState<PatientSearchResult | null>(null)
  const [guestName, setGuestName] = useState('')
  const [guestPhone, setGuestPhone] = useState('')
  const [guestEmail, setGuestEmail] = useState('')
  const [providerId, setProviderId] = useState('auto')
  const [reasonForVisit, setReasonForVisit] = useState('')

  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [bookingResult, setBookingResult] = useState<WalkInBookingResponse | null>(null)

  const canSubmit = useMemo(() => {
    if (mode === 'existing') {
      return selectedPatient !== null
    }

    return guestName.trim().length > 0
  }, [mode, selectedPatient, guestName])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!canSubmit) {
      setSubmitError(
        mode === 'existing'
          ? 'Select an existing patient before submitting.'
          : 'Guest name is required.',
      )
      return
    }

    setSubmitError(null)
    setIsSubmitting(true)

    try {
      const result = await bookingApi.submitWalkIn({
        patientId: mode === 'existing' ? selectedPatient?.id : undefined,
        guestName: mode === 'guest' ? guestName.trim() : undefined,
        guestPhone: mode === 'guest' ? guestPhone.trim() || undefined : undefined,
        guestEmail: mode === 'guest' ? guestEmail.trim() || undefined : undefined,
        providerId: providerId === 'auto' ? undefined : providerId,
        reasonForVisit: reasonForVisit.trim() || undefined,
      })

      setBookingResult(result)
      setReasonForVisit('')
      if (mode === 'guest') {
        setGuestName('')
        setGuestPhone('')
        setGuestEmail('')
      }
    } catch {
      setSubmitError('Unable to create walk-in booking. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="min-h-screen bg-background text-foreground" id="main-content">
      <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 lg:px-8">
        <nav className="mb-4 flex items-center gap-2 text-sm text-muted-foreground" aria-label="Breadcrumb">
          <a href="/dashboard/staff" className="transition-colors hover:text-foreground">
            Dashboard
          </a>
          <span aria-hidden="true">/</span>
          <span aria-current="page" className="text-foreground">
            Walk-in booking
          </span>
        </nav>

        <h1 className="text-2xl font-semibold tracking-tight">Walk-in booking</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Search an existing patient or register a guest walk-in and add them to the same-day queue.
        </p>

        <div className="mt-6 grid gap-5">
          <div className="inline-flex w-fit rounded-md border border-border bg-muted p-1">
            <button
              type="button"
              className={`rounded px-3 py-1.5 text-sm font-medium transition ${
                mode === 'existing' ? 'bg-background text-foreground shadow-sm' : 'text-muted-foreground'
              }`}
              onClick={() => {
                setMode('existing')
                setSubmitError(null)
              }}
            >
              Existing patient
            </button>
            <button
              type="button"
              className={`rounded px-3 py-1.5 text-sm font-medium transition ${
                mode === 'guest' ? 'bg-background text-foreground shadow-sm' : 'text-muted-foreground'
              }`}
              onClick={() => {
                setMode('guest')
                setSubmitError(null)
              }}
            >
              Guest walk-in
            </button>
          </div>

          {mode === 'existing' ? (
            <PatientSearchInput
              selectedPatient={selectedPatient}
              onSelectPatient={(patient) => {
                setSelectedPatient(patient)
                setSubmitError(null)
              }}
            />
          ) : (
            <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
              <h2 className="text-lg font-semibold tracking-tight">Guest details</h2>
              <p className="mt-1 text-sm text-muted-foreground">
                Minimum details required for a guest walk-in booking.
              </p>

              <div className="mt-4 grid gap-4 sm:grid-cols-2">
                <div className="sm:col-span-2">
                  <label htmlFor="guest-name" className="text-sm font-medium text-foreground">
                    Full name
                  </label>
                  <input
                    id="guest-name"
                    value={guestName}
                    onChange={(event) => setGuestName(event.target.value)}
                    className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                    placeholder="e.g., Jordan Walker"
                    required
                  />
                </div>

                <div>
                  <label htmlFor="guest-phone" className="text-sm font-medium text-foreground">
                    Phone (optional)
                  </label>
                  <input
                    id="guest-phone"
                    value={guestPhone}
                    onChange={(event) => setGuestPhone(event.target.value)}
                    className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                    placeholder="(555) 123-4567"
                  />
                </div>

                <div>
                  <label htmlFor="guest-email" className="text-sm font-medium text-foreground">
                    Email (optional)
                  </label>
                  <input
                    id="guest-email"
                    type="email"
                    value={guestEmail}
                    onChange={(event) => setGuestEmail(event.target.value)}
                    className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                    placeholder="jordan@example.com"
                  />
                </div>
              </div>
            </section>
          )}

          <form className="rounded-xl border border-border bg-card p-5 shadow-sm" onSubmit={(event) => void handleSubmit(event)}>
            <h2 className="text-lg font-semibold tracking-tight">Booking summary</h2>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <div className="sm:col-span-2 rounded-md border border-border bg-muted/40 p-3 text-sm">
                <p className="text-muted-foreground">Patient</p>
                <p className="mt-1 font-medium text-foreground">
                  {mode === 'existing'
                    ? selectedPatient?.name ?? 'No patient selected'
                    : guestName.trim() || 'Guest (name required)'}
                </p>
              </div>

              <div>
                <label htmlFor="walkin-provider" className="text-sm font-medium text-foreground">
                  Assign provider
                </label>
                <select
                  id="walkin-provider"
                  value={providerId}
                  onChange={(event) => setProviderId(event.target.value)}
                  className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                >
                  {PROVIDERS.map((provider) => (
                    <option key={provider.id} value={provider.id}>
                      {provider.label}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label htmlFor="reason-for-visit" className="text-sm font-medium text-foreground">
                  Reason for visit
                </label>
                <input
                  id="reason-for-visit"
                  value={reasonForVisit}
                  onChange={(event) => setReasonForVisit(event.target.value)}
                  className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                  placeholder="e.g., Acute ear pain"
                />
              </div>
            </div>

            {submitError ? (
              <p className="mt-4 rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm text-destructive" role="alert">
                {submitError}
              </p>
            ) : null}

            {bookingResult ? (
              <section className="mt-4 rounded-md border border-emerald-500/40 bg-emerald-500/10 px-4 py-3" aria-live="polite">
                <p className="text-sm font-semibold text-emerald-800">Walk-in added to same-day queue</p>
                <p className="mt-1 text-sm text-emerald-900">
                  {bookingResult.patientDisplayName} is queued with an estimated wait of {bookingResult.estimatedWaitMinutes} minutes.
                </p>
                <p className="mt-1 text-xs text-emerald-800">Queue ID: {bookingResult.queueId}</p>
              </section>
            ) : null}

            <button
              type="submit"
              disabled={isSubmitting}
              className="mt-5 min-h-11 rounded-md bg-primary px-5 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? 'Adding to queue...' : 'Add to queue'}
            </button>
          </form>
        </div>
      </div>
    </main>
  )
}