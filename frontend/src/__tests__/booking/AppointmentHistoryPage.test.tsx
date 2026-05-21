import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { AppointmentHistoryPage } from '../../pages/booking/AppointmentHistoryPage'

describe('AppointmentHistoryPage', () => {
  it('paginates appointments in pages of 10', () => {
    render(
      <MemoryRouter>
        <AppointmentHistoryPage />
      </MemoryRouter>,
    )

    expect(screen.getByText('Showing 1-10 of 12 appointments')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: '2' }))

    expect(screen.getByText('Showing 11-12 of 12 appointments')).toBeInTheDocument()
  })

  it('filters by status and date range', () => {
    render(
      <MemoryRouter>
        <AppointmentHistoryPage />
      </MemoryRouter>,
    )

    fireEvent.change(screen.getByLabelText('Status'), {
      target: { value: 'Cancelled' },
    })

    expect(screen.getByText('Showing 1-2 of 2 appointments')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('From'), {
      target: { value: '2025-02-01' },
    })
    fireEvent.change(screen.getByLabelText('To'), {
      target: { value: '2025-02-28' },
    })

    expect(screen.getByText('Showing 1-1 of 1 appointments')).toBeInTheDocument()
  })

  it('shows empty state for patients with no appointments', () => {
    render(
      <MemoryRouter>
        <AppointmentHistoryPage appointments={[]} />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'No appointments yet' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Book your first appointment' })).toBeInTheDocument()
  })
})
