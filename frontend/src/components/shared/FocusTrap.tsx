import { useRef, type ReactNode } from 'react'
import { useFocusTrap } from '../../hooks/useFocusTrap'

interface FocusTrapProps {
  /** Whether the trap is currently active (i.e. the modal is open). */
  isOpen: boolean
  /** Called when the Escape key is pressed. Should close the modal. */
  onClose: () => void
  /** Element that triggered the modal; receives focus on close. */
  triggerRef?: React.RefObject<HTMLElement | null>
  children: ReactNode
  className?: string
}

/**
 * Renders a container that traps keyboard focus while `isOpen` is true.
 *
 * Wrap modal/dialog content with this component to satisfy AC-04 (focus trap
 * in modals) and AC-03 (full keyboard navigation).
 *
 * @example
 * <FocusTrap isOpen={isModalOpen} onClose={handleClose} triggerRef={triggerRef}>
 *   <dialog role="dialog" aria-modal="true" aria-labelledby="dialog-title">
 *     ...
 *   </dialog>
 * </FocusTrap>
 */
export function FocusTrap({
  isOpen,
  onClose,
  triggerRef,
  children,
  className,
}: FocusTrapProps) {
  const internalTriggerRef = useRef<HTMLElement>(null)
  const resolvedTriggerRef = triggerRef ?? internalTriggerRef

  const containerRef = useFocusTrap<HTMLDivElement>({
    isActive: isOpen,
    onEscape: onClose,
    triggerRef: resolvedTriggerRef,
  })

  return (
    <div
      ref={containerRef}
      className={className}
    >
      {children}
    </div>
  )
}
