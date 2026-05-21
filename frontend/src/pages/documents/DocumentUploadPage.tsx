import { useCallback, useEffect, useRef, useState } from 'react'
import { ArrowLeft, FolderOpen } from 'lucide-react'
import { Link } from 'react-router-dom'
import { DropZone } from '../../components/documents/DropZone'
import { UploadProgress, type UploadItem } from '../../components/documents/UploadProgress'

// Simulated upload: increments 5% every 100ms (~2 s total per file).
// Inferred decision — no backend upload API endpoint is defined in TASK_001 scope.
const PROGRESS_STEP = 5
const PROGRESS_INTERVAL_MS = 100

const generateId = (): string =>
  `upload-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`

export const DocumentUploadPage = () => {
  const [items, setItems] = useState<UploadItem[]>([])
  const intervalsRef = useRef<Map<string, number>>(new Map())

  // Clear all pending intervals on unmount to prevent memory leaks
  useEffect(() => {
    const intervals = intervalsRef.current
    return () => {
      for (const id of intervals.values()) {
        window.clearInterval(id)
      }
    }
  }, [])

  const simulateUpload = useCallback((id: string) => {
    const interval = window.setInterval(() => {
      setItems((prev) =>
        prev.map((item) => {
          if (item.id !== id) return item

          const nextProgress = Math.min(item.progress + PROGRESS_STEP, 100)

          if (nextProgress === 100) {
            window.clearInterval(intervalsRef.current.get(id))
            intervalsRef.current.delete(id)
            return { ...item, progress: 100, status: 'done' }
          }

          return { ...item, progress: nextProgress }
        }),
      )
    }, PROGRESS_INTERVAL_MS)

    intervalsRef.current.set(id, interval)
  }, [])

  const handleFilesAccepted = useCallback(
    (files: File[]) => {
      const newItems: UploadItem[] = files.map((file) => ({
        id: generateId(),
        file,
        status: 'uploading',
        progress: 0,
      }))

      setItems((prev) => [...prev, ...newItems])

      for (const item of newItems) {
        simulateUpload(item.id)
      }
    },
    [simulateUpload],
  )

  return (
    <main className="min-h-screen bg-background text-foreground" id="main-content">
      <a
        href="#upload-zone"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
      >
        Skip to upload zone
      </a>

      <div className="mx-auto max-w-[720px] px-4 py-8 sm:px-6">
        <header className="mb-6 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <Link
              to="/dashboard/patient"
              className="inline-flex items-center justify-center rounded-md p-1.5 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              aria-label="Back to dashboard"
            >
              <ArrowLeft className="h-5 w-5" aria-hidden="true" />
            </Link>
            <h1 className="text-xl font-semibold tracking-tight">Upload documents</h1>
          </div>

          <Link
            to="/documents"
            className="inline-flex items-center gap-2 rounded-md border border-border bg-card px-3 py-2 text-sm font-medium text-foreground transition hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring"
          >
            <FolderOpen className="h-4 w-4" aria-hidden="true" />
            View all documents
          </Link>
        </header>

        <section id="upload-zone" aria-label="Document upload area">
          <DropZone onFilesAccepted={handleFilesAccepted} />
        </section>

        {items.length > 0 ? (
          <div className="mt-8">
            <UploadProgress items={items} />
          </div>
        ) : null}
      </div>
    </main>
  )
}
