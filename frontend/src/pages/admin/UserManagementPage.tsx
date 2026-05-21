import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ChevronLeft, ChevronRight, Users } from 'lucide-react'
import { cn } from '../../lib/utils'
import { useAuth } from '../../hooks/useAuth'
import { UserFormDialog, type UserFormValues } from '../../components/admin/UserFormDialog'
import { DeactivateDialog } from '../../components/admin/DeactivateDialog'
import {
  userManagementApi,
  type ManagedUser,
  type UserStatus,
} from '../../services/userManagementApi'
import type { UserRole } from '../../services/authApi'

const PAGE_SIZE = 10

const ROLE_LABELS: Record<UserRole, string> = {
  patient: 'Patient',
  staff: 'Staff',
  admin: 'Admin',
}

const STATUS_CLASSES: Record<UserStatus, string> = {
  active: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  inactive: 'bg-muted text-muted-foreground',
  locked: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
}

const STATUS_LABELS: Record<UserStatus, string> = {
  active: 'Active',
  inactive: 'Inactive',
  locked: 'Locked',
}

const ROLE_BADGE_CLASSES: Record<UserRole, string> = {
  patient: 'bg-muted text-muted-foreground',
  staff: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
  admin: 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400',
}

const formatDate = (iso: string | null): string => {
  if (!iso) return '—'
  const [year, month, day] = iso.split('-').map(Number)
  const d = new Date(year, month - 1, day)
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

type DialogMode = 'none' | 'create' | 'edit' | 'deactivate'

export const UserManagementPage = () => {
  const navigate = useNavigate()
  const { user: currentUser } = useAuth()

  // List state
  const [search, setSearch] = useState('')
  const [roleFilter, setRoleFilter] = useState<UserRole | ''>('')
  const [page, setPage] = useState(1)
  const [users, setUsers] = useState<ManagedUser[]>([])
  const [total, setTotal] = useState(0)
  const [isLoading, setIsLoading] = useState(false)
  const [listError, setListError] = useState<string | null>(null)

  // Dialog state
  const [dialogMode, setDialogMode] = useState<DialogMode>('none')
  const [selectedUser, setSelectedUser] = useState<ManagedUser | undefined>(undefined)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [dialogError, setDialogError] = useState<string | null>(null)

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE))

  const fetchUsers = useCallback(async () => {
    setIsLoading(true)
    setListError(null)
    try {
      const result = await userManagementApi.listUsers({
        search,
        role: roleFilter,
        page,
        pageSize: PAGE_SIZE,
      })
      setUsers(result.users)
      setTotal(result.total)
    } catch {
      setListError('Failed to load users. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }, [search, roleFilter, page])

  // Reset to page 1 when filters change
  useEffect(() => {
    setPage(1)
  }, [search, roleFilter])

  useEffect(() => {
    void fetchUsers()
  }, [fetchUsers])

  const openCreate = () => {
    setSelectedUser(undefined)
    setDialogError(null)
    setDialogMode('create')
  }

  const openEdit = (user: ManagedUser) => {
    setSelectedUser(user)
    setDialogError(null)
    setDialogMode('edit')
  }

  const openDeactivate = (user: ManagedUser) => {
    setSelectedUser(user)
    setDialogError(null)
    setDialogMode('deactivate')
  }

  const closeDialog = () => {
    setDialogMode('none')
    setSelectedUser(undefined)
    setDialogError(null)
  }

  // AC-01: create user
  const handleFormSubmit = async (values: UserFormValues) => {
    setIsSubmitting(true)
    setDialogError(null)
    try {
      if (dialogMode === 'create') {
        await userManagementApi.createUser(values)
      } else if (dialogMode === 'edit' && selectedUser) {
        await userManagementApi.updateUser(selectedUser.id, {
          fullName: values.fullName,
          email: values.email,
          role: values.role,
        })
      }
      closeDialog()
      void fetchUsers()
    } catch (err) {
      setDialogError(err instanceof Error ? err.message : 'An error occurred. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  // AC-03: deactivate prevents login without deleting data
  const handleDeactivateConfirm = async () => {
    if (!selectedUser) return
    setIsSubmitting(true)
    setDialogError(null)
    try {
      await userManagementApi.deactivateUser(selectedUser.id)
      closeDialog()
      void fetchUsers()
    } catch (err) {
      setDialogError(err instanceof Error ? err.message : 'Deactivation failed. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  // Edge case: prevent admin self-deactivation
  const isSelf = selectedUser?.email === currentUser?.email

  return (
    <>
      <a href="#main" className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground">
        Skip to main content
      </a>

      <div className="flex min-h-screen bg-background">
        {/* Sidebar */}
        <nav className="hidden w-56 shrink-0 border-r border-border bg-card p-4 lg:flex lg:flex-col" aria-label="Admin navigation">
          <div className="mb-6 flex items-center gap-2 text-sm font-semibold text-foreground">
            <Users className="h-5 w-5 text-primary" aria-hidden="true" />
            HealthAccess
          </div>
          <ul className="flex flex-col gap-1">
            <li>
              <a
                href="/dashboard/admin"
                className="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
              >
                Dashboard
              </a>
            </li>
            <li>
              <span
                className="flex items-center gap-2 rounded-md bg-muted px-3 py-2 text-sm font-medium text-foreground"
                aria-current="page"
              >
                Users
              </span>
            </li>
          </ul>
        </nav>

        {/* Main content */}
        <div className="flex flex-1 flex-col">
          {/* Header */}
          <header className="flex items-center justify-between border-b border-border bg-card px-6 py-4">
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => navigate('/dashboard/admin')}
                className="flex items-center justify-center rounded-md p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                aria-label="Back to dashboard"
              >
                <ChevronLeft className="h-5 w-5" aria-hidden="true" />
              </button>
              <h1 className="text-xl font-semibold text-foreground">User management</h1>
            </div>
            <button
              type="button"
              onClick={openCreate}
              className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-ring"
            >
              + Create user
            </button>
          </header>

          <main id="main" className="flex-1 p-6">
            {/* Filters */}
            <div className="mb-4 flex flex-wrap gap-3">
              <input
                type="search"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search by name or email…"
                aria-label="Search users"
                className="w-64 rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              />
              <select
                value={roleFilter}
                onChange={(e) => setRoleFilter(e.target.value as UserRole | '')}
                aria-label="Filter by role"
                className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <option value="">All roles</option>
                <option value="patient">Patient</option>
                <option value="staff">Staff</option>
                <option value="admin">Admin</option>
              </select>
            </div>

            {/* Error banner */}
            {listError && (
              <p role="alert" className="mb-4 rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
                {listError}
              </p>
            )}

            {/* Table */}
            <div className="overflow-x-auto rounded-lg border border-border bg-card">
              <table className="w-full text-sm" aria-label="User accounts">
                <thead>
                  <tr className="border-b border-border bg-muted/50 text-left text-xs font-medium uppercase tracking-wide text-muted-foreground">
                    <th scope="col" className="px-4 py-3">Name</th>
                    <th scope="col" className="px-4 py-3">Email</th>
                    <th scope="col" className="px-4 py-3">Role</th>
                    <th scope="col" className="px-4 py-3">Status</th>
                    <th scope="col" className="px-4 py-3">Last login</th>
                    <th scope="col" className="px-4 py-3">
                      <span className="sr-only">Actions</span>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {isLoading && (
                    <tr>
                      <td colSpan={6} className="px-4 py-8 text-center text-muted-foreground">
                        Loading…
                      </td>
                    </tr>
                  )}
                  {!isLoading && users.length === 0 && (
                    <tr>
                      <td colSpan={6} className="px-4 py-8 text-center text-muted-foreground">
                        No users found.
                      </td>
                    </tr>
                  )}
                  {!isLoading &&
                    users.map((u) => (
                      <tr
                        key={u.id}
                        className="border-b border-border last:border-0 hover:bg-muted/30"
                      >
                        <td className="px-4 py-3 font-medium text-foreground">{u.fullName}</td>
                        <td className="px-4 py-3 text-muted-foreground">{u.email}</td>
                        <td className="px-4 py-3">
                          <span
                            className={cn(
                              'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
                              ROLE_BADGE_CLASSES[u.role],
                            )}
                          >
                            {ROLE_LABELS[u.role]}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <span
                            className={cn(
                              'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
                              STATUS_CLASSES[u.status],
                            )}
                          >
                            {STATUS_LABELS[u.status]}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-muted-foreground">
                          {formatDate(u.lastLoginDate)}
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-2">
                            <button
                              type="button"
                              onClick={() => openEdit(u)}
                              className="rounded px-2 py-1 text-xs font-medium text-foreground hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring"
                              aria-label={`Edit ${u.fullName}`}
                            >
                              Edit
                            </button>
                            {u.status !== 'inactive' && (
                              <button
                                type="button"
                                onClick={() => openDeactivate(u)}
                                className="rounded px-2 py-1 text-xs font-medium text-destructive hover:bg-destructive/10 focus:outline-none focus:ring-2 focus:ring-ring"
                                aria-label={`Deactivate ${u.fullName}`}
                              >
                                Deactivate
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            <div className="mt-4 flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                {total === 0
                  ? 'No users'
                  : `Showing ${(page - 1) * PAGE_SIZE + 1}–${Math.min(page * PAGE_SIZE, total)} of ${total} users`}
              </p>
              <div className="flex items-center gap-1" role="navigation" aria-label="Pagination">
                <button
                  type="button"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page <= 1}
                  className="flex h-8 w-8 items-center justify-center rounded border border-border text-muted-foreground hover:bg-muted disabled:opacity-40 focus:outline-none focus:ring-2 focus:ring-ring"
                  aria-label="Previous page"
                >
                  <ChevronLeft className="h-4 w-4" aria-hidden="true" />
                </button>
                {Array.from({ length: totalPages }, (_, i) => i + 1)
                  .filter((p) => p === 1 || p === totalPages || Math.abs(p - page) <= 1)
                  .reduce<(number | '…')[]>((acc, p, idx, arr) => {
                    if (idx > 0 && p - (arr[idx - 1] as number) > 1) acc.push('…')
                    acc.push(p)
                    return acc
                  }, [])
                  .map((item, idx) =>
                    item === '…' ? (
                      <span key={`ellipsis-${idx}`} className="px-1 text-muted-foreground">
                        …
                      </span>
                    ) : (
                      <button
                        key={item}
                        type="button"
                        onClick={() => setPage(item as number)}
                        aria-current={page === item ? 'page' : undefined}
                        className={cn(
                          'flex h-8 w-8 items-center justify-center rounded border text-sm focus:outline-none focus:ring-2 focus:ring-ring',
                          page === item
                            ? 'border-primary bg-primary text-primary-foreground'
                            : 'border-border text-muted-foreground hover:bg-muted',
                        )}
                      >
                        {item}
                      </button>
                    ),
                  )}
                <button
                  type="button"
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page >= totalPages}
                  className="flex h-8 w-8 items-center justify-center rounded border border-border text-muted-foreground hover:bg-muted disabled:opacity-40 focus:outline-none focus:ring-2 focus:ring-ring"
                  aria-label="Next page"
                >
                  <ChevronRight className="h-4 w-4" aria-hidden="true" />
                </button>
              </div>
            </div>
          </main>
        </div>
      </div>

      {/* MOD-004: Create / Edit dialog */}
      <UserFormDialog
        user={dialogMode === 'edit' ? selectedUser : undefined}
        open={dialogMode === 'create' || dialogMode === 'edit'}
        isSubmitting={isSubmitting}
        error={dialogError}
        onSubmit={(values) => void handleFormSubmit(values)}
        onCancel={closeDialog}
      />

      {/* MOD-004: Deactivate dialog */}
      <DeactivateDialog
        user={selectedUser ?? null}
        open={dialogMode === 'deactivate'}
        isSubmitting={isSubmitting}
        error={dialogError}
        isSelf={isSelf}
        onConfirm={() => void handleDeactivateConfirm()}
        onCancel={closeDialog}
      />
    </>
  )
}
