import { authHeaders } from './authTokenStore'

export type DateRange = '7d' | '30d' | '90d' | 'ytd'

export interface MetricsSummary {
  totalAppointments: number
  totalAppointmentsTrend: string
  totalAppointmentsTrendUp: boolean
  avgWaitTimeMin: number
  avgWaitTimeTrend: string
  avgWaitTimeTrendUp: boolean
  noShowRate: number
  noShowRateTrend: string
  noShowRateTrendUp: boolean
  activeUsers: number
  activeUsersTrend: string
  activeUsersTrendUp: boolean
}

export interface DailyVolume {
  date: string
  count: number
}

export interface StatusBreakdown {
  status: string
  count: number
}

export interface ConfidencePoint {
  date: string
  score: number
}

export interface MetricsData {
  summary: MetricsSummary
  dailyVolume: DailyVolume[]
  statusBreakdown: StatusBreakdown[]
  confidenceTrend: ConfidencePoint[]
}

export interface MetricsParams {
  range: DateRange
}

export class MetricsApiError extends Error {
  status: number
  code?: string

  constructor(message: string, status: number, code?: string) {
    super(message)
    this.name = 'MetricsApiError'
    this.status = status
    this.code = code
  }
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const USE_MOCK = (import.meta.env.VITE_USE_MOCK_AUTH ?? 'true') !== 'false'

const wait = (ms = 250): Promise<void> =>
  new Promise((resolve) => {
    window.setTimeout(resolve, ms)
  })

const getJson = async <TResponse>(path: string): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'GET',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    credentials: 'include',
  })

  if (!response.ok) {
    let message = 'Request failed'
    let code: string | undefined
    try {
      const parsed = (await response.json()) as { message?: string; code?: string }
      message = parsed.message ?? message
      code = parsed.code
    } catch {
      // Fallback to default message
    }
    throw new MetricsApiError(message, response.status, code)
  }

  return (await response.json()) as TResponse
}

const buildDailyVolume = (days: number): DailyVolume[] => {
  const result: DailyVolume[] = []
  const base = new Date('2026-05-22')
  for (let i = days - 1; i >= 0; i--) {
    const d = new Date(base)
    d.setDate(d.getDate() - i)
    const mm = String(d.getMonth() + 1).padStart(2, '0')
    const dd = String(d.getDate()).padStart(2, '0')
    result.push({
      date: `${mm}/${dd}`,
      count: Math.floor(30 + Math.sin(i * 0.4) * 12 + (days - i) * 0.3),
    })
  }
  return result
}

const buildConfidenceTrend = (days: number): ConfidencePoint[] => {
  const result: ConfidencePoint[] = []
  const base = new Date('2026-05-22')
  for (let i = days - 1; i >= 0; i--) {
    const d = new Date(base)
    d.setDate(d.getDate() - i)
    const mm = String(d.getMonth() + 1).padStart(2, '0')
    const dd = String(d.getDate()).padStart(2, '0')
    result.push({
      date: `${mm}/${dd}`,
      score: Math.min(99, Math.round(78 + ((days - i) / days) * 11 + Math.sin(i * 0.3) * 2)),
    })
  }
  return result
}

const MOCK_STATUS: StatusBreakdown[] = [
  { status: 'Completed', count: 848 },
  { status: 'Cancelled', count: 187 },
  { status: 'No-show', count: 52 },
  { status: 'Scheduled', count: 162 },
]

const getMockData = (range: DateRange): MetricsData => {
  const days = range === '7d' ? 7 : range === '30d' ? 30 : range === '90d' ? 90 : 143
  return {
    summary: {
      totalAppointments: 1247,
      totalAppointmentsTrend: '↑ 12% vs. prior period',
      totalAppointmentsTrendUp: true,
      avgWaitTimeMin: 18,
      avgWaitTimeTrend: '↓ 3 min improvement',
      avgWaitTimeTrendUp: true,
      noShowRate: 4.2,
      noShowRateTrend: '↓ 1.1%',
      noShowRateTrendUp: true,
      activeUsers: 342,
      activeUsersTrend: '↑ 8%',
      activeUsersTrendUp: true,
    },
    dailyVolume: buildDailyVolume(days),
    statusBreakdown: MOCK_STATUS,
    confidenceTrend: buildConfidenceTrend(days),
  }
}

// ---------------------------------------------------------------------------
// Backend ↔ Frontend shape adapters
// ---------------------------------------------------------------------------

interface BackendMetricsSummary {
  totalAppointments: number
  avgWaitTimeMinutes: number
  noShowRatePercent: number
  activeUsers: number
}

interface BackendTrendPoint {
  period: string
  count: number
}

interface BackendStatusBreakdown {
  status: string
  count: number
}

interface BackendConfidenceTrendPoint {
  period: string
  avgConfidencePercent: number
}

interface BackendMetricsResult {
  summary: BackendMetricsSummary
  dailyVolume: BackendTrendPoint[]
  statusBreakdown: BackendStatusBreakdown[]
  confidenceTrend: BackendConfidenceTrendPoint[]
}

const mapMetrics = (raw: BackendMetricsResult): MetricsData => ({
  summary: {
    totalAppointments: raw.summary.totalAppointments,
    totalAppointmentsTrend: '',
    totalAppointmentsTrendUp: true,
    avgWaitTimeMin: Math.round(raw.summary.avgWaitTimeMinutes),
    avgWaitTimeTrend: '',
    avgWaitTimeTrendUp: true,
    noShowRate: Math.round(raw.summary.noShowRatePercent * 10) / 10,
    noShowRateTrend: '',
    noShowRateTrendUp: false,
    activeUsers: raw.summary.activeUsers,
    activeUsersTrend: '',
    activeUsersTrendUp: true,
  },
  dailyVolume: raw.dailyVolume.map((p) => ({ date: p.period, count: p.count })),
  statusBreakdown: raw.statusBreakdown,
  confidenceTrend: raw.confidenceTrend.map((p) => ({
    date: p.period,
    score: Math.round(p.avgConfidencePercent),
  })),
})

export const metricsApi = {
  getMetrics: async (params: MetricsParams): Promise<MetricsData> => {
    if (USE_MOCK) {
      await wait()
      return getMockData(params.range)
    }

    // Map frontend DateRange keys to backend MetricsDateRange enum names
    const RANGE_MAP: Record<DateRange, string> = {
      '7d': 'Last7Days',
      '30d': 'Last30Days',
      '90d': 'Last90Days',
      ytd: 'YearToDate',
    }
    const qs = new URLSearchParams({
      range: RANGE_MAP[params.range],
      groupBy: 'Day',
    })

    const raw = await getJson<BackendMetricsResult>(`/api/admin/metrics?${qs.toString()}`)
    return mapMetrics(raw)
  },
}
