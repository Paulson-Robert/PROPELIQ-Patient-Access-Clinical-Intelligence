import { useNavigate } from 'react-router-dom'
import { BarChart3, FileText, LayoutDashboard, LogOut, Users } from 'lucide-react'
// import { cn } from '../../lib/utils'
import { useAuth } from '../../hooks/useAuth'

export type AdminPage = 'dashboard' | 'users' | 'audit-log' | 'metrics'

interface AdminSidebarProps {
  activePage: AdminPage
}

const NAV_ITEMS: { id: AdminPage; label: string; href: string; icon: React.ReactNode }[] = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    href: '/dashboard/admin',
    icon: <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden="true" />,
  },
  {
    id: 'users',
    label: 'User management',
    href: '/admin/users',
    icon: <Users className="h-4 w-4 shrink-0" aria-hidden="true" />,
  },
  {
    id: 'audit-log',
    label: 'Audit log',
    href: '/admin/audit-log',
    icon: <FileText className="h-4 w-4 shrink-0" aria-hidden="true" />,
  },
  {
    id: 'metrics',
    label: 'Platform metrics',
    href: '/admin/metrics',
    icon: <BarChart3 className="h-4 w-4 shrink-0" aria-hidden="true" />,
  },
]

export const AdminSidebar = ({ activePage }: AdminSidebarProps) => {
  const navigate = useNavigate()
  const { logout } = useAuth()

  return (
    <nav
      className="hidden w-56 shrink-0 border-r border-border bg-card p-4 lg:flex lg:flex-col"
      aria-label="Admin navigation"
    >
      <div className="mb-6 flex items-center gap-2 text-sm font-semibold text-foreground">
        <LayoutDashboard className="h-5 w-5 text-primary" aria-hidden="true" />
        PropelIQ
      </div>

      <ul className="flex flex-1 flex-col gap-1" role="list">
        {NAV_ITEMS.map((item) => {
          const isActive = item.id === activePage
          return (
            <li key={item.id}>
              {isActive ? (
                <span
                  className="flex items-center gap-2 rounded-md bg-primary/10 px-3 py-2 text-sm font-medium text-primary"
                  aria-current="page"
                >
                  {item.icon}
                  {item.label}
                </span>
              ) : (
                <button
                  type="button"
                  onClick={() => navigate(item.href)}
                  className="flex w-full items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground focus-visible:outline focus-visible:outline-2 focus-visible:outline-ring"
                >
                  {item.icon}
                  {item.label}
                </button>
              )}
            </li>
          )
        })}
      </ul>

      <div className="mt-auto border-t border-border pt-3">
        <button
          type="button"
          onClick={logout}
          className="flex w-full items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground focus-visible:outline focus-visible:outline-2 focus-visible:outline-ring"
        >
          <LogOut className="h-4 w-4 shrink-0" aria-hidden="true" />
          Log out
        </button>
      </div>
    </nav>
  )
}
