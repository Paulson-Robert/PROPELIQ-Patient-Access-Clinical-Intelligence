import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach } from 'vitest'
import { PatientSearchInput } from '../../components/walkin/PatientSearchInput'

const searchPatientsMock = vi.fn()

vi.mock('../../services/bookingApi', () => ({
  bookingApi: {
    searchPatients: (...args: unknown[]) => searchPatientsMock(...args),
  },
}))

describe('PatientSearchInput', () => {
  beforeEach(() => {
    searchPatientsMock.mockReset()
  })

  it('shows typeahead patient matches and selects a patient', async () => {
    searchPatientsMock.mockResolvedValue([
      {
        id: 'pat-001',
        name: 'Maria Santos',
        email: 'maria.santos@example.com',
        phone: '(555) 234-5678',
        lastVisitDate: '2026-04-10',
      },
      {
        id: 'pat-002',
        name: 'Maria Del Carmen Lopez',
        email: 'maria.lopez@example.com',
        phone: '(555) 987-6543',
        lastVisitDate: '2026-01-08',
      },
    ])

    const onSelectPatient = vi.fn()

    render(
      <PatientSearchInput
        selectedPatient={null}
        onSelectPatient={onSelectPatient}
      />,
    )

    fireEvent.change(screen.getByLabelText('Patient search'), {
      target: { value: 'ma' },
    })

    expect(await screen.findByRole('listbox', { name: 'Patient search results' })).toBeInTheDocument()
    expect(await screen.findByText('Maria Santos')).toBeInTheDocument()
    expect(screen.getByText('Maria Del Carmen Lopez')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('option', { name: /Maria Santos/i }))

    expect(onSelectPatient).toHaveBeenCalledWith(
      expect.objectContaining({ id: 'pat-001', name: 'Maria Santos' }),
    )
  })

  it('does not run search for single-character input', async () => {
    render(
      <PatientSearchInput
        selectedPatient={null}
        onSelectPatient={vi.fn()}
      />,
    )

    fireEvent.change(screen.getByLabelText('Patient search'), {
      target: { value: 'm' },
    })

    await waitFor(() => {
      expect(searchPatientsMock).not.toHaveBeenCalled()
    })
  })
})