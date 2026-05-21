import { useState } from 'react'
import { cn } from '../../lib/utils'
import { ConfidenceIndicator } from './ConfidenceIndicator'

export type CodeType = 'ICD-10' | 'CPT'
export type SuggestionStatus = 'pending' | 'accepted' | 'modified' | 'rejected'

export interface CodeSuggestion {
  id: string
  code: string
  codeType: CodeType
  description: string
  confidenceScore: number
  source: string
}

// AC-03: Audit entry logged for every verify/modify/reject action
export interface CodeAuditEntry {
  suggestionId: string
  code: string
  action: 'accepted' | 'modified' | 'rejected'
  modifiedCode?: string
  reason?: string
  performedAt: string
  performedBy: string
}

interface CodeSuggestionRowProps {
  suggestion: CodeSuggestion
  status: SuggestionStatus
  performedBy?: string
  onAccept: (entry: CodeAuditEntry) => void
  onModify: (entry: CodeAuditEntry) => void
  onReject: (entry: CodeAuditEntry) => void
}

const STATUS_BADGE: Record<Exclude<SuggestionStatus, 'pending'>, string> = {
  accepted: 'bg-emerald-500/10 text-emerald-700',
  modified: 'bg-blue-500/10 text-blue-700',
  rejected: 'bg-destructive/10 text-destructive',
}

const STATUS_LABELS: Record<Exclude<SuggestionStatus, 'pending'>, string> = {
  accepted: 'Accepted',
  modified: 'Modified',
  rejected: 'Rejected',
}

export const CodeSuggestionRow = ({
  suggestion,
  status,
  performedBy = 'Staff',
  onAccept,
  onModify,
  onReject,
}: CodeSuggestionRowProps) => {
  const [modifying, setModifying] = useState(false)
  const [modifiedCode, setModifiedCode] = useState(suggestion.code)
  const [reason, setReason] = useState('')

  // AC-01: high confidence (≥80%) uses primary accent per wireframe SCR-021
  const isHighConfidence = suggestion.confidenceScore >= 80

  const buildEntry = (action: CodeAuditEntry['action']): CodeAuditEntry => ({
    suggestionId: suggestion.id,
    code: suggestion.code,
    action,
    performedAt: new Date().toISOString(),
    performedBy,
  })

  const handleAccept = () => onAccept(buildEntry('accepted'))

  const handleReject = () => onReject(buildEntry('rejected'))

  const handleModifySubmit = () => {
    if (!reason.trim()) return
    onModify({
      ...buildEntry('modified'),
      modifiedCode: modifiedCode.trim() || suggestion.code,
      reason: reason.trim(),
    })
    setModifying(false)
    setReason('')
  }

  // Resolved state — dimmed row with status badge, no action buttons
  if (status !== 'pending') {
    return (
      <tr className="opacity-60" data-uxr="SCR-021">
        <td className="px-4 py-3 font-mono text-sm">{suggestion.code}</td>
        <td className="px-4 py-3 text-sm">{suggestion.description}</td>
        <td className="px-4 py-3">
          <ConfidenceIndicator score={suggestion.confidenceScore} />
        </td>
        <td className="px-4 py-3 text-sm text-muted-foreground">{suggestion.source}</td>
        <td className="px-4 py-3">
          <span
            className={cn(
              'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium',
              STATUS_BADGE[status],
            )}
          >
            {STATUS_LABELS[status]}
          </span>
        </td>
      </tr>
    )
  }

  return (
    <>
      {/* AC-02: pending row with verify/modify/reject actions */}
      <tr data-uxr="SCR-021">
        <td className="px-4 py-3 font-mono text-sm">{suggestion.code}</td>
        <td className="px-4 py-3 text-sm">{suggestion.description}</td>
        <td className="px-4 py-3">
          <ConfidenceIndicator score={suggestion.confidenceScore} />
        </td>
        <td className="px-4 py-3 text-sm text-muted-foreground">{suggestion.source}</td>
        <td className="px-4 py-3">
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={handleAccept}
              className={cn(
                'rounded-lg px-3 py-1.5 text-xs font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
                isHighConfidence
                  ? 'bg-primary text-primary-foreground hover:bg-primary/90'
                  : 'border border-border bg-background text-foreground hover:bg-muted',
              )}
              aria-label={`Accept ${suggestion.code}`}
            >
              Accept
            </button>
            <button
              type="button"
              onClick={() => setModifying((v) => !v)}
              aria-expanded={modifying}
              aria-controls={`modify-form-${suggestion.id}`}
              className="rounded-lg px-3 py-1.5 text-xs font-medium text-foreground transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
              aria-label={`Modify ${suggestion.code}`}
            >
              Modify
            </button>
            <button
              type="button"
              onClick={handleReject}
              className="rounded-lg px-3 py-1.5 text-xs font-medium text-destructive transition-colors hover:bg-destructive/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
              aria-label={`Reject ${suggestion.code}`}
            >
              Reject
            </button>
          </div>
        </td>
      </tr>

      {/* AC-02 / AC-03: inline modify form — captures reason per requirement */}
      {modifying && (
        <tr>
          <td colSpan={5} className="px-4 pb-4 pt-0">
            <div
              id={`modify-form-${suggestion.id}`}
              className="rounded-lg border border-border bg-muted/40 p-4"
              role="region"
              aria-label={`Modify ${suggestion.code}`}
            >
              <p className="mb-3 text-xs font-semibold text-foreground">Modify suggestion</p>
              <div className="flex flex-col gap-3">
                <div>
                  <label
                    htmlFor={`modified-code-${suggestion.id}`}
                    className="mb-1 block text-xs text-muted-foreground"
                  >
                    Replacement code
                  </label>
                  <input
                    id={`modified-code-${suggestion.id}`}
                    type="text"
                    value={modifiedCode}
                    onChange={(e) => setModifiedCode(e.target.value)}
                    className="w-full rounded-md border border-border bg-background px-3 py-1.5 font-mono text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                    placeholder="e.g., E11.65"
                    autoComplete="off"
                  />
                </div>
                <div>
                  <label
                    htmlFor={`reason-${suggestion.id}`}
                    className="mb-1 block text-xs text-muted-foreground"
                  >
                    Reason for modification{' '}
                    <span aria-hidden="true" className="text-destructive">
                      *
                    </span>
                  </label>
                  <input
                    id={`reason-${suggestion.id}`}
                    type="text"
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    className="w-full rounded-md border border-border bg-background px-3 py-1.5 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                    placeholder="e.g., More specific code applies"
                    aria-required="true"
                    autoComplete="off"
                  />
                </div>
                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={handleModifySubmit}
                    disabled={!reason.trim()}
                    className="rounded-lg bg-primary px-3 py-1.5 text-xs font-medium text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Save modification
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setModifying(false)
                      setReason('')
                      setModifiedCode(suggestion.code)
                    }}
                    className="rounded-lg border border-border px-3 py-1.5 text-xs font-medium text-foreground transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          </td>
        </tr>
      )}
    </>
  )
}
