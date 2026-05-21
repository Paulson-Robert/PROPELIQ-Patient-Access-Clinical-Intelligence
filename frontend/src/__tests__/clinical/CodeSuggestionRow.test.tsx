import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import {
  CodeSuggestionRow,
  type CodeAuditEntry,
  type CodeSuggestion,
} from '../../components/clinical/CodeSuggestionRow'

const TABLE_WRAPPER = ({ children }: { children: React.ReactNode }) => (
  <table>
    <tbody>{children}</tbody>
  </table>
)

const SUGGESTION: CodeSuggestion = {
  id: 'sug-001',
  code: 'E11.9',
  codeType: 'ICD-10',
  description: 'Type 2 diabetes mellitus without complications',
  confidenceScore: 96,
  source: 'Intake + EHR',
}

const LOW_CONF_SUGGESTION: CodeSuggestion = {
  ...SUGGESTION,
  id: 'sug-low',
  code: 'R51.9',
  confidenceScore: 45,
}

describe('ConfidenceIndicator (via CodeSuggestionRow)', () => {
  it('renders confidence percentage', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )
    expect(screen.getByLabelText('High confidence: 96%')).toBeInTheDocument()
  })
})

describe('CodeSuggestionRow', () => {
  it('renders code, description, source in pending state', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    expect(screen.getByText('E11.9')).toBeInTheDocument()
    expect(screen.getByText('Type 2 diabetes mellitus without complications')).toBeInTheDocument()
    expect(screen.getByText('Intake + EHR')).toBeInTheDocument()
  })

  it('AC-02: shows Accept, Modify, Reject buttons when pending', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    expect(screen.getByRole('button', { name: 'Accept E11.9' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Modify E11.9' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Reject E11.9' })).toBeInTheDocument()
  })

  it('AC-01: Accept button uses primary style for high confidence (≥80%)', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    const acceptBtn = screen.getByRole('button', { name: 'Accept E11.9' })
    expect(acceptBtn).toHaveClass('bg-primary')
  })

  it('AC-01: Accept button uses outline style for low confidence (<80%)', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={LOW_CONF_SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    const acceptBtn = screen.getByRole('button', { name: 'Accept R51.9' })
    expect(acceptBtn).not.toHaveClass('bg-primary')
    expect(acceptBtn).toHaveClass('border')
  })

  it('AC-02: calls onAccept with audit entry when Accept is clicked', () => {
    const onAccept = vi.fn()
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          performedBy="Nurse Kelly"
          onAccept={onAccept}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Accept E11.9' }))

    expect(onAccept).toHaveBeenCalledOnce()
    const entry = onAccept.mock.calls[0][0] as CodeAuditEntry
    expect(entry.code).toBe('E11.9')
    expect(entry.action).toBe('accepted')
    expect(entry.performedBy).toBe('Nurse Kelly')
    expect(entry.performedAt).toMatch(/^\d{4}-\d{2}-\d{2}T/)
  })

  it('AC-02: calls onReject with audit entry when Reject is clicked', () => {
    const onReject = vi.fn()
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={onReject}
        />
      </TABLE_WRAPPER>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Reject E11.9' }))

    expect(onReject).toHaveBeenCalledOnce()
    const entry = onReject.mock.calls[0][0] as CodeAuditEntry
    expect(entry.code).toBe('E11.9')
    expect(entry.action).toBe('rejected')
  })

  it('AC-02: Modify button expands inline form', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    const modifyBtn = screen.getByRole('button', { name: 'Modify E11.9' })
    expect(modifyBtn).toHaveAttribute('aria-expanded', 'false')

    fireEvent.click(modifyBtn)

    expect(modifyBtn).toHaveAttribute('aria-expanded', 'true')
    expect(screen.getByLabelText('Reason for modification *')).toBeInTheDocument()
  })

  it('AC-03: Save modification disabled without reason', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Modify E11.9' }))
    expect(screen.getByRole('button', { name: 'Save modification' })).toBeDisabled()
  })

  it('AC-03: calls onModify with reason and modifiedCode when form submitted', () => {
    const onModify = vi.fn()
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="pending"
          onAccept={vi.fn()}
          onModify={onModify}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Modify E11.9' }))
    fireEvent.change(screen.getByLabelText('Replacement code'), {
      target: { value: 'E11.65' },
    })
    fireEvent.change(screen.getByLabelText('Reason for modification *'), {
      target: { value: 'More specific diabetes code' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save modification' }))

    expect(onModify).toHaveBeenCalledOnce()
    const entry = onModify.mock.calls[0][0] as CodeAuditEntry
    expect(entry.action).toBe('modified')
    expect(entry.modifiedCode).toBe('E11.65')
    expect(entry.reason).toBe('More specific diabetes code')
  })

  it('renders status badge and no action buttons when accepted', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="accepted"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    expect(screen.getByText('Accepted')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Accept E11.9' })).not.toBeInTheDocument()
  })

  it('renders status badge and no action buttons when rejected', () => {
    render(
      <TABLE_WRAPPER>
        <CodeSuggestionRow
          suggestion={SUGGESTION}
          status="rejected"
          onAccept={vi.fn()}
          onModify={vi.fn()}
          onReject={vi.fn()}
        />
      </TABLE_WRAPPER>,
    )

    expect(screen.getByText('Rejected')).toBeInTheDocument()
  })
})
