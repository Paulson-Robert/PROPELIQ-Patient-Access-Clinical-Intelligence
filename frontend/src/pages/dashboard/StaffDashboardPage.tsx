import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  AlertCircle,
  AlertTriangle,
  Info,
  TrendingUp,
  Users,
  UserCheck,
  Clock,
} from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'
import { queueApi, type QueueEntry, type QueueSummary } from '../../services/queueApi'

const RISK_STYLES: Record<string, string> = {
  High: 'inline-flex items-center gap-1 rounded-full bg-destructive/10 px-2 py-0.5 text-xs font-medium text-destructive',
  Medium: 'inline-flex items-center gap-1 rounded-full bg-amber-500/10 px-2 py-0.5 text-xs font-medium text-amber-700',
  Low: 'inline-flex items-center gap-1 rounded-full bg-green-500/10 px-2 py-0.5 text-xs font-medium text-green-700',
}

const STATUS_STYLES: Record<string, string> = {
  Arrived: 'inline-flex rounded-full bg-green-500/10 px-2 py-0.5 text-xs font-medium text-green-700',
  Scheduled: 'inline-flex rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary',
  Waiting: 'inline-flex rounded-full bg-amber-500/10 px-2 py-0.5 text-xs font-medium text-amber-700',
  WalkIn: 'inline-flex rounded-full bg-amber-500/10 px-2 py-0.5 text-xs font-medium text-amber-700',
  Cancelled: 'inline-flex rounded-full bg-destructive/10 px-2 py-0.5 text-xs font-medium text-destructive',
  Completed: 'inline-flex rounded-full bg-muted px-2 py-0.5 text-xs font-medium text-muted-foreground',
}

const getInitials = (name: string) =>
  name
    .split(' ')
    .slice(0, 2)
    .map((n) => n[0]?.toUpperCase() ?? '')
    .join('')

const today = new Date().toLocaleDateString('en-US', {
  weekday: 'long',
  year: 'numeric',
  month: 'long',
  day: 'numeric',
})

export const StaffDashboardPage = () => {
  const { user } = useAuth()
  const firstName = user?.fullName?.split(' ')[0] ?? 'there'

  const [entries, setEntries] = useState<QueueEntry[]>([])
  const [summary, setSummary] = useState<QueueSummary>({
    totalInQueue: 0,
    walkInCount: 0,
    arrivedCount: 0,
    avgWaitMinutes: 0,
  })
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let cancelled = false

    const load = async () => {
      try {
        const data = await queueApi.getTodayQueue()
        if (!cancelled) {
          setEntries(data.entries.slice(0, 5))
          setSummary(data.summary)
        }
      } catch {
        // Non-critical; dashboard degrades gracefully
      } finally {
        if (!cancelled) setIsLoading(false)
      }
    }

    load()
    return () => { cancelled = true }
  }, [])

  const pendingCount = summary.totalInQueue - summary.arrivedCount

  return (
    <>
      {/* Greeting */}
      <section className="mb-6" aria-labelledby="dashboard-greeting">
        <h2
          id="dashboard-greeting"
          className="text-2xl font-semibold tracking-tight text-foreground"
        >
          Good morning, {firstName}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">{today}</p>
      </section>

      {/* Summary cards */}
      <div
        className="mb-8 grid grid-cols-2 gap-4 sm:grid-cols-4"
        role="region"
        aria-label="Today's appointment summary"
      >
        <div className="rounded-xl border border-border bg-card p-4 shadow-sm">
          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">Total appointments</p>
            <Users className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
          </div>
          <p className="mt-2 text-3xl font-semibold text-foreground">
            {isLoading ? '—' : summary.totalInQueue}
          </p>
          <p className="mt-1 flex items-center gap-1 text-xs text-green-600">
            <TrendingUp className="h-3 w-3" aria-hidden="true" />
            +8% from last Monday
          </p>
        </div>

        <div className="rounded-xl border border-border bg-card p-4 shadow-sm">
          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">Walk-ins</p>
            <Users className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
          </div>
          <p className="mt-2 text-3xl font-semibold text-foreground">
            {isLoading ? '—' : summary.walkInCount}
          </p>
        </div>

        <div className="rounded-xl border border-border bg-card p-4 shadow-sm">
          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">Arrived</p>
            <UserCheck className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
          </div>
          <p className="mt-2 text-3xl font-semibold text-green-600">
            {isLoading ? '—' : summary.arrivedCount}
          </p>
        </div>

        <div className="rounded-xl border border-border bg-card p-4 shadow-sm">
          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">Pending</p>
            <Clock className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
          </div>
          <p className="mt-2 text-3xl font-semibold text-amber-600">
            {isLoading ? '—' : pendingCount}
          </p>
        </div>
      </div>

      {/* Main panels */}
      <div className="mb-6 grid grid-cols-1 gap-6 lg:grid-cols-2" style={{ alignItems: 'start' }}>
        {/* Queue preview */}
        <section
          className="rounded-xl border border-border bg-card shadow-sm"
          aria-labelledby="queue-preview-title"
        >
          <div className="flex items-center justify-between border-b border-border px-5 py-4">
            <h3
              id="queue-preview-title"
              className="text-base font-semibold text-foreground"
            >
              Queue preview
            </h3>
            <Link
              to="/queue/same-day"
              className="text-sm font-medium text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring rounded"
            >
              View full queue
            </Link>
          </div>

          {isLoading ? (
            <div className="px-5 py-8 text-center text-sm text-muted-foreground" aria-live="polite">
              Loading queue…
            </div>
          ) : entries.length === 0 ? (
            <div className="px-5 py-8 text-center text-sm text-muted-foreground">
              No patients in queue yet.
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border">
                    <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground">#</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground">Patient</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground">Time</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground">Status</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground">Risk</th>
                  </tr>
                </thead>
                <tbody>
                  {entries.map((entry) => (
                    <tr
                      key={entry.id}
                      className="border-b border-border last:border-0 transition-colors hover:bg-muted/50"
                    >
                      <td className="px-4 py-3 text-muted-foreground">{entry.position}</td>
                      <td className="px-4 py-3">
                        <Link
                          to={`/clinical/patient/${entry.patientId}`}
                          className="flex items-center gap-2 text-foreground hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring rounded"
                        >
                          <span
                            className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary"
                            aria-hidden="true"
                          >
                            {getInitials(entry.patientName)}
                          </span>
                          <span className="font-medium">{entry.patientName}</span>
                        </Link>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{entry.appointmentTime}</td>
                      <td className="px-4 py-3">
                        <span
                          className={
                            STATUS_STYLES[entry.bookingType === 'WalkIn' ? 'WalkIn' : entry.status] ??
                            STATUS_STYLES.Scheduled
                          }
                        >
                          {entry.bookingType === 'WalkIn' ? 'Walk-in' : entry.status}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <span className={RISK_STYLES[entry.riskLevel] ?? RISK_STYLES.Low}>
                          {entry.riskLevel}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        {/* Alerts panel */}
        <section
          className="rounded-xl border border-border bg-card shadow-sm"
          aria-labelledby="alerts-title"
        >
          <div className="flex items-center justify-between border-b border-border px-5 py-4">
            <h3
              id="alerts-title"
              className="text-base font-semibold text-foreground"
            >
              Alerts
            </h3>
            <span className="rounded-full bg-destructive/10 px-2 py-0.5 text-xs font-semibold text-destructive">
              3
            </span>
          </div>
          <div className="flex flex-col gap-3 px-5 py-4">
            {/* Failed notification */}
            <div
              role="alert"
              className="flex gap-3 rounded-lg border border-destructive/30 bg-destructive/5 p-3"
            >
              <AlertCircle
                className="mt-0.5 h-5 w-5 shrink-0 text-destructive"
                aria-hidden="true"
              />
              <div>
                <p className="text-sm font-semibold text-foreground">Failed notification</p>
                <p className="mt-0.5 text-sm text-muted-foreground">
                  SMS delivery failed for PAT-003 appointment reminder.
                </p>
              </div>
            </div>

            {/* Conflicts */}
            <div
              role="alert"
              className="flex gap-3 rounded-lg border border-amber-300/50 bg-amber-500/5 p-3"
            >
              <AlertTriangle
                className="mt-0.5 h-5 w-5 shrink-0 text-amber-600"
                aria-hidden="true"
              />
              <div>
                <p className="text-sm font-semibold text-foreground">2 conflicts pending</p>
                <Link
                  to="/queue/same-day"
                  className="mt-0.5 text-sm text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring rounded"
                >
                  Review conflicts for James O'Brien
                </Link>
              </div>
            </div>

            {/* Low-confidence extraction */}
            <div
              role="alert"
              className="flex gap-3 rounded-lg border border-blue-300/50 bg-blue-500/5 p-3"
            >
              <Info
                className="mt-0.5 h-5 w-5 shrink-0 text-blue-600"
                aria-hidden="true"
              />
              <div>
                <p className="text-sm font-semibold text-foreground">
                  Low-confidence extraction
                </p>
                <Link
                  to="/queue/same-day"
                  className="mt-0.5 text-sm text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring rounded"
                >
                  Review code mapping for Maria Santos
                </Link>
              </div>
            </div>
          </div>
        </section>
      </div>

      {/* Quick actions */}
      <div
        className="flex flex-wrap gap-3"
        role="region"
        aria-label="Quick actions"
      >
        <Link
          to="/booking/walk-in"
          id="quick-walkin"
          className="inline-flex items-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground shadow-sm transition hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          Walk-in booking
        </Link>
        <Link
          to="/queue/same-day"
          id="quick-queue"
          className="inline-flex items-center rounded-md border border-border bg-secondary px-4 py-2 text-sm font-medium text-secondary-foreground shadow-sm transition hover:bg-secondary/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          View full queue
        </Link>
        {/* <Link
          to="/intake"
          id="quick-intake"
          className="inline-flex items-center rounded-md border border-border bg-background px-4 py-2 text-sm font-medium text-foreground shadow-sm transition hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          Patient intake
        </Link> */}
      </div>
    </>
  )
}
