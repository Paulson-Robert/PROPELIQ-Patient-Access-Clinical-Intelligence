import { useCallback, useEffect, useRef, useState } from 'react'
import { ArrowLeft, FolderOpen } from 'lucide-react'
import { Link, useSearchParams } from 'react-router-dom'
import { DropZone } from '../../components/documents/DropZone'
import { UploadProgress, type UploadItem } from '../../components/documents/UploadProgress'
import { PatientSearchInput } from '../../components/walkin/PatientSearchInput'
import { bookingApi, type PatientSearchResult } from '../../services/bookingApi'
import { useAuth } from '../../hooks/useAuth'
import {
  documentPathForPatient,
  patientFromSearchParams,
  patientToSearchParams,
} from './documentPatientContext'

const generateId = (): string =>
  `upload-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`

const getErrorMessage = (error: unknown): string => {
  if (error instanceof Error && error.message.trim().length > 0) {
    return error.message
  }

  return 'Upload failed. Please try again.'
}

export const DocumentUploadPage = () => {
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const queryPatient = patientFromSearchParams(searchParams)
  const isStaff = user?.role === 'staff'
  const [items, setItems] = useState<UploadItem[]>([])
  const [selectedPatient, setSelectedPatient] = useState<PatientSearchResult | null>(queryPatient)
  const mountedRef = useRef(true)
  const startTimeoutsRef = useRef<number[]>([])
  const dashboardPath = user?.role === 'staff' ? '/dashboard/staff' : '/dashboard/patient'
  const documentsPath = isStaff ? documentPathForPatient('/documents', selectedPatient) : '/documents'
  const canUpload = !isStaff || selectedPatient !== null

  useEffect(() => {
    mountedRef.current = true
    const startTimeouts = startTimeoutsRef.current
    return () => {
      mountedRef.current = false
      for (const timeoutId of startTimeouts) {
        window.clearTimeout(timeoutId)
      }
    }
  }, [])

  useEffect(() => {
    if (!isStaff) return
    setSelectedPatient(queryPatient)
  }, [isStaff, queryPatient?.email, queryPatient?.id, queryPatient?.name, queryPatient?.phone])

  useEffect(() => {
    setItems([])
  }, [selectedPatient?.id])

  const updateProgress = useCallback((id: string, progress: number) => {
    if (!mountedRef.current) return

    setItems((prev) =>
      prev.map((current) =>
        current.id === id && current.status === 'uploading'
          ? { ...current, progress: Math.max(current.progress, Math.min(progress, 95)) }
          : current,
      ),
    )
  }, [])

  const uploadFile = useCallback(
    async (id: string, file: File, patientUserId?: string) => {
      try {
        await bookingApi.uploadDocument(
          file,
          (progress) => updateProgress(id, progress),
          patientUserId,
        )

        if (!mountedRef.current) return

        setItems((prev) =>
          prev.map((current) =>
            current.id === id
              ? { ...current, progress: 100, status: 'done' }
              : current,
          ),
        )
      } catch (error) {
        if (!mountedRef.current) return

        setItems((prev) =>
          prev.map((current) =>
            current.id === id
              ? {
                  ...current,
                  status: 'error',
                  errorMessage: getErrorMessage(error),
                }
              : current,
          ),
        )
      }
    },
    [updateProgress],
  )

  const handleFilesAccepted = useCallback(
    (files: File[]) => {
      const patientUserId = isStaff ? selectedPatient?.id : undefined
      if (isStaff && !patientUserId) return

      const newItems: UploadItem[] = files.map((file) => ({
        id: generateId(),
        file,
        status: 'uploading',
        progress: 5,
      }))

      setItems((prev) => [...prev, ...newItems])

      for (const item of newItems) {
        const timeoutId = window.setTimeout(() => {
          startTimeoutsRef.current = startTimeoutsRef.current.filter((id) => id !== timeoutId)
          void uploadFile(item.id, item.file, patientUserId)
        }, 0)
        startTimeoutsRef.current.push(timeoutId)
      }
    },
    [isStaff, selectedPatient?.id, uploadFile],
  )

  const handleSelectPatient = useCallback(
    (patient: PatientSearchResult) => {
      setSelectedPatient(patient)
      setSearchParams(patientToSearchParams(patient), { replace: true })
    },
    [setSearchParams],
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
              to={dashboardPath}
              className="inline-flex items-center justify-center rounded-md p-1.5 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              aria-label="Back to dashboard"
            >
              <ArrowLeft className="h-5 w-5" aria-hidden="true" />
            </Link>
            <h1 className="text-xl font-semibold tracking-tight">Upload documents</h1>
          </div>

          <Link
            to={documentsPath}
            className="inline-flex items-center gap-2 rounded-md border border-border bg-card px-3 py-2 text-sm font-medium text-foreground transition hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring"
          >
            <FolderOpen className="h-4 w-4" aria-hidden="true" />
            View all documents
          </Link>
        </header>

        {isStaff ? (
          <div className="mb-6">
            <PatientSearchInput
              selectedPatient={selectedPatient}
              onSelectPatient={handleSelectPatient}
              title="Select patient"
              description="Search for the patient whose documents you want to upload"
              emptyMessage="No matching patients found."
            />
            {selectedPatient ? (
              <p className="mt-3 rounded-md border border-border bg-muted/40 px-3 py-2 text-sm text-muted-foreground">
                Uploading for <span className="font-medium text-foreground">{selectedPatient.name}</span>
              </p>
            ) : null}
          </div>
        ) : null}

        <section id="upload-zone" aria-label="Document upload area">
          <DropZone onFilesAccepted={handleFilesAccepted} disabled={!canUpload} />
          {isStaff && !selectedPatient ? (
            <p className="mt-3 text-center text-sm text-muted-foreground" role="status">
              Select a patient before uploading documents.
            </p>
          ) : null}
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
