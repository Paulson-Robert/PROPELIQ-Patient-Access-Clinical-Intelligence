import type { ReactNode } from 'react'
import { cn } from '../../lib/utils'

export interface ColumnDef<T> {
  key: keyof T
  header: string
  className?: string
  render?: (value: T[keyof T], row: T) => ReactNode
}

interface ResponsiveTableProps<T> {
  columns: ColumnDef<T>[]
  rows: T[]
  rowKey: keyof T
  caption?: string
  className?: string
}

export function ResponsiveTable<T>({
  columns,
  rows,
  rowKey,
  caption,
  className,
}: ResponsiveTableProps<T>) {
  return (
    <div className={cn('w-full', className)}>
      {/* Desktop: standard table (md and above) */}
      <div className="hidden md:block overflow-x-auto rounded-xl border border-border">
        <table className="w-full text-sm">
          {caption && (
            <caption className="sr-only">{caption}</caption>
          )}
          <thead>
            <tr className="border-b border-border bg-background-secondary">
              {columns.map((col) => (
                <th
                  key={String(col.key)}
                  scope="col"
                  className={cn(
                    'px-4 py-3 text-left font-medium text-foreground-secondary',
                    col.className,
                  )}
                >
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr
                key={String(row[rowKey])}
                className="border-b border-border last:border-0 hover:bg-background-secondary/50 transition-colors"
              >
                {columns.map((col) => (
                  <td
                    key={String(col.key)}
                    className={cn('px-4 py-3 text-foreground', col.className)}
                  >
                    {col.render
                      ? col.render(row[col.key], row)
                      : String(row[col.key] ?? '')}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile: card layout (below md) */}
      <ul className="flex flex-col gap-3 md:hidden" role="list" aria-label={caption}>
        {rows.map((row) => (
          <li
            key={String(row[rowKey])}
            className="rounded-xl border border-border bg-card p-4 shadow-sm"
          >
            {columns.map((col) => (
              <div
                key={String(col.key)}
                className="flex items-start justify-between gap-2 py-1.5 border-b border-border/50 last:border-0"
              >
                <span className="text-xs font-medium text-foreground-secondary min-w-0 shrink-0">
                  {col.header}
                </span>
                {/* min-h-[44px] satisfies AC-03 touch target requirement */}
                <span className="text-sm text-foreground text-right min-h-[44px] flex items-center justify-end">
                  {col.render
                    ? col.render(row[col.key], row)
                    : String(row[col.key] ?? '')}
                </span>
              </div>
            ))}
          </li>
        ))}
      </ul>
    </div>
  )
}
