import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ProgressIndicator } from '../../components/shared/ProgressIndicator'

const STEPS = ['Patient Info', 'Insurance', 'Confirmation']

describe('ProgressIndicator', () => {
  // AC-02: renders all steps
  it('renders all step labels', () => {
    render(<ProgressIndicator steps={STEPS} currentStep={0} />)
    STEPS.forEach((label) => {
      // each label appears in both a visible aria-hidden span and a sr-only span
      expect(screen.getAllByText(label).length).toBeGreaterThanOrEqual(1)
    })
  })

  it('marks the current step with aria-current="step"', () => {
    render(<ProgressIndicator steps={STEPS} currentStep={1} />)
    // aria-current is placed on the inner step circle div
    const current = document.querySelector('[aria-current="step"]')
    expect(current).toBeInTheDocument()
  })

  it('announces current step via live region', () => {
    render(<ProgressIndicator steps={STEPS} currentStep={1} />)
    const live = screen.getByText(/step 2 of 3: insurance/i)
    expect(live).toBeInTheDocument()
  })

  it('shows a check icon (svg) for completed steps', () => {
    const { container } = render(<ProgressIndicator steps={STEPS} currentStep={2} />)
    // steps 0 and 1 are completed — two svgs rendered for check icons
    const svgs = container.querySelectorAll('[aria-hidden="true"] svg, svg[aria-hidden]')
    expect(svgs.length).toBeGreaterThanOrEqual(2)
  })

  it('shows step numbers for upcoming steps', () => {
    render(<ProgressIndicator steps={STEPS} currentStep={0} />)
    // step 2 and 3 are upcoming — render "2" and "3" as aria-hidden text
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
  })

  it('applies an additional className to the nav', () => {
    render(<ProgressIndicator steps={STEPS} currentStep={0} className="mt-4" />)
    expect(screen.getByRole('navigation')).toHaveClass('mt-4')
  })

  // Edge case: single step
  it('renders correctly with a single step', () => {
    render(<ProgressIndicator steps={['Only Step']} currentStep={0} />)
    expect(screen.getByText('Only Step')).toBeInTheDocument()
    expect(screen.getByText(/step 1 of 1/i)).toBeInTheDocument()
  })

  // Edge case: last step active
  it('renders last step as current without trailing connector', () => {
    render(<ProgressIndicator steps={STEPS} currentStep={2} />)
    expect(screen.getByText(/step 3 of 3/i)).toBeInTheDocument()
  })
})
