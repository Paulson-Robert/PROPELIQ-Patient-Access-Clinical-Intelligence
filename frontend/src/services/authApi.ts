export type UserRole = 'patient' | 'staff' | 'admin'
export type RegistrationRole = Extract<UserRole, 'patient' | 'staff'>
export type MfaMethod = 'totp' | 'sms'
export type MfaChallengeState = 'verified' | 'verify' | 'setup'
export type MfaVerificationStatus = 'success' | 'invalid' | 'expired' | 'locked'

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
  role: RegistrationRole
}

export interface RequestPasswordResetCodePayload {
  email: string
}

export interface ResetPasswordPayload {
  email: string
  newPassword: string
  verificationCode: string
}

export interface AuthResponse {
  user: AuthUser
  accessToken: string
  expiresAtUtc?: string
  tokenType?: string
  mfaChallenge?: {
    state: MfaChallengeState
    method: MfaMethod
    attemptsRemaining?: number
    expiresAtUtc?: string
  }
}

export interface MfaSetupResponse {
  email: string
  method: MfaMethod
  manualKey: string
  qrCodeDataUri: string
}

export interface MfaVerificationRequest {
  email: string
  code: string
  method: MfaMethod
}

export interface MfaVerificationResponse {
  status: MfaVerificationStatus
  attemptsRemaining: number
}

export interface MfaCodeRequest {
  email: string
  method: MfaMethod
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const USE_MOCK_AUTH = (import.meta.env.VITE_USE_MOCK_AUTH ?? 'true') !== 'false'

export class ApiError extends Error {
  status: number
  code?: string
  attemptsRemaining?: number
  cooldownSeconds?: number

  constructor(message: string, status: number, details?: { code?: string; attemptsRemaining?: number; cooldownSeconds?: number }) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = details?.code
    this.attemptsRemaining = details?.attemptsRemaining
    this.cooldownSeconds = details?.cooldownSeconds
  }
}

const wait = (ms = 300): Promise<void> =>
  new Promise((resolve) => {
    window.setTimeout(resolve, ms)
  })

const mockMfaAttempts = new Map<string, number>()
const mockPasswordResetCodes = new Map<string, string>()

const buildMfaAttemptKey = (email: string, method: MfaMethod): string =>
  `${email.trim().toLowerCase()}:${method}`

const normalizeEmail = (email: string): string => email.trim().toLowerCase()

const createQrCodeDataUri = (label: string): string => {
  const svg = `
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160" role="img" aria-label="${label}">
      <rect width="160" height="160" rx="20" fill="white" />
      <rect x="14" y="14" width="40" height="40" rx="6" fill="currentColor" />
      <rect x="20" y="20" width="28" height="28" rx="4" fill="white" />
      <rect x="106" y="14" width="40" height="40" rx="6" fill="currentColor" />
      <rect x="112" y="20" width="28" height="28" rx="4" fill="white" />
      <rect x="14" y="106" width="40" height="40" rx="6" fill="currentColor" />
      <rect x="20" y="112" width="28" height="28" rx="4" fill="white" />
      <g fill="currentColor">
        <rect x="66" y="18" width="10" height="10" rx="2" />
        <rect x="82" y="18" width="10" height="10" rx="2" />
        <rect x="66" y="34" width="10" height="10" rx="2" />
        <rect x="82" y="34" width="10" height="10" rx="2" />
        <rect x="66" y="66" width="10" height="10" rx="2" />
        <rect x="82" y="66" width="10" height="10" rx="2" />
        <rect x="98" y="66" width="10" height="10" rx="2" />
        <rect x="50" y="82" width="10" height="10" rx="2" />
        <rect x="66" y="82" width="10" height="10" rx="2" />
        <rect x="98" y="82" width="10" height="10" rx="2" />
        <rect x="114" y="82" width="10" height="10" rx="2" />
        <rect x="50" y="98" width="10" height="10" rx="2" />
        <rect x="66" y="98" width="10" height="10" rx="2" />
        <rect x="82" y="98" width="10" height="10" rx="2" />
        <rect x="114" y="98" width="10" height="10" rx="2" />
        <rect x="66" y="114" width="10" height="10" rx="2" />
        <rect x="82" y="114" width="10" height="10" rx="2" />
        <rect x="98" y="114" width="10" height="10" rx="2" />
      </g>
    </svg>
  `

  return `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}`
}

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
    let code: string | undefined
    let attemptsRemaining: number | undefined
    let cooldownSeconds: number | undefined

    try {
      const parsed = (await response.json()) as {
        message?: string
        code?: string
        attemptsRemaining?: number
        cooldownSeconds?: number
      }
      message = parsed.message ?? message
      code = parsed.code
      attemptsRemaining = parsed.attemptsRemaining
      cooldownSeconds = parsed.cooldownSeconds
    } catch {
      // If no JSON payload is available, use the default error message.
    }

    throw new ApiError(message, response.status, {
      code,
      attemptsRemaining,
      cooldownSeconds,
    })
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

const mockMfaMethodFromEmail = (email: string): MfaMethod =>
  email.includes('sms') ? 'sms' : 'totp'

const mockShouldRequireMfaSetup = (email: string): boolean =>
  email.includes('first') || email.includes('setup') || email.includes('onboard')

const mockShouldDisableAccount = (email: string): boolean =>
  email.includes('disabled') || email.includes('deactivated')

const mockAuthApi = {
  async login(payload: LoginPayload): Promise<AuthResponse> {
    await wait()

    const normalizedEmail = payload.email.trim().toLowerCase()

    if (mockShouldDisableAccount(normalizedEmail)) {
      throw new ApiError(
        'Your account has been disabled. Contact your administrator.',
        423,
      )
    }

    const role = mockRoleFromEmail(normalizedEmail)
    const method = mockMfaMethodFromEmail(normalizedEmail)

    return {
      user: {
        email: normalizedEmail,
        role,
      },
      accessToken: role === 'patient' ? 'mock-access-token' : 'mock-pending-mfa-token',
      mfaChallenge:
        role === 'patient'
          ? undefined
          : {
              state: mockShouldRequireMfaSetup(normalizedEmail) ? 'setup' : 'verify',
              method,
              attemptsRemaining: 3,
              expiresAtUtc: new Date(Date.now() + 30_000).toISOString(),
            },
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
        email: normalizedEmail,
        role: payload.role,
      },
      accessToken: 'mock-access-token',
    }
  },

  async getMfaSetup(email: string, method: MfaMethod = 'totp'): Promise<MfaSetupResponse> {
    await wait(250)

    const normalizedEmail = email.trim().toLowerCase()

    return {
      email: normalizedEmail,
      method,
      manualKey: 'JBSWY3DPEHPK3PXP',
      qrCodeDataUri: createQrCodeDataUri(`MFA setup for ${normalizedEmail}`),
    }
  },

  async verifyMfaCode(payload: MfaVerificationRequest): Promise<MfaVerificationResponse> {
    await wait(250)

    const attemptKey = buildMfaAttemptKey(payload.email, payload.method)
    const currentAttempts = mockMfaAttempts.get(attemptKey) ?? 0

    if (payload.code.trim() === '000000') {
      return {
        status: 'expired',
        attemptsRemaining: Math.max(0, 3 - currentAttempts),
      }
    }

    if (payload.code.trim() === '123456') {
      mockMfaAttempts.delete(attemptKey)
      return {
        status: 'success',
        attemptsRemaining: 3,
      }
    }

    const nextAttempts = currentAttempts + 1
    mockMfaAttempts.set(attemptKey, nextAttempts)

    return {
      status: nextAttempts >= 3 ? 'locked' : 'invalid',
      attemptsRemaining: Math.max(0, 3 - nextAttempts),
    }
  },

  async requestMfaCode(payload: MfaCodeRequest): Promise<{ sent: true }> {
    await wait(300)
    mockMfaAttempts.delete(buildMfaAttemptKey(payload.email, payload.method))
    return { sent: true }
  },

  async requestPasswordResetCode(payload: RequestPasswordResetCodePayload): Promise<{ code: string; message: string; cooldownSeconds: number }> {
    await wait(300)
    mockPasswordResetCodes.set(normalizeEmail(payload.email), '123456')
    return {
      code: 'accepted',
      message: 'If an account exists, a verification code has been sent to your email.',
      cooldownSeconds: 30,
    }
  },

  async resetPassword(payload: ResetPasswordPayload): Promise<{ code: string; message: string }> {
    await wait(300)

    const normalizedEmail = normalizeEmail(payload.email)
    const expectedCode = mockPasswordResetCodes.get(normalizedEmail)

    if (!expectedCode || payload.verificationCode.trim() !== expectedCode) {
      throw new ApiError('Verification code is invalid or expired.', 400)
    }

    mockPasswordResetCodes.delete(normalizedEmail)

    return {
      code: 'accepted',
      message: 'If an account exists and code is valid, the password has been reset.',
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
      mfaChallenge?: {
        state: MfaChallengeState
        method: MfaMethod
        attemptsRemaining?: number
        expiresAtUtc?: string
      }
    }>('/api/auth/login', payload)

    return {
      accessToken: response.accessToken,
      expiresAtUtc: response.expiresAtUtc,
      tokenType: response.tokenType,
      user: {
        email: response.user.email,
        role: response.user.role as UserRole,
      },
      mfaChallenge: response.mfaChallenge
        ? {
            state: response.mfaChallenge.state,
            method: response.mfaChallenge.method,
            attemptsRemaining: response.mfaChallenge.attemptsRemaining,
            expiresAtUtc: response.mfaChallenge.expiresAtUtc,
          }
        : undefined,
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

  async getMfaSetup(email: string, method: MfaMethod = 'totp'): Promise<MfaSetupResponse> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.getMfaSetup(email, method)
    }

    return postJson<MfaSetupResponse>('/api/auth/mfa/setup', { email, method })
  },

  async verifyMfaCode(payload: MfaVerificationRequest): Promise<MfaVerificationResponse> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.verifyMfaCode(payload)
    }

    return postJson<MfaVerificationResponse>('/api/auth/mfa/verify', payload)
  },

  async requestMfaCode(payload: MfaCodeRequest): Promise<{ sent: true }> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.requestMfaCode(payload)
    }

    await postJson<{ sent: true }>('/api/auth/mfa/request-code', payload)
    return { sent: true }
  },

  async requestPasswordResetCode(payload: RequestPasswordResetCodePayload): Promise<{ code: string; message: string; cooldownSeconds: number }> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.requestPasswordResetCode(payload)
    }

    return postJson<{ code: string; message: string; cooldownSeconds: number }>('/api/auth/password/reset/request', payload)
  },

  async resetPassword(payload: ResetPasswordPayload): Promise<{ code: string; message: string }> {
    if (USE_MOCK_AUTH || !API_BASE_URL) {
      return mockAuthApi.resetPassword(payload)
    }

    return postJson<{ code: string; message: string }>('/api/auth/password/reset', payload)
  },
}
