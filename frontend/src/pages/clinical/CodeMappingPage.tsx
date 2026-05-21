import { useState } from 'react'
import { ArrowLeft } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import {
  CodeSuggestionRow,
  type CodeAuditEntry,
  type CodeSuggestion,
  type SuggestionStatus,
} from '../../components/clinical/CodeSuggestionRow'

// Inferred decision: No code-suggestion API endpoint in scope for this task.
// Static mock data demonstrates ICD-10/CPT suggestion display per SCR-021.
const MOCK_SUGGESTIONS: CodeSuggestion[] = [
  {
    id: 'sug-001',
    code: 'E11.9',
    codeType: 'ICD-10',
    description: 'Type 2 diabetes mellitus without complications',
    confidenceScore: 96,
    source: 'Intake + EHR',
  },
  {
    id: 'sug-002',
    code: 'I10',
    codeType: 'ICD-10',
    description: 'Essential (primary) hypertension',
    confidenceScore: 94,
    source: 'Intake + EHR',
  },
  {
    id: 'sug-003',
    code: '99213',
    codeType: 'CPT',
    description: 'Office visit, est. patient, low-mod complexity',
    confidenceScore: 72,
    source: 'ML model',
  },
  {
    id: 'sug-004',
    code: 'R51.9',
    codeType: 'ICD-10',
    description: 'Headache, unspecified',
    confidenceScore: 45,
    source: 'NLP extraction',
  },
]

// Inferred decision: patient name resolved from patientId using a static map
// until a patient API is wired in a future task.
const PATIENT_NAMES: Record<string, string> = {
  'pat-001': 'Maria Santos',
}

type StatusMap = Record<string, SuggestionStatus>

export const CodeMappingPage = () => {
  const { patientId = 'pat-001' } = useParams<{ patientId: string }>()
  const patientName = PATIENT_NAMES[patientId] ?? 'Patient'

  const [statusMap, setStatusMap] = useState<StatusMap>({})
  const [manualCode, setManualCode] = useState('')
  const [manualCodes, setManualCodes] = useState<string[]>([])

  const setStatus = (id: string, status: SuggestionStatus) =>
    setStatusMap((prev) => ({ ...prev, [id]: status }))

  const handleAccept = (entry: CodeAuditEntry) => {
    // AC-03: audit entry available for upstream persistence
    console.info('[CodeMapping] accept', entry)
    setStatus(entry.suggestionId, 'accepted')
  }

  const handleModify = (entry: CodeAuditEntry) => {
    console.info('[CodeMapping] modify', entry)
    setStatus(entry.suggestionId, 'modified')
  }

  const handleReject = (entry: CodeAuditEntry) => {
    console.info('[CodeMapping] reject', entry)
    setStatus(entry.suggestionId, 'rejected')
  }

  const handleManualAdd = () => {
    const trimmed = manualCode.trim().toUpperCase()
    if (!trimmed) return
    setManualCodes((prev) => [...prev, trimmed])
    setManualCode('')
  }

  const hasSuggestions = MOCK_SUGGESTIONS.length > 0

  return (
    <main className="min-h-screen bg-background text-foreground" id="main-content">
      <a
        href="#code-table"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
      >
        Skip to code table
      </a>

      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
        {/* Page header */}
        <header className="mb-6 flex items-center gap-3">
          <Link
            to={`/clinical/patient/${patientId}`}
            className="inline-flex items-center justify-center rounded-md p-1.5 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
            aria-label="Back to patient view"
          >
            <ArrowLeft className="h-5 w-5" aria-hidden="true" />
          </Link>
          <h1 className="text-xl font-semibold tracking-tight">
            Code mapping — {patientName}
          </h1>
        </header>

        {/* AC-01: AI-suggested codes table */}
        <section aria-labelledby="suggestions-heading" id="code-table" className="mb-6">
          <h2
            id="suggestions-heading"
            className="mb-4 text-base font-semibold tracking-tight text-foreground"
          >
            AI-suggested codes
          </h2>

          {/* Edge case: no suggestions — "Unable to map" with manual entry prompt */}
          {!hasSuggestions ? (
            <div
              className="rounded-xl border border-border bg-card p-8 text-center"
              data-uxr="SCR-021"
              role="status"
              aria-label="No code suggestions available"
            >
              <p className="text-sm font-medium text-foreground">Unable to map</p>
              <p className="mt-1 text-sm text-muted-foreground">
                No AI suggestions could be generated. Use the manual entry form below to add
                codes.
              </p>
            </div>
          ) : (
            <div
              className="overflow-x-auto rounded-xl border border-border bg-card"
              data-uxr="SCR-021"
            >
              <table
                className="w-full border-collapse text-sm"
                aria-label="AI-suggested medical codes"
              >
                <thead>
                  <tr className="border-b border-border bg-muted/40">
                    <th
                      scope="col"
                      className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground"
                    >
                      Code
                    </th>
                    <th
                      scope="col"
                      className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground"
                    >
                      Description
                    </th>
                    <th
                      scope="col"
                      className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground"
                    >
                      Confidence
                    </th>
                    <th
                      scope="col"
                      className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground"
                    >
                      Source
                    </th>
                    <th
                      scope="col"
                      className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground"
                    >
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {MOCK_SUGGESTIONS.map((suggestion) => (
                    <CodeSuggestionRow
                      key={suggestion.id}
                      suggestion={suggestion}
                      status={statusMap[suggestion.id] ?? 'pending'}
                      onAccept={handleAccept}
                      onModify={handleModify}
                      onReject={handleReject}
                    />
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        {/* Manual entry — always visible (edge case: primary entry when no suggestions) */}
        <section
          className="rounded-xl border border-border bg-card p-6"
          aria-labelledby="manual-heading"
          data-uxr="SCR-021"
        >
          <h2
            id="manual-heading"
            className="mb-4 text-base font-semibold tracking-tight text-foreground"
          >
            Add code manually
          </h2>
          <div className="flex items-end gap-3">
            <div className="flex-1">
              <label
                htmlFor="manual-code-input"
                className="mb-1.5 block text-sm text-muted-foreground"
              >
                ICD-10 / CPT code
              </label>
              <input
                id="manual-code-input"
                type="text"
                value={manualCode}
                onChange={(e) => setManualCode(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') handleManualAdd()
                }}
                className="w-full rounded-lg border border-border bg-background px-3 py-2 font-mono text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                placeholder="e.g., J06.9"
                autoComplete="off"
                aria-describedby="manual-code-hint"
              />
              <p id="manual-code-hint" className="sr-only">
                Enter an ICD-10 or CPT code and press Add or Enter
              </p>
            </div>
            <button
              type="button"
              onClick={handleManualAdd}
              disabled={!manualCode.trim()}
              className="rounded-lg bg-secondary px-4 py-2 text-sm font-medium text-secondary-foreground transition-colors hover:bg-secondary/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              Add
            </button>
          </div>

          {/* Manually added codes list */}
          {manualCodes.length > 0 && (
            <ul
              className="mt-4 flex flex-wrap gap-2"
              aria-label="Manually added codes"
            >
              {manualCodes.map((code) => (
                <li
                  key={code}
                  className="inline-flex items-center rounded-full border border-border bg-muted px-3 py-1 font-mono text-xs"
                >
                  {code}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </main>
  )
}
