export type SocialProvider = 'google' | 'microsoft'
export type UserRole = 'patient' | 'staff' | 'admin'

export interface AuthUser {
  email: string
  role: UserRole
  fullName?: string
}

export interface LoginPayload {
  email: string
  password: string
}

export interface RegisterPayload {
  email: string
  password: string
}

export interface AuthResponse {
  user: AuthUser
  accessToken: string
  expiresAtUtc?: string
  tokenType?: string
}

export interface SocialLoginResponse {
  redirectUrl: string
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const USE_MOCK_AUTH = (import.meta.env.VITE_USE_MOCK_AUTH ?? 'true') !== 'false'

class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

const wait = (ms = 300): Promise<void> =>
  new Promise((resolve) => {
    window.setTimeout(resolve, ms)
  })

const postJson = async <TResponse>(
  path: string,
  body: unknown,
): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    let message = 'Request failed'

    try {
      const parsed = (await response.json()) as { message?: string }
      message = parsed.message ?? message
    } catch {
      // If no JSON payload is available, use the default error message.
    }

    throw new ApiError(message, response.status)
  }

  return (await response.json()) as TResponse
}

const mockRoleFromEmail = (email: string): UserRole => {
  if (email.includes('admin')) {
    return 'admin'
  }

  if (email.includes('staff')) {
    return 'staff'
  }

  return 'patient'
}

const mockAuthApi = {
  async login(payload: LoginPayload): Promise<AuthResponse> {
    await wait()

    return {
      user: {
        email: payload.email,
        role: mockRoleFromEmail(payload.email),
      },
      accessToken: 'mock-access-token',
    }
  },

  async register(payload: RegisterPayload): Promise<AuthResponse> {
    await wait()

    const normalizedEmail = payload.email.trim().toLowerCase()

    if (normalizedEmail.includes('duplicate') || normalizedEmail.includes('taken')) {
      throw new ApiError(
        'An account with this email already exists. Please sign in or reset your password.',
        409,
      )
    }

    return {
      user: {
        email: payload.email,
        role: mockRoleFromEmail(payload.email),
      },
      accessToken: 'mock-access-token',
    }
  },

  async startSocialLogin(provider: SocialProvider): Promise<SocialLoginResponse> {
    await wait(200)

    return {
      redirectUrl: `/dashboard/patient?provider=${provider}`,
    }
  },
}

export const authApi = {
  async login(payload: LoginPayload): Promise<AuthResponse> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.login(payload)
    }

    const response = await postJson<{
      accessToken: string
      expiresAtUtc: string
      tokenType: string
      user: { email: string; role: string }
    }>('/api/auth/login', payload)

    return {
      accessToken: response.accessToken,
      expiresAtUtc: response.expiresAtUtc,
      tokenType: response.tokenType,
      user: {
        email: response.user.email,
        role: response.user.role as UserRole,
      },
    }
  },

  async register(payload: RegisterPayload): Promise<AuthResponse> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.register(payload)
    }

    const response = await postJson<{
      accessToken: string
      expiresAtUtc: string
      tokenType: string
      user: { email: string; role: string }
    }>('/api/auth/register', payload)

    return {
      accessToken: response.accessToken,
      expiresAtUtc: response.expiresAtUtc,
      tokenType: response.tokenType,
      user: {
        email: response.user.email,
        role: response.user.role as UserRole,
      },
    }
  },

  async startSocialLogin(provider: SocialProvider): Promise<SocialLoginResponse> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.startSocialLogin(provider)
    }

    const response = await postJson<{ redirectUrl: string }>(
      '/api/auth/social/start',
      { provider },
    )

    return {
      redirectUrl: response.redirectUrl,
    }
  },
}
