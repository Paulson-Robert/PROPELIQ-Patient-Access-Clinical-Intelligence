import { useId, useRef, useState } from 'react'
import { Upload } from 'lucide-react'
import { cn } from '../../lib/utils'

// AC-02: accepted formats — PDF, DOCX, PNG, JPG, DICOM
const ACCEPTED_MIME_TYPES = new Set([
  'application/pdf',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'image/png',
  'image/jpeg',
  'application/dicom',
])

const ACCEPTED_EXTENSIONS = new Set(['.pdf', '.docx', '.png', '.jpg', '.jpeg', '.dcm'])

// AC-03: max 25 MB per file
const MAX_SIZE_BYTES = 25 * 1024 * 1024

const ACCEPTED_FORMATS_LABEL = 'PDF, DOCX, PNG, JPG, DICOM'

const INPUT_ACCEPT = [
  '.pdf',
  '.docx',
  '.png',
  '.jpg',
  '.jpeg',
  '.dcm',
  'application/pdf',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'image/png',
  'image/jpeg',
  'application/dicom',
].join(',')

export interface FileValidationError {
  fileName: string
  reason: string
}

const isFileTypeAccepted = (file: File): boolean => {
  if (ACCEPTED_MIME_TYPES.has(file.type)) return true
  const ext = `.${file.name.split('.').pop()?.toLowerCase() ?? ''}`
  return ACCEPTED_EXTENSIONS.has(ext)
}

const validateFiles = (
  files: File[],
): { accepted: File[]; errors: FileValidationError[] } => {
  const accepted: File[] = []
  const errors: FileValidationError[] = []

  for (const file of files) {
    if (!isFileTypeAccepted(file)) {
      errors.push({
        fileName: file.name,
        reason: `Unsupported format. Accepted: ${ACCEPTED_FORMATS_LABEL}.`,
      })
    } else if (file.size > MAX_SIZE_BYTES) {
      errors.push({
        fileName: file.name,
        reason: 'File exceeds the 25 MB size limit.',
      })
    } else {
      accepted.push(file)
    }
  }

  return { accepted, errors }
}

interface DropZoneProps {
  onFilesAccepted: (files: File[]) => void
  disabled?: boolean
}

export const DropZone = ({ onFilesAccepted, disabled = false }: DropZoneProps) => {
  const inputId = useId()
  const inputRef = useRef<HTMLInputElement>(null)
  const [isDragOver, setIsDragOver] = useState(false)
  const [validationErrors, setValidationErrors] = useState<FileValidationError[]>([])

  const handleFilesSelected = (files: FileList | File[]) => {
    const { accepted, errors } = validateFiles(Array.from(files))
    setValidationErrors(errors)
    if (accepted.length > 0) {
      onFilesAccepted(accepted)
    }
  }

  const handleDragOver = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    if (!disabled) setIsDragOver(true)
  }

  const handleDragLeave = (event: React.DragEvent<HTMLDivElement>) => {
    // Only clear drag state when leaving the drop zone itself, not child elements
    if (!event.currentTarget.contains(event.relatedTarget as Node)) {
      setIsDragOver(false)
    }
  }

  const handleDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    setIsDragOver(false)
    if (disabled) return
    handleFilesSelected(event.dataTransfer.files)
  }

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if ((event.key === 'Enter' || event.key === ' ') && !disabled) {
      event.preventDefault()
      inputRef.current?.click()
    }
  }

  const handleClick = () => {
    if (!disabled) inputRef.current?.click()
  }

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.files) {
      handleFilesSelected(event.target.files)
    }
    // Reset so the same file can be re-selected
    event.target.value = ''
  }

  return (
    <div className="flex flex-col gap-3" data-uxr="SCR-011">
      <div
        role="button"
        tabIndex={disabled ? -1 : 0}
        aria-label="Drag files here or click to browse"
        aria-disabled={disabled}
        onClick={handleClick}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onKeyDown={handleKeyDown}
        className={cn(
          'flex cursor-pointer flex-col items-center gap-4 rounded-xl border-2 border-dashed px-6 py-10 text-center transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
          isDragOver && !disabled
            ? 'border-primary bg-primary/5'
            : 'border-border bg-muted/40 hover:border-primary hover:bg-primary/5',
          disabled && 'cursor-not-allowed opacity-60',
        )}
      >
        <Upload className="h-10 w-10 text-muted-foreground" aria-hidden="true" />
        <div>
          <p className="text-sm font-medium text-foreground">
            Drag files here or click to browse
          </p>
          <p className="mt-1 text-xs text-muted-foreground">
            Supported: {ACCEPTED_FORMATS_LABEL}
          </p>
        </div>
      </div>

      <p className="text-center text-xs text-muted-foreground">
        Maximum file size: 25 MB per file
      </p>

      {/* Hidden file input — AC-01 click fallback */}
      <input
        ref={inputRef}
        id={inputId}
        type="file"
        multiple
        accept={INPUT_ACCEPT}
        className="sr-only"
        onChange={handleInputChange}
        disabled={disabled}
        aria-hidden="true"
        tabIndex={-1}
      />

      {/* AC-02 / Edge Case: invalid file type — clear error message */}
      {validationErrors.length > 0 ? (
        <ul
          role="alert"
          aria-label="File validation errors"
          className="flex flex-col gap-1.5"
        >
          {validationErrors.map((error) => (
            <li
              key={error.fileName}
              className="rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm text-destructive"
            >
              <span className="font-medium">{error.fileName}:</span> {error.reason}
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  )
}
