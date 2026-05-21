import type { ReactNode } from 'react'
import { cn } from '../../lib/utils'

// AC-03: DataCategoryCard groups data by category with an empty-state fallback
interface DataCategoryCardProps {
  title: string
  children?: ReactNode
  isEmpty?: boolean
  className?: string
}

export const DataCategoryCard = ({
  title,
  children,
  isEmpty = false,
  className,
}: DataCategoryCardProps) => {
  return (
    <section
      className={cn('rounded-xl border border-border bg-card p-4', className)}
      aria-label={title}
    >
      <h3 className="mb-4 text-sm font-semibold tracking-tight text-foreground">{title}</h3>
      {isEmpty ? (
        // Edge case: No records available for this category
        <p className="text-sm text-muted-foreground">No records found</p>
      ) : (
        children
      )}
    </section>
  )
}
