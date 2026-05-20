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

export interface RegisterResponse {
  requiresVerification: boolean
  duplicateEmail: boolean
}

export interface SocialLoginResponse {
  redirectUrl: string
}

export type VerificationStatus = 'success' | 'expired' | 'invalid' | 'pending'

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

const getJson = async <TResponse>(path: string): Promise<TResponse> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
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

  async register(payload: RegisterPayload): Promise<RegisterResponse> {
    await wait()

    const normalizedEmail = payload.email.trim().toLowerCase()

    if (normalizedEmail.includes('duplicate') || normalizedEmail.includes('taken')) {
      return {
        requiresVerification: false,
        duplicateEmail: true,
      }
    }

    return {
      requiresVerification: true,
      duplicateEmail: false,
    }
  },

  async startSocialLogin(provider: SocialProvider): Promise<SocialLoginResponse> {
    await wait(200)

    return {
      redirectUrl: `/dashboard/patient?provider=${provider}`,
    }
  },

  async verifyEmail(token?: string): Promise<{ status: VerificationStatus }> {
    await wait(250)

    if (!token) {
      return { status: 'pending' }
    }

    if (token.startsWith('expired')) {
      return { status: 'expired' }
    }

    if (token.startsWith('invalid')) {
      return { status: 'invalid' }
    }

    return { status: 'success' }
  },

  async resendVerification(): Promise<{ sent: true }> {
    await wait(350)
    return { sent: true }
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

  async register(payload: RegisterPayload): Promise<RegisterResponse> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.register(payload)
    }

    try {
      await postJson<{ status: string; email: string }>('/api/auth/register', payload)
      return {
        duplicateEmail: false,
        requiresVerification: true,
      }
    } catch (error) {
      if (error instanceof ApiError && error.status === 409) {
        return {
          duplicateEmail: true,
          requiresVerification: false,
        }
      }

      throw error
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

  async verifyEmail(token?: string): Promise<{ status: VerificationStatus }> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.verifyEmail(token)
    }

    if (!token) {
      return { status: 'pending' }
    }

    try {
      const response = await getJson<{ status: string; email: string }>(
        `/api/auth/verify-email?token=${encodeURIComponent(token)}`,
      )
      return { status: (response.status === 'verified' ? 'success' : 'pending') as VerificationStatus }
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.status === 410) {
          return { status: 'expired' }
        }
        if (error.status === 400 || error.status === 404) {
          return { status: 'invalid' }
        }
      }
      throw error
    }
  },

  async resendVerification(email: string): Promise<{ sent: true }> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.resendVerification()
    }

    await postJson<{ status: string; message: string }>(
      '/api/auth/resend-verification',
      { email },
    )

    return { sent: true }
  },
}
