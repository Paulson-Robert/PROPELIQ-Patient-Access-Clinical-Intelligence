import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Loader2 } from 'lucide-react'
import { StatusBadge, type AppointmentStatus } from '../../components/booking/StatusBadge'
import { bookingApi, type AppointmentRecord } from '../../services/bookingApi'

interface AppointmentHistoryItem {
  id: string
  date: string
  startTime: string
  providerName: string
  specialty: string
  status: AppointmentStatus
}

const PAGE_SIZE = 10

const mapRecordToHistoryItem = (record: AppointmentRecord): AppointmentHistoryItem => {
  const statusMap: Record<string, AppointmentStatus> = {
    Scheduled: 'Confirmed',
    Arrived: 'Confirmed',
    Completed: 'Completed',
    Cancelled: 'Cancelled',
    NoShow: 'No-Show',
  }

  return {
    id: record.id,
    date: record.date,
    startTime: record.startTime,
    providerName: record.providerName,
    specialty: record.specialty,
    status: statusMap[record.status] ?? 'Confirmed',
  }
}

const STATUS_FILTERS: Array<{ value: 'all' | AppointmentStatus; label: string }> = [
  { value: 'all', label: 'All statuses' },
  { value: 'Confirmed', label: 'Confirmed' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Cancelled', label: 'Cancelled' },
  { value: 'No-Show', label: 'No-Show' },
]

const formatDateTime = (date: string, startTime: string): string => {
  const [year, month, day] = date.split('-').map(Number)
  const [hour, minute] = startTime.split(':').map(Number)

  const value = new Date(year, month - 1, day, hour, minute)
  return value.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
}

const toDateTimeKey = (item: AppointmentHistoryItem): number => {
  return new Date(`${item.date}T${item.startTime}:00`).getTime()
}

export const AppointmentHistoryPage = () => {
  const [appointments, setAppointments] = useState<AppointmentHistoryItem[]>([])
  const [loading, setLoading] = useState(true)
  const [statusFilter, setStatusFilter] = useState<'all' | AppointmentStatus>('all')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [page, setPage] = useState(1)

  useEffect(() => {
    let cancelled = false

    const fetchAppointments = async () => {
      try {
        setLoading(true)
        const records = await bookingApi.getMyAppointments()
        if (!cancelled) {
          setAppointments(records.map(mapRecordToHistoryItem))
        }
      } catch {
        if (!cancelled) {
          setAppointments([])
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void fetchAppointments()
    return () => { cancelled = true }
  }, [])

  const filteredAppointments = useMemo(() => {
    const sorted = [...appointments].sort((a, b) => toDateTimeKey(b) - toDateTimeKey(a))

    return sorted.filter((item) => {
      const statusMatch = statusFilter === 'all' || item.status === statusFilter
      const fromMatch = fromDate === '' || item.date >= fromDate
      const toMatch = toDate === '' || item.date <= toDate

      return statusMatch && fromMatch && toMatch
    })
  }, [appointments, statusFilter, fromDate, toDate])

  const totalPages = Math.max(1, Math.ceil(filteredAppointments.length / PAGE_SIZE))
  const safePage = Math.min(page, totalPages)
  const startIndex = (safePage - 1) * PAGE_SIZE
  const pageItems = filteredAppointments.slice(startIndex, startIndex + PAGE_SIZE)

  const fromItem = filteredAppointments.length === 0 ? 0 : startIndex + 1
  const toItem = Math.min(startIndex + PAGE_SIZE, filteredAppointments.length)

  const updateFilter = (nextStatus: 'all' | AppointmentStatus, nextFrom: string, nextTo: string) => {
    setStatusFilter(nextStatus)
    setFromDate(nextFrom)
    setToDate(nextTo)
    setPage(1)
  }

  return (
    <div className="text-foreground">
      <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
        <nav className="mb-4 flex items-center gap-2 text-sm text-muted-foreground" aria-label="Breadcrumb">
          <Link to="/dashboard/patient" className="transition-colors hover:text-foreground">
            Dashboard
          </Link>
          <span aria-hidden="true">/</span>
          <span aria-current="page" className="text-foreground">
            Appointment history
          </span>
        </nav>

        <header className="mb-6 flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-2xl font-semibold tracking-tight">Appointment history</h1>
          <Link
            to="/booking/search"
            className="inline-flex h-11 items-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring"
          >
            Book new
          </Link>
        </header>

        {loading ? (
          <div className="flex items-center justify-center py-20">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <span className="ml-3 text-sm text-muted-foreground">Loading appointments…</span>
          </div>
        ) : (
        <>
        <section className="mb-4 flex flex-wrap items-end gap-3 rounded-lg border border-border bg-muted/40 p-4" aria-label="Appointment history filters">
          <div className="flex min-w-44 flex-1 flex-col gap-1">
            <label htmlFor="history-status" className="text-xs font-medium text-muted-foreground">
              Status
            </label>
            <select
              id="history-status"
              value={statusFilter}
              onChange={(event) => updateFilter(event.target.value as 'all' | AppointmentStatus, fromDate, toDate)}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            >
              {STATUS_FILTERS.map((status) => (
                <option key={status.value} value={status.value}>
                  {status.label}
                </option>
              ))}
            </select>
          </div>

          <div className="flex min-w-40 flex-1 flex-col gap-1">
            <label htmlFor="history-from" className="text-xs font-medium text-muted-foreground">
              From
            </label>
            <input
              id="history-from"
              type="date"
              value={fromDate}
              onChange={(event) => updateFilter(statusFilter, event.target.value, toDate)}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </div>

          <div className="flex min-w-40 flex-1 flex-col gap-1">
            <label htmlFor="history-to" className="text-xs font-medium text-muted-foreground">
              To
            </label>
            <input
              id="history-to"
              type="date"
              value={toDate}
              onChange={(event) => updateFilter(statusFilter, fromDate, event.target.value)}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </div>
        </section>

        {pageItems.length === 0 ? (
          <section className="rounded-xl border border-dashed border-border bg-card p-8 text-center">
            <h2 className="text-lg font-semibold">No appointments yet</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              Once you book appointments, your full history will appear here.
            </p>
            <Link
              to="/booking/search"
              className="mt-5 inline-flex h-11 items-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring"
            >
              Book your first appointment
            </Link>
          </section>
        ) : (
          <>
            <div className="overflow-hidden rounded-xl border border-border bg-card">
              <div className="hidden md:block">
                <table className="min-w-full" aria-label="Appointment history table">
                  <thead>
                    <tr className="border-b border-border bg-muted/40 text-left text-xs uppercase tracking-wide text-muted-foreground">
                      <th className="px-4 py-3 font-medium">Date</th>
                      <th className="px-4 py-3 font-medium">Provider</th>
                      <th className="px-4 py-3 font-medium">Specialty</th>
                      <th className="px-4 py-3 font-medium">Status</th>
                      <th className="px-4 py-3 font-medium">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {pageItems.map((item) => (
                      <tr key={item.id} className="border-b border-border/70 text-sm last:border-b-0">
                        <td className="px-4 py-3 font-medium text-foreground">{formatDateTime(item.date, item.startTime)}</td>
                        <td className="px-4 py-3">{item.providerName}</td>
                        <td className="px-4 py-3">{item.specialty}</td>
                        <td className="px-4 py-3">
                          <StatusBadge status={item.status} />
                        </td>
                        <td className="px-4 py-3">
                          <Link
                            to={`/booking/appointments/${item.id}`}
                            className="rounded-md px-2 py-1 text-sm font-medium text-primary transition hover:bg-primary/10 focus:outline-none focus:ring-2 focus:ring-ring"
                          >
                            View
                          </Link>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="grid gap-3 p-3 md:hidden" aria-label="Appointment history cards">
                {pageItems.map((item) => (
                  <article key={item.id} className="rounded-lg border border-border bg-background p-4">
                    <p className="text-sm font-medium text-foreground">{formatDateTime(item.date, item.startTime)}</p>
                    <p className="mt-1 text-sm">{item.providerName}</p>
                    <p className="text-sm text-muted-foreground">{item.specialty}</p>
                    <div className="mt-3 flex items-center justify-between gap-3">
                      <StatusBadge status={item.status} />
                      <Link
                        to={`/booking/appointments/${item.id}`}
                        className="rounded-md px-2 py-1 text-sm font-medium text-primary transition hover:bg-primary/10 focus:outline-none focus:ring-2 focus:ring-ring"
                      >
                        View
                      </Link>
                    </div>
                  </article>
                ))}
              </div>
            </div>

            <footer className="mt-4 flex flex-wrap items-center justify-between gap-3 text-sm text-muted-foreground">
              <p>
                Showing {fromItem}-{toItem} of {filteredAppointments.length} appointments
              </p>
              <nav className="flex items-center gap-1" aria-label="Pagination">
                <button
                  type="button"
                  onClick={() => setPage((current) => Math.max(1, current - 1))}
                  disabled={safePage === 1}
                  className="h-9 w-9 rounded-md border border-border bg-card px-2 text-foreground disabled:cursor-not-allowed disabled:opacity-60"
                  aria-label="Previous page"
                >
                  &lt;
                </button>
                {Array.from({ length: totalPages }).map((_, index) => {
                  const pageNumber = index + 1
                  return (
                    <button
                      key={pageNumber}
                      type="button"
                      onClick={() => setPage(pageNumber)}
                      aria-current={safePage === pageNumber ? 'page' : undefined}
                      className={
                        safePage === pageNumber
                          ? 'h-9 w-9 rounded-md border border-primary bg-primary/10 px-2 font-medium text-primary'
                          : 'h-9 w-9 rounded-md border border-border bg-card px-2 text-foreground'
                      }
                    >
                      {pageNumber}
                    </button>
                  )
                })}
                <button
                  type="button"
                  onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
                  disabled={safePage === totalPages}
                  className="h-9 w-9 rounded-md border border-border bg-card px-2 text-foreground disabled:cursor-not-allowed disabled:opacity-60"
                  aria-label="Next page"
                >
                  &gt;
                </button>
              </nav>
            </footer>
          </>
        )}
        </>
        )}
      </div>
    </div>
  )
}

export type { AppointmentHistoryItem }
