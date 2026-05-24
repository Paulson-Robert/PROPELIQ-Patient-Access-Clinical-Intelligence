import { useEffect, useState } from 'react'
import { ArrowLeft, Clock, FileText, Loader2, Plus, Trash2 } from 'lucide-react'
import { Link } from 'react-router-dom'
import { cn } from '../../lib/utils'
import {
  DeleteConfirmDialog,
  type DeleteMode,
} from '../../components/documents/DeleteConfirmDialog'
import { bookingApi, type PatientDocumentRecord } from '../../services/bookingApi'
import { useAuth } from '../../hooks/useAuth'

// --- Types ---

type DocumentStatus = 'uploading' | 'scanning' | 'processing' | 'completed' | 'failed'

interface ClinicalDocument {
  id: string
  fileName: string
  format: string
  uploadDate: string
  sizeBytes: number
  status: DocumentStatus
  retentionPolicy: 'indefinite'
}

// --- Helpers ---

const formatBytes = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

const formatDate = (iso: string): string => {
  const d = new Date(iso)
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

const mapProcessingStatus = (status: string): DocumentStatus => {
  const lower = status.toLowerCase()
  if (lower === 'completed') return 'completed'
  if (lower === 'failed') return 'failed'
  if (lower === 'scanning') return 'scanning'
  if (lower === 'uploading') return 'uploading'
  return 'processing'
}

const mapApiDocToLocal = (doc: PatientDocumentRecord): ClinicalDocument => ({
  id: doc.id,
  fileName: doc.fileName,
  format: doc.fileFormat.toUpperCase(),
  uploadDate: formatDate(doc.uploadedAt),
  sizeBytes: doc.fileSizeBytes,
  status: mapProcessingStatus(doc.processingStatus),
  retentionPolicy: 'indefinite',
})

const getErrorMessage = (error: unknown): string => {
  if (error instanceof Error && error.message.trim().length > 0) {
    return error.message
  }

  return 'Document could not be deleted. Please try again.'
}

const STATUS_CONFIG: Record<DocumentStatus, { label: string; className: string }> = {
  uploading: { label: 'Uploading', className: 'bg-blue-500/10 text-blue-700' },
  scanning: { label: 'Scanning', className: 'bg-amber-500/10 text-amber-700' },
  processing: { label: 'Processing', className: 'bg-amber-500/10 text-amber-700' },
  completed: { label: 'Completed', className: 'bg-emerald-500/10 text-emerald-700' },
  failed: { label: 'Failed', className: 'bg-destructive/10 text-destructive' },
}

// --- Component ---

export const DocumentListPage = () => {
  const { user } = useAuth()
  const [documents, setDocuments] = useState<ClinicalDocument[]>([])
  const [loading, setLoading] = useState(true)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<DeleteMode>('single')
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null)
  const [deleting, setDeleting] = useState(false)
  const [deleteError, setDeleteError] = useState<string | null>(null)

  const pendingDocument = documents.find((d) => d.id === pendingDeleteId)
  const dashboardPath = user?.role === 'staff' ? '/dashboard/staff' : '/dashboard/patient'

  useEffect(() => {
    let cancelled = false

    const fetchDocuments = async () => {
      try {
        setLoading(true)
        const records = await bookingApi.getMyDocuments()
        if (!cancelled) {
          setDocuments(records.map(mapApiDocToLocal))
        }
      } catch {
        if (!cancelled) {
          setDocuments([])
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void fetchDocuments()
    return () => { cancelled = true }
  }, [])

  // AC-02: open delete dialog for a single document
  const openDeleteSingle = (id: string) => {
    setDeleteError(null)
    setPendingDeleteId(id)
    setDialogMode('single')
    setDialogOpen(true)
  }

  // AC-03: open delete dialog for all documents
  const openDeleteAll = () => {
    setDeleteError(null)
    setPendingDeleteId(null)
    setDialogMode('all')
    setDialogOpen(true)
  }

  const handleConfirm = async () => {
    setDeleteError(null)
    setDeleting(true)

    try {
      if (dialogMode === 'all') {
        await bookingApi.deleteAllDocuments()
        setDocuments([])
      } else if (pendingDeleteId !== null) {
        await bookingApi.deleteDocument(pendingDeleteId)
        setDocuments((prev) => prev.filter((d) => d.id !== pendingDeleteId))
      }

      setDialogOpen(false)
      setPendingDeleteId(null)
    } catch (error) {
      setDeleteError(getErrorMessage(error))
    } finally {
      setDeleting(false)
    }
  }

  const handleCancel = () => {
    if (deleting) return

    setDialogOpen(false)
    setPendingDeleteId(null)
    setDeleteError(null)
  }

  return (
    <>
      <main className="min-h-screen bg-background text-foreground" id="main-content">
        <a
          href="#documents-table"
          className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
        >
          Skip to documents table
        </a>

        <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
          {/* Page header */}
          <header className="mb-6 flex flex-wrap items-center justify-between gap-3">
            <div className="flex items-center gap-3">
              <Link
                to={dashboardPath}
                className="inline-flex items-center justify-center rounded-md p-1.5 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                aria-label="Back to dashboard"
              >
                <ArrowLeft className="h-5 w-5" aria-hidden="true" />
              </Link>
              <h1 className="text-xl font-semibold tracking-tight">Documents</h1>
            </div>

            <div className="flex items-center gap-2">
              {/* AC-03: delete-all action — only visible when documents exist */}
              {documents.length > 0 && (
                <button
                  type="button"
                  onClick={openDeleteAll}
                  disabled={deleting}
                  className="inline-flex items-center gap-2 rounded-md border border-destructive/40 bg-card px-3 py-2 text-sm font-medium text-destructive transition hover:bg-destructive/5 focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
                >
                  <Trash2 className="h-4 w-4" aria-hidden="true" />
                  Delete all
                </button>
              )}

              <Link
                to="/documents/upload"
                className="inline-flex items-center gap-2 rounded-md bg-primary px-3 py-2 text-sm font-medium text-primary-foreground transition hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <Plus className="h-4 w-4" aria-hidden="true" />
                Upload
              </Link>
            </div>
          </header>

          {/* Empty state — shown after all documents are deleted (edge case: last document) */}
          {loading ? (
            <div className="flex items-center justify-center py-20">
              <Loader2 className="h-8 w-8 animate-spin text-primary" />
              <span className="ml-3 text-sm text-muted-foreground">Loading documents…</span>
            </div>
          ) : documents.length === 0 ? (
            <section
              className="flex flex-col items-center rounded-xl border border-dashed border-border bg-muted/30 py-16 text-center"
              aria-label="No documents"
            >
              <FileText className="mb-3 h-10 w-10 text-muted-foreground" aria-hidden="true" />
              <p className="text-base font-medium text-foreground">No documents uploaded yet</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Upload clinical documents to get started
              </p>
              <Link
                to="/documents/upload"
                className="mt-4 inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <Plus className="h-4 w-4" aria-hidden="true" />
                Upload document
              </Link>
            </section>
          ) : (
            <section id="documents-table" aria-label="Uploaded documents">
              <div className="overflow-x-auto rounded-xl border border-border">
                <table className="w-full text-sm" aria-label="Uploaded documents">
                  <thead>
                    <tr className="border-b border-border bg-muted/40 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                      <th scope="col" className="px-4 py-3">
                        File name
                      </th>
                      <th scope="col" className="px-4 py-3">
                        Format
                      </th>
                      <th scope="col" className="px-4 py-3">
                        Upload date
                      </th>
                      <th scope="col" className="px-4 py-3 text-right">
                        Size
                      </th>
                      <th scope="col" className="px-4 py-3">
                        Status
                      </th>
                      {/* AC-01: retention period column */}
                      <th scope="col" className="px-4 py-3">
                        Retention
                      </th>
                      <th scope="col" className="px-4 py-3 text-right">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border bg-card">
                    {documents.map((doc) => {
                      const statusConfig = STATUS_CONFIG[doc.status]
                      return (
                        <tr
                          key={doc.id}
                          className="transition-colors hover:bg-muted/30"
                        >
                          <td className="px-4 py-3 font-medium text-foreground">
                            {doc.fileName}
                          </td>
                          <td className="px-4 py-3">
                            <span className="inline-flex items-center rounded-full border border-border bg-background px-2 py-0.5 text-xs font-medium text-foreground">
                              {doc.format}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-muted-foreground">{doc.uploadDate}</td>
                          <td className="px-4 py-3 text-right font-mono text-muted-foreground">
                            {formatBytes(doc.sizeBytes)}
                          </td>
                          <td className="px-4 py-3">
                            <span
                              className={cn(
                                'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium',
                                statusConfig.className,
                              )}
                            >
                              {statusConfig.label}
                            </span>
                          </td>
                          {/* AC-01: per-document retention period */}
                          <td className="px-4 py-3">
                            <span className="inline-flex items-center gap-1 text-xs text-muted-foreground">
                              <Clock className="h-3 w-3" aria-hidden="true" />
                              Retained indefinitely
                            </span>
                          </td>
                          {/* AC-02: per-document delete action */}
                          <td className="px-4 py-3 text-right">
                            <button
                              type="button"
                              onClick={() => openDeleteSingle(doc.id)}
                              disabled={deleting}
                              className="inline-flex items-center gap-1.5 rounded-md px-2.5 py-1.5 text-xs font-medium text-destructive transition hover:bg-destructive/10 focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
                              aria-label={`Delete ${doc.fileName}`}
                            >
                              <Trash2 className="h-3.5 w-3.5" aria-hidden="true" />
                              Delete
                            </button>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
              <p className="mt-2 text-xs text-muted-foreground">
                Pipeline: 1 Uploading → 2 Scanning → 3 Processing → 4 Completed
              </p>
            </section>
          )}
        </div>
      </main>

      {/* AC-02 / AC-03: deletion confirmation dialog with consequences warning */}
      <DeleteConfirmDialog
        open={dialogOpen}
        mode={dialogMode}
        documentName={pendingDocument?.fileName}
        busy={deleting}
        errorMessage={deleteError}
        onConfirm={handleConfirm}
        onCancel={handleCancel}
      />
    </>
  )
}
