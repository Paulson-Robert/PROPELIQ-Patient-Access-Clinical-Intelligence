import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { RiskTierBadge } from '../../components/clinical/RiskTierBadge'

describe('RiskTierBadge', () => {
  // AC-01: color variants per tier
  it('renders Low badge with green styling', () => {
    render(<RiskTierBadge tier="Low" />)
    const badge = screen.getByLabelText('Risk tier: Low')
    expect(badge).toBeInTheDocument()
    expect(badge).toHaveClass('bg-emerald-500/10', 'text-emerald-700')
    expect(badge).toHaveTextContent('Low')
  })

  it('renders Medium badge with amber styling', () => {
    render(<RiskTierBadge tier="Medium" />)
    const badge = screen.getByLabelText('Risk tier: Medium')
    expect(badge).toBeInTheDocument()
    expect(badge).toHaveClass('bg-amber-500/10', 'text-amber-700')
    expect(badge).toHaveTextContent('Medium')
  })

  it('renders High badge with red styling', () => {
    render(<RiskTierBadge tier="High" />)
    const badge = screen.getByLabelText('Risk tier: High')
    expect(badge).toBeInTheDocument()
    expect(badge).toHaveClass('bg-red-500/10', 'text-red-700')
    expect(badge).toHaveTextContent('High')
  })

  // AC-02: icon presence (lucide renders an svg inside the badge)
  it('renders an icon inside every tier badge', () => {
    const { container } = render(<RiskTierBadge tier="High" />)
    expect(container.querySelector('svg')).toBeInTheDocument()
  })

  // Edge Case: pending state
  it('renders grey Calculating... badge when tier is null', () => {
    render(<RiskTierBadge tier={null} />)
    const badge = screen.getByLabelText('Risk score pending')
    expect(badge).toBeInTheDocument()
    expect(badge).toHaveClass('bg-muted', 'text-muted-foreground')
    expect(badge).toHaveTextContent('Calculating...')
  })

  it('renders grey Calculating... badge when tier is undefined', () => {
    render(<RiskTierBadge tier={undefined} />)
    const badge = screen.getByLabelText('Risk score pending')
    expect(badge).toBeInTheDocument()
    expect(badge).toHaveTextContent('Calculating...')
  })

  // Extra: className prop forwarded
  it('forwards an extra className to the badge span', () => {
    render(<RiskTierBadge tier="Low" className="custom-class" />)
    expect(screen.getByLabelText('Risk tier: Low')).toHaveClass('custom-class')
  })
})
