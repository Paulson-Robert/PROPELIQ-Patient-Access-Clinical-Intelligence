import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Activity, BarChart3, FileText, LayoutDashboard, LogOut, Users } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'

interface SystemStat {
  totalUsers: number
  totalUsersTrend: string
  todayActivityCount: number
  systemHealthy: boolean
}

const MOCK_STATS: SystemStat = {
  totalUsers: 892,
  totalUsersTrend: '+12 this week',
  todayActivityCount: 156,
  systemHealthy: true,
}

// AC-018/019/020: Admin dashboard landing — system overview with navigation to admin sections
export const AdminDashboardPage = () => {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const [stats, setStats] = useState<SystemStat | null>(null)

  useEffect(() => {
    // Replace with real API call when available
    setStats(MOCK_STATS)
  }, [])

  return (
    <>
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
      >
        Skip to main content
      </a>

      <div className="flex min-h-screen bg-background">
        {/* Sidebar — matches pattern used by other admin pages */}
        <nav
          className="hidden w-56 shrink-0 border-r border-border bg-card p-4 lg:flex lg:flex-col"
          aria-label="Admin navigation"
        >
          <div className="mb-6 flex items-center gap-2 text-sm font-semibold text-foreground">
            <LayoutDashboard className="h-5 w-5 text-primary" aria-hidden="true" />
            PropelIQ
          </div>
          <ul className="flex flex-1 flex-col gap-1" role="list">
            <li>
              <span
                className="flex items-center gap-2 rounded-md bg-primary/10 px-3 py-2 text-sm font-medium text-primary"
                aria-current="page"
              >
                <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden="true" />
                Dashboard
              </span>
            </li>
            <li>
              <a
                href="/admin/users"
                className="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
              >
                <Users className="h-4 w-4 shrink-0" aria-hidden="true" />
                User management
              </a>
            </li>
            <li>
              <a
                href="/admin/audit-log"
                className="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
              >
                <FileText className="h-4 w-4 shrink-0" aria-hidden="true" />
                Audit log
              </a>
            </li>
            <li>
              <a
                href="/admin/metrics"
                className="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
              >
                <BarChart3 className="h-4 w-4 shrink-0" aria-hidden="true" />
                Platform metrics
              </a>
            </li>
          </ul>

          <div className="mt-auto border-t border-border pt-3">
            <button
              type="button"
              onClick={logout}
              className="flex w-full items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground focus-visible:outline focus-visible:outline-2 focus-visible:outline-ring"
            >
              <LogOut className="h-4 w-4 shrink-0" aria-hidden="true" />
              Log out
            </button>
          </div>
        </nav>

        {/* Main content */}
        <div className="flex flex-1 flex-col">
          {/* Header */}
          <header className="flex items-center justify-between border-b border-border bg-card px-6 py-3">
            <h1 className="text-base font-semibold text-foreground">Admin dashboard</h1>
            <div
              className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-xs font-semibold text-primary-foreground"
              aria-label="User avatar"
            >
              AU
            </div>
          </header>

          <main className="flex-1 overflow-auto p-6" id="main">
            {/* Page heading */}
            <div className="mb-6">
              <h2 className="text-xl font-semibold text-foreground">System overview</h2>
              <p className="mt-1 text-sm text-muted-foreground">Platform health and activity summary.</p>
            </div>

            {/* Overview stat cards */}
            <section aria-label="System overview statistics" className="mb-8">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
                {/* Total users */}
                <div className="rounded-lg border border-border bg-card p-5">
                  <div className="text-sm text-muted-foreground">Total users</div>
                  <div className="mt-1 text-3xl font-bold text-foreground">
                    {stats ? stats.totalUsers.toLocaleString() : '—'}
                  </div>
                  {stats && (
                    <div className="mt-1 flex items-center gap-1 text-xs text-green-600 dark:text-green-400">
                      <Activity className="h-3 w-3" aria-hidden="true" />
                      {stats.totalUsersTrend}
                    </div>
                  )}
                </div>

                {/* Today's activity */}
                <div className="rounded-lg border border-border bg-card p-5">
                  <div className="text-sm text-muted-foreground">Today's activity</div>
                  <div className="mt-1 text-3xl font-bold text-foreground">
                    {stats ? stats.todayActivityCount.toLocaleString() : '—'}
                  </div>
                  <div className="mt-1 text-xs text-muted-foreground">Audit log entries</div>
                </div>

                {/* System health */}
                <div className="rounded-lg border border-border bg-card p-5">
                  <div className="text-sm text-muted-foreground">System health</div>
                  <div className="mt-2">
                    {stats?.systemHealthy ? (
                      <span
                        className="inline-flex items-center gap-1.5 rounded-full bg-green-100 px-2.5 py-1 text-xs font-medium text-green-800 dark:bg-green-900/30 dark:text-green-400"
                        role="status"
                      >
                        <svg
                          className="h-3 w-3"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2"
                          aria-hidden="true"
                        >
                          <polyline points="20 6 9 17 4 12" />
                        </svg>
                        All systems operational
                      </span>
                    ) : (
                      <span
                        className="inline-flex items-center gap-1.5 rounded-full bg-red-100 px-2.5 py-1 text-xs font-medium text-red-800 dark:bg-red-900/30 dark:text-red-400"
                        role="status"
                      >
                        Degraded
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </section>

            {/* Navigation cards */}
            <section aria-label="Admin sections">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
                <button
                  type="button"
                  onClick={() => navigate('/admin/users')}
                  className="group flex flex-col gap-3 rounded-lg border border-border bg-card p-5 text-left transition-colors hover:border-primary/40 hover:bg-accent focus-visible:outline focus-visible:outline-2 focus-visible:outline-ring"
                  aria-label="Go to User management"
                >
                  <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary/10 text-primary group-hover:bg-primary/20">
                    <Users className="h-6 w-6" aria-hidden="true" />
                  </div>
                  <div>
                    <div className="font-medium text-foreground">User management</div>
                    <p className="mt-1 text-sm text-muted-foreground">
                      Create, edit, and deactivate user accounts. Manage role assignments.
                    </p>
                  </div>
                </button>

                <button
                  type="button"
                  onClick={() => navigate('/admin/audit-log')}
                  className="group flex flex-col gap-3 rounded-lg border border-border bg-card p-5 text-left transition-colors hover:border-amber-400/40 hover:bg-accent focus-visible:outline focus-visible:outline-2 focus-visible:outline-ring"
                  aria-label="Go to Audit log"
                >
                  <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-amber-100 text-amber-700 group-hover:bg-amber-200 dark:bg-amber-900/30 dark:text-amber-400">
                    <FileText className="h-6 w-6" aria-hidden="true" />
                  </div>
                  <div>
                    <div className="font-medium text-foreground">Audit log</div>
                    <p className="mt-1 text-sm text-muted-foreground">
                      Review system activity for compliance. Search, filter, and export log entries.
                    </p>
                  </div>
                </button>

                <button
                  type="button"
                  onClick={() => navigate('/admin/metrics')}
                  className="group flex flex-col gap-3 rounded-lg border border-border bg-card p-5 text-left transition-colors hover:border-green-400/40 hover:bg-accent focus-visible:outline focus-visible:outline-2 focus-visible:outline-ring"
                  aria-label="Go to Platform metrics"
                >
                  <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-green-100 text-green-700 group-hover:bg-green-200 dark:bg-green-900/30 dark:text-green-400">
                    <BarChart3 className="h-6 w-6" aria-hidden="true" />
                  </div>
                  <div>
                    <div className="font-medium text-foreground">Platform metrics</div>
                    <p className="mt-1 text-sm text-muted-foreground">
                      Monitor adoption rates, appointment trends, and AI agreement metrics.
                    </p>
                  </div>
                </button>
              </div>
            </section>
          </main>
        </div>
      </div>
    </>
  )
}
