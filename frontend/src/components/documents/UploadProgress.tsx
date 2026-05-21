import { CheckCircle, FileText, XCircle } from 'lucide-react'
import { cn } from '../../lib/utils'

export interface UploadItem {
  id: string
  file: File
  status: 'uploading' | 'done' | 'error'
  /** Progress 0–100. Relevant only when status is 'uploading'. */
  progress: number
  errorMessage?: string
}

const formatBytes = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

interface UploadItemRowProps {
  item: UploadItem
}

const UploadItemRow = ({ item }: UploadItemRowProps) => {
  const isUploading = item.status === 'uploading'
  const isDone = item.status === 'done'
  const isError = item.status === 'error'

  const subtext = isUploading
    ? 'Uploading...'
    : isDone
      ? 'Uploaded'
      : (item.errorMessage ?? 'Upload failed')

  return (
    <li
      className="flex items-center gap-3 rounded-lg border border-border bg-muted/40 p-3"
      aria-label={`${item.file.name}: ${subtext}`}
    >
      <FileText
        className={cn(
          'h-5 w-5 shrink-0',
          isDone ? 'text-emerald-600' : 'text-muted-foreground',
        )}
        aria-hidden="true"
      />

      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium text-foreground">{item.file.name}</p>
        <p className="mt-0.5 text-xs text-muted-foreground">
          {formatBytes(item.file.size)} · {subtext}
        </p>

        {/* AC-04: per-file progress bar */}
        {isUploading ? (
          <div
            className="mt-1.5 h-1.5 w-full overflow-hidden rounded-full bg-border"
            role="progressbar"
            aria-valuenow={item.progress}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-label={`Upload progress for ${item.file.name}`}
          >
            <div
              className="h-full rounded-full bg-primary transition-all duration-300 ease-out"
              style={{ width: `${item.progress}%` }}
            />
          </div>
        ) : null}

        {isError ? (
          <p className="mt-1 text-xs text-destructive" role="alert">
            {item.errorMessage ?? 'Upload failed. Please try again.'}
          </p>
        ) : null}
      </div>

      {/* Trailing indicator */}
      {isUploading ? (
        <span
          className="shrink-0 text-xs font-medium tabular-nums text-primary"
          aria-hidden="true"
        >
          {item.progress}%
        </span>
      ) : null}

      {isDone ? (
        <span className="inline-flex shrink-0 items-center gap-1 rounded-full bg-emerald-500/10 px-2 py-0.5 text-xs font-medium text-emerald-700">
          <CheckCircle className="h-3 w-3" aria-hidden="true" />
          Done
        </span>
      ) : null}

      {isError ? (
        <XCircle className="h-5 w-5 shrink-0 text-destructive" aria-hidden="true" />
      ) : null}
    </li>
  )
}

interface UploadProgressProps {
  items: UploadItem[]
}

export const UploadProgress = ({ items }: UploadProgressProps) => {
  if (items.length === 0) return null

  return (
    <section aria-labelledby="upload-queue-heading" data-uxr="SCR-011">
      <h2 id="upload-queue-heading" className="mb-3 text-sm font-semibold text-foreground">
        Upload queue
      </h2>
      <ul
        className="flex flex-col gap-2"
        aria-label="Upload queue items"
        aria-live="polite"
        aria-relevant="additions"
      >
        {items.map((item) => (
          <UploadItemRow key={item.id} item={item} />
        ))}
      </ul>
    </section>
  )
}
