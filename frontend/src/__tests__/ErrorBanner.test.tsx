import { fireEvent, render, screen } from '@testing-library/react'
import { describe, it, expect, vi } from 'vitest'
import { ErrorBanner, type NetworkError } from '../components/shared/ErrorBanner'

const mockErrors: NetworkError[] = [
  { id: '1', message: 'Failed to load appointments', severity: 'critical', retryable: true },
  { id: '2', message: 'Search index unavailable', severity: 'warning', retryable: true },
  { id: '3', message: 'Analytics update delayed', severity: 'info', retryable: false },
]

describe('ErrorBanner', () => {
  it('renders nothing when errors array is empty', () => {
    const { container } = render(<ErrorBanner errors={[]} />)
    expect(container).toBeEmptyDOMElement()
  })

  it('renders all error messages', () => {
    render(<ErrorBanner errors={mockErrors} />)

    expect(screen.getByText('Failed to load appointments')).toBeInTheDocument()
    expect(screen.getByText('Search index unavailable')).toBeInTheDocument()
    expect(screen.getByText('Analytics update delayed')).toBeInTheDocument()
  })

  it('displays errors sorted by severity — critical first', () => {
    const reversed: NetworkError[] = [...mockErrors].reverse()
    render(<ErrorBanner errors={reversed} />)

    const messages = screen.getAllByText(/Failed|Search|Analytics/)
    expect(messages[0]).toHaveTextContent('Failed to load appointments')
    expect(messages[1]).toHaveTextContent('Search index unavailable')
    expect(messages[2]).toHaveTextContent('Analytics update delayed')
  })

  it('shows retry button only for retryable errors', () => {
    const onRetry = vi.fn()
    render(<ErrorBanner errors={mockErrors} onRetry={onRetry} />)

    const retryButtons = screen.getAllByRole('button', { name: /retry/i })
    // Only 2 errors are retryable
    expect(retryButtons).toHaveLength(2)
  })

  it('calls onRetry with the error id when retry is clicked', () => {
    const onRetry = vi.fn()
    render(<ErrorBanner errors={[mockErrors[0]]} onRetry={onRetry} />)

    fireEvent.click(screen.getByRole('button', { name: /retry/i }))
    expect(onRetry).toHaveBeenCalledWith('1')
  })

  it('calls onDismiss with the error id when dismiss is clicked', () => {
    const onDismiss = vi.fn()
    render(<ErrorBanner errors={[mockErrors[0]]} onDismiss={onDismiss} />)

    fireEvent.click(screen.getByRole('button', { name: /dismiss/i }))
    expect(onDismiss).toHaveBeenCalledWith('1')
  })

  it('has proper aria attributes for screen readers', () => {
    render(<ErrorBanner errors={mockErrors} />)

    const status = screen.getByRole('status')
    expect(status).toHaveAttribute('aria-live', 'polite')
    expect(status).toHaveAttribute('aria-atomic', 'true')
  })
})
