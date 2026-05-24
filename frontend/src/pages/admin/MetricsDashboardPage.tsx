import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ChevronLeft } from 'lucide-react'
import { AdminSidebar } from '../../components/admin/AdminSidebar'
import { KpiCard } from '../../components/metrics/KpiCard'
import { TrendChart, type ChartDataPoint } from '../../components/metrics/TrendChart'
import { metricsApi, type DateRange, type MetricsData } from '../../services/metricsApi'

const DATE_RANGE_OPTIONS: { value: DateRange; label: string }[] = [
  { value: '7d', label: 'Last 7 days' },
  { value: '30d', label: 'Last 30 days' },
  { value: '90d', label: 'Last 90 days' },
  { value: 'ytd', label: 'Year to date' },
]

// Thin data down to at most N evenly-spaced points so charts stay readable
const downsample = (data: { label: string; value: number }[], max = 14): ChartDataPoint[] => {
  if (data.length <= max) return data
  const step = Math.ceil(data.length / max)
  return data.filter((_, i) => i % step === 0)
}

export const MetricsDashboardPage = () => {
  const navigate = useNavigate()
  const [range, setRange] = useState<DateRange>('30d')
  const [data, setData] = useState<MetricsData | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [fetchError, setFetchError] = useState<string | null>(null)

  const fetchData = useCallback(async () => {
    setIsLoading(true)
    setFetchError(null)
    try {
      const result = await metricsApi.getMetrics({ range })
      setData(result)
    } catch {
      setFetchError('Failed to load metrics. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }, [range])

  useEffect(() => {
    void fetchData()
  }, [fetchData])

  const volumePoints: ChartDataPoint[] = downsample(
    (data?.dailyVolume ?? []).map((d) => ({ label: d.date, value: d.count })),
  )
  const statusPoints: ChartDataPoint[] = (data?.statusBreakdown ?? []).map((d) => ({
    label: d.status,
    value: d.count,
  }))
  const confidencePoints: ChartDataPoint[] = downsample(
    (data?.confidenceTrend ?? []).map((d) => ({ label: d.date, value: d.score })),
  )

  return (
    <>
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
      >
        Skip to main content
      </a>

      <div className="flex min-h-screen bg-background">
        <AdminSidebar activePage="metrics" />

        {/* Main content */}
        <div className="flex flex-1 flex-col">
          {/* Header */}
          <header className="flex items-center justify-between border-b border-border bg-card px-6 py-3">
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => navigate('/dashboard/admin')}
                className="rounded-md p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground focus-visible:outline focus-visible:outline-2 focus-visible:outline-ring"
                aria-label="Back to dashboard"
              >
                <ChevronLeft className="h-5 w-5" aria-hidden="true" />
              </button>
              <h1 className="text-base font-semibold text-foreground">Platform metrics</h1>
            </div>

            {/* AC-03: Date range filter */}
            <div className="flex items-center gap-3">
              <label htmlFor="date-range-select" className="sr-only">
                Date range
              </label>
              <select
                id="date-range-select"
                value={range}
                onChange={(e) => setRange(e.target.value as DateRange)}
                className="rounded-md border border-border bg-background px-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                aria-label="Select date range"
              >
                {DATE_RANGE_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>
          </header>

          <main className="flex-1 overflow-auto p-6" id="main">
            {fetchError && (
              <div
                role="alert"
                className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/20 dark:text-red-400"
              >
                {fetchError}
              </div>
            )}

            {isLoading && (
              <div
                role="status"
                aria-live="polite"
                aria-label="Loading metrics"
                className="mb-4 text-sm text-muted-foreground"
              >
                Loading…
              </div>
            )}

            {/* AC-01: KPI cards */}
            <section aria-label="Key performance indicators">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
                <KpiCard
                  label="Total appointments"
                  value={data ? data.summary.totalAppointments.toLocaleString() : '—'}
                  trend={data?.summary.totalAppointmentsTrend ?? ''}
                  trendUp={data?.summary.totalAppointmentsTrendUp}
                />
                <KpiCard
                  label="Avg. wait time"
                  value={data ? `${data.summary.avgWaitTimeMin} min` : '—'}
                  trend={data?.summary.avgWaitTimeTrend ?? ''}
                  trendUp={data?.summary.avgWaitTimeTrendUp}
                />
                <KpiCard
                  label="No-show rate"
                  value={data ? `${data.summary.noShowRate}%` : '—'}
                  trend={data?.summary.noShowRateTrend ?? ''}
                  trendUp={data?.summary.noShowRateTrendUp}
                />
                <KpiCard
                  label="Active users"
                  value={data ? data.summary.activeUsers.toLocaleString() : '—'}
                  trend={data?.summary.activeUsersTrend ?? ''}
                  trendUp={data?.summary.activeUsersTrendUp}
                />
              </div>
            </section>

            {/* AC-02: Trend charts */}
            <section aria-label="Trend charts" className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
              <TrendChart
                type="line"
                data={volumePoints}
                title="Appointments per day"
                ariaLabel="Line chart showing daily appointment volume"
              />
              <TrendChart
                type="bar"
                data={statusPoints}
                title="Appointments by status"
                ariaLabel="Bar chart showing appointment status distribution"
              />
              <div className="lg:col-span-2">
                <TrendChart
                  type="line"
                  data={confidencePoints}
                  title="AI code mapping confidence"
                  ariaLabel="Line chart showing average AI confidence score trend"
                  valueFormatter={(v) => `${v}%`}
                />
              </div>
            </section>
          </main>
        </div>
      </div>
    </>
  )
}
