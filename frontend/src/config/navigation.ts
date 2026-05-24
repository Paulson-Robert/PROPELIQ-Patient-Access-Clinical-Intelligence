import type { ComponentType } from 'react'
import {
  BarChart3,
  Calendar,
  ClipboardList,
  FileText,
  LayoutDashboard,
  LogIn,
  Settings,
  Users,
  Activity,
} from 'lucide-react'
import type { UserRole } from '../services/authApi'

// AC-03: Role-based menu configuration — each item declares which roles may access it
export interface NavItem {
  label: string
  path: string
  // LucideIcon component type
  icon: ComponentType<{ className?: string }>
  roles: ReadonlyArray<UserRole>
}

// Single source of truth for all navigable routes and their role access rules.
// Items are ordered per role; getNavItemsForRole preserves array order.
// Each role has its own Dashboard entry pointing to its role-specific landing page.
export const NAV_ITEMS: ReadonlyArray<NavItem> = [
  { label: 'Dashboard',      path: '/dashboard/patient',  icon: LayoutDashboard, roles: ['patient'] },
  { label: 'Appointments',   path: '/booking/history',   icon: Calendar,        roles: ['patient'] },
  { label: 'Book',           path: '/booking/search',    icon: Calendar,        roles: ['patient'] },
  { label: 'Intake',         path: '/intake/history', icon: ClipboardList,   roles: ['patient'] },
  { label: 'Documents',      path: '/documents',         icon: FileText,        roles: ['patient'] },

  // ── Staff ─────────────────────────────────────────────────────────────────
  { label: 'Dashboard',      path: '/dashboard/staff',   icon: LayoutDashboard, roles: ['staff'] },
  { label: 'Queue',          path: '/queue/same-day',    icon: Activity,        roles: ['staff'] },
  { label: 'Walk-in booking', path: '/booking/walk-in',  icon: LogIn,           roles: ['staff'] },
  { label: 'Intake',         path: '/intake',            icon: ClipboardList,   roles: ['staff'] },
  { label: 'Documents',      path: '/documents',         icon: FileText,        roles: ['staff'] },

  // ── Admin ─────────────────────────────────────────────────────────────────
  { label: 'Dashboard',      path: '/dashboard/admin',   icon: LayoutDashboard, roles: ['admin'] },
  { label: 'Users',          path: '/admin/users',       icon: Users,           roles: ['admin'] },
  { label: 'Audit Log',      path: '/admin/audit-log',   icon: Settings,        roles: ['admin'] },
  { label: 'Metrics',        path: '/admin/metrics',     icon: BarChart3,       roles: ['admin'] },
] as const

// AC-03 / Edge Case: returns only items the given role is authorised to see
export const getNavItemsForRole = (role: UserRole): NavItem[] =>
  NAV_ITEMS.filter((item) => item.roles.includes(role))
