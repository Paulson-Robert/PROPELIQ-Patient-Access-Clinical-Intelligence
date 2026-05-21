import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { GripVertical } from 'lucide-react'
import type { QueueEntry, QueueStatus, RiskLevel } from '../../services/queueApi'

interface QueueItemProps {
  entry: QueueEntry
  isMarkingArrived: boolean
  onMarkArrived: (entryId: string) => void
}

const STATUS_STYLES: Record<QueueStatus, string> = {
  Scheduled: 'bg-primary/10 text-primary',
  Waiting: 'bg-yellow-500/10 text-yellow-700',
  Arrived: 'bg-amber-500/10 text-amber-700',
  Cancelled: 'bg-secondary text-secondary-foreground',
  Completed: 'bg-emerald-500/10 text-emerald-700',
}

const RISK_STYLES: Record<RiskLevel, string> = {
  High: 'bg-red-500/10 text-red-700',
  Medium: 'bg-amber-500/10 text-amber-700',
  Low: 'bg-emerald-500/10 text-emerald-700',
}

const formatArrivalTime = (isoTimestamp: string): string => {
  return new Date(isoTimestamp).toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
  })
}

export const QueueItem = ({ entry, isMarkingArrived, onMarkArrived }: QueueItemProps) => {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: entry.id,
  })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.4 : 1,
  }

  const canMarkArrived = entry.status === 'Scheduled' || entry.status === 'Waiting'
  const isCancelled = entry.status === 'Cancelled'

  return (
    <tr
      ref={setNodeRef}
      style={style}
      className={`border-b border-border last:border-0 ${isCancelled ? 'opacity-50' : ''}`}
      aria-label={`Queue position ${entry.position}: ${entry.patientName}`}
    >
      {/* Drag handle + position */}
      <td className="py-3 pl-4 pr-2">
        <div className="flex items-center gap-1">
          <button
            type="button"
            className="cursor-grab touch-none text-muted-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring active:cursor-grabbing"
            aria-label={`Drag to reorder ${entry.patientName}`}
            {...attributes}
            {...listeners}
          >
            <GripVertical size={16} aria-hidden="true" />
          </button>
          <span className="flex h-7 w-7 items-center justify-center rounded-full bg-muted text-sm font-semibold">
            {entry.position}
          </span>
        </div>
      </td>

      {/* Patient name */}
      <td className="px-3 py-3">
        <span className={`font-medium ${isCancelled ? 'line-through' : ''}`}>
          {entry.patientName}
        </span>
        {entry.bookingType === 'WalkIn' && (
          <span className="ml-2 rounded-full bg-secondary px-2 py-0.5 text-[10px] font-medium text-secondary-foreground">
            Walk-in
          </span>
        )}
      </td>

      {/* Appointment time */}
      <td className="px-3 py-3 text-sm">{entry.appointmentTime}</td>

      {/* Provider */}
      <td className="px-3 py-3 text-sm">{entry.providerName}</td>

      {/* Status */}
      <td className="px-3 py-3">
        <span
          className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${STATUS_STYLES[entry.status]}`}
          aria-label={`Status: ${entry.status}`}
        >
          {entry.status}
        </span>
      </td>

      {/* Risk */}
      <td className="px-3 py-3">
        <span
          className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${RISK_STYLES[entry.riskLevel]}`}
          aria-label={`Risk level: ${entry.riskLevel}`}
        >
          {entry.riskLevel}
        </span>
      </td>

      {/* Actions */}
      <td className="px-3 py-3">
        <div className="flex items-center gap-2">
          {entry.status === 'Arrived' && entry.arrivalTimestamp ? (
            <span className="text-xs text-muted-foreground" aria-label="Arrived at">
              Arrived {formatArrivalTime(entry.arrivalTimestamp)}
            </span>
          ) : (
            <button
              type="button"
              onClick={() => onMarkArrived(entry.id)}
              disabled={!canMarkArrived || isMarkingArrived}
              className="rounded-md bg-primary px-3 py-1.5 text-xs font-medium text-primary-foreground transition-colors hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label={`Mark ${entry.patientName} as arrived`}
            >
              Mark Arrived
            </button>
          )}
        </div>
      </td>
    </tr>
  )
}
