import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { AppointmentHistoryPage } from '../../pages/booking/AppointmentHistoryPage'
import { bookingApi, type AppointmentRecord } from '../../services/bookingApi'

vi.mock('../../services/bookingApi', () => ({
  bookingApi: {
    getMyAppointments: vi.fn(),
  },
}))

const createMockAppointments = (count: number): AppointmentRecord[] =>
  Array.from({ length: count }, (_, i) => ({
    id: `appt-${(i + 1).toString().padStart(3, '0')}`,
    slotId: `slot-${(i + 1).toString().padStart(3, '0')}`,
    providerName: `Dr. Provider ${i + 1}`,
    providerInitials: `P${i + 1}`,
    specialty: i % 2 === 0 ? 'Internal Medicine' : 'Cardiology',
    date: `2025-02-${(18 - i).toString().padStart(2, '0')}`,
    startTime: '09:00',
    durationMinutes: 30,
    status: (['Scheduled', 'Completed', 'Cancelled'] as const)[i % 3],
    patientEmail: 'patient@example.com',
  }))

describe('AppointmentHistoryPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('paginates appointments in pages of 10', async () => {
    vi.mocked(bookingApi.getMyAppointments).mockResolvedValue(createMockAppointments(12))

    render(
      <MemoryRouter>
        <AppointmentHistoryPage />
      </MemoryRouter>,
    )

    await waitFor(() => {
      expect(screen.getByText('Showing 1-10 of 12 appointments')).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: '2' }))

    expect(screen.getByText('Showing 11-12 of 12 appointments')).toBeInTheDocument()
  })

  it('filters by status', async () => {
    vi.mocked(bookingApi.getMyAppointments).mockResolvedValue(createMockAppointments(12))

    render(
      <MemoryRouter>
        <AppointmentHistoryPage />
      </MemoryRouter>,
    )

    await waitFor(() => {
      expect(screen.getByText(/Showing/)).toBeInTheDocument()
    })

    fireEvent.change(screen.getByLabelText('Status'), {
      target: { value: 'Cancelled' },
    })

    expect(screen.getByText(/of 4 appointments/)).toBeInTheDocument()
  })

  it('shows empty state when no appointments exist', async () => {
    vi.mocked(bookingApi.getMyAppointments).mockResolvedValue([])

    render(
      <MemoryRouter>
        <AppointmentHistoryPage />
      </MemoryRouter>,
    )

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'No appointments yet' })).toBeInTheDocument()
    })
    expect(screen.getByRole('link', { name: 'Book your first appointment' })).toBeInTheDocument()
  })
})
