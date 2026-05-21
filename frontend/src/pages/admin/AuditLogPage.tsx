import { Fragment, useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ChevronLeft, ChevronRight, Download, FileText } from 'lucide-react'
import { cn } from '../../lib/utils'
import {
  auditLogApi,
  type AuditAction,
  type AuditLogEntry,
  type AuditResource,
} from '../../services/auditLogApi'

const PAGE_SIZE = 25

const ACTION_TYPES: AuditAction[] = [
  'Login',
  'Logout',
  'Create',
  'Update',
  'Delete',
  'View',
  'Export',
]

const RESOURCE_TYPES: AuditResource[] = [
  'User',
  'Appointment',
  'Patient',
  'Document',
  'Code mapping',
  'Session',
  'Audit log',
]

const ACTION_BADGE_CLASSES: Record<AuditAction, string> = {
  Login: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  Logout: 'bg-muted text-muted-foreground',
  Create: 'bg-muted text-muted-foreground',
  Update: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
  Delete: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
  View: 'bg-muted text-muted-foreground',
  Export: 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400',
}

const formatTimestamp = (iso: string): string => {
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}

export const AuditLogPage = () => {
  const navigate = useNavigate()

  // AC-02: actor and action type filter state
  const [actorFilter, setActorFilter] = useState('')
  const [actionFilter, setActionFilter] = useState<AuditAction | ''>('')
  const [resourceFilter, setResourceFilter] = useState<AuditResource | ''>('')
  // AC-01: date range filter state
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  // AC-03: pagination state
  const [page, setPage] = useState(1)

  const [entries, setEntries] = useState<AuditLogEntry[]>([])
  const [total, setTotal] = useState(0)
  const [isLoading, setIsLoading] = useState(false)
  const [listError, setListError] = useState<string | null>(null)
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set())

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE))

  const fetchEntries = useCallback(async () => {
    setIsLoading(true)
    setListError(null)
    try {
      const result = await auditLogApi.listEntries({
        actor: actorFilter,
        action: actionFilter,
        resource: resourceFilter,
        fromDate,
        toDate,
        page,
        pageSize: PAGE_SIZE,
      })
      setEntries(result.entries)
      setTotal(result.total)
    } catch {
      setListError('Failed to load audit log. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }, [actorFilter, actionFilter, resourceFilter, fromDate, toDate, page])

  // Reset to page 1 when filters change
  useEffect(() => {
    setPage(1)
  }, [actorFilter, actionFilter, resourceFilter, fromDate, toDate])

  useEffect(() => {
    void fetchEntries()
  }, [fetchEntries])

  const toggleExpand = (id: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) {
        next.delete(id)
      } else {
        next.add(id)
      }
      return next
    })
  }

  return (
    <>
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
      >
        Skip to main content
      </a>

      <div className="flex min-h-screen bg-background">
        {/* Sidebar */}
        <nav
          className="hidden w-56 shrink-0 border-r border-border bg-card p-4 lg:flex lg:flex-col"
          aria-label="Admin navigation"
        >
          <div className="mb-6 flex items-center gap-2 text-sm font-semibold text-foreground">
            <FileText className="h-5 w-5 text-primary" aria-hidden="true" />
            HealthAccess
          </div>
          <ul className="flex flex-col gap-1">
            <li>
              <a
                href="/dashboard/admin"
                className="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
              >
                Dashboard
              </a>
            </li>
            <li>
              <a
                href="/admin/users"
                className="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
              >
                Users
              </a>
            </li>
            <li>
              <span
                className="flex items-center gap-2 rounded-md bg-muted px-3 py-2 text-sm font-medium text-foreground"
                aria-current="page"
              >
                Audit log
              </span>
            </li>
          </ul>
        </nav>

        {/* Main content */}
        <div className="flex flex-1 flex-col">
          {/* Header */}
          <header className="flex items-center justify-between border-b border-border bg-card px-6 py-4">
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => navigate('/dashboard/admin')}
                className="flex items-center justify-center rounded-md p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                aria-label="Back to dashboard"
              >
                <ChevronLeft className="h-5 w-5" aria-hidden="true" />
              </button>
              <h1 className="text-xl font-semibold text-foreground">Audit log</h1>
            </div>
            {/* Export stub — format/endpoint not yet specified */}
            <button
              type="button"
              className="flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-sm font-medium text-foreground hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring"
              aria-label="Export audit log"
            >
              <Download className="h-4 w-4" aria-hidden="true" />
              Export
            </button>
          </header>

          <main id="main" className="flex-1 p-6">
            {/* AC-01 + AC-02: Filters */}
            <div className="mb-4 flex flex-wrap gap-3">
              {/* AC-02: actor filter */}
              <input
                type="search"
                value={actorFilter}
                onChange={(e) => setActorFilter(e.target.value)}
                placeholder="Filter by actor…"
                aria-label="Filter by actor"
                className="w-48 rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              />
              {/* AC-02: action type filter */}
              <select
                value={actionFilter}
                onChange={(e) => setActionFilter(e.target.value as AuditAction | '')}
                aria-label="Filter by action type"
                className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <option value="">All actions</option>
                {ACTION_TYPES.map((a) => (
                  <option key={a} value={a}>
                    {a}
                  </option>
                ))}
              </select>
              {/* Resource filter — per wireframe SCR-024 */}
              <select
                value={resourceFilter}
                onChange={(e) => setResourceFilter(e.target.value as AuditResource | '')}
                aria-label="Filter by resource"
                className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <option value="">All resources</option>
                {RESOURCE_TYPES.map((r) => (
                  <option key={r} value={r}>
                    {r}
                  </option>
                ))}
              </select>
              {/* AC-01: date range */}
              <input
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                aria-label="From date"
                className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              />
              <input
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                aria-label="To date"
                className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              />
            </div>

            {/* Error banner */}
            {listError && (
              <p
                role="alert"
                className="mb-4 rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive"
              >
                {listError}
              </p>
            )}

            {/* AC-01 + AC-02: Audit log table */}
            <div className="overflow-x-auto rounded-lg border border-border bg-card">
              <table className="w-full text-sm" aria-label="Audit log entries">
                <thead>
                  <tr className="border-b border-border bg-muted/50 text-left text-xs font-medium uppercase tracking-wide text-muted-foreground">
                    <th scope="col" className="px-4 py-3">
                      Timestamp
                    </th>
                    <th scope="col" className="px-4 py-3">
                      Actor
                    </th>
                    <th scope="col" className="px-4 py-3">
                      Action
                    </th>
                    <th scope="col" className="px-4 py-3">
                      Resource
                    </th>
                    <th scope="col" className="px-4 py-3">
                      Resource ID
                    </th>
                    <th scope="col" className="px-4 py-3">
                      <span className="sr-only">Details</span>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {isLoading && (
                    <tr>
                      <td colSpan={6} className="px-4 py-8 text-center text-muted-foreground">
                        Loading…
                      </td>
                    </tr>
                  )}
                  {!isLoading && entries.length === 0 && (
                    <tr>
                      <td colSpan={6} className="px-4 py-8 text-center text-muted-foreground">
                        No audit log entries found.
                      </td>
                    </tr>
                  )}
                  {!isLoading &&
                    entries.map((entry) => (
                      <Fragment key={entry.id}>
                        <tr className="border-b border-border last:border-0 hover:bg-muted/30">
                          <td className="px-4 py-3 font-mono text-xs text-muted-foreground">
                            {formatTimestamp(entry.timestamp)}
                          </td>
                          <td className="px-4 py-3 font-medium text-foreground">{entry.actor}</td>
                          <td className="px-4 py-3">
                            <span
                              className={cn(
                                'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
                                ACTION_BADGE_CLASSES[entry.action],
                              )}
                            >
                              {entry.action}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-muted-foreground">{entry.resource}</td>
                          <td className="px-4 py-3 font-mono text-xs text-muted-foreground">
                            {entry.resourceId ?? '—'}
                          </td>
                          <td className="px-4 py-3">
                            <button
                              type="button"
                              onClick={() => toggleExpand(entry.id)}
                              aria-expanded={expandedIds.has(entry.id)}
                              aria-controls={`details-${entry.id}`}
                              className="rounded px-2 py-1 text-xs font-medium text-foreground hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring"
                            >
                              {expandedIds.has(entry.id) ? 'Collapse' : 'Expand'}
                            </button>
                          </td>
                        </tr>
                        {expandedIds.has(entry.id) && (
                          <tr className="border-b border-border bg-muted/20">
                            <td colSpan={6} className="px-6 py-3" id={`details-${entry.id}`}>
                              <pre className="overflow-x-auto whitespace-pre-wrap break-words font-mono text-xs text-muted-foreground">
                                {JSON.stringify(entry.details, null, 2)}
                              </pre>
                            </td>
                          </tr>
                        )}
                      </Fragment>
                    ))}
                </tbody>
              </table>
            </div>

            {/* AC-03: Server-side pagination (25/page) */}
            <div className="mt-4 flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                {total === 0
                  ? 'No entries'
                  : `Showing ${(page - 1) * PAGE_SIZE + 1}–${Math.min(page * PAGE_SIZE, total)} of ${total} entries`}
              </p>
              <div className="flex items-center gap-1" role="navigation" aria-label="Pagination">
                <button
                  type="button"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page <= 1}
                  className="flex h-8 w-8 items-center justify-center rounded border border-border text-muted-foreground hover:bg-muted disabled:opacity-40 focus:outline-none focus:ring-2 focus:ring-ring"
                  aria-label="Previous page"
                >
                  <ChevronLeft className="h-4 w-4" aria-hidden="true" />
                </button>
                {Array.from({ length: totalPages }, (_, i) => i + 1)
                  .filter((p) => p === 1 || p === totalPages || Math.abs(p - page) <= 1)
                  .reduce<(number | '…')[]>((acc, p, idx, arr) => {
                    if (idx > 0 && p - (arr[idx - 1] as number) > 1) acc.push('…')
                    acc.push(p)
                    return acc
                  }, [])
                  .map((item, idx) =>
                    item === '…' ? (
                      <span key={`ellipsis-${idx}`} className="px-1 text-muted-foreground">
                        …
                      </span>
                    ) : (
                      <button
                        key={item}
                        type="button"
                        onClick={() => setPage(item as number)}
                        aria-current={page === item ? 'page' : undefined}
                        className={cn(
                          'flex h-8 w-8 items-center justify-center rounded border text-sm focus:outline-none focus:ring-2 focus:ring-ring',
                          page === item
                            ? 'border-primary bg-primary text-primary-foreground'
                            : 'border-border text-muted-foreground hover:bg-muted',
                        )}
                      >
                        {item}
                      </button>
                    ),
                  )}
                <button
                  type="button"
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page >= totalPages}
                  className="flex h-8 w-8 items-center justify-center rounded border border-border text-muted-foreground hover:bg-muted disabled:opacity-40 focus:outline-none focus:ring-2 focus:ring-ring"
                  aria-label="Next page"
                >
                  <ChevronRight className="h-4 w-4" aria-hidden="true" />
                </button>
              </div>
            </div>
          </main>
        </div>
      </div>
    </>
  )
}
