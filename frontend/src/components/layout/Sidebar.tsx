import { LogOut } from 'lucide-react'
import { NavLink } from 'react-router-dom'
import { cn } from '../../lib/utils'
import { useAuth } from '../../hooks/useAuth'
import { getNavItemsForRole } from '../../config/navigation'
import type { UserRole } from '../../services/authApi'

interface SidebarProps {
  role: UserRole
}

// AC-01: Desktop sidebar — visible at md breakpoint and above
// AC-03: Nav items filtered to the authenticated role (Edge Case: hidden items never rendered)
// AC-04: NavLink applies aria-current="page" and active styles when path matches
export const Sidebar = ({ role }: SidebarProps) => {
  const { logout } = useAuth()
  const items = getNavItemsForRole(role)

  return (
    <aside className="hidden md:flex md:w-64 md:flex-col md:shrink-0">
      <nav
        className="flex flex-1 flex-col gap-1 overflow-y-auto border-r border-sidebar-border bg-sidebar px-3 py-4"
        aria-label="Main navigation"
      >
        <div className="mb-4 px-3">
          <span className="text-lg font-semibold tracking-tight text-sidebar-foreground font-display">
            PropelIQ
          </span>
        </div>

        <ul role="list" className="flex flex-1 flex-col gap-0.5">
          {items.map((item) => {
            const Icon = item.icon
            return (
              <li key={item.path}>
                <NavLink
                  to={item.path}
                  className={({ isActive }) =>
                    cn(
                      'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                      isActive
                        ? 'bg-primary-muted text-primary' // AC-04: active indicator
                        : 'text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground',
                    )
                  }
                  end={item.path.startsWith('/dashboard/')}
                >
                  {({ isActive }) => (
                    <>
                      <Icon
                        className={cn(
                          'h-4 w-4 shrink-0',
                          isActive ? 'text-primary' : 'text-foreground-secondary',
                        )}
                        aria-hidden="true"
                      />
                      <span>{item.label}</span>
                    </>
                  )}
                </NavLink>
              </li>
            )
          })}
        </ul>

        <div className="mt-auto border-t border-sidebar-border pt-3">
          <button
            type="button"
            onClick={logout}
            className="flex w-full items-center gap-3 rounded-md px-3 py-2 text-sm font-medium text-sidebar-foreground transition-colors hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
          >
            <LogOut className="h-4 w-4 shrink-0 text-foreground-secondary" aria-hidden="true" />
            <span>Log out</span>
          </button>
        </div>
      </nav>
    </aside>
  )
}
