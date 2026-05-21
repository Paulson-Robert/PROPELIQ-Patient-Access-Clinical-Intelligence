import { useCallback } from 'react'
import { AiIntakePage } from './AiIntakePage'
import { ManualIntakePage } from './ManualIntakePage'
import { useIntakeMode, type AiExtractedData } from '../../hooks/useIntakeMode'

// ---------------------------------------------------------------------------
// IntakePage
//
// Parent shell that owns mode state and data bridging (AC-01, AC-02, AC-03).
// The child pages (AiIntakePage / ManualIntakePage) render their own headers
// including the mode toggle; IntakePage intercepts those toggle events via
// optional callback props so no navigation occurs — only a local re-render.
// ---------------------------------------------------------------------------

export const IntakePage = () => {
  const { mode, manualRemountKey, updateAiData, bridgeAndSwitchToManual, switchToAi } =
    useIntakeMode()

  // Capture the latest AI data snapshot and bridge to manual draft before
  // switching modes (AC-02 + Edge Case: partial AI data mapped to fields).
  const handleSwitchToManual = useCallback(() => {
    bridgeAndSwitchToManual()
  }, [bridgeAndSwitchToManual])

  // Relay AI-collected field answers up to the hook so the bridge has current
  // data even before the user explicitly switches (AC-02).
  const handleExtractedDataChange = useCallback(
    (data: Partial<AiExtractedData>) => {
      updateAiData(data)
    },
    [updateAiData],
  )

  if (mode === 'ai') {
    return (
      <AiIntakePage
        onSwitchToManual={handleSwitchToManual}
        onAiUnavailable={handleSwitchToManual}
        onExtractedDataChange={handleExtractedDataChange}
      />
    )
  }

  // key={manualRemountKey} forces ManualIntakePage to re-mount after the AI→
  // manual bridge writes the mapped draft to localStorage, so the page reads
  // the pre-filled data (AC-02).
  return <ManualIntakePage key={manualRemountKey} onSwitchToAi={switchToAi} />
}
