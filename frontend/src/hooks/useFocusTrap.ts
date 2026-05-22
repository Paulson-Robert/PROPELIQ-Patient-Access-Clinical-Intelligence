import { useCallback, useEffect, useRef } from 'react'

const FOCUSABLE_SELECTORS = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
  '[contenteditable="true"]',
].join(', ')

interface UseFocusTrapOptions {
  /** Whether the trap is currently active. */
  isActive: boolean
  /** Called when the Escape key is pressed while the trap is active. */
  onEscape?: () => void
  /** Element that triggered the trap; receives focus when trap is deactivated. */
  triggerRef?: React.RefObject<HTMLElement | null>
}

/**
 * Traps keyboard focus inside a container element while `isActive` is true.
 *
 * Returns a ref to attach to the container element.
 *
 * Satisfies AC-04 (focus trap in modals) and AC-03 (keyboard navigation).
 * WCAG 2.2 SC 2.1.2 — No Keyboard Trap (wraps rather than blocks).
 */
export function useFocusTrap<T extends HTMLElement = HTMLElement>({
  isActive,
  onEscape,
  triggerRef,
}: UseFocusTrapOptions) {
  const containerRef = useRef<T>(null)

  const getFocusableElements = useCallback((): HTMLElement[] => {
    if (!containerRef.current) return []
    return Array.from(
      containerRef.current.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTORS),
    ).filter((el) => !el.closest('[inert]') && el.offsetParent !== null)
  }, [])

  // Move focus into container when trap becomes active
  useEffect(() => {
    if (!isActive || !containerRef.current) return

    const focusable = getFocusableElements()
    const target = focusable[0] ?? containerRef.current
    // Defer so the element is rendered before focusing
    const id = requestAnimationFrame(() => {
      target.focus({ preventScroll: false })
    })
    return () => cancelAnimationFrame(id)
  }, [isActive, getFocusableElements])

  // Restore focus to trigger element when trap is deactivated
  useEffect(() => {
    if (isActive) return
    triggerRef?.current?.focus()
  }, [isActive, triggerRef])

  // Intercept Tab and Escape inside the container
  useEffect(() => {
    if (!isActive) return

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onEscape?.()
        return
      }

      if (event.key !== 'Tab') return

      const focusable = getFocusableElements()
      if (focusable.length === 0) {
        event.preventDefault()
        return
      }

      const first = focusable[0]
      const last = focusable[focusable.length - 1]
      const active = document.activeElement as HTMLElement

      if (event.shiftKey) {
        // Shift+Tab: if on first element, wrap to last
        if (active === first) {
          event.preventDefault()
          last.focus()
        }
      } else {
        // Tab: if on last element, wrap to first
        if (active === last) {
          event.preventDefault()
          first.focus()
        }
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [isActive, onEscape, getFocusableElements])

  return containerRef
}
