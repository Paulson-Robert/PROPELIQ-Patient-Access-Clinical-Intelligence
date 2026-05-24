export interface AvailabilitySlot {
  id: string
  providerId: string
  providerName: string
  providerInitials: string
  specialty: string
  date: string
  startTime: string
  endTime: string
  durationMinutes: number
  isAvailable: boolean
  isLocked: boolean
}

export interface SlotSearchParams {
  provider?: string
  specialty?: string
  from?: string
  to?: string
}

export interface PatientSearchResult {
  id: string
  name: string
  email?: string
  phone?: string
  dateOfBirth?: string
  lastVisitDate?: string
}

export interface LockSlotResponse {
  slotId: string
  lockToken: string
  expiresAtUtc: string
  lockDurationSeconds: number
}

export interface ConfirmBookingPayload {
  slotId: string
  lockToken: string
  insuranceProvider?: string
  insurancePolicyNumber?: string
}

export interface CancelAppointmentPayload {
  appointmentId: string
}

export interface WalkInBookingPayload {
  patientId?: string
  guestName?: string
  guestEmail?: string
  guestPhone?: string
  providerId?: string
  reasonForVisit?: string
}

export interface WalkInBookingResponse {
  appointmentId: string
  queueId: string
  bookingType: 'WalkIn'
  status: 'Scheduled'
  patientDisplayName: string
  estimatedWaitMinutes: number
}

export interface AppointmentRecord {
  id: string
  slotId: string
  providerName: string
  providerInitials: string
  specialty: string
  date: string
  startTime: string
  endTime?: string
  durationMinutes: number
  status: 'Scheduled' | 'Cancelled' | 'Completed' | 'Arrived' | 'NoShow'
  patientEmail: string
  insuranceProvider?: string
  insurancePolicyNumber?: string
}

export interface PatientDocumentRecord {
  id: string
  fileName: string
  fileFormat: string
  fileSizeBytes: number
  processingStatus: string
  uploadedAt: string
}

export interface PatientNotificationRecord {
  id: string
  appointmentId: string
  channel: string
  notificationType: string
  status: string
  createdAt: string
}

export type BookingErrorCode =
  | 'SLOT_LOCKED'
  | 'SLOT_UNAVAILABLE'
  | 'LOCK_EXPIRED'
  | 'NOT_FOUND'

export class BookingError extends Error {
  code: BookingErrorCode
  status: number

  constructor(message: string, code: BookingErrorCode, status: number) {
    super(message)
    this.name = 'BookingError'
    this.code = code
    this.status = status
  }
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const USE_MOCK_BOOKING = (import.meta.env.VITE_USE_MOCK_AUTH ?? 'true') !== 'false'

// ---------------------------------------------------------------------------
// Auth token store — populated by the auth layer after login.
// bookingApi reads it so every real API call includes Authorization: Bearer.
// Persisted in sessionStorage so the token survives page refreshes within
// the same browser tab.
// ---------------------------------------------------------------------------
const TOKEN_STORAGE_KEY = 'propeliq_access_token'

let _accessToken: string | null = (() => {
  try {
    return sessionStorage.getItem(TOKEN_STORAGE_KEY)
  } catch {
    return null
  }
})()

export const setBookingAuthToken = (token: string | null): void => {
  _accessToken = token
  try {
    if (token) {
      sessionStorage.setItem(TOKEN_STORAGE_KEY, token)
    } else {
      sessionStorage.removeItem(TOKEN_STORAGE_KEY)
    }
  } catch {
    // sessionStorage unavailable (e.g. private browsing quota exceeded)
  }
}

const authHeaders = (): Record<string, string> =>
  _accessToken ? { Authorization: `Bearer ${_accessToken}` } : {}

const wait = (ms = 300): Promise<void> =>
  new Promise((resolve) => {
    window.setTimeout(resolve, ms)
  })

const getJson = async <TResponse>(path: string): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    credentials: 'include',
    headers: authHeaders(),
  })

  if (!response.ok) {
    let message = 'Request failed'
    let code: BookingErrorCode = 'NOT_FOUND'

    try {
      const parsed = (await response.json()) as { message?: string; code?: BookingErrorCode }
      message = parsed.message ?? message
      code = parsed.code ?? code
    } catch {
      // use defaults
    }

    throw new BookingError(message, code, response.status)
  }

  return (await response.json()) as TResponse
}

const postJson = async <TResponse>(path: string, body: unknown): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    credentials: 'include',
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    let message = 'Request failed'
    let code: BookingErrorCode = 'NOT_FOUND'

    try {
      const parsed = (await response.json()) as { message?: string; code?: BookingErrorCode }
      message = parsed.message ?? message
      code = parsed.code ?? code
    } catch {
      // use defaults
    }

    throw new BookingError(message, code, response.status)
  }

  return (await response.json()) as TResponse
}

const putJson = async <TResponse>(path: string, body: unknown): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    credentials: 'include',
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    let message = 'Request failed'
    let code: BookingErrorCode = 'NOT_FOUND'

    try {
      const parsed = (await response.json()) as { message?: string; code?: BookingErrorCode }
      message = parsed.message ?? message
      code = parsed.code ?? code
    } catch {
      // use defaults
    }

    throw new BookingError(message, code, response.status)
  }

  return (await response.json()) as TResponse
}

// ---------------------------------------------------------------------------
// Response adapters — convert backend DTO shapes to frontend model shapes
// ---------------------------------------------------------------------------

/** Derives "AB" initials from "Dr. Alice Brown" → "AB" */
const initialsFromName = (name: string): string =>
  name
    .replace(/^Dr\.\s*/i, '')
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? '')
    .join('')

/** Formats a UTC DateTime string into "YYYY-MM-DD" */
const toDateString = (utcDateTime: string): string =>
  utcDateTime.substring(0, 10)

/** Formats a UTC DateTime string into "HH:mm" local (display) time */
const toTimeString = (utcDateTime: string): string => {
  const d = new Date(utcDateTime)
  return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`
}

const adaptSlotDto = (raw: {
  slotId: string
  providerId: string
  providerName: string
  specialty: string
  startTime: string
  endTime: string
  durationMinutes: number
  isAvailable: boolean
  isLocked: boolean
}): AvailabilitySlot => ({
  id: raw.slotId,
  providerId: raw.providerId,
  providerName: raw.providerName,
  providerInitials: initialsFromName(raw.providerName),
  specialty: raw.specialty,
  date: toDateString(raw.startTime),
  startTime: toTimeString(raw.startTime),
  endTime: toTimeString(raw.endTime),
  durationMinutes: raw.durationMinutes,
  isAvailable: raw.isAvailable,
  isLocked: raw.isLocked,
})

const adaptConfirmationDto = (
  raw: {
    appointmentId: string
    providerName: string
    specialty: string
    startTime: string
    durationMinutes: number
    status: string
    patientEmail: string
    insuranceProvider?: string
    insurancePolicyNumber?: string
  },
  slotId: string,
): AppointmentRecord => ({
  id: raw.appointmentId,
  slotId,
  providerName: raw.providerName,
  providerInitials: initialsFromName(raw.providerName),
  specialty: raw.specialty,
  date: toDateString(raw.startTime),
  startTime: toTimeString(raw.startTime),
  durationMinutes: raw.durationMinutes,
  status: mapStatus(raw.status),
  patientEmail: raw.patientEmail,
  insuranceProvider: raw.insuranceProvider,
  insurancePolicyNumber: raw.insurancePolicyNumber,
})

const adaptAppointmentDetail = (raw: {
  appointmentId: string
  slotId: string
  providerName: string
  specialty: string
  startTime: string
  endTime: string
  durationMinutes: number
  status: string
  patientEmail: string
  insuranceProvider?: string
  insurancePolicyNumber?: string
}): AppointmentRecord => ({
  id: raw.appointmentId,
  slotId: raw.slotId,
  providerName: raw.providerName,
  providerInitials: initialsFromName(raw.providerName),
  specialty: raw.specialty,
  date: toDateString(raw.startTime),
  startTime: toTimeString(raw.startTime),
  endTime: toTimeString(raw.endTime),
  durationMinutes: raw.durationMinutes,
  status: mapStatus(raw.status),
  patientEmail: raw.patientEmail,
  insuranceProvider: raw.insuranceProvider,
  insurancePolicyNumber: raw.insurancePolicyNumber,
})

const mapStatus = (status: string): AppointmentRecord['status'] => {
  switch (status) {
    case 'Cancelled': return 'Cancelled'
    case 'Completed': return 'Completed'
    case 'Arrived': return 'Arrived'
    case 'NoShow': return 'NoShow'
    default: return 'Scheduled'
  }
}

const adaptPatientAppointmentDto = (raw: {
  appointmentId: string
  slotId: string
  providerName: string
  specialty: string
  startTime: string
  endTime: string
  durationMinutes: number
  status: string
  insuranceProvider?: string
}): AppointmentRecord => ({
  id: raw.appointmentId,
  slotId: raw.slotId,
  providerName: raw.providerName,
  providerInitials: initialsFromName(raw.providerName),
  specialty: raw.specialty,
  date: toDateString(raw.startTime),
  startTime: toTimeString(raw.startTime),
  endTime: toTimeString(raw.endTime),
  durationMinutes: raw.durationMinutes,
  status: mapStatus(raw.status),
  patientEmail: '',
  insuranceProvider: raw.insuranceProvider,
})

const MOCK_SLOTS: AvailabilitySlot[] = [
  {
    id: 'slot-001',
    providerId: 'prov-001',
    providerName: 'Dr. Sarah Chen',
    providerInitials: 'SC',
    specialty: 'Internal Medicine',
    date: '2025-01-28',
    startTime: '09:00',
    endTime: '09:30',
    durationMinutes: 30,
    isAvailable: true,
    isLocked: false,
  },
  {
    id: 'slot-002',
    providerId: 'prov-001',
    providerName: 'Dr. Sarah Chen',
    providerInitials: 'SC',
    specialty: 'Internal Medicine',
    date: '2025-01-28',
    startTime: '09:30',
    endTime: '10:00',
    durationMinutes: 30,
    isAvailable: true,
    isLocked: false,
  },
  {
    id: 'slot-003',
    providerId: 'prov-002',
    providerName: 'Dr. Michael Okafor',
    providerInitials: 'MO',
    specialty: 'Cardiology',
    date: '2025-01-28',
    startTime: '10:00',
    endTime: '10:45',
    durationMinutes: 45,
    isAvailable: true,
    isLocked: false,
  },
  {
    id: 'slot-004',
    providerId: 'prov-003',
    providerName: 'Dr. Emily Johansson',
    providerInitials: 'EJ',
    specialty: 'Dermatology',
    date: '2025-01-28',
    startTime: '11:00',
    endTime: '11:20',
    durationMinutes: 20,
    isAvailable: true,
    isLocked: false,
  },
  {
    id: 'slot-005',
    providerId: 'prov-004',
    providerName: 'Dr. Lisa Nakamura',
    providerInitials: 'LN',
    specialty: 'Family Medicine',
    date: '2025-01-29',
    startTime: '14:00',
    endTime: '14:30',
    durationMinutes: 30,
    isAvailable: true,
    isLocked: false,
  },
  {
    id: 'slot-006',
    providerId: 'prov-002',
    providerName: 'Dr. Michael Okafor',
    providerInitials: 'MO',
    specialty: 'Cardiology',
    date: '2025-01-29',
    startTime: '15:00',
    endTime: '15:45',
    durationMinutes: 45,
    isAvailable: true,
    isLocked: false,
  },
]

const MOCK_APPOINTMENTS: AppointmentRecord[] = [
  {
    id: 'appt-001',
    slotId: 'slot-900',
    providerName: 'Dr. Sarah Chen',
    providerInitials: 'SC',
    specialty: 'Internal Medicine',
    date: '2025-01-27',
    startTime: '09:00',
    durationMinutes: 30,
    status: 'Scheduled',
    patientEmail: 'patient@example.com',
  },
]

const MOCK_PATIENTS: PatientSearchResult[] = [
  {
    id: 'pat-001',
    name: 'Maria Santos',
    email: 'maria.santos@example.com',
    phone: '(555) 234-5678',
    dateOfBirth: '1988-03-15',
    lastVisitDate: '2026-04-10',
  },
  {
    id: 'pat-002',
    name: 'Maria Del Carmen Lopez',
    email: 'maria.lopez@example.com',
    phone: '(555) 987-6543',
    dateOfBirth: '1975-07-22',
    lastVisitDate: '2026-01-08',
  },
  {
    id: 'pat-003',
    name: 'Marcus Reid',
    email: 'marcus.reid@example.com',
    phone: '(555) 111-2233',
    dateOfBirth: '1992-11-03',
    lastVisitDate: '2025-12-21',
  },
]

const mockLockedSlotIds = new Set<string>()
const mockConfirmedAppointments: AppointmentRecord[] = []

let mockWalkInCounter = 1

const mockBookingApi = {
  async getAppointment(appointmentId: string): Promise<AppointmentRecord> {
    await wait(200)

    const confirmed = mockConfirmedAppointments.find((appt) => appt.id === appointmentId)
    if (confirmed) return { ...confirmed }

    const record = MOCK_APPOINTMENTS.find((appt) => appt.id === appointmentId)
    if (!record) {
      throw new BookingError('Appointment not found', 'NOT_FOUND', 404)
    }

    return { ...record }
  },

  async searchSlots(params: SlotSearchParams): Promise<AvailabilitySlot[]> {
    await wait()

    const filtered = MOCK_SLOTS.filter((slot) => {
      if (!slot.isAvailable) return false

      const providerMatch =
        !params.provider ||
        slot.providerName.toLowerCase().includes(params.provider.toLowerCase())

      const specialtyMatch =
        !params.specialty || slot.specialty === params.specialty

      const fromMatch = !params.from || slot.date >= params.from
      const toMatch = !params.to || slot.date <= params.to

      return providerMatch && specialtyMatch && fromMatch && toMatch
    }).map((slot) => ({
      ...slot,
      isLocked: mockLockedSlotIds.has(slot.id),
    }))

    return filtered
  },

  async searchPatients(query: string): Promise<PatientSearchResult[]> {
    await wait(180)

    const trimmed = query.trim().toLowerCase()
    if (trimmed.length < 2) {
      return []
    }

    return MOCK_PATIENTS.filter((patient) =>
      [patient.name, patient.email, patient.phone]
        .filter(Boolean)
        .some((value) => value!.toLowerCase().includes(trimmed)),
    )
  },

  async lockSlot(slotId: string): Promise<LockSlotResponse> {
    await wait(200)

    const slot = MOCK_SLOTS.find((s) => s.id === slotId)

    if (!slot) {
      throw new BookingError('Slot not found', 'NOT_FOUND', 404)
    }

    if (mockLockedSlotIds.has(slotId)) {
      throw new BookingError(
        'This slot is temporarily held by another user. Please select a different time.',
        'SLOT_LOCKED',
        409,
      )
    }

    if (!slot.isAvailable) {
      throw new BookingError(
        'This slot is no longer available.',
        'SLOT_UNAVAILABLE',
        409,
      )
    }

    mockLockedSlotIds.add(slotId)

    const expiresAt = new Date(Date.now() + 30_000)

    return {
      slotId,
      lockToken: `mock-lock-${slotId}-${Date.now()}`,
      expiresAtUtc: expiresAt.toISOString(),
      lockDurationSeconds: 30,
    }
  },

  async confirmBooking(payload: ConfirmBookingPayload): Promise<AppointmentRecord> {
    await wait(400)

    const slot = MOCK_SLOTS.find((s) => s.id === payload.slotId)

    if (!slot) {
      throw new BookingError('Slot not found', 'NOT_FOUND', 404)
    }

    mockLockedSlotIds.delete(payload.slotId)
    slot.isAvailable = false

    const newAppointment: AppointmentRecord = {
      id: `appt-${Date.now()}`,
      slotId: slot.id,
      providerName: slot.providerName,
      providerInitials: slot.providerInitials,
      specialty: slot.specialty,
      date: slot.date,
      startTime: slot.startTime,
      durationMinutes: slot.durationMinutes,
      status: 'Scheduled',
      patientEmail: 'patient@example.com',
      insuranceProvider: payload.insuranceProvider,
      insurancePolicyNumber: payload.insurancePolicyNumber,
    }

    mockConfirmedAppointments.push(newAppointment)
    return newAppointment
  },

  async cancelAppointment({ appointmentId }: CancelAppointmentPayload): Promise<AppointmentRecord> {
    await wait(250)

    const record =
      mockConfirmedAppointments.find((appt) => appt.id === appointmentId) ??
      MOCK_APPOINTMENTS.find((appt) => appt.id === appointmentId)
    if (!record) {
      throw new BookingError('Appointment not found', 'NOT_FOUND', 404)
    }

    if (record.status === 'Cancelled') {
      return { ...record }
    }

    if (mockLockedSlotIds.has(record.slotId)) {
      throw new BookingError(
        'This slot is temporarily held by another user. Please try again in a moment.',
        'SLOT_LOCKED',
        409,
      )
    }

    const releasedSlot = MOCK_SLOTS.find((slot) => slot.id === record.slotId)
    if (releasedSlot) {
      releasedSlot.isAvailable = true
      releasedSlot.isLocked = false
    }

    record.status = 'Cancelled'
    return { ...record }
  },

  async submitWalkIn(payload: WalkInBookingPayload): Promise<WalkInBookingResponse> {
    await wait(320)

    const selectedPatient = payload.patientId
      ? MOCK_PATIENTS.find((patient) => patient.id === payload.patientId)
      : null

    const patientDisplayName =
      selectedPatient?.name ?? payload.guestName?.trim() ?? ''

    if (!patientDisplayName) {
      throw new BookingError('Patient selection or guest name is required.', 'NOT_FOUND', 400)
    }

    const walkInId = `walkin-${mockWalkInCounter.toString().padStart(3, '0')}`
    mockWalkInCounter += 1

    return {
      appointmentId: `appt-${walkInId}`,
      queueId: `queue-${walkInId}`,
      bookingType: 'WalkIn',
      status: 'Scheduled',
      patientDisplayName,
      estimatedWaitMinutes: 20,
    }
  },
}

export const bookingApi = {
  async getAppointment(appointmentId: string): Promise<AppointmentRecord> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.getAppointment(appointmentId)
    }

    // Backend returns AppointmentDetailDto — adapt to AppointmentRecord
    const raw = await getJson<{
      appointmentId: string
      slotId: string
      providerName: string
      specialty: string
      startTime: string
      endTime: string
      durationMinutes: number
      status: string
      patientEmail: string
      insuranceProvider?: string
      insurancePolicyNumber?: string
    }>(`/api/appointments/${appointmentId}`)

    return adaptAppointmentDetail(raw)
  },

  async searchSlots(params: SlotSearchParams): Promise<AvailabilitySlot[]> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.searchSlots(params)
    }

    const query = new URLSearchParams()
    if (params.provider) query.set('provider', params.provider)
    if (params.specialty) query.set('specialty', params.specialty)
    if (params.from) query.set('from', params.from)
    if (params.to) query.set('to', params.to)

    // Backend returns SlotDto[] — adapt each to AvailabilitySlot
    const raw = await getJson<{
      slotId: string
      providerId: string
      providerName: string
      specialty: string
      startTime: string
      endTime: string
      durationMinutes: number
      isAvailable: boolean
      isLocked: boolean
    }[]>(`/api/appointments/slots?${query.toString()}`)

    return raw.map(adaptSlotDto)
  },

  async searchPatients(query: string): Promise<PatientSearchResult[]> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.searchPatients(query)
    }

    return getJson<PatientSearchResult[]>(`/api/patients?search=${encodeURIComponent(query)}`)
  },

  async lockSlot(slotId: string): Promise<LockSlotResponse> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.lockSlot(slotId)
    }

    // Backend returns SlotLockResult — adapt to LockSlotResponse
    const raw = await postJson<{
      slotId: string
      lockToken: string
      expiresAt: string
      lockDurationSeconds: number
    }>(`/api/appointments/slots/${slotId}/lock`, {})

    return {
      slotId: raw.slotId,
      lockToken: raw.lockToken,
      expiresAtUtc: raw.expiresAt,
      lockDurationSeconds: raw.lockDurationSeconds,
    }
  },

  async confirmBooking(payload: ConfirmBookingPayload): Promise<AppointmentRecord> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.confirmBooking(payload)
    }

    // Backend returns AppointmentConfirmationDto — adapt to AppointmentRecord
    const raw = await postJson<{
      appointmentId: string
      providerName: string
      specialty: string
      startTime: string
      durationMinutes: number
      status: string
      patientEmail: string
      insuranceProvider?: string
      insurancePolicyNumber?: string
    }>('/api/appointments', payload)

    return adaptConfirmationDto(raw, payload.slotId)
  },

  async cancelAppointment(payload: CancelAppointmentPayload): Promise<AppointmentRecord> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.cancelAppointment(payload)
    }

    const raw = await postJson<{
      appointmentId: string
      slotId: string
      status: string
      updatedAtUtc: string
    }>(`/api/appointments/${payload.appointmentId}/cancel`, {})

    // Return minimal record sufficient for UI to reflect cancellation
    return {
      id: raw.appointmentId,
      slotId: raw.slotId,
      providerName: '',
      providerInitials: '',
      specialty: '',
      date: '',
      startTime: '',
      durationMinutes: 0,
      status: raw.status === 'Cancelled' ? 'Cancelled' : 'Scheduled',
      patientEmail: '',
    }
  },

  async rescheduleAppointment(
    appointmentId: string,
    newSlotId: string,
    lockToken: string,
  ): Promise<AppointmentRecord> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      throw new BookingError('Reschedule not supported in mock mode', 'NOT_FOUND', 501)
    }

    const raw = await putJson<{
      appointmentId: string
      slotId: string
      status: string
      updatedAtUtc: string
    }>(`/api/appointments/${appointmentId}/reschedule`, { newSlotId, lockToken })

    return {
      id: raw.appointmentId,
      slotId: raw.slotId,
      providerName: '',
      providerInitials: '',
      specialty: '',
      date: '',
      startTime: '',
      durationMinutes: 0,
      status: raw.status === 'Cancelled' ? 'Cancelled' : 'Scheduled',
      patientEmail: '',
    }
  },

  async submitWalkIn(payload: WalkInBookingPayload): Promise<WalkInBookingResponse> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.submitWalkIn(payload)
    }

    return postJson<WalkInBookingResponse>('/api/appointments/walkin', payload)
  },

  async getMyAppointments(statusFilter?: string): Promise<AppointmentRecord[]> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      await wait(250)
      return [...MOCK_APPOINTMENTS]
    }

    const query = new URLSearchParams()
    if (statusFilter) query.set('status', statusFilter)

    const raw = await getJson<{
      appointmentId: string
      slotId: string
      providerName: string
      specialty: string
      startTime: string
      endTime: string
      durationMinutes: number
      status: string
      insuranceProvider?: string
    }[]>(`/api/appointments/my?${query.toString()}`)

    return raw.map(adaptPatientAppointmentDto)
  },

  async getMyDocuments(): Promise<PatientDocumentRecord[]> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      await wait(200)
      return []
    }

    const raw = await getJson<{
      documentId: string
      fileName: string
      fileFormat: string
      fileSizeBytes: number
      processingStatus: string
      uploadedAt: string
    }[]>('/api/documents/my')

    return raw.map((d) => ({
      id: d.documentId,
      fileName: d.fileName,
      fileFormat: d.fileFormat,
      fileSizeBytes: d.fileSizeBytes,
      processingStatus: d.processingStatus,
      uploadedAt: d.uploadedAt,
    }))
  },

  async getMyNotifications(): Promise<PatientNotificationRecord[]> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      await wait(200)
      return []
    }

    const raw = await getJson<{
      notificationId: string
      appointmentId: string
      channel: string
      notificationType: string
      status: string
      createdAt: string
    }[]>('/api/notifications/my')

    return raw.map((n) => ({
      id: n.notificationId,
      appointmentId: n.appointmentId,
      channel: n.channel,
      notificationType: n.notificationType,
      status: n.status,
      createdAt: n.createdAt,
    }))
  },
}
