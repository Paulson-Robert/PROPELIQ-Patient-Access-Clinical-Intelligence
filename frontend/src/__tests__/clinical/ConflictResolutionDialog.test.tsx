import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import {
  ConflictResolutionDialog,
  type ConflictAuditEntry,
  type ConflictSource,
} from '../../components/clinical/ConflictResolutionDialog'

const SOURCES: ConflictSource[] = [
  { source: 'EHR', value: '142/88 mmHg', recordedAt: 'Jan 20, 2025' },
  { source: 'Intake', value: '138/85 mmHg', recordedAt: 'Jan 26, 2025' },
]

describe('ConflictResolutionDialog', () => {
  it('renders nothing when closed', () => {
    render(
      <ConflictResolutionDialog
        open={false}
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    )
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('renders the dialog with field label when open', () => {
    render(
      <ConflictResolutionDialog
        open
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByText('Resolve conflict — Blood pressure')).toBeInTheDocument()
  })

  it('renders all source radio options (edge case: multiple sources)', () => {
    render(
      <ConflictResolutionDialog
        open
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByLabelText(/Accept value from EHR: 142\/88 mmHg/)).toBeInTheDocument()
    expect(screen.getByLabelText(/Accept value from Intake: 138\/85 mmHg/)).toBeInTheDocument()
    expect(screen.getByLabelText('Enter a custom value')).toBeInTheDocument()
  })

  it('keeps Confirm button disabled until a source is selected', () => {
    render(
      <ConflictResolutionDialog
        open
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByRole('button', { name: 'Confirm resolution' })).toBeDisabled()
  })

  it('enables Confirm and calls onConfirm with audit entry when source is selected', () => {
    const onConfirm = vi.fn()
    render(
      <ConflictResolutionDialog
        open
        fieldLabel="Blood pressure"
        sources={SOURCES}
        resolvedBy="Dr. Jones"
        onConfirm={onConfirm}
        onCancel={vi.fn()}
      />,
    )

    fireEvent.click(screen.getByLabelText(/Accept value from EHR: 142\/88 mmHg/))
    const confirmBtn = screen.getByRole('button', { name: 'Confirm resolution' })
    expect(confirmBtn).not.toBeDisabled()

    fireEvent.click(confirmBtn)
    expect(onConfirm).toHaveBeenCalledOnce()
    const auditEntry = onConfirm.mock.calls[0][0] as ConflictAuditEntry
    expect(auditEntry.fieldLabel).toBe('Blood pressure')
    expect(auditEntry.acceptedSource).toBe('EHR')
    expect(auditEntry.acceptedValue).toBe('142/88 mmHg')
    expect(auditEntry.resolvedBy).toBe('Dr. Jones')
    expect(auditEntry.resolvedAt).toMatch(/^\d{4}-\d{2}-\d{2}T/)
  })

  it('enables Confirm when custom value is entered and captures it in audit entry', () => {
    const onConfirm = vi.fn()
    render(
      <ConflictResolutionDialog
        open
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onConfirm={onConfirm}
        onCancel={vi.fn()}
      />,
    )

    fireEvent.click(screen.getByLabelText('Enter a custom value'))
    const customInput = screen.getByLabelText('Custom value for Blood pressure')
    fireEvent.change(customInput, { target: { value: '140/87 mmHg' } })

    const confirmBtn = screen.getByRole('button', { name: 'Confirm resolution' })
    expect(confirmBtn).not.toBeDisabled()

    fireEvent.click(confirmBtn)
    const auditEntry = onConfirm.mock.calls[0][0] as ConflictAuditEntry
    expect(auditEntry.acceptedSource).toBe('Custom')
    expect(auditEntry.acceptedValue).toBe('140/87 mmHg')
  })

  it('calls onCancel when Cancel button is clicked', () => {
    const onCancel = vi.fn()
    render(
      <ConflictResolutionDialog
        open
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onConfirm={vi.fn()}
        onCancel={onCancel}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))
    expect(onCancel).toHaveBeenCalledOnce()
  })

  it('calls onCancel when Escape key is pressed', () => {
    const onCancel = vi.fn()
    render(
      <ConflictResolutionDialog
        open
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onConfirm={vi.fn()}
        onCancel={onCancel}
      />,
    )

    fireEvent.keyDown(document, { key: 'Escape' })
    expect(onCancel).toHaveBeenCalledOnce()
  })

  it('calls onCancel when backdrop is clicked', () => {
    const onCancel = vi.fn()
    render(
      <ConflictResolutionDialog
        open
        fieldLabel="Blood pressure"
        sources={SOURCES}
        onConfirm={vi.fn()}
        onCancel={onCancel}
      />,
    )

    // The backdrop is the aria-hidden sibling div
    const backdrop = document.querySelector('[aria-hidden="true"]') as HTMLElement
    fireEvent.click(backdrop)
    expect(onCancel).toHaveBeenCalledOnce()
  })
})
