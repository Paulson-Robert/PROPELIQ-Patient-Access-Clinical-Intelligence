import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { SubmitButton } from '../../components/shared/SubmitButton'

describe('SubmitButton', () => {
  // AC-03: renders children when not submitting
  it('renders label text when idle', () => {
    render(<SubmitButton isSubmitting={false}>Save</SubmitButton>)
    expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument()
  })

  it('is not disabled when idle', () => {
    render(<SubmitButton isSubmitting={false}>Save</SubmitButton>)
    expect(screen.getByRole('button')).not.toBeDisabled()
  })

  // AC-03: spinner shown and button disabled during submission
  it('shows a spinner icon when submitting', () => {
    const { container } = render(
      <SubmitButton isSubmitting={true}>Save</SubmitButton>,
    )
    expect(container.querySelector('svg')).toBeInTheDocument()
  })

  it('is disabled when isSubmitting is true', () => {
    render(<SubmitButton isSubmitting={true}>Save</SubmitButton>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('sets aria-busy when submitting', () => {
    render(<SubmitButton isSubmitting={true}>Save</SubmitButton>)
    expect(screen.getByRole('button')).toHaveAttribute('aria-busy', 'true')
  })

  // AC-03: double-submission prevention — clicking disabled button fires no handler
  it('does not call onClick when disabled by isSubmitting', () => {
    const onClick = vi.fn()
    render(
      <SubmitButton isSubmitting={true} onClick={onClick}>
        Save
      </SubmitButton>,
    )
    fireEvent.click(screen.getByRole('button'))
    expect(onClick).not.toHaveBeenCalled()
  })

  // loadingLabel replaces children while submitting
  it('shows loadingLabel instead of children while submitting', () => {
    render(
      <SubmitButton isSubmitting={true} loadingLabel="Saving…">
        Save
      </SubmitButton>,
    )
    expect(screen.getByText('Saving…')).toBeInTheDocument()
    expect(screen.queryByText('Save')).not.toBeInTheDocument()
  })

  it('shows children when idle even if loadingLabel is set', () => {
    render(
      <SubmitButton isSubmitting={false} loadingLabel="Saving…">
        Save
      </SubmitButton>,
    )
    expect(screen.getByText('Save')).toBeInTheDocument()
  })

  // Forwarded disabled prop
  it('is disabled when disabled prop is true even if isSubmitting is false', () => {
    render(
      <SubmitButton isSubmitting={false} disabled>
        Save
      </SubmitButton>,
    )
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('merges extra className', () => {
    render(
      <SubmitButton isSubmitting={false} className="w-auto">
        Save
      </SubmitButton>,
    )
    expect(screen.getByRole('button')).toHaveClass('w-auto')
  })
})
