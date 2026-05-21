import type { AvailabilitySlot } from '../../services/bookingApi'
import { CountdownTimer } from './CountdownTimer'

interface SlotCardProps {
  slot: AvailabilitySlot
  isSelected: boolean
  lockSecondsRemaining?: number
  onSelect: (slot: AvailabilitySlot) => void
  onLockExpire?: () => void
}

const formatTime = (time: string): string => {
  const [hourStr, minuteStr] = time.split(':')
  const hour = parseInt(hourStr, 10)
  const suffix = hour >= 12 ? 'PM' : 'AM'
  const displayHour = hour % 12 === 0 ? 12 : hour % 12
  return `${displayHour.toString().padStart(2, '0')}:${minuteStr} ${suffix}`
}

export const SlotCard = ({
  slot,
  isSelected,
  lockSecondsRemaining,
  onSelect,
  onLockExpire,
}: SlotCardProps) => {
  const isUnavailable = !slot.isAvailable || (slot.isLocked && !isSelected)

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault()
      if (!isUnavailable) {
        onSelect(slot)
      }
    }
  }

  const cardLabel = [
    slot.providerName,
    slot.specialty,
    formatTime(slot.startTime),
    `${slot.durationMinutes} minutes`,
    isSelected ? '— selected' : '',
    isUnavailable ? '— unavailable' : '',
  ]
    .filter(Boolean)
    .join(', ')

  const cardClasses = [
    'p-4 border-2 rounded-lg transition-all duration-150',
    isSelected
      ? 'border-primary bg-primary/5'
      : isUnavailable
        ? 'opacity-50 cursor-not-allowed bg-muted border-border'
        : 'border-border bg-card cursor-pointer hover:border-primary hover:shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
  ].join(' ')

  return (
    <div
      role="button"
      tabIndex={isUnavailable ? -1 : 0}
      aria-label={cardLabel}
      aria-pressed={isSelected}
      aria-disabled={isUnavailable}
      className={cardClasses}
      data-uxr="UXR-502"
      onClick={() => !isUnavailable && onSelect(slot)}
      onKeyDown={handleKeyDown}
    >
      <div className="flex items-center gap-2 mb-2">
        <div
          className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-xs font-semibold text-primary-foreground"
          aria-hidden="true"
        >
          {slot.providerInitials}
        </div>
        <div>
          <div className="text-sm font-medium text-foreground">{slot.providerName}</div>
          <div className="text-xs text-muted-foreground">{slot.specialty}</div>
        </div>
      </div>

      <div className="flex items-center justify-between mt-2">
        <div
          className={`text-sm font-semibold ${isSelected ? 'text-primary' : 'text-foreground'}`}
        >
          {formatTime(slot.startTime)}
        </div>
        <div className="text-xs text-muted-foreground">{slot.durationMinutes} min</div>
      </div>

      {isUnavailable && slot.isLocked && !isSelected ? (
        <p className="mt-2 text-xs text-muted-foreground border-t border-border pt-2">
          Temporarily held by another user
        </p>
      ) : null}

      {isSelected && typeof lockSecondsRemaining === 'number' ? (
        <div className="mt-3 pt-3 border-t border-border flex items-center gap-2">
          <CountdownTimer
            totalSeconds={30}
            secondsRemaining={lockSecondsRemaining}
            size="sm"
            onExpire={onLockExpire}
          />
          <span className="text-xs font-medium text-primary">
            Locked · {lockSecondsRemaining}s remaining
          </span>
        </div>
      ) : null}
    </div>
  )
}
