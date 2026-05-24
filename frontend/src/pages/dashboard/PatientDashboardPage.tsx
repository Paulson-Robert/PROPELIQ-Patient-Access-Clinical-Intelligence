import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  Calendar,
  FileText,
  MessageSquare,
  Upload,
  CheckCircle,
  Loader2,
  AlertCircle,
} from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'
import {
  bookingApi,
  type AppointmentRecord,
  type PatientDocumentRecord,
  type PatientNotificationRecord,
} from '../../services/bookingApi'

export const PatientDashboardPage = () => {
  const { user } = useAuth()
  const firstName = user?.fullName?.split(' ')[0] ?? 'there'

  const [appointments, setAppointments] = useState<AppointmentRecord[]>([])
  const [documents, setDocuments] = useState<PatientDocumentRecord[]>([])
  const [notifications, setNotifications] = useState<PatientNotificationRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchDashboardData = useCallback(async (signal?: AbortSignal) => {
    setLoading(true)
    setError(null)

    const [apptResult, docResult, notifResult] = await Promise.allSettled([
      bookingApi.getMyAppointments(),
      bookingApi.getMyDocuments(),
      bookingApi.getMyNotifications(),
    ])

    if (signal?.aborted) return

    // At least appointments must succeed for the dashboard to be useful
    if (apptResult.status === 'rejected') {
      setError('Unable to load dashboard data. Please try again.')
      setLoading(false)
      return
    }

    setAppointments(apptResult.value)
    setDocuments(docResult.status === 'fulfilled' ? docResult.value : [])
    setNotifications(notifResult.status === 'fulfilled' ? notifResult.value : [])
    setLoading(false)
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    void fetchDashboardData(controller.signal)
    return () => { controller.abort() }
  }, [fetchDashboardData])

  const upcomingAppointments = appointments
    .filter((a) => a.status === 'Scheduled')
    .sort((a, b) => `${a.date}T${a.startTime}`.localeCompare(`${b.date}T${b.startTime}`))
    .slice(0, 5)

  const recentDocuments = documents.slice(0, 5)
  const recentNotifications = notifications.slice(0, 5)

  const formatDate = (dateStr: string): string => {
    const [year, month, day] = dateStr.split('-').map(Number)
    const d = new Date(year, month - 1, day)
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
  }

  const formatTime = (time: string): string => {
    const [h, m] = time.split(':').map(Number)
    const period = h >= 12 ? 'PM' : 'AM'
    const hour12 = h % 12 || 12
    return `${hour12.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')} ${period}`
  }

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return `${bytes} B`
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
  }

  const formatRelativeTime = (isoDate: string): string => {
    const now = Date.now()
    const then = new Date(isoDate).getTime()
    const diffMs = now - then
    const diffMins = Math.floor(diffMs / 60_000)
    if (diffMins < 1) return 'Just now'
    if (diffMins < 60) return `${diffMins} min ago`
    const diffHours = Math.floor(diffMins / 60)
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`
    const diffDays = Math.floor(diffHours / 24)
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`
    return formatDate(isoDate.substring(0, 10))
  }

  const docStatusBadge = (status: string) => {
    if (status === 'Completed') {
      return (
        <span className="inline-flex items-center gap-1 rounded-full bg-green-500/10 px-2.5 py-0.5 text-xs font-medium text-green-700">
          <CheckCircle className="h-3 w-3" aria-hidden="true" />
          Completed
        </span>
      )
    }
    if (status === 'Failed') {
      return (
        <span className="inline-flex items-center gap-1 rounded-full bg-red-500/10 px-2.5 py-0.5 text-xs font-medium text-red-700">
          <AlertCircle className="h-3 w-3" aria-hidden="true" />
          Failed
        </span>
      )
    }
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-amber-500/10 px-2.5 py-0.5 text-xs font-medium text-amber-700">
        <Loader2 className="h-3 w-3 animate-spin" aria-hidden="true" />
        {status}
      </span>
    )
  }

  const notificationTitle = (type: string): string => {
    switch (type) {
      case 'AppointmentReminder': return 'Appointment reminder'
      case 'BookingConfirmation': return 'Booking confirmed'
      case 'CancellationConfirmation': return 'Appointment cancelled'
      case 'DocumentProcessed': return 'Document processed'
      default: return 'Notification'
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
        <span className="ml-3 text-sm text-muted-foreground">Loading dashboard…</span>
      </div>
    )
  }

  if (error) {
    return (
      <div className="rounded-xl border border-destructive/30 bg-destructive/5 p-6 text-center">
        <AlertCircle className="mx-auto h-8 w-8 text-destructive" />
        <p className="mt-2 text-sm text-destructive">{error}</p>
        <button
          type="button"
          onClick={() => void fetchDashboardData()}
          className="mt-4 inline-flex items-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground"
        >
          Retry
        </button>
      </div>
    )
  }

  return (
    <>
      {/* Welcome */}
      <section className="mb-8">
        <h2 className="text-2xl font-semibold tracking-tight text-foreground">
          Welcome back, {firstName}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">
          Here&apos;s an overview of your health activities.
        </p>
      </section>

      {/* Quick Actions */}
      <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Link
          to="/booking/search"
          id="quick-book"
          className="rounded-xl border border-border bg-card p-4 shadow-sm transition hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
              <Calendar className="h-5 w-5" aria-hidden="true" />
            </div>
            <div>
              <div className="text-sm font-semibold text-foreground">Book appointment</div>
              <div className="text-xs text-muted-foreground">Find available slots</div>
            </div>
          </div>
        </Link>

        <Link
          to="/intake"
          id="quick-intake"
          className="rounded-xl border border-border bg-card p-4 shadow-sm transition hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-green-500/10 text-green-600">
              <MessageSquare className="h-5 w-5" aria-hidden="true" />
            </div>
            <div>
              <div className="text-sm font-semibold text-foreground">Start intake</div>
              <div className="text-xs text-muted-foreground">Complete pre-visit form</div>
            </div>
          </div>
        </Link>

        <Link
          to="/documents/upload"
          id="quick-upload"
          className="rounded-xl border border-border bg-card p-4 shadow-sm transition hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-amber-500/10 text-amber-600">
              <Upload className="h-5 w-5" aria-hidden="true" />
            </div>
            <div>
              <div className="text-sm font-semibold text-foreground">Upload documents</div>
              <div className="text-xs text-muted-foreground">Add clinical records</div>
            </div>
          </div>
        </Link>
      </div>

      {/* Two-column layout: Upcoming Appointments + Recent Documents */}
      <div className="mb-6 grid grid-cols-1 gap-6 lg:grid-cols-2" style={{ alignItems: 'start' }}>
        {/* Upcoming Appointments */}
        <section className="rounded-xl border border-border bg-card shadow-sm">
          <div className="flex items-center justify-between border-b border-border px-5 py-4">
            <h3 className="text-base font-semibold text-foreground">Upcoming appointments</h3>
            <Link to="/booking/history" className="text-sm font-medium text-primary hover:underline">
              View all
            </Link>
          </div>
          <div className="divide-y divide-border px-5">
            {upcomingAppointments.length === 0 ? (
              <div className="py-6 text-center text-sm text-muted-foreground">
                No upcoming appointments. Book one to get started.
              </div>
            ) : (
              upcomingAppointments.map((appt) => (
                <Link
                  key={appt.id}
                  to={`/booking/appointments/${appt.id}`}
                  className="flex items-center gap-4 py-3 transition-colors hover:bg-muted/50"
                >
                  <div
                    className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary"
                    aria-hidden="true"
                  >
                    {appt.providerInitials}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-sm font-medium text-foreground">{appt.providerName}</div>
                    <div className="text-xs text-muted-foreground">{appt.specialty}</div>
                  </div>
                  <div className="text-right">
                    <div className="text-sm font-medium text-foreground">{formatDate(appt.date)}</div>
                    <div className="text-xs text-muted-foreground">{formatTime(appt.startTime)} · {appt.durationMinutes} min</div>
                  </div>
                  <span className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
                    Scheduled
                  </span>
                </Link>
              ))
            )}
          </div>
          <div className="border-t border-border px-5 py-3">
            <Link
              to="/booking/search"
              id="btn-book-now"
              className="inline-flex items-center rounded-md border border-border bg-background px-3 py-1.5 text-sm font-medium text-foreground transition hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              Book new appointment
            </Link>
          </div>
        </section>

        {/* Recent Documents */}
        <section className="rounded-xl border border-border bg-card shadow-sm">
          <div className="flex items-center justify-between border-b border-border px-5 py-4">
            <h3 className="text-base font-semibold text-foreground">Recent documents</h3>
            <Link to="/documents" className="text-sm font-medium text-primary hover:underline">
              View all
            </Link>
          </div>
          <div className="divide-y divide-border px-5">
            {recentDocuments.length === 0 ? (
              <div className="py-6 text-center text-sm text-muted-foreground">
                No documents uploaded yet.
              </div>
            ) : (
              recentDocuments.map((doc) => (
                <div key={doc.id} className="flex items-center gap-4 py-3">
                  <FileText className="h-5 w-5 shrink-0 text-muted-foreground" aria-hidden="true" />
                  <div className="flex-1 min-w-0">
                    <div className="text-sm font-medium text-foreground">{doc.fileName}</div>
                    <div className="text-xs text-muted-foreground">
                      Uploaded {formatRelativeTime(doc.uploadedAt)} · {formatFileSize(doc.fileSizeBytes)}
                    </div>
                  </div>
                  {docStatusBadge(doc.processingStatus)}
                </div>
              ))
            )}
          </div>
        </section>
      </div>

      {/* Notifications */}
      <section className="rounded-xl border border-border bg-card shadow-sm">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h3 className="text-base font-semibold text-foreground">Notifications</h3>
        </div>
        <div className="divide-y divide-border px-5">
          {recentNotifications.length === 0 ? (
            <div className="py-6 text-center text-sm text-muted-foreground">
              No notifications yet.
            </div>
          ) : (
            recentNotifications.map((notif) => (
              <div key={notif.id} className="flex gap-3 py-3">
                <span
                  className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-primary"
                  aria-hidden="true"
                />
                <div>
                  <div className="text-sm font-medium text-foreground">
                    {notificationTitle(notif.notificationType)}
                  </div>
                  <div className="text-sm text-muted-foreground">
                    Sent via {notif.channel.toLowerCase()} · {notif.status.toLowerCase()}
                  </div>
                  <div className="mt-1 text-xs text-muted-foreground">
                    {formatRelativeTime(notif.createdAt)}
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </section>
    </>
  )
}
