import { useCallback, useState } from 'react'

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export type IntakeMode = 'ai' | 'manual'

// Mirror of AiIntakePage's ExtractedData — only the fields that can be
// bridged to ManualIntakePage's IntakeFormData.
export interface AiExtractedData {
  chronicConditions: string | null
  currentMedications: string | null
  allergies: string | null
  surgicalHistory: string | null
  reasonForVisit: string | null
}

// Subset of ManualIntakePage's IntakeFormData that AI data maps onto.
interface ManualFormPatch {
  chronicConditions: string
  currentMedications: string
  knownAllergies: string
  pastSurgeries: string
  reasonForVisit: string
}

// ---------------------------------------------------------------------------
// Constants — must match ManualIntakePage.DRAFT_KEY
// ---------------------------------------------------------------------------

const MANUAL_DRAFT_KEY = 'intake-manual-draft'

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

const mapAiToManual = (ai: Partial<AiExtractedData>): ManualFormPatch => ({
  chronicConditions: ai.chronicConditions ?? '',
  currentMedications: ai.currentMedications ?? '',
  knownAllergies: ai.allergies ?? '',
  pastSurgeries: ai.surgicalHistory ?? '',
  reasonForVisit: ai.reasonForVisit ?? '',
})

const readManualDraft = (): Record<string, string> | null => {
  try {
    const raw = localStorage.getItem(MANUAL_DRAFT_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as unknown
    if (typeof parsed !== 'object' || parsed === null) return null
    return parsed as Record<string, string>
  } catch {
    return null
  }
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

export interface UseIntakeModeReturn {
  mode: IntakeMode
  /** Increment key — pass as `key` prop to ManualIntakePage so it re-mounts
   *  and picks up the bridged localStorage draft after an AI→manual switch. */
  manualRemountKey: number
  /** Called by IntakePage whenever AiIntakePage reports new extracted data. */
  updateAiData: (data: Partial<AiExtractedData>) => void
  /** Bridge AI-collected data to ManualIntakePage's localStorage draft, then
   *  switch to manual mode.  Accepts an optional latest snapshot so callers
   *  can pass the most recent extraction before state has settled. */
  bridgeAndSwitchToManual: (latestAiData?: Partial<AiExtractedData>) => void
  /** Switch back to AI mode. */
  switchToAi: () => void
}

export function useIntakeMode(): UseIntakeModeReturn {
  const [mode, setMode] = useState<IntakeMode>('ai')
  const [aiData, setAiData] = useState<Partial<AiExtractedData>>({})
  const [manualRemountKey, setManualRemountKey] = useState(0)

  const updateAiData = useCallback((data: Partial<AiExtractedData>) => {
    setAiData((prev) => ({ ...prev, ...data }))
  }, [])

  const bridgeAndSwitchToManual = useCallback(
    (latestAiData?: Partial<AiExtractedData>) => {
      const source = latestAiData ?? aiData
      const patch = mapAiToManual(source)

      // Merge: non-empty AI values overwrite existing manual draft fields so
      // partial AI progress is preserved (AC-02 / Edge Case).
      const existing = readManualDraft() ?? {}
      const merged: Record<string, string> = { ...existing }
      for (const [k, v] of Object.entries(patch)) {
        if (v) merged[k] = v
      }
      localStorage.setItem(MANUAL_DRAFT_KEY, JSON.stringify(merged))

      // Force ManualIntakePage to re-mount so it re-reads the updated draft.
      setManualRemountKey((n) => n + 1)
      setMode('manual')
    },
    [aiData],
  )

  const switchToAi = useCallback(() => {
    setMode('ai')
  }, [])

  return { mode, manualRemountKey, updateAiData, bridgeAndSwitchToManual, switchToAi }
}
