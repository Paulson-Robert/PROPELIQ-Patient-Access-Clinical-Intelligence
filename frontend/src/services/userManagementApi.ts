import type { UserRole } from './authApi'

export type UserStatus = 'active' | 'inactive' | 'locked'

export interface ManagedUser {
  id: string
  fullName: string
  email: string
  role: UserRole
  status: UserStatus
  lastLoginDate: string | null
}

export interface CreateUserPayload {
  fullName: string
  email: string
  role: UserRole
  password: string
}

export interface UpdateUserPayload {
  fullName: string
  email: string
  role: UserRole
}

export interface DeactivateUserPayload {
  userId: string
}

export interface UserListResponse {
  users: ManagedUser[]
  total: number
  page: number
  pageSize: number
}

export interface UserListParams {
  search?: string
  role?: UserRole | ''
  page?: number
  pageSize?: number
}

export class UserManagementApiError extends Error {
  status: number
  code?: string

  constructor(message: string, status: number, code?: string) {
    super(message)
    this.name = 'UserManagementApiError'
    this.status = status
    this.code = code
  }
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const USE_MOCK = (import.meta.env.VITE_USE_MOCK_AUTH ?? 'true') !== 'false'

const wait = (ms = 250): Promise<void> =>
  new Promise((resolve) => {
    window.setTimeout(resolve, ms)
  })

const getJson = async <TResponse>(path: string): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'GET',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
  })

  if (!response.ok) {
    let message = 'Request failed'
    let code: string | undefined
    try {
      const parsed = (await response.json()) as { message?: string; code?: string }
      message = parsed.message ?? message
      code = parsed.code
    } catch {
      // Fallback to default message
    }
    throw new UserManagementApiError(message, response.status, code)
  }

  return (await response.json()) as TResponse
}

const postJson = async <TResponse>(path: string, body: unknown): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    let message = 'Request failed'
    let code: string | undefined
    try {
      const parsed = (await response.json()) as { message?: string; code?: string }
      message = parsed.message ?? message
      code = parsed.code
    } catch {
      // Fallback to default message
    }
    throw new UserManagementApiError(message, response.status, code)
  }

  return (await response.json()) as TResponse
}

const patchJson = async <TResponse>(path: string, body: unknown): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    let message = 'Request failed'
    let code: string | undefined
    try {
      const parsed = (await response.json()) as { message?: string; code?: string }
      message = parsed.message ?? message
      code = parsed.code
    } catch {
      // Fallback to default message
    }
    throw new UserManagementApiError(message, response.status, code)
  }

  return (await response.json()) as TResponse
}

// Mock data — mirrors wireframe SCR-023 seed users
let mockUsers: ManagedUser[] = [
  {
    id: 'usr-001',
    fullName: 'Maria Santos',
    email: 'maria.santos@email.com',
    role: 'patient',
    status: 'active',
    lastLoginDate: '2025-01-26',
  },
  {
    id: 'usr-002',
    fullName: 'Jennifer Walsh',
    email: 'j.walsh@healthaccess.io',
    role: 'staff',
    status: 'active',
    lastLoginDate: '2025-01-27',
  },
  {
    id: 'usr-003',
    fullName: 'Admin User',
    email: 'admin@healthaccess.io',
    role: 'admin',
    status: 'active',
    lastLoginDate: '2025-01-27',
  },
  {
    id: 'usr-004',
    fullName: 'Robert Kim',
    email: 'r.kim@email.com',
    role: 'patient',
    status: 'inactive',
    lastLoginDate: '2024-12-15',
  },
  {
    id: 'usr-005',
    fullName: 'Priya Sharma',
    email: 'priya.s@email.com',
    role: 'patient',
    status: 'locked',
    lastLoginDate: '2025-01-10',
  },
]

const PAGE_SIZE = 10

const mockListUsers = async (params: UserListParams): Promise<UserListResponse> => {
  await wait()
  const { search = '', role = '', page = 1, pageSize = PAGE_SIZE } = params

  let filtered = mockUsers.filter((u) => {
    const matchesSearch =
      !search ||
      u.fullName.toLowerCase().includes(search.toLowerCase()) ||
      u.email.toLowerCase().includes(search.toLowerCase())
    const matchesRole = !role || u.role === role
    return matchesSearch && matchesRole
  })

  const total = filtered.length
  const start = (page - 1) * pageSize
  filtered = filtered.slice(start, start + pageSize)

  return { users: filtered, total, page, pageSize }
}

const mockCreateUser = async (payload: CreateUserPayload): Promise<ManagedUser> => {
  await wait()
  const newUser: ManagedUser = {
    id: `usr-${Date.now()}`,
    fullName: payload.fullName,
    email: payload.email,
    role: payload.role,
    status: 'active',
    lastLoginDate: null,
  }
  mockUsers = [...mockUsers, newUser]
  return newUser
}

const mockUpdateUser = async (userId: string, payload: UpdateUserPayload): Promise<ManagedUser> => {
  await wait()
  const idx = mockUsers.findIndex((u) => u.id === userId)
  if (idx === -1) throw new UserManagementApiError('User not found', 404)
  const updated = { ...mockUsers[idx], ...payload }
  mockUsers = mockUsers.map((u) => (u.id === userId ? updated : u))
  return updated
}

const mockDeactivateUser = async (userId: string): Promise<ManagedUser> => {
  await wait()
  const idx = mockUsers.findIndex((u) => u.id === userId)
  if (idx === -1) throw new UserManagementApiError('User not found', 404)
  const updated = { ...mockUsers[idx], status: 'inactive' as UserStatus }
  mockUsers = mockUsers.map((u) => (u.id === userId ? updated : u))
  return updated
}

export const userManagementApi = {
  listUsers: async (params: UserListParams): Promise<UserListResponse> => {
    if (USE_MOCK) return mockListUsers(params)
    const qs = new URLSearchParams()
    if (params.search) qs.set('search', params.search)
    if (params.role) qs.set('role', params.role)
    if (params.page) qs.set('page', String(params.page))
    if (params.pageSize) qs.set('pageSize', String(params.pageSize))
    return getJson<UserListResponse>(`/api/admin/users?${qs}`)
  },

  createUser: async (payload: CreateUserPayload): Promise<ManagedUser> => {
    if (USE_MOCK) return mockCreateUser(payload)
    return postJson<ManagedUser>('/api/admin/users', payload)
  },

  updateUser: async (userId: string, payload: UpdateUserPayload): Promise<ManagedUser> => {
    if (USE_MOCK) return mockUpdateUser(userId, payload)
    return patchJson<ManagedUser>(`/api/admin/users/${encodeURIComponent(userId)}`, payload)
  },

  deactivateUser: async (userId: string): Promise<ManagedUser> => {
    if (USE_MOCK) return mockDeactivateUser(userId)
    return patchJson<ManagedUser>(`/api/admin/users/${encodeURIComponent(userId)}/deactivate`, {})
  },
}
