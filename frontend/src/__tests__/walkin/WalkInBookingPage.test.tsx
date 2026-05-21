import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { WalkInBookingPage } from '../../pages/walkin/WalkInBookingPage'

const searchPatientsMock = vi.fn()
const submitWalkInMock = vi.fn()

vi.mock('../../services/bookingApi', () => ({
  bookingApi: {
    searchPatients: (...args: unknown[]) => searchPatientsMock(...args),
    submitWalkIn: (...args: unknown[]) => submitWalkInMock(...args),
  },
}))

describe('WalkInBookingPage', () => {
  beforeEach(() => {
    searchPatientsMock.mockReset()
    submitWalkInMock.mockReset()
  })

  it('submits walk-in for an existing patient', async () => {
    searchPatientsMock.mockResolvedValue([
      {
        id: 'pat-001',
        name: 'Maria Santos',
        email: 'maria.santos@example.com',
        phone: '(555) 234-5678',
      },
    ])

    submitWalkInMock.mockResolvedValue({
      appointmentId: 'appt-001',
      queueId: 'queue-001',
      bookingType: 'WalkIn',
      status: 'Scheduled',
      patientDisplayName: 'Maria Santos',
      estimatedWaitMinutes: 20,
    })

    render(
      <MemoryRouter>
        <WalkInBookingPage />
      </MemoryRouter>,
    )

    fireEvent.change(screen.getByLabelText('Patient search'), {
      target: { value: 'ma' },
    })

    fireEvent.click(await screen.findByRole('option', { name: /Maria Santos/i }))
    fireEvent.click(screen.getByRole('button', { name: 'Add to queue' }))

    await waitFor(() => {
      expect(submitWalkInMock).toHaveBeenCalledWith(
        expect.objectContaining({
          patientId: 'pat-001',
          guestName: undefined,
        }),
      )
    })

    expect(await screen.findByText('Walk-in added to same-day queue')).toBeInTheDocument()
  })

  it('submits guest walk-in with minimal required info', async () => {
    submitWalkInMock.mockResolvedValue({
      appointmentId: 'appt-002',
      queueId: 'queue-002',
      bookingType: 'WalkIn',
      status: 'Scheduled',
      patientDisplayName: 'Jordan Walker',
      estimatedWaitMinutes: 15,
    })

    render(
      <MemoryRouter>
        <WalkInBookingPage />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Guest walk-in' }))
    fireEvent.change(screen.getByLabelText('Full name'), {
      target: { value: 'Jordan Walker' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Add to queue' }))

    await waitFor(() => {
      expect(submitWalkInMock).toHaveBeenCalledWith(
        expect.objectContaining({
          guestName: 'Jordan Walker',
          patientId: undefined,
        }),
      )
    })

    expect(await screen.findByText('Walk-in added to same-day queue')).toBeInTheDocument()
  })
})