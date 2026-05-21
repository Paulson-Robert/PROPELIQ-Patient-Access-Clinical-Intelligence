import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { StatusBadge } from '../../components/booking/StatusBadge'

describe('StatusBadge', () => {
  it('renders each required appointment history status', () => {
    const statuses = ['Confirmed', 'Cancelled', 'Completed', 'No-Show'] as const

    render(
      <div>
        {statuses.map((status) => (
          <StatusBadge key={status} status={status} />
        ))}
      </div>,
    )

    expect(screen.getByLabelText('Status: Confirmed')).toBeInTheDocument()
    expect(screen.getByLabelText('Status: Cancelled')).toBeInTheDocument()
    expect(screen.getByLabelText('Status: Completed')).toBeInTheDocument()
    expect(screen.getByLabelText('Status: No-Show')).toBeInTheDocument()
  })
})
