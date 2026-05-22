import { Link } from 'react-router-dom'
import {
  Calendar,
  FileText,
  MessageSquare,
  Upload,
  CheckCircle,
  Loader2,
} from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'

export const PatientDashboardPage = () => {
  const { user } = useAuth()
  const firstName = user?.fullName?.split(' ')[0] ?? 'there'

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
            <Link
              to="/booking/appointments/1"
              className="flex items-center gap-4 py-3 transition-colors hover:bg-muted/50"
            >
              <div
                className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary"
                aria-hidden="true"
              >
                SC
              </div>
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium text-foreground">Dr. Sarah Chen</div>
                <div className="text-xs text-muted-foreground">Internal Medicine</div>
              </div>
              <div className="text-right">
                <div className="text-sm font-medium text-foreground">Jan 27, 2025</div>
                <div className="text-xs text-muted-foreground">09:00 AM · 30 min</div>
              </div>
              <span className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
                Scheduled
              </span>
            </Link>
            <Link
              to="/booking/appointments/2"
              className="flex items-center gap-4 py-3 transition-colors hover:bg-muted/50"
            >
              <div
                className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary"
                aria-hidden="true"
              >
                LN
              </div>
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium text-foreground">Dr. Lisa Nakamura</div>
                <div className="text-xs text-muted-foreground">Family Medicine</div>
              </div>
              <div className="text-right">
                <div className="text-sm font-medium text-foreground">Jan 29, 2025</div>
                <div className="text-xs text-muted-foreground">02:00 PM · 30 min</div>
              </div>
              <span className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
                Scheduled
              </span>
            </Link>
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
            <div className="flex items-center gap-4 py-3">
              <FileText className="h-5 w-5 shrink-0 text-muted-foreground" aria-hidden="true" />
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium text-foreground">CBC-Results-2025-01.pdf</div>
                <div className="text-xs text-muted-foreground">Uploaded Jan 25 · 2.4 MB</div>
              </div>
              <span className="inline-flex items-center gap-1 rounded-full bg-green-500/10 px-2.5 py-0.5 text-xs font-medium text-green-700">
                <CheckCircle className="h-3 w-3" aria-hidden="true" />
                Completed
              </span>
            </div>
            <div className="flex items-center gap-4 py-3">
              <FileText className="h-5 w-5 shrink-0 text-muted-foreground" aria-hidden="true" />
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium text-foreground">Chest-Xray-Anterior.dicom</div>
                <div className="text-xs text-muted-foreground">Uploaded Jan 25 · 18.7 MB</div>
              </div>
              <span className="inline-flex items-center gap-1 rounded-full bg-amber-500/10 px-2.5 py-0.5 text-xs font-medium text-amber-700">
                <Loader2 className="h-3 w-3 animate-spin" aria-hidden="true" />
                Processing
              </span>
            </div>
          </div>
        </section>
      </div>

      {/* Notifications */}
      <section className="rounded-xl border border-border bg-card shadow-sm">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h3 className="text-base font-semibold text-foreground">Notifications</h3>
          <button
            type="button"
            className="text-sm font-medium text-muted-foreground transition hover:text-foreground"
          >
            Mark all read
          </button>
        </div>
        <div className="divide-y divide-border px-5">
          <div className="flex gap-3 py-3">
            <span
              className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-primary"
              aria-hidden="true"
            />
            <div>
              <div className="text-sm font-medium text-foreground">Appointment reminder</div>
              <div className="text-sm text-muted-foreground">
                Your appointment with Dr. Sarah Chen is tomorrow at 9:00 AM.
              </div>
              <div className="mt-1 text-xs text-muted-foreground">2 hours ago</div>
            </div>
          </div>
          <div className="flex gap-3 py-3">
            <span
              className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-primary"
              aria-hidden="true"
            />
            <div>
              <div className="text-sm font-medium text-foreground">Document processed</div>
              <div className="text-sm text-muted-foreground">
                CBC-Results-2025-01.pdf has been processed successfully.
              </div>
              <div className="mt-1 text-xs text-muted-foreground">Yesterday</div>
            </div>
          </div>
          <div className="flex gap-3 py-3">
            <span
              className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-transparent"
              aria-hidden="true"
            />
            <div>
              <div className="text-sm font-medium text-muted-foreground">Calendar synced</div>
              <div className="text-sm text-muted-foreground">
                Google Calendar sync completed. 2 appointments added.
              </div>
              <div className="mt-1 text-xs text-muted-foreground">2 days ago</div>
            </div>
          </div>
        </div>
      </section>
    </>
  )
}
