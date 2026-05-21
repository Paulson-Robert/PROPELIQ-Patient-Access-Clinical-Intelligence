import { fireEvent, render, screen, within } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import {
  ConflictHighlight,
  type ConflictAuditEntry,
  type ConflictSource,
} from '../../components/clinical/ConflictHighlight'

const SOURCES: ConflictSource[] = [
  { source: 'EHR', value: '142/88 mmHg', recordedAt: 'Jan 20, 2025' },
  { source: 'Intake', value: '138/85 mmHg', recordedAt: 'Jan 26, 2025' },
]

describe('ConflictHighlight', () => {
  it('renders amber background and Pending badge when status is pending', () => {
    render(
      <ConflictHighlight fieldLabel="Blood pressure" sources={SOURCES} />,
    )

    const card = screen.getByText('Blood pressure').closest('[data-conflict-status]')
    expect(card).toHaveAttribute('data-conflict-status', 'pending')
    expect(card).toHaveClass('border-amber-500', 'bg-amber-500/5')
    expect(screen.getByLabelText('Conflict pending resolution')).toBeInTheDocument()
  })

  it('renders all source values for the same field (edge case: multiple sources)', () => {
    render(
      <ConflictHighlight fieldLabel="Blood pressure" sources={SOURCES} />,
    )

    expect(screen.getByText('142/88 mmHg')).toBeInTheDocument()
    expect(screen.getByText('138/85 mmHg')).toBeInTheDocument()
    expect(screen.getByLabelText('Accept value from EHR: 142/88 mmHg')).toBeInTheDocument()
    expect(screen.getByLabelText('Accept value from Intake: 138/85 mmHg')).toBeInTheDocument()
  })

  it('calls onResolve with audit entry when inline Accept button is clicked', () => {
    const onResolve = vi.fn()
    render(
      <ConflictHighlight
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onResolve={onResolve}
        resolvedBy="Dr. Smith"
      />,
    )

    fireEvent.click(screen.getByLabelText('Accept value from EHR: 142/88 mmHg'))

    expect(onResolve).toHaveBeenCalledOnce()
    const auditEntry = onResolve.mock.calls[0][0] as ConflictAuditEntry
    expect(auditEntry.fieldLabel).toBe('Blood pressure')
    expect(auditEntry.acceptedSource).toBe('EHR')
    expect(auditEntry.acceptedValue).toBe('142/88 mmHg')
    expect(auditEntry.resolvedBy).toBe('Dr. Smith')
    expect(auditEntry.resolvedAt).toMatch(/^\d{4}-\d{2}-\d{2}T/)
  })

  it('opens the resolution dialog when Details button is clicked', () => {
    render(
      <ConflictHighlight fieldLabel="Blood pressure" sources={SOURCES} />,
    )

    fireEvent.click(screen.getByLabelText('Open details for Blood pressure conflict'))
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByText('Resolve conflict — Blood pressure')).toBeInTheDocument()
  })

  it('opens the resolution dialog when Enter custom value button is clicked', () => {
    render(
      <ConflictHighlight fieldLabel="Blood pressure" sources={SOURCES} />,
    )

    fireEvent.click(screen.getByLabelText('Enter custom value for Blood pressure'))
    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })

  it('renders resolved state with summary and no accept buttons', () => {
    render(
      <ConflictHighlight
        fieldLabel="Blood pressure"
        sources={SOURCES}
        status="resolved"
        resolvedSummary="Resolved by Dr. Chen on Jan 25, 2025 — accepted EHR value: 142/88 mmHg"
      />,
    )

    const card = screen.getByText('Blood pressure').closest('[data-conflict-status]')
    expect(card).toHaveAttribute('data-conflict-status', 'resolved')
    expect(screen.getByLabelText('Conflict resolved')).toBeInTheDocument()
    expect(
      screen.getByText('Resolved by Dr. Chen on Jan 25, 2025 — accepted EHR value: 142/88 mmHg'),
    ).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Accept/ })).not.toBeInTheDocument()
  })

  it('renders more than two sources (3+ sources edge case)', () => {
    const threeSources: ConflictSource[] = [
      ...SOURCES,
      { source: 'Insurance', value: '140/90 mmHg', recordedAt: 'Jan 28, 2025' },
    ]
    render(
      <ConflictHighlight fieldLabel="Blood pressure" sources={threeSources} />,
    )

    const group = screen.getByRole('group', { name: 'Conflicting values for Blood pressure' })
    expect(within(group).getAllByRole('button')).toHaveLength(3)
  })
})
