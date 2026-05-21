import { useState } from 'react'
import { ArrowLeft, Clock, FileText, Plus, Trash2 } from 'lucide-react'
import { Link } from 'react-router-dom'
import { cn } from '../../lib/utils'
import {
  DeleteConfirmDialog,
  type DeleteMode,
} from '../../components/documents/DeleteConfirmDialog'

// --- Types ---

type DocumentStatus = 'uploading' | 'scanning' | 'processing' | 'completed' | 'failed'

interface ClinicalDocument {
  id: string
  fileName: string
  format: 'PDF' | 'DOCX' | 'PNG' | 'JPG' | 'DICOM'
  uploadDate: string
  sizeBytes: number
  status: DocumentStatus
  /** AC-01: Retention policy. Patient-uploaded documents are retained indefinitely. */
  retentionPolicy: 'indefinite'
}

// --- Helpers ---

const formatBytes = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

const STATUS_CONFIG: Record<DocumentStatus, { label: string; className: string }> = {
  uploading: { label: 'Uploading', className: 'bg-blue-500/10 text-blue-700' },
  scanning: { label: 'Scanning', className: 'bg-amber-500/10 text-amber-700' },
  processing: { label: 'Processing', className: 'bg-amber-500/10 text-amber-700' },
  completed: { label: 'Completed', className: 'bg-emerald-500/10 text-emerald-700' },
  failed: { label: 'Failed', className: 'bg-destructive/10 text-destructive' },
}

// --- Mock data ---
// Inferred decision: No document list API endpoint is in scope for this task (frontend only).
// Static mock data is used to demonstrate the UI, matching the wireframe-SCR-012 samples.
const MOCK_DOCUMENTS: ClinicalDocument[] = [
  {
    id: 'doc-001',
    fileName: 'CBC-Results-2025-01.pdf',
    format: 'PDF',
    uploadDate: 'Jan 25, 2025',
    sizeBytes: 2.4 * 1024 * 1024,
    status: 'completed',
    retentionPolicy: 'indefinite',
  },
  {
    id: 'doc-002',
    fileName: 'Chest-Xray-Anterior.dicom',
    format: 'DICOM',
    uploadDate: 'Jan 25, 2025',
    sizeBytes: 18.7 * 1024 * 1024,
    status: 'processing',
    retentionPolicy: 'indefinite',
  },
  {
    id: 'doc-003',
    fileName: 'MRI-Knee-Left.dicom',
    format: 'DICOM',
    uploadDate: 'Jan 23, 2025',
    sizeBytes: 45.2 * 1024 * 1024,
    status: 'failed',
    retentionPolicy: 'indefinite',
  },
]

// --- Component ---

export const DocumentListPage = () => {
  const [documents, setDocuments] = useState<ClinicalDocument[]>(MOCK_DOCUMENTS)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<DeleteMode>('single')
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null)

  const pendingDocument = documents.find((d) => d.id === pendingDeleteId)

  // AC-02: open delete dialog for a single document
  const openDeleteSingle = (id: string) => {
    setPendingDeleteId(id)
    setDialogMode('single')
    setDialogOpen(true)
  }

  // AC-03: open delete dialog for all documents
  const openDeleteAll = () => {
    setPendingDeleteId(null)
    setDialogMode('all')
    setDialogOpen(true)
  }

  const handleConfirm = () => {
    if (dialogMode === 'all') {
      setDocuments([])
    } else if (pendingDeleteId !== null) {
      setDocuments((prev) => prev.filter((d) => d.id !== pendingDeleteId))
    }
    setDialogOpen(false)
    setPendingDeleteId(null)
  }

  const handleCancel = () => {
    setDialogOpen(false)
    setPendingDeleteId(null)
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
                to="/dashboard/patient"
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
                  className="inline-flex items-center gap-2 rounded-md border border-destructive/40 bg-card px-3 py-2 text-sm font-medium text-destructive transition hover:bg-destructive/5 focus:outline-none focus:ring-2 focus:ring-ring"
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
          {documents.length === 0 ? (
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
                              className="inline-flex items-center gap-1.5 rounded-md px-2.5 py-1.5 text-xs font-medium text-destructive transition hover:bg-destructive/10 focus:outline-none focus:ring-2 focus:ring-ring"
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
        onConfirm={handleConfirm}
        onCancel={handleCancel}
      />
    </>
  )
}
