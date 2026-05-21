import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { BookingConfirmationPage } from '../../pages/booking/BookingConfirmationPage'
import { bookingApi } from '../../services/bookingApi'

const locationState = {
  slot: {
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
  lockToken: 'mock-lock-token',
  expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
}

describe('BookingConfirmationPage', () => {
  it('submits and displays insurance details without blocking booking flow', async () => {
    const confirmSpy = vi.spyOn(bookingApi, 'confirmBooking').mockResolvedValue({
      id: 'appt-001',
      slotId: 'slot-001',
      providerName: 'Dr. Sarah Chen',
      providerInitials: 'SC',
      specialty: 'Internal Medicine',
      date: '2025-01-28',
      startTime: '09:00',
      durationMinutes: 30,
      status: 'Scheduled',
      patientEmail: 'patient@example.com',
      insuranceProvider: 'Blue Cross Blue Shield',
      insurancePolicyNumber: 'BCBS-882341',
    })

    render(
      <MemoryRouter
        initialEntries={[
          {
            pathname: '/booking/confirm',
            state: locationState,
          },
        ]}
      >
        <Routes>
          <Route path="/booking/confirm" element={<BookingConfirmationPage />} />
        </Routes>
      </MemoryRouter>,
    )

    fireEvent.change(screen.getByLabelText('Insurance provider'), {
      target: { value: 'Blue Cross Blue Shield' },
    })
    fireEvent.change(screen.getByLabelText('Policy number'), {
      target: { value: 'BCBS-882341' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Confirm booking' }))

    expect(await screen.findByRole('heading', { name: 'Appointment confirmed' })).toBeInTheDocument()

    expect(confirmSpy).toHaveBeenCalledWith(
      expect.objectContaining({
        insuranceProvider: 'Blue Cross Blue Shield',
        insurancePolicyNumber: 'BCBS-882341',
      }),
    )

    expect(screen.getByText('Blue Cross Blue Shield')).toBeInTheDocument()
    expect(screen.getByText('BCBS-882341')).toBeInTheDocument()
  })
})
