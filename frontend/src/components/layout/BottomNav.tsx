import { NavLink } from 'react-router-dom'
import { cn } from '../../lib/utils'
import { getNavItemsForRole } from '../../config/navigation'
import type { UserRole } from '../../services/authApi'

interface BottomNavProps {
  role: UserRole
}

// AC-02: Mobile bottom navigation — visible below md breakpoint
// AC-03: Nav items filtered to the authenticated role (Edge Case: hidden items never rendered)
// AC-04: NavLink applies active styles when path matches
export const BottomNav = ({ role }: BottomNavProps) => {
  const items = getNavItemsForRole(role)

  return (
    <nav
      className="fixed bottom-0 left-0 right-0 z-40 flex items-center justify-around border-t border-border bg-background pb-safe md:hidden"
      aria-label="Mobile navigation"
    >
      {items.map((item) => {
        const Icon = item.icon
        return (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              cn(
                'flex flex-1 flex-col items-center gap-0.5 px-2 py-2 text-xs font-medium transition-colors',
                isActive
                  ? 'text-primary' // AC-04: active indicator
                  : 'text-muted-foreground',
              )
            }
            end={item.path === '/dashboard'}
          >
            {({ isActive }) => (
              <>
                <Icon
                  className={cn(
                    'h-5 w-5 shrink-0',
                    isActive ? 'text-primary' : 'text-muted-foreground',
                  )}
                  aria-hidden="true"
                />
                <span>{item.label}</span>
              </>
            )}
          </NavLink>
        )
      })}
    </nav>
  )
}
