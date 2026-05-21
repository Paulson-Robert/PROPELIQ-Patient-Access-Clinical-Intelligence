import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach } from 'vitest'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { AppointmentDetailPage } from '../../pages/booking/AppointmentDetailPage'
import { bookingApi, type AppointmentRecord } from '../../services/bookingApi'

const baseAppointment: AppointmentRecord = {
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
}

const LocationProbe = () => {
  const location = useLocation()
  return <div data-testid="location-probe">{`${location.pathname}${location.search}`}</div>
}

describe('AppointmentDetailPage', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('opens cancel confirmation dialog and updates status after cancel', async () => {
    vi.spyOn(bookingApi, 'getAppointment').mockResolvedValue(baseAppointment)
    vi.spyOn(bookingApi, 'cancelAppointment').mockResolvedValue({
      ...baseAppointment,
      status: 'Cancelled',
    })

    render(
      <MemoryRouter initialEntries={['/booking/appointments/appt-001']}>
        <Routes>
          <Route path="/booking/appointments/:appointmentId" element={<AppointmentDetailPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Appointment detail' })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Cancel appointment' }))

    expect(await screen.findByRole('alertdialog')).toBeInTheDocument()
    expect(screen.getByText('Are you sure you want to cancel your appointment?')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Yes, cancel appointment' }))

    expect(await screen.findByText('Appointment cancelled. Calendar sync status updated.')).toBeInTheDocument()
    expect(screen.getAllByText('Cancelled').length).toBeGreaterThan(0)
    expect(screen.getByText('Removed')).toBeInTheDocument()
  })

  it('navigates to search with provider and specialty prefilled on reschedule', async () => {
    vi.spyOn(bookingApi, 'getAppointment').mockResolvedValue(baseAppointment)

    render(
      <MemoryRouter initialEntries={['/booking/appointments/appt-001']}>
        <Routes>
          <Route path="/booking/appointments/:appointmentId" element={<AppointmentDetailPage />} />
          <Route path="*" element={<LocationProbe />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Appointment detail' })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Reschedule' }))

    await waitFor(() => {
      expect(screen.getByTestId('location-probe').textContent).toContain('/booking/search?')
      expect(screen.getByTestId('location-probe').textContent).toContain('provider=Dr.+Sarah+Chen')
      expect(screen.getByTestId('location-probe').textContent).toContain('specialty=Internal+Medicine')
    })
  })
})
