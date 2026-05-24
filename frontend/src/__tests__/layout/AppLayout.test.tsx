import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { getNavItemsForRole, NAV_ITEMS } from '../../config/navigation'
import { Sidebar } from '../../components/layout/Sidebar'
import { BottomNav } from '../../components/layout/BottomNav'
import { AppLayout } from '../../components/layout/AppLayout'

// Mock useAuth so layout tests don't require a live AuthProvider
vi.mock('../../hooks/useAuth', () => ({
  useAuth: vi.fn(() => ({
    user: { email: 'test@example.com', role: 'patient' },
    logout: vi.fn(),
  })),
}))

// --- navigation config ---

describe('getNavItemsForRole', () => {
  it('returns only patient-accessible items for role=patient', () => {
    const items = getNavItemsForRole('patient')
    expect(items.every((i) => i.roles.includes('patient'))).toBe(true)
  })

  it('returns only staff-accessible items for role=staff', () => {
    const items = getNavItemsForRole('staff')
    expect(items.every((i) => i.roles.includes('staff'))).toBe(true)
  })

  it('returns only admin-accessible items for role=admin', () => {
    const items = getNavItemsForRole('admin')
    expect(items.every((i) => i.roles.includes('admin'))).toBe(true)
  })

  it('excludes admin-only items from patient list', () => {
    const patient = getNavItemsForRole('patient')
    const admin = getNavItemsForRole('admin')
    const adminOnlyPaths = admin
      .filter((a) => !a.roles.includes('patient'))
      .map((a) => a.path)

    adminOnlyPaths.forEach((path) => {
      expect(patient.find((p) => p.path === path)).toBeUndefined()
    })
  })

  it('returns a non-empty list for every role', () => {
    expect(getNavItemsForRole('patient').length).toBeGreaterThan(0)
    expect(getNavItemsForRole('staff').length).toBeGreaterThan(0)
    expect(getNavItemsForRole('admin').length).toBeGreaterThan(0)
  })

  it('returns one role-specific dashboard for every role', () => {
    const expectedDashboards = {
      patient: '/dashboard/patient',
      staff: '/dashboard/staff',
      admin: '/dashboard/admin',
    } as const

    Object.entries(expectedDashboards).forEach(([role, path]) => {
      const dashboards = getNavItemsForRole(role as keyof typeof expectedDashboards).filter(
        (item) => item.label === 'Dashboard',
      )

      expect(dashboards).toHaveLength(1)
      expect(dashboards[0].path).toBe(path)
    })
  })

  it('covers all NAV_ITEMS — every item is reachable by at least one role', () => {
    const covered = NAV_ITEMS.every((item) => item.roles.length > 0)
    expect(covered).toBe(true)
  })
})

// --- Sidebar (desktop) ---

describe('Sidebar', () => {
  const renderSidebar = (role: 'patient' | 'staff' | 'admin') =>
    render(
      <MemoryRouter>
        <Sidebar role={role} />
      </MemoryRouter>,
    )

  it('renders navigation landmark with accessible label', () => {
    renderSidebar('patient')
    expect(screen.getByRole('navigation', { name: 'Main navigation' })).toBeInTheDocument()
  })

  it('shows only patient items for role=patient', () => {
    renderSidebar('patient')
    const patientPaths = new Set(getNavItemsForRole('patient').map((item) => item.path))
    const staffOnly = getNavItemsForRole('staff').filter((item) => !patientPaths.has(item.path))
    staffOnly.forEach((item) => {
      expect(document.querySelector(`a[href="${item.path}"]`)).not.toBeInTheDocument()
    })
  })

  it('shows only admin items for role=admin', () => {
    renderSidebar('admin')
    const adminItems = getNavItemsForRole('admin')
    adminItems.forEach((item) => {
      expect(screen.getByRole('link', { name: item.label })).toBeInTheDocument()
    })
  })

  it('hides admin-only nav items from patient (Edge Case — unauthorised items hidden)', () => {
    renderSidebar('patient')
    const adminOnly = NAV_ITEMS.filter(
      (i) => i.roles.includes('admin') && !i.roles.includes('patient'),
    )
    adminOnly.forEach((item) => {
      expect(document.querySelector(`a[href="${item.path}"]`)).not.toBeInTheDocument()
    })
  })

  it('renders a log-out button', () => {
    renderSidebar('patient')
    expect(screen.getByRole('button', { name: /log out/i })).toBeInTheDocument()
  })
})

// --- BottomNav (mobile) ---

describe('BottomNav', () => {
  const renderBottomNav = (role: 'patient' | 'staff' | 'admin') =>
    render(
      <MemoryRouter>
        <BottomNav role={role} />
      </MemoryRouter>,
    )

  it('renders mobile navigation landmark', () => {
    renderBottomNav('patient')
    expect(screen.getByRole('navigation', { name: 'Mobile navigation' })).toBeInTheDocument()
  })

  it('shows only staff items for role=staff', () => {
    renderBottomNav('staff')
    const staffItems = getNavItemsForRole('staff')
    staffItems.forEach((item) => {
      expect(screen.getByRole('link', { name: new RegExp(item.label, 'i') })).toBeInTheDocument()
    })
  })

  it('hides staff-only items from patient (Edge Case)', () => {
    renderBottomNav('patient')
    const patientPaths = new Set(getNavItemsForRole('patient').map((item) => item.path))
    const staffOnly = NAV_ITEMS.filter(
      (item) => item.roles.includes('staff') && !patientPaths.has(item.path),
    )
    staffOnly.forEach((item) => {
      expect(document.querySelector(`a[href="${item.path}"]`)).not.toBeInTheDocument()
    })
  })

  it('AC-04: active item receives text-primary class on the current route', () => {
    const firstPatientItem = getNavItemsForRole('patient')[0]
    render(
      <MemoryRouter initialEntries={[firstPatientItem.path]}>
        <BottomNav role="patient" />
      </MemoryRouter>,
    )
    const activeLink = screen.getByRole('link', { name: new RegExp(firstPatientItem.label, 'i') })
    expect(activeLink).toHaveClass('text-primary')
  })
})

// --- AppLayout shell ---

describe('AppLayout', () => {
  it('AC-01: renders main content landmark with correct id', () => {
    render(
      <MemoryRouter>
        <AppLayout>
          <p>Page content</p>
        </AppLayout>
      </MemoryRouter>,
    )
    expect(screen.getByRole('main')).toBeInTheDocument()
    expect(document.getElementById('main-content')).toBeInTheDocument()
  })

  it('AC-01: renders a header element', () => {
    render(
      <MemoryRouter>
        <AppLayout>
          <span>content</span>
        </AppLayout>
      </MemoryRouter>,
    )
    expect(screen.getByRole('banner')).toBeInTheDocument()
  })

  it('renders children inside the main content area', () => {
    render(
      <MemoryRouter>
        <AppLayout>
          <p>Test content</p>
        </AppLayout>
      </MemoryRouter>,
    )
    expect(screen.getByText('Test content')).toBeInTheDocument()
  })
})
