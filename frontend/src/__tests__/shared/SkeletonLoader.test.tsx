import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { SkeletonLoader } from '../../components/shared/SkeletonLoader'

describe('SkeletonLoader', () => {
  // AC-01: skeleton renders as a loading placeholder
  it('renders a loading status element by default', () => {
    render(<SkeletonLoader />)
    expect(screen.getByRole('status')).toBeInTheDocument()
    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('applies text variant classes by default', () => {
    render(<SkeletonLoader variant="text" />)
    const status = screen.getByRole('status')
    expect(status).toHaveClass('h-4', 'w-full', 'rounded', 'animate-pulse')
  })

  it('applies avatar variant classes', () => {
    render(<SkeletonLoader variant="avatar" />)
    expect(screen.getByRole('status')).toHaveClass('h-10', 'w-10', 'rounded-full')
  })

  it('applies card variant classes', () => {
    render(<SkeletonLoader variant="card" />)
    expect(screen.getByRole('status')).toHaveClass('h-24', 'w-full', 'rounded-xl')
  })

  it('applies button variant classes', () => {
    render(<SkeletonLoader variant="button" />)
    expect(screen.getByRole('status')).toHaveClass('h-10', 'w-32', 'rounded-md')
  })

  it('merges additional className onto the skeleton', () => {
    render(<SkeletonLoader className="my-custom-class" />)
    expect(screen.getByRole('status')).toHaveClass('my-custom-class')
  })

  // Edge case: network timeout — skeleton replaced with retry prompt
  it('shows timeout message and no spinner when timedOut is true', () => {
    render(<SkeletonLoader timedOut />)
    const status = screen.getByRole('status')
    expect(status).toBeInTheDocument()
    expect(screen.getByText('Content took too long to load.')).toBeInTheDocument()
    expect(status).not.toHaveClass('animate-pulse')
  })

  it('shows Retry button when timedOut and onRetry provided', () => {
    const onRetry = vi.fn()
    render(<SkeletonLoader timedOut onRetry={onRetry} />)
    const retryButton = screen.getByRole('button', { name: /retry/i })
    expect(retryButton).toBeInTheDocument()
    fireEvent.click(retryButton)
    expect(onRetry).toHaveBeenCalledOnce()
  })

  it('does not render a Retry button when timedOut but no onRetry provided', () => {
    render(<SkeletonLoader timedOut />)
    expect(screen.queryByRole('button', { name: /retry/i })).not.toBeInTheDocument()
  })
})
