import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ArrowLeft, ClipboardList, Loader2, Plus } from 'lucide-react'
import { bookingApi, type PatientIntakeRecord } from '../../services/bookingApi'

const formatDate = (iso: string): string => {
  const d = new Date(iso)
  return d.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
  })
}

const modeBadge = (mode: string) => {
  if (mode === 'AI') {
    return (
      <span className="inline-flex rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
        AI-assisted
      </span>
    )
  }
  return (
    <span className="inline-flex rounded-full bg-amber-500/10 px-2.5 py-0.5 text-xs font-medium text-amber-700">
      Manual
    </span>
  )
}

const statusBadge = (completedAt: string | null) => {
  if (completedAt) {
    return (
      <span className="inline-flex rounded-full bg-emerald-500/10 px-2.5 py-0.5 text-xs font-medium text-emerald-700">
        Completed
      </span>
    )
  }
  return (
    <span className="inline-flex rounded-full bg-amber-500/10 px-2.5 py-0.5 text-xs font-medium text-amber-700">
      Draft
    </span>
  )
}

export const IntakeListPage = () => {
  const [intakes, setIntakes] = useState<PatientIntakeRecord[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false

    const fetchIntakes = async () => {
      try {
        setLoading(true)
        const records = await bookingApi.getMyIntakes()
        if (!cancelled) {
          setIntakes(records)
        }
      } catch {
        if (!cancelled) {
          setIntakes([])
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void fetchIntakes()
    return () => { cancelled = true }
  }, [])

  return (
    <main className="min-h-screen bg-background text-foreground" id="main-content">
      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
        <header className="mb-6 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <Link
              to="/dashboard/patient"
              className="inline-flex items-center justify-center rounded-md p-1.5 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              aria-label="Back to dashboard"
            >
              <ArrowLeft className="h-5 w-5" aria-hidden="true" />
            </Link>
            <h1 className="text-xl font-semibold tracking-tight">Intake history</h1>
          </div>

          <Link
            to="/intake"
            className="inline-flex items-center gap-2 rounded-md bg-primary px-3 py-2 text-sm font-medium text-primary-foreground transition hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-ring"
          >
            <Plus className="h-4 w-4" aria-hidden="true" />
            New intake
          </Link>
        </header>

        {loading ? (
          <div className="flex items-center justify-center py-20">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <span className="ml-3 text-sm text-muted-foreground">Loading intakes…</span>
          </div>
        ) : intakes.length === 0 ? (
          <section
            className="flex flex-col items-center rounded-xl border border-dashed border-border bg-muted/30 py-16 text-center"
            aria-label="No intakes"
          >
            <ClipboardList className="mb-3 h-10 w-10 text-muted-foreground" aria-hidden="true" />
            <p className="text-base font-medium text-foreground">No intake records yet</p>
            <p className="mt-1 text-sm text-muted-foreground">
              Complete a pre-visit intake to see your history here.
            </p>
            <Link
              to="/intake"
              className="mt-4 inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-ring"
            >
              <Plus className="h-4 w-4" aria-hidden="true" />
              Start intake
            </Link>
          </section>
        ) : (
          <section aria-label="Intake records">
            <div className="overflow-x-auto rounded-xl border border-border">
              <table className="w-full text-sm" aria-label="Intake records">
                <thead>
                  <tr className="border-b border-border bg-muted/40 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                    <th scope="col" className="px-4 py-3">Mode</th>
                    <th scope="col" className="px-4 py-3">Reason for visit</th>
                    <th scope="col" className="px-4 py-3">Status</th>
                    <th scope="col" className="px-4 py-3">Date</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border bg-card">
                  {intakes.map((intake) => (
                    <tr
                      key={intake.id}
                      className="transition-colors hover:bg-muted/30"
                    >
                      <td className="px-4 py-3">{modeBadge(intake.intakeMode)}</td>
                      <td className="px-4 py-3 font-medium text-foreground">
                        {intake.reasonForVisit ?? '—'}
                      </td>
                      <td className="px-4 py-3">{statusBadge(intake.completedAt)}</td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {formatDate(intake.completedAt ?? intake.lastModifiedAt)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}
      </div>
    </main>
  )
}
