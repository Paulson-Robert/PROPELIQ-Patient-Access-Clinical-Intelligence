import type { ReactNode } from 'react'
import { useAuth } from '../../hooks/useAuth'
import { Sidebar } from './Sidebar'
import { BottomNav } from './BottomNav'
import type { UserRole } from '../../services/authApi'

interface AppLayoutProps {
  children: ReactNode
  childrenOwnMain?: boolean
}

// AC-01: Layout shell — header, sidebar (desktop), content area
// AC-02: BottomNav rendered for mobile via Tailwind responsive classes
// AC-03/Edge Case: role passed to Sidebar/BottomNav; unauthorised items never reach nav
export const AppLayout = ({ children, childrenOwnMain = false }: AppLayoutProps) => {
  const { user } = useAuth()

  // Fallback gracefully if role is unavailable; components guard their own render
  const role = (user?.role ?? 'patient') as UserRole

  return (
    <div className="flex min-h-screen bg-background">
      {/* AC-01: Desktop sidebar — hidden on mobile via md:flex */}
      <Sidebar role={role} />

      <div className="flex flex-1 flex-col overflow-hidden">
        {/* AC-01: Application header */}
        <header className="sticky top-0 z-30 flex h-14 items-center justify-between border-b border-border bg-background px-4 md:px-6">
          {/* Brand name shown only on mobile (sidebar shows it on desktop) */}
          <span className="text-base font-semibold tracking-tight text-foreground font-display md:hidden">
            HealthAccess
          </span>
        </header>

        {/* AC-01: Main content area */}
        {childrenOwnMain ? (
          <div className="flex-1 overflow-y-auto pb-20 md:pb-0">
            {children}
          </div>
        ) : (
          <main
            id="main-content"
            className="flex-1 overflow-y-auto px-4 py-6 pb-20 md:px-6 md:pb-6"
            tabIndex={-1}
          >
            {children}
          </main>
        )}
      </div>

      {/* AC-02: Mobile bottom nav — hidden on desktop via md:hidden in BottomNav */}
      <BottomNav role={role} />
    </div>
  )
}
