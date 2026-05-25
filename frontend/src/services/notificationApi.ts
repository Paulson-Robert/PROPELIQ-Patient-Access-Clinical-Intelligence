import { authHeaders } from './authTokenStore'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const USE_MOCK = (import.meta.env.VITE_USE_MOCK_AUTH ?? 'true') !== 'false'

export interface StaffNotificationRecord {
  id: string
  variant: 'info' | 'success' | 'warning' | 'error'
  title: string
  message: string | null
  isRead: boolean
  readAt: string | null
  createdAt: string
}

export interface StaffNotificationPage {
  items: StaffNotificationRecord[]
  totalCount: number
  page: number
  pageSize: number
}

export class NotificationApiError extends Error {
  status: number
  constructor(message: string, status: number) {
    super(message)
    this.name = 'NotificationApiError'
    this.status = status
  }
}

const getJson = async <T>(path: string): Promise<T> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    credentials: 'include',
    headers: authHeaders(),
  })
  if (!response.ok) {
    throw new NotificationApiError('Request failed', response.status)
  }
  return (await response.json()) as T
}

const postJson = async <T>(path: string): Promise<T> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
  })
  if (!response.ok) {
    throw new NotificationApiError('Request failed', response.status)
  }
  return (await response.json()) as T
}

// ---------------------------------------------------------------------------
// Mock data
// ---------------------------------------------------------------------------
const mockStaffNotifications: StaffNotificationRecord[] = [
  {
    id: 'sn-001',
    variant: 'error',
    title: 'SMS delivery failed',
    message: 'Appointment reminder for patient Maria Santos could not be delivered via SMS.',
    isRead: false,
    readAt: null,
    createdAt: new Date(Date.now() - 15 * 60_000).toISOString(),
  },
  {
    id: 'sn-002',
    variant: 'warning',
    title: 'Scheduling conflict detected',
    message: 'Double-booking detected for Dr. Chen on slot 10:00–10:30. Review required.',
    isRead: false,
    readAt: null,
    createdAt: new Date(Date.now() - 45 * 60_000).toISOString(),
  },
  {
    id: 'sn-003',
    variant: 'info',
    title: 'Low-confidence AI extraction',
    message: 'Intake document for Marcus Reid has confidence score below threshold. Manual review recommended.',
    isRead: false,
    readAt: null,
    createdAt: new Date(Date.now() - 2 * 3_600_000).toISOString(),
  },
  {
    id: 'sn-004',
    variant: 'success',
    title: 'Walk-in booking confirmed',
    message: 'Walk-in appointment for James O\'Brien added to today\'s queue (position #4).',
    isRead: true,
    readAt: new Date(Date.now() - 3 * 3_600_000).toISOString(),
    createdAt: new Date(Date.now() - 4 * 3_600_000).toISOString(),
  },
]

const mockStaffNotificationsState = [...mockStaffNotifications]

// ---------------------------------------------------------------------------
// API
// ---------------------------------------------------------------------------
export const notificationApi = {
  async getStaffNotifications(
    page = 1,
    pageSize = 20,
  ): Promise<StaffNotificationPage> {
    if (USE_MOCK || !API_BASE_URL) {
      await new Promise<void>((resolve) => { window.setTimeout(resolve, 200) })
      const start = (page - 1) * pageSize
      return {
        items: mockStaffNotificationsState.slice(start, start + pageSize),
        totalCount: mockStaffNotificationsState.length,
        page,
        pageSize,
      }
    }

    const raw = await getJson<{
      items: {
        staffNotificationId: string
        staffUserId: string
        variant: string
        title: string
        message: string | null
        isRead: boolean
        readAt: string | null
        createdAt: string
      }[]
      totalCount: number
      page: number
      pageSize: number
    }>(`/api/notifications/staff?page=${page}&pageSize=${pageSize}`)

    return {
      items: raw.items.map((n) => ({
        id: n.staffNotificationId,
        variant: mapVariant(n.variant),
        title: n.title,
        message: n.message,
        isRead: n.isRead,
        readAt: n.readAt,
        createdAt: n.createdAt,
      })),
      totalCount: raw.totalCount,
      page: raw.page,
      pageSize: raw.pageSize,
    }
  },

  async markStaffNotificationRead(notificationId: string): Promise<StaffNotificationRecord> {
    if (USE_MOCK || !API_BASE_URL) {
      await new Promise<void>((resolve) => { window.setTimeout(resolve, 100) })
      const item = mockStaffNotificationsState.find((n) => n.id === notificationId)
      if (item) {
        item.isRead = true
        item.readAt = new Date().toISOString()
        return { ...item }
      }
      throw new NotificationApiError('Notification not found', 404)
    }

    const raw = await postJson<{
      staffNotificationId: string
      staffUserId: string
      variant: string
      title: string
      message: string | null
      isRead: boolean
      readAt: string | null
      createdAt: string
    }>(`/api/notifications/staff/${notificationId}/read`)

    return {
      id: raw.staffNotificationId,
      variant: mapVariant(raw.variant),
      title: raw.title,
      message: raw.message,
      isRead: raw.isRead,
      readAt: raw.readAt,
      createdAt: raw.createdAt,
    }
  },
}

const mapVariant = (raw: string): StaffNotificationRecord['variant'] => {
  switch (raw.toLowerCase()) {
    case 'success': return 'success'
    case 'warning': return 'warning'
    case 'error': return 'error'
    default: return 'info'
  }
}
