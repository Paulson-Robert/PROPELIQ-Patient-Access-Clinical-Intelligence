import { useCallback, useEffect, useRef, useState } from 'react'
import {
  DndContext,
  DragOverlay,
  KeyboardSensor,
  PointerSensor,
  closestCenter,
  useSensor,
  useSensors,
  type DragEndEvent,
  type DragStartEvent,
} from '@dnd-kit/core'
import {
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable'
import { restrictToVerticalAxis } from '@dnd-kit/modifiers'
import { Plus, WifiOff } from 'lucide-react'
import { Link } from 'react-router-dom'
import { QueueItem } from '../../components/queue/QueueItem'
import { ReorderReasonDialog } from '../../components/queue/ReorderReasonDialog'
import { queueApi, type QueueEntry, type QueueSummary } from '../../services/queueApi'

const POLL_INTERVAL_MS = 5_000

interface PendingReorder {
  entryId: string
  patientName: string
  newPosition: number
}

export const SameDayQueuePage = () => {
  const [entries, setEntries] = useState<QueueEntry[]>([])
  const [summary, setSummary] = useState<QueueSummary>({
    totalInQueue: 0,
    walkInCount: 0,
    arrivedCount: 0,
    avgWaitMinutes: 0,
  })
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [isOffline, setIsOffline] = useState(false)

  const [markingArrivedId, setMarkingArrivedId] = useState<string | null>(null)
  const [markArrivedError, setMarkArrivedError] = useState<string | null>(null)

  const [activeDragEntry, setActiveDragEntry] = useState<QueueEntry | null>(null)
  const [pendingReorder, setPendingReorder] = useState<PendingReorder | null>(null)
  const [isReordering, setIsReordering] = useState(false)
  const [reorderError, setReorderError] = useState<string | null>(null)

  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const loadQueue = useCallback(async (silent = false) => {
    if (!silent) setIsLoading(true)
    setLoadError(null)

    try {
      const { entries: fetched, summary: fetchedSummary } = await queueApi.getTodayQueue()
      setEntries(fetched)
      setSummary(fetchedSummary)
      setIsOffline(false)
    } catch {
      if (silent) {
        setIsOffline(true)
      } else {
        setLoadError('Unable to load the queue. Please try again.')
      }
    } finally {
      if (!silent) setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadQueue()
  }, [loadQueue])

  // AC-04: real-time polling every 5 seconds
  useEffect(() => {
    pollRef.current = setInterval(() => {
      void loadQueue(true)
    }, POLL_INTERVAL_MS)

    return () => {
      if (pollRef.current !== null) clearInterval(pollRef.current)
    }
  }, [loadQueue])

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  )

  const handleDragStart = (event: DragStartEvent) => {
    const active = entries.find((e) => e.id === event.active.id)
    if (active) setActiveDragEntry(active)
  }

  const handleDragEnd = (event: DragEndEvent) => {
    setActiveDragEntry(null)

    const { active, over } = event
    if (!over || active.id === over.id) return

    const movedEntry = entries.find((e) => e.id === active.id)
    const targetEntry = entries.find((e) => e.id === over.id)
    if (!movedEntry || !targetEntry) return

    // Optimistically reorder in UI
    setEntries((prev) => {
      const fromIndex = prev.findIndex((e) => e.id === active.id)
      const toIndex = prev.findIndex((e) => e.id === over.id)
      const updated = [...prev]
      const [moved] = updated.splice(fromIndex, 1)
      updated.splice(toIndex, 0, moved)
      return updated.map((e, i) => ({ ...e, position: i + 1 }))
    })

    setPendingReorder({
      entryId: movedEntry.id,
      patientName: movedEntry.patientName,
      newPosition: targetEntry.position,
    })
  }

  const handleReorderConfirm = async (reason: string) => {
    if (!pendingReorder) return

    setIsReordering(true)
    setReorderError(null)

    try {
      const { entries: updated, summary: updatedSummary } = await queueApi.reorderQueue({
        entryId: pendingReorder.entryId,
        newPosition: pendingReorder.newPosition,
        reason,
      })
      setEntries(updated)
      setSummary(updatedSummary)
      setPendingReorder(null)
    } catch {
      setReorderError('Unable to save queue reorder. Please try again.')
    } finally {
      setIsReordering(false)
    }
  }

  const handleReorderCancel = () => {
    // Rollback optimistic update by reloading from source
    setPendingReorder(null)
    void loadQueue(true)
  }

  const handleMarkArrived = async (entryId: string) => {
    setMarkingArrivedId(entryId)
    setMarkArrivedError(null)

    try {
      const { arrivalTimestamp } = await queueApi.markArrived({ entryId })
      setEntries((prev) =>
        prev.map((e) =>
          e.id === entryId
            ? { ...e, status: 'Arrived', arrivalTimestamp }
            : e,
        ),
      )
      setSummary((prev) => ({ ...prev, arrivedCount: prev.arrivedCount + 1 }))
    } catch {
      setMarkArrivedError('Unable to mark patient as arrived. Please try again.')
    } finally {
      setMarkingArrivedId(null)
    }
  }

  const entryIds = entries.map((e) => e.id)

  return (
    <main className="min-h-screen bg-background text-foreground" id="main-content">
      <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
        {/* Breadcrumb */}
        <nav
          className="mb-4 flex items-center gap-2 text-sm text-muted-foreground"
          aria-label="Breadcrumb"
        >
          <Link to="/dashboard/staff" className="transition-colors hover:text-foreground">
            Dashboard
          </Link>
          <span aria-hidden="true">/</span>
          <span aria-current="page" className="text-foreground">
            Same-day queue
          </span>
        </nav>

        {/* Page header */}
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-2xl font-semibold tracking-tight">Same-day queue</h1>
          <Link
            to="/booking/walk-in"
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
          >
            <Plus size={16} aria-hidden="true" />
            Walk-in
          </Link>
        </div>

        {/* Offline banner */}
        {isOffline && (
          <div
            role="status"
            aria-live="polite"
            className="mt-4 flex items-center gap-2 rounded-md border border-amber-500/40 bg-amber-500/10 px-4 py-3 text-sm text-amber-700"
          >
            <WifiOff size={16} aria-hidden="true" />
            Network issue detected — showing stale data. Reconnecting…
          </div>
        )}

        {/* Mark-arrived error */}
        {markArrivedError && (
          <div
            role="alert"
            className="mt-4 rounded-md border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive"
          >
            {markArrivedError}
          </div>
        )}

        {/* Loading state */}
        {isLoading && (
          <p className="mt-8 text-center text-sm text-muted-foreground" role="status">
            Loading queue…
          </p>
        )}

        {/* Load error */}
        {!isLoading && loadError && (
          <div
            role="alert"
            className="mt-8 rounded-md border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive"
          >
            {loadError}
          </div>
        )}

        {/* Queue content */}
        {!isLoading && !loadError && (
          <>
            {/* Summary bar — AC-01 */}
            <div
              role="status"
              aria-label="Queue summary"
              className="mt-6 flex flex-wrap gap-6 rounded-lg border border-border bg-muted/40 px-4 py-3"
            >
              <div className="text-sm">
                <strong className="font-semibold">{summary.totalInQueue}</strong> in queue
              </div>
              <div className="text-sm">
                <strong className="font-semibold">{summary.walkInCount}</strong> walk-ins
              </div>
              <div className="text-sm">
                <strong className="font-semibold">{summary.arrivedCount}</strong> arrived
              </div>
              <div className="text-sm">
                <strong className="font-semibold">~{summary.avgWaitMinutes} min</strong> avg wait
              </div>
            </div>

            {/* Empty state */}
            {entries.length === 0 ? (
              <div className="mt-12 text-center text-sm text-muted-foreground" role="status">
                No patients scheduled for today.
                <br />
                <Link
                  to="/booking/walk-in"
                  className="mt-2 inline-block underline underline-offset-2 hover:text-foreground"
                >
                  Create a walk-in booking
                </Link>
              </div>
            ) : (
              <>
                {/* Queue table with DnD — AC-01, AC-03, AC-04 */}
                <DndContext
                  sensors={sensors}
                  collisionDetection={closestCenter}
                  modifiers={[restrictToVerticalAxis]}
                  onDragStart={handleDragStart}
                  onDragEnd={handleDragEnd}
                >
                  <div className="mt-4 overflow-x-auto rounded-lg border border-border">
                    <table className="w-full text-sm" aria-label="Patient queue">
                      <thead>
                        <tr className="border-b border-border bg-muted/30 text-left text-xs font-medium uppercase tracking-wide text-muted-foreground">
                          <th className="py-3 pl-4 pr-2 w-16">#</th>
                          <th className="px-3 py-3">Patient</th>
                          <th className="px-3 py-3">Time</th>
                          <th className="px-3 py-3">Provider</th>
                          <th className="px-3 py-3">Status</th>
                          <th className="px-3 py-3">Risk</th>
                          <th className="px-3 py-3">Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        <SortableContext
                          items={entryIds}
                          strategy={verticalListSortingStrategy}
                        >
                          {entries.map((entry) => (
                            <QueueItem
                              key={entry.id}
                              entry={entry}
                              isMarkingArrived={markingArrivedId === entry.id}
                              onMarkArrived={handleMarkArrived}
                            />
                          ))}
                        </SortableContext>
                      </tbody>
                    </table>
                  </div>

                  <DragOverlay>
                    {activeDragEntry && (
                      <table className="w-full rounded-lg border border-primary/40 bg-card shadow-lg text-sm">
                        <tbody>
                          <QueueItem
                            entry={activeDragEntry}
                            isMarkingArrived={false}
                            onMarkArrived={() => undefined}
                          />
                        </tbody>
                      </table>
                    )}
                  </DragOverlay>
                </DndContext>

                <p className="mt-2 text-xs text-muted-foreground">
                  Drag rows to reorder queue position. Walk-in patients are flagged.
                </p>
              </>
            )}
          </>
        )}
      </div>

      {/* Reorder reason dialog — AC-03 */}
      <ReorderReasonDialog
        open={pendingReorder !== null}
        patientName={pendingReorder?.patientName ?? ''}
        isSubmitting={isReordering}
        errorMessage={reorderError}
        onConfirm={handleReorderConfirm}
        onCancel={handleReorderCancel}
      />
    </main>
  )
}
