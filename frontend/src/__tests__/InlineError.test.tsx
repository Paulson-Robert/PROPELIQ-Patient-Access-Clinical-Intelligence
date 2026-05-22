import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { InlineError } from '../components/shared/InlineError'

describe('InlineError', () => {
  it('renders the error message with alert role', () => {
    render(<InlineError id="email-error" message="Email is required" />)

    const alert = screen.getByRole('alert')
    expect(alert).toBeInTheDocument()
    expect(alert).toHaveTextContent('Email is required')
  })

  it('renders with the correct id for aria-describedby linking', () => {
    render(<InlineError id="name-error" message="Name is required" />)

    const alert = screen.getByRole('alert')
    expect(alert).toHaveAttribute('id', 'name-error')
  })

  it('displays a recovery suggestion when provided', () => {
    render(
      <InlineError
        id="password-error"
        message="Password is too short"
        suggestion="Use at least 8 characters with a mix of letters and numbers"
      />,
    )

    expect(screen.getByText('Password is too short')).toBeInTheDocument()
    expect(
      screen.getByText(
        'Use at least 8 characters with a mix of letters and numbers',
      ),
    ).toBeInTheDocument()
  })

  it('uses assertive aria-live for immediate announcement', () => {
    render(<InlineError id="field-error" message="Invalid input" />)

    expect(screen.getByRole('alert')).toHaveAttribute(
      'aria-live',
      'assertive',
    )
  })
})
