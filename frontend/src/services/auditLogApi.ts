import { authHeaders } from './authTokenStore'

export type AuditAction = 'Login' | 'Logout' | 'Create' | 'Update' | 'Delete' | 'View' | 'Export'

export type AuditResource =
  | 'User'
  | 'Appointment'
  | 'Patient'
  | 'Document'
  | 'Code mapping'
  | 'Session'
  | 'Audit log'

export interface AuditLogEntry {
  id: string
  timestamp: string
  actor: string
  action: AuditAction
  resource: AuditResource
  resourceId: string | null
  details: Record<string, unknown>
}

export interface AuditLogListParams {
  actor?: string
  action?: AuditAction | ''
  resource?: AuditResource | ''
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
}

export interface AuditLogListResponse {
  entries: AuditLogEntry[]
  total: number
  page: number
  pageSize: number
}

export class AuditLogApiError extends Error {
  status: number
  code?: string

  constructor(message: string, status: number, code?: string) {
    super(message)
    this.name = 'AuditLogApiError'
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
    throw new AuditLogApiError(message, response.status, code)
  }

  return (await response.json()) as TResponse
}

// Mock data — mirrors wireframe SCR-024 seed entries
const mockEntries: AuditLogEntry[] = [
  {
    id: 'al-001',
    timestamp: '2025-01-27T09:15:22Z',
    actor: 'Jennifer Walsh',
    action: 'Update',
    resource: 'Appointment',
    resourceId: 'APT-006',
    details: { field: 'status', from: 'pending', to: 'confirmed' },
  },
  {
    id: 'al-002',
    timestamp: '2025-01-27T09:10:05Z',
    actor: 'Maria Santos',
    action: 'Login',
    resource: 'Session',
    resourceId: 'SES-4421',
    details: { ip: '192.168.1.10', userAgent: 'Chrome/120' },
  },
  {
    id: 'al-003',
    timestamp: '2025-01-27T08:55:18Z',
    actor: 'System',
    action: 'Create',
    resource: 'Code mapping',
    resourceId: 'CM-012',
    details: { icdCode: 'Z00.00', description: 'General adult medical examination' },
  },
  {
    id: 'al-004',
    timestamp: '2025-01-26T17:30:00Z',
    actor: 'Admin User',
    action: 'Export',
    resource: 'Audit log',
    resourceId: null,
    details: { format: 'CSV', rowCount: 200 },
  },
  {
    id: 'al-005',
    timestamp: '2025-01-26T16:45:33Z',
    actor: 'Admin User',
    action: 'Delete',
    resource: 'User',
    resourceId: 'USR-008',
    details: { email: 'removed.user@example.com', reason: 'Account deactivated' },
  },
  {
    id: 'al-006',
    timestamp: '2025-01-26T15:22:11Z',
    actor: 'Maria Santos',
    action: 'View',
    resource: 'Patient',
    resourceId: 'PAT-031',
    details: { section: 'clinical-summary' },
  },
  {
    id: 'al-007',
    timestamp: '2025-01-26T14:10:45Z',
    actor: 'Jennifer Walsh',
    action: 'Create',
    resource: 'Appointment',
    resourceId: 'APT-007',
    details: { patientId: 'PAT-031', slot: '2025-01-28T10:00:00Z' },
  },
  {
    id: 'al-008',
    timestamp: '2025-01-26T13:55:00Z',
    actor: 'System',
    action: 'Update',
    resource: 'Patient',
    resourceId: 'PAT-015',
    details: { field: 'riskTier', from: 'medium', to: 'high' },
  },
  {
    id: 'al-009',
    timestamp: '2025-01-26T13:02:30Z',
    actor: 'Admin User',
    action: 'Update',
    resource: 'User',
    resourceId: 'USR-003',
    details: { field: 'role', from: 'staff', to: 'admin' },
  },
  {
    id: 'al-010',
    timestamp: '2025-01-26T12:48:17Z',
    actor: 'Maria Santos',
    action: 'Logout',
    resource: 'Session',
    resourceId: 'SES-4420',
    details: { sessionDurationMinutes: 47 },
  },
  {
    id: 'al-011',
    timestamp: '2025-01-26T11:30:00Z',
    actor: 'Jennifer Walsh',
    action: 'Login',
    resource: 'Session',
    resourceId: 'SES-4422',
    details: { ip: '10.0.0.5', userAgent: 'Firefox/121' },
  },
  {
    id: 'al-012',
    timestamp: '2025-01-26T11:15:44Z',
    actor: 'System',
    action: 'Create',
    resource: 'Document',
    resourceId: 'DOC-089',
    details: { type: 'intake-summary', patientId: 'PAT-031' },
  },
  {
    id: 'al-013',
    timestamp: '2025-01-26T10:55:20Z',
    actor: 'Jennifer Walsh',
    action: 'View',
    resource: 'Document',
    resourceId: 'DOC-085',
    details: { fileName: 'lab-results-2025-01.pdf' },
  },
  {
    id: 'al-014',
    timestamp: '2025-01-26T10:30:05Z',
    actor: 'Admin User',
    action: 'Create',
    resource: 'User',
    resourceId: 'USR-009',
    details: { email: 'new.staff@healthaccess.io', role: 'staff' },
  },
  {
    id: 'al-015',
    timestamp: '2025-01-26T09:45:00Z',
    actor: 'Maria Santos',
    action: 'Login',
    resource: 'Session',
    resourceId: 'SES-4420',
    details: { ip: '192.168.1.10', userAgent: 'Chrome/120' },
  },
  {
    id: 'al-016',
    timestamp: '2025-01-25T17:50:11Z',
    actor: 'System',
    action: 'Update',
    resource: 'Code mapping',
    resourceId: 'CM-009',
    details: { field: 'confidence', from: 0.72, to: 0.91 },
  },
  {
    id: 'al-017',
    timestamp: '2025-01-25T16:22:40Z',
    actor: 'Jennifer Walsh',
    action: 'Update',
    resource: 'Appointment',
    resourceId: 'APT-004',
    details: { field: 'status', from: 'confirmed', to: 'cancelled', reason: 'Patient request' },
  },
  {
    id: 'al-018',
    timestamp: '2025-01-25T15:10:00Z',
    actor: 'Admin User',
    action: 'View',
    resource: 'Audit log',
    resourceId: null,
    details: { filter: { fromDate: '2025-01-20', toDate: '2025-01-25' } },
  },
  {
    id: 'al-019',
    timestamp: '2025-01-25T14:05:33Z',
    actor: 'Maria Santos',
    action: 'View',
    resource: 'Patient',
    resourceId: 'PAT-022',
    details: { section: 'appointments' },
  },
  {
    id: 'al-020',
    timestamp: '2025-01-25T13:40:15Z',
    actor: 'System',
    action: 'Create',
    resource: 'Code mapping',
    resourceId: 'CM-013',
    details: { icdCode: 'J06.9', description: 'Acute upper respiratory infection, unspecified' },
  },
  {
    id: 'al-021',
    timestamp: '2025-01-25T12:30:00Z',
    actor: 'Jennifer Walsh',
    action: 'Login',
    resource: 'Session',
    resourceId: 'SES-4419',
    details: { ip: '10.0.0.5', userAgent: 'Firefox/121' },
  },
  {
    id: 'al-022',
    timestamp: '2025-01-25T11:55:48Z',
    actor: 'Admin User',
    action: 'Update',
    resource: 'User',
    resourceId: 'USR-002',
    details: { field: 'status', from: 'active', to: 'locked', reason: 'Failed login attempts' },
  },
  {
    id: 'al-023',
    timestamp: '2025-01-25T11:20:00Z',
    actor: 'System',
    action: 'Delete',
    resource: 'Document',
    resourceId: 'DOC-042',
    details: { reason: 'Retention policy — exceeded 7-year limit' },
  },
  {
    id: 'al-024',
    timestamp: '2025-01-25T10:15:22Z',
    actor: 'Maria Santos',
    action: 'Create',
    resource: 'Appointment',
    resourceId: 'APT-005',
    details: { patientId: 'PAT-022', slot: '2025-01-27T14:00:00Z' },
  },
  {
    id: 'al-025',
    timestamp: '2025-01-25T09:00:00Z',
    actor: 'Admin User',
    action: 'Export',
    resource: 'Patient',
    resourceId: null,
    details: { format: 'JSON', recordCount: 47 },
  },
  {
    id: 'al-026',
    timestamp: '2025-01-24T17:30:55Z',
    actor: 'Jennifer Walsh',
    action: 'View',
    resource: 'Patient',
    resourceId: 'PAT-008',
    details: { section: 'risk-tier' },
  },
  {
    id: 'al-027',
    timestamp: '2025-01-24T16:45:00Z',
    actor: 'System',
    action: 'Update',
    resource: 'Patient',
    resourceId: 'PAT-008',
    details: { field: 'riskTier', from: 'low', to: 'medium' },
  },
  {
    id: 'al-028',
    timestamp: '2025-01-24T15:20:11Z',
    actor: 'Maria Santos',
    action: 'Logout',
    resource: 'Session',
    resourceId: 'SES-4418',
    details: { sessionDurationMinutes: 112 },
  },
  {
    id: 'al-029',
    timestamp: '2025-01-24T14:10:00Z',
    actor: 'Admin User',
    action: 'Delete',
    resource: 'Code mapping',
    resourceId: 'CM-003',
    details: { reason: 'Superseded by CM-013' },
  },
  {
    id: 'al-030',
    timestamp: '2025-01-24T13:05:44Z',
    actor: 'Jennifer Walsh',
    action: 'Update',
    resource: 'Patient',
    resourceId: 'PAT-019',
    details: { field: 'preferredContact', from: 'phone', to: 'email' },
  },
]

function applyMockFilters(params: AuditLogListParams): AuditLogListResponse {
  const { actor = '', action = '', resource = '', fromDate, toDate, page = 1, pageSize = 25 } =
    params

  const filtered = mockEntries.filter((e) => {
    if (actor && !e.actor.toLowerCase().includes(actor.toLowerCase())) return false
    if (action && e.action !== action) return false
    if (resource && e.resource !== resource) return false
    if (fromDate && e.timestamp.slice(0, 10) < fromDate) return false
    if (toDate && e.timestamp.slice(0, 10) > toDate) return false
    return true
  })

  const total = filtered.length
  const start = (page - 1) * pageSize
  const entries = filtered.slice(start, start + pageSize)

  return { entries, total, page, pageSize }
}

// ---------------------------------------------------------------------------
// Backend ↔ Frontend shape adapters
// ---------------------------------------------------------------------------

interface BackendAuditEntry {
  auditLogId: number
  timestamp: string
  actorName: string | null
  actorUserId: string | null
  actorRole: string
  actionType: string
  resourceType: string
  resourceId: string | null
  details: string | null
}

interface BackendAuditPageResult {
  entries: BackendAuditEntry[]
  total: number
  page: number
  pageSize: number
}

const mapEntry = (e: BackendAuditEntry): AuditLogEntry => ({
  id: String(e.auditLogId),
  timestamp: e.timestamp,
  actor: e.actorName ?? 'System',
  action: e.actionType as AuditAction,
  resource: e.resourceType as AuditResource,
  resourceId: e.resourceId,
  details: (() => {
    if (!e.details) return {}
    try {
      return JSON.parse(e.details) as Record<string, unknown>
    } catch {
      return { raw: e.details }
    }
  })(),
})

/** Base URL for audit log export (used by AuditLogPage for the CSV download link). */
export const AUDIT_LOG_EXPORT_URL = (
  params: Omit<AuditLogListParams, 'page' | 'pageSize'> = {},
): string => {
  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
  const qs = new URLSearchParams()
  if (params.actor) qs.set('actor', params.actor)
  if (params.action) qs.set('action', params.action)
  if (params.resource) qs.set('resource', params.resource)
  if (params.fromDate) qs.set('fromDate', params.fromDate)
  if (params.toDate) qs.set('toDate', params.toDate)
  return `${API_BASE_URL}/api/admin/audit-log/export?${qs.toString()}`
}

export const auditLogApi = {
  listEntries: async (params: AuditLogListParams = {}): Promise<AuditLogListResponse> => {
    if (USE_MOCK) {
      await wait()
      return applyMockFilters(params)
    }

    const qs = new URLSearchParams()
    if (params.actor) qs.set('actor', params.actor)
    if (params.action) qs.set('action', params.action)
    if (params.resource) qs.set('resource', params.resource)
    if (params.fromDate) qs.set('fromDate', params.fromDate)
    if (params.toDate) qs.set('toDate', params.toDate)
    qs.set('page', String(params.page ?? 1))
    qs.set('pageSize', String(params.pageSize ?? 25))

    const raw = await getJson<BackendAuditPageResult>(`/api/admin/audit-log?${qs.toString()}`)
    return {
      entries: raw.entries.map(mapEntry),
      total: raw.total,
      page: raw.page,
      pageSize: raw.pageSize,
    }
  },
}
