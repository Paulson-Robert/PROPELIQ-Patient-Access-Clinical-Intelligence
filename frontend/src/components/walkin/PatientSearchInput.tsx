import { useEffect, useId, useState } from 'react'
import { bookingApi, type PatientSearchResult } from '../../services/bookingApi'

interface PatientSearchInputProps {
  selectedPatient: PatientSearchResult | null
  onSelectPatient: (patient: PatientSearchResult) => void
  title?: string
  description?: string
  emptyMessage?: string
}

const formatPatientMeta = (patient: PatientSearchResult): string => {
  const parts: string[] = []
  if (patient.phone) parts.push(patient.phone)
  if (patient.email) parts.push(patient.email)
  if (patient.lastVisitDate) {
    const date = new Date(patient.lastVisitDate)
    parts.push(`Last visit: ${date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}`)
  }
  return parts.join(' · ')
}

export const PatientSearchInput = ({
  selectedPatient,
  onSelectPatient,
  title = 'Find patient',
  description = 'Search by name, phone, or email',
  emptyMessage = 'No matching patients found. You can continue with guest walk-in.',
}: PatientSearchInputProps) => {
  const inputId = useId()
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<PatientSearchResult[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [searchError, setSearchError] = useState<string | null>(null)

  useEffect(() => {
    const trimmedQuery = query.trim()

    if (trimmedQuery.length < 2) {
      setResults([])
      setSearchError(null)
      setIsLoading(false)
      return
    }

    let isActive = true
    setIsLoading(true)
    setSearchError(null)

    const timeoutId = window.setTimeout(async () => {
      try {
        const searchResults = await bookingApi.searchPatients(trimmedQuery)
        if (!isActive) return
        setResults(searchResults)
      } catch {
        if (!isActive) return
        setSearchError('Unable to search patients right now. Please try again.')
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }, 250)

    return () => {
      isActive = false
      window.clearTimeout(timeoutId)
    }
  }, [query])

  return (
    <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
      <h2 className="text-lg font-semibold tracking-tight">{title}</h2>
      <p className="mt-1 text-sm text-muted-foreground">{description}</p>

      <div className="mt-4 space-y-2">
        <label htmlFor={inputId} className="text-sm font-medium text-foreground">
          Patient search
        </label>
        <input
          id={inputId}
          type="search"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="e.g., Maria Santos"
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          aria-describedby={`${inputId}-hint`}
        />
        <p id={`${inputId}-hint`} className="text-xs text-muted-foreground">
          Enter at least 2 characters.
        </p>
      </div>

      {isLoading ? (
        <p className="mt-3 text-sm text-muted-foreground" role="status">
          Searching patients...
        </p>
      ) : null}

      {searchError ? (
        <p className="mt-3 rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm text-destructive" role="alert">
          {searchError}
        </p>
      ) : null}

      {!isLoading && query.trim().length >= 2 && results.length === 0 && !searchError ? (
        <p className="mt-3 rounded-md border border-border bg-muted/40 px-3 py-2 text-sm text-muted-foreground">
          {emptyMessage}
        </p>
      ) : null}

      {results.length > 0 ? (
        <div className="mt-3 overflow-hidden rounded-md border border-border" role="listbox" aria-label="Patient search results">
          {results.map((patient) => {
            const isSelected = selectedPatient?.id === patient.id

            return (
              <button
                type="button"
                key={patient.id}
                role="option"
                aria-selected={isSelected}
                onClick={() => onSelectPatient(patient)}
                className="flex w-full items-start justify-between border-b border-border px-3 py-3 text-left transition hover:bg-muted/40 last:border-b-0"
              >
                <span>
                  <span className="block text-sm font-medium text-foreground">{patient.name}</span>
                  <span className="block text-xs text-muted-foreground">{formatPatientMeta(patient)}</span>
                </span>
                {isSelected ? (
                  <span className="rounded-full bg-primary/10 px-2 py-1 text-xs font-medium text-primary">
                    Selected
                  </span>
                ) : results.length > 1 ? (
                  <span className="rounded-full bg-amber-100 px-2 py-1 text-xs font-medium text-amber-700">
                    Match
                  </span>
                ) : null}
              </button>
            )
          })}
        </div>
      ) : null}
    </section>
  )
}
