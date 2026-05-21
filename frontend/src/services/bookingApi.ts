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

export interface LockSlotResponse {
  slotId: string
  lockToken: string
  expiresAtUtc: string
  lockDurationSeconds: number
}

export interface ConfirmBookingPayload {
  slotId: string
  lockToken: string
}

export interface CancelAppointmentPayload {
  appointmentId: string
}

export interface AppointmentRecord {
  id: string
  slotId: string
  providerName: string
  providerInitials: string
  specialty: string
  date: string
  startTime: string
  durationMinutes: number
  status: 'Scheduled' | 'Cancelled'
  patientEmail: string
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

const wait = (ms = 300): Promise<void> =>
  new Promise((resolve) => {
    window.setTimeout(resolve, ms)
  })

const getJson = async <TResponse>(path: string): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    credentials: 'include',
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
    headers: { 'Content-Type': 'application/json' },
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

const mockLockedSlotIds = new Set<string>()

const mockBookingApi = {
  async getAppointment(appointmentId: string): Promise<AppointmentRecord> {
    await wait(200)

    const record = MOCK_APPOINTMENTS.find((appt) => appt.id === appointmentId)
    if (!record) {
      throw new BookingError('Appointment not found', 'NOT_FOUND', 404)
    }

    return { ...record }
  },

  async searchSlots(params: SlotSearchParams): Promise<AvailabilitySlot[]> {
    await wait()

    return MOCK_SLOTS.filter((slot) => {
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

    return {
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
    }
  },

  async cancelAppointment({ appointmentId }: CancelAppointmentPayload): Promise<AppointmentRecord> {
    await wait(250)

    const record = MOCK_APPOINTMENTS.find((appt) => appt.id === appointmentId)
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
}

export const bookingApi = {
  async getAppointment(appointmentId: string): Promise<AppointmentRecord> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.getAppointment(appointmentId)
    }

    return getJson<AppointmentRecord>(`/api/appointments/${appointmentId}`)
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

    return getJson<AvailabilitySlot[]>(`/api/appointments/slots?${query.toString()}`)
  },

  async lockSlot(slotId: string): Promise<LockSlotResponse> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.lockSlot(slotId)
    }

    return postJson<LockSlotResponse>(`/api/appointments/slots/${slotId}/lock`, {})
  },

  async confirmBooking(payload: ConfirmBookingPayload): Promise<AppointmentRecord> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.confirmBooking(payload)
    }

    return postJson<AppointmentRecord>('/api/appointments', payload)
  },

  async cancelAppointment(payload: CancelAppointmentPayload): Promise<AppointmentRecord> {
    if (USE_MOCK_BOOKING || !API_BASE_URL) {
      return mockBookingApi.cancelAppointment(payload)
    }

    return postJson<AppointmentRecord>(`/api/appointments/${payload.appointmentId}/cancel`, {})
  },
}
