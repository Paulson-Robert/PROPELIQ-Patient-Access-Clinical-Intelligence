import { authHeaders } from './authTokenStore'

export type QueueStatus = 'Scheduled' | 'Waiting' | 'Arrived' | 'Cancelled' | 'Completed'

export type RiskLevel = 'High' | 'Medium' | 'Low'

export type BookingType = 'Scheduled' | 'WalkIn'

export interface QueueEntry {
  id: string
  position: number
  patientId: string
  patientName: string
  appointmentTime: string
  providerName: string
  status: QueueStatus
  bookingType: BookingType
  arrivalTimestamp?: string
  riskLevel: RiskLevel
  estimatedWaitMinutes: number
}

export interface QueueSummary {
  totalInQueue: number
  walkInCount: number
  arrivedCount: number
  avgWaitMinutes: number
}

export interface QueueResponse {
  entries: QueueEntry[]
  summary: QueueSummary
}

export interface MarkArrivedPayload {
  entryId: string
}

export interface MarkArrivedResponse {
  entryId: string
  arrivalTimestamp: string
}

export interface ReorderPayload {
  entryId: string
  newPosition: number
  reason: string
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const USE_MOCK_QUEUE = (import.meta.env.VITE_USE_MOCK_AUTH ?? 'true') !== 'false'

const wait = (ms = 250): Promise<void> =>
  new Promise((resolve) => {
    window.setTimeout(resolve, ms)
  })

const postJson = async <TResponse>(path: string, body: unknown): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    credentials: 'include',
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    const message = await response.text().catch(() => 'Request failed')
    throw new Error(message)
  }

  return (await response.json()) as TResponse
}

const getJson = async <TResponse>(path: string): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: authHeaders(),
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error('Request failed')
  }

  return (await response.json()) as TResponse
}

const adaptQueueEntryDto = (raw: {
  appointmentId: string
  position: number
  patientId: string
  patientName: string
  appointmentTime: string
  providerName: string
  status: QueueStatus
  bookingType: BookingType
  arrivalTimestamp?: string | null
  riskLevel: RiskLevel
  estimatedWaitMinutes: number
}): QueueEntry => ({
  id: raw.appointmentId,
  position: raw.position,
  patientId: raw.patientId,
  patientName: raw.patientName,
  appointmentTime: raw.appointmentTime,
  providerName: raw.providerName,
  status: raw.status,
  bookingType: raw.bookingType,
  arrivalTimestamp: raw.arrivalTimestamp ?? undefined,
  riskLevel: raw.riskLevel,
  estimatedWaitMinutes: raw.estimatedWaitMinutes,
})

const adaptQueueResponseDto = (raw: {
  entries: Array<{
    appointmentId: string
    position: number
    patientId: string
    patientName: string
    appointmentTime: string
    providerName: string
    status: QueueStatus
    bookingType: BookingType
    arrivalTimestamp?: string | null
    riskLevel: RiskLevel
    estimatedWaitMinutes: number
  }>
  summary: QueueSummary
}): QueueResponse => ({
  entries: raw.entries.map(adaptQueueEntryDto),
  summary: raw.summary,
})

const mockQueue: QueueEntry[] = [
  {
    id: 'appt-001',
    position: 1,
    patientId: 'pat-001',
    patientName: 'Maria Santos',
    appointmentTime: '09:00',
    providerName: 'Dr. Sarah Chen',
    status: 'Arrived',
    bookingType: 'Scheduled',
    arrivalTimestamp: new Date(Date.now() - 20 * 60_000).toISOString(),
    riskLevel: 'High',
    estimatedWaitMinutes: 5,
  },
  {
    id: 'appt-002',
    position: 2,
    patientId: 'pat-002',
    patientName: "James O'Brien-Fitzgerald",
    appointmentTime: '09:15',
    providerName: 'Dr. Lisa Nakamura',
    status: 'Waiting',
    bookingType: 'WalkIn',
    riskLevel: 'Medium',
    estimatedWaitMinutes: 15,
  },
  {
    id: 'appt-003',
    position: 3,
    patientId: 'pat-003',
    patientName: 'Priya Sharma',
    appointmentTime: '09:30',
    providerName: 'Dr. Michael Okafor',
    status: 'Scheduled',
    bookingType: 'Scheduled',
    riskLevel: 'Low',
    estimatedWaitMinutes: 25,
  },
  {
    id: 'appt-004',
    position: 4,
    patientId: 'pat-004',
    patientName: 'Robert Kim',
    appointmentTime: '09:30',
    providerName: 'Dr. Sarah Chen',
    status: 'Scheduled',
    bookingType: 'Scheduled',
    riskLevel: 'Low',
    estimatedWaitMinutes: 30,
  },
  {
    id: 'appt-005',
    position: 5,
    patientId: 'pat-005',
    patientName: 'Aisha Johnson',
    appointmentTime: '09:45',
    providerName: 'Unassigned',
    status: 'Waiting',
    bookingType: 'WalkIn',
    riskLevel: 'High',
    estimatedWaitMinutes: 35,
  },
]

const computeSummary = (entries: QueueEntry[]): QueueSummary => ({
  totalInQueue: entries.length,
  walkInCount: entries.filter((e) => e.bookingType === 'WalkIn').length,
  arrivedCount: entries.filter((e) => e.status === 'Arrived').length,
  avgWaitMinutes:
    entries.length === 0
      ? 0
      : Math.round(
          entries.reduce((sum, e) => sum + e.estimatedWaitMinutes, 0) / entries.length,
        ),
})

const mockQueueApi = {
  async getTodayQueue(): Promise<QueueResponse> {
    await wait()
    const sorted = [...mockQueue].sort((a, b) => a.position - b.position)
    return { entries: sorted, summary: computeSummary(sorted) }
  },

  async markArrived(payload: MarkArrivedPayload): Promise<MarkArrivedResponse> {
    await wait(300)

    const entry = mockQueue.find((e) => e.id === payload.entryId)
    if (!entry) {
      throw new Error('Queue entry not found')
    }

    if (entry.status === 'Arrived') {
      return { entryId: entry.id, arrivalTimestamp: entry.arrivalTimestamp! }
    }

    const ts = new Date().toISOString()
    entry.status = 'Arrived'
    entry.arrivalTimestamp = ts

    return { entryId: entry.id, arrivalTimestamp: ts }
  },

  async reorderQueue(payload: ReorderPayload): Promise<QueueResponse> {
    await wait(200)

    const fromIndex = mockQueue.findIndex((e) => e.id === payload.entryId)
    if (fromIndex === -1) {
      throw new Error('Queue entry not found')
    }

    const toIndex = payload.newPosition - 1
    const [moved] = mockQueue.splice(fromIndex, 1)
    mockQueue.splice(toIndex, 0, moved)

    mockQueue.forEach((e, i) => {
      e.position = i + 1
    })

    const sorted = [...mockQueue]
    return { entries: sorted, summary: computeSummary(sorted) }
  },
}

export const queueApi = {
  async getTodayQueue(): Promise<QueueResponse> {
    if (USE_MOCK_QUEUE || !API_BASE_URL) {
      return mockQueueApi.getTodayQueue()
    }

    const raw = await getJson<{
      entries: Array<{
        appointmentId: string
        position: number
        patientId: string
        patientName: string
        appointmentTime: string
        providerName: string
        status: QueueStatus
        bookingType: BookingType
        arrivalTimestamp?: string | null
        riskLevel: RiskLevel
        estimatedWaitMinutes: number
      }>
      summary: QueueSummary
    }>('/api/queue/today')

    return adaptQueueResponseDto(raw)
  },

  async markArrived(payload: MarkArrivedPayload): Promise<MarkArrivedResponse> {
    if (USE_MOCK_QUEUE || !API_BASE_URL) {
      return mockQueueApi.markArrived(payload)
    }

    const raw = await postJson<{
      appointmentId: string
      arrivalTimestamp: string
    }>(`/api/queue/${payload.entryId}/arrived`, {})

    return {
      entryId: raw.appointmentId,
      arrivalTimestamp: raw.arrivalTimestamp,
    }
  },

  async reorderQueue(payload: ReorderPayload): Promise<QueueResponse> {
    if (USE_MOCK_QUEUE || !API_BASE_URL) {
      return mockQueueApi.reorderQueue(payload)
    }

    const raw = await postJson<{
      entries: Array<{
        appointmentId: string
        position: number
        patientId: string
        patientName: string
        appointmentTime: string
        providerName: string
        status: QueueStatus
        bookingType: BookingType
        arrivalTimestamp?: string | null
        riskLevel: RiskLevel
        estimatedWaitMinutes: number
      }>
      summary: QueueSummary
    }>(`/api/queue/${payload.entryId}/reorder`, {
      newPosition: payload.newPosition,
      reason: payload.reason,
    })

    return adaptQueueResponseDto(raw)
  },
}
