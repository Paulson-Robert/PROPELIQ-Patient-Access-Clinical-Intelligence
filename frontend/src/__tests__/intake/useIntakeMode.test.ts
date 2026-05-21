import { renderHook, act } from '@testing-library/react'
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { useIntakeMode } from '../../hooks/useIntakeMode'

// ---------------------------------------------------------------------------
// localStorage mock
// ---------------------------------------------------------------------------

const localStorageMock = (() => {
  let store: Record<string, string> = {}
  return {
    getItem: (key: string) => store[key] ?? null,
    setItem: (key: string, value: string) => { store[key] = value },
    removeItem: (key: string) => { delete store[key] },
    clear: () => { store = {} },
  }
})()

beforeEach(() => {
  Object.defineProperty(window, 'localStorage', { value: localStorageMock, writable: true })
  localStorageMock.clear()
})

afterEach(() => {
  vi.restoreAllMocks()
})

const MANUAL_DRAFT_KEY = 'intake-manual-draft'

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('useIntakeMode', () => {
  describe('initial state', () => {
    it('starts in ai mode', () => {
      const { result } = renderHook(() => useIntakeMode())
      expect(result.current.mode).toBe('ai')
    })

    it('starts with manualRemountKey = 0', () => {
      const { result } = renderHook(() => useIntakeMode())
      expect(result.current.manualRemountKey).toBe(0)
    })
  })

  describe('switchToAi', () => {
    it('switches mode back to ai from manual', () => {
      const { result } = renderHook(() => useIntakeMode())

      act(() => { result.current.bridgeAndSwitchToManual() })
      expect(result.current.mode).toBe('manual')

      act(() => { result.current.switchToAi() })
      expect(result.current.mode).toBe('ai')
    })
  })

  describe('bridgeAndSwitchToManual', () => {
    it('switches mode to manual (AC-01)', () => {
      const { result } = renderHook(() => useIntakeMode())

      act(() => { result.current.bridgeAndSwitchToManual() })

      expect(result.current.mode).toBe('manual')
    })

    it('increments manualRemountKey on each switch so ManualIntakePage re-mounts (AC-02)', () => {
      const { result } = renderHook(() => useIntakeMode())

      act(() => { result.current.bridgeAndSwitchToManual() })
      expect(result.current.manualRemountKey).toBe(1)

      act(() => { result.current.switchToAi() })
      act(() => { result.current.bridgeAndSwitchToManual() })
      expect(result.current.manualRemountKey).toBe(2)
    })

    it('maps AI data to manual draft in localStorage (AC-02)', () => {
      const { result } = renderHook(() => useIntakeMode())

      act(() => {
        result.current.updateAiData({
          chronicConditions: 'Diabetes',
          currentMedications: 'Metformin',
          allergies: 'Penicillin',
          surgicalHistory: 'Appendectomy',
          reasonForVisit: 'Check-up',
        })
      })

      act(() => { result.current.bridgeAndSwitchToManual() })

      const draft = JSON.parse(localStorageMock.getItem(MANUAL_DRAFT_KEY) ?? '{}')
      expect(draft.chronicConditions).toBe('Diabetes')
      expect(draft.currentMedications).toBe('Metformin')
      expect(draft.knownAllergies).toBe('Penicillin')      // AC-02: allergies → knownAllergies
      expect(draft.pastSurgeries).toBe('Appendectomy')     // AC-02: surgicalHistory → pastSurgeries
      expect(draft.reasonForVisit).toBe('Check-up')
    })

    it('accepts a latestAiData override for mid-render snapshots (Edge Case)', () => {
      const { result } = renderHook(() => useIntakeMode())

      act(() => {
        result.current.bridgeAndSwitchToManual({ allergies: 'Latex', reasonForVisit: 'Rash' })
      })

      const draft = JSON.parse(localStorageMock.getItem(MANUAL_DRAFT_KEY) ?? '{}')
      expect(draft.knownAllergies).toBe('Latex')
      expect(draft.reasonForVisit).toBe('Rash')
    })

    it('merges into an existing manual draft without overwriting non-empty fields (AC-02)', () => {
      localStorageMock.setItem(
        MANUAL_DRAFT_KEY,
        JSON.stringify({ chronicConditions: 'Hypertension', familyHistory: 'Heart disease' }),
      )

      const { result } = renderHook(() => useIntakeMode())

      act(() => {
        result.current.updateAiData({ chronicConditions: 'Diabetes' })
      })
      act(() => { result.current.bridgeAndSwitchToManual() })

      const draft = JSON.parse(localStorageMock.getItem(MANUAL_DRAFT_KEY) ?? '{}')
      // AI value wins over existing draft when AI value is non-empty
      expect(draft.chronicConditions).toBe('Diabetes')
      // Pre-existing manual-only fields are preserved
      expect(draft.familyHistory).toBe('Heart disease')
    })

    it('does not overwrite existing manual fields with empty AI values (Edge Case)', () => {
      localStorageMock.setItem(
        MANUAL_DRAFT_KEY,
        JSON.stringify({ knownAllergies: 'Penicillin' }),
      )

      const { result } = renderHook(() => useIntakeMode())

      // No allergies in AI data
      act(() => {
        result.current.updateAiData({ chronicConditions: 'Asthma' })
      })
      act(() => { result.current.bridgeAndSwitchToManual() })

      const draft = JSON.parse(localStorageMock.getItem(MANUAL_DRAFT_KEY) ?? '{}')
      // Empty string AI value must NOT overwrite existing non-empty manual value
      expect(draft.knownAllergies).toBe('Penicillin')
    })
  })

  describe('updateAiData', () => {
    it('accumulates partial AI data across multiple calls', () => {
      const { result } = renderHook(() => useIntakeMode())

      act(() => { result.current.updateAiData({ chronicConditions: 'Asthma' }) })
      act(() => { result.current.updateAiData({ allergies: 'Dust' }) })
      act(() => { result.current.bridgeAndSwitchToManual() })

      const draft = JSON.parse(localStorageMock.getItem(MANUAL_DRAFT_KEY) ?? '{}')
      expect(draft.chronicConditions).toBe('Asthma')
      expect(draft.knownAllergies).toBe('Dust')
    })
  })

  describe('auto-fallback (AC-03)', () => {
    it('bridgeAndSwitchToManual works with no AI data collected (empty state)', () => {
      const { result } = renderHook(() => useIntakeMode())

      act(() => { result.current.bridgeAndSwitchToManual() })

      expect(result.current.mode).toBe('manual')
      // Draft written with empty strings — no error thrown
      const raw = localStorageMock.getItem(MANUAL_DRAFT_KEY)
      expect(raw).not.toBeNull()
    })
  })
})
