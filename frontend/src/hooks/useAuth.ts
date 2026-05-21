import {
  createElement,
  createContext,
  useCallback,
  useContext,
  useMemo,
  useReducer,
  type PropsWithChildren,
} from 'react'
import {
  authApi,
  type AuthResponse,
  type AuthUser,
  type MfaCodeRequest,
  type MfaMethod,
  type MfaSetupResponse,
  type MfaVerificationRequest,
  type MfaVerificationResponse,
  type RegisterPayload,
  type RegisterResponse,
  type SocialProvider,
  type UserRole,
  type VerificationStatus,
} from '../services/authApi'

interface AuthState {
  user: AuthUser | null
  isLoading: boolean
  error: string | null
}

interface AuthContextValue extends AuthState {
  loginWithPassword: (email: string, password: string) => Promise<AuthResponse>
  registerWithEmail: (payload: RegisterPayload) => Promise<RegisterResponse>
  startSocialLogin: (provider: SocialProvider) => Promise<string>
  verifyEmailToken: (token?: string) => Promise<VerificationStatus>
  resendVerificationEmail: (email: string) => Promise<void>
  getMfaSetup: (email: string, method?: MfaMethod) => Promise<MfaSetupResponse>
  verifyMfaCode: (payload: MfaVerificationRequest) => Promise<MfaVerificationResponse>
  requestMfaCode: (payload: MfaCodeRequest) => Promise<{ sent: true }>
  logout: () => void
  redirectPathForRole: (role: UserRole) => string
}

type AuthAction =
  | { type: 'start' }
  | { type: 'success'; payload: AuthUser }
  | { type: 'failure'; payload: string }
  | { type: 'clear-error' }
  | { type: 'logout' }

const initialState: AuthState = {
  user: null,
  isLoading: false,
  error: null,
}

const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'start':
      return {
        ...state,
        isLoading: true,
        error: null,
      }
    case 'success':
      return {
        user: action.payload,
        isLoading: false,
        error: null,
      }
    case 'failure':
      return {
        ...state,
        isLoading: false,
        error: action.payload,
      }
    case 'clear-error':
      return {
        ...state,
        error: null,
      }
    case 'logout':
      return {
        ...initialState,
      }
    default:
      return state
  }
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

const roleRoutes: Record<UserRole, string> = {
  patient: '/dashboard/patient',
  staff: '/dashboard/staff',
  admin: '/dashboard/admin',
}

export const AuthProvider = ({ children }: PropsWithChildren) => {
  const [state, dispatch] = useReducer(authReducer, initialState)

  const loginWithPassword = useCallback(
    async (email: string, password: string): Promise<AuthResponse> => {
      dispatch({ type: 'start' })

      try {
        const response = await authApi.login({ email, password })
        dispatch({ type: 'success', payload: response.user })
        return response
      } catch (error) {
        const errorMessage =
          error instanceof Error ? error.message : 'Unable to log in at this time'

        dispatch({ type: 'failure', payload: errorMessage })
        throw error
      }
    },
    [],
  )

  const registerWithEmail = useCallback(async (payload: RegisterPayload) => {
    dispatch({ type: 'start' })

    try {
      const response = await authApi.register(payload)
      dispatch({ type: 'clear-error' })
      return response
    } catch (error) {
      const errorMessage =
        error instanceof Error
          ? error.message
          : 'Unable to create your account right now'

      dispatch({ type: 'failure', payload: errorMessage })
      throw error
    }
  }, [])

  const startSocialLogin = useCallback(async (provider: SocialProvider) => {
    dispatch({ type: 'start' })

    try {
      const response = await authApi.startSocialLogin(provider)
      dispatch({ type: 'clear-error' })
      return response.redirectUrl
    } catch (error) {
      const errorMessage =
        error instanceof Error
          ? error.message
          : 'Social login is temporarily unavailable'

      dispatch({ type: 'failure', payload: errorMessage })
      throw error
    }
  }, [])

  const verifyEmailToken = useCallback(async (token?: string) => {
    try {
      const response = await authApi.verifyEmail(token)
      dispatch({ type: 'clear-error' })
      return response.status
    } catch (error) {
      const errorMessage =
        error instanceof Error
          ? error.message
          : 'Unable to verify email with this link'

      dispatch({ type: 'failure', payload: errorMessage })
      throw error
    }
  }, [])

  const resendVerificationEmail = useCallback(async (email: string) => {
    try {
      await authApi.resendVerification(email)
      dispatch({ type: 'clear-error' })
    } catch (error) {
      const errorMessage =
        error instanceof Error
          ? error.message
          : 'Unable to resend verification email'

      dispatch({ type: 'failure', payload: errorMessage })
      throw error
    }
  }, [])

  const getMfaSetup = useCallback(async (email: string, method?: MfaMethod) => {
    try {
      const response = await authApi.getMfaSetup(email, method)
      dispatch({ type: 'clear-error' })
      return response
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : 'Unable to prepare MFA setup'

      dispatch({ type: 'failure', payload: errorMessage })
      throw error
    }
  }, [])

  const verifyMfaCode = useCallback(async (payload: MfaVerificationRequest) => {
    try {
      const response = await authApi.verifyMfaCode(payload)
      dispatch({ type: 'clear-error' })
      return response
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : 'Unable to verify your code'

      dispatch({ type: 'failure', payload: errorMessage })
      throw error
    }
  }, [])

  const requestMfaCode = useCallback(async (payload: MfaCodeRequest) => {
    try {
      const response = await authApi.requestMfaCode(payload)
      dispatch({ type: 'clear-error' })
      return response
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : 'Unable to request a new code'

      dispatch({ type: 'failure', payload: errorMessage })
      throw error
    }
  }, [])

  const logout = useCallback(() => {
    dispatch({ type: 'logout' })
  }, [])

  const redirectPathForRole = useCallback(
    (role: UserRole) => roleRoutes[role] ?? roleRoutes.patient,
    [],
  )

  const value = useMemo<AuthContextValue>(
    () => ({
      ...state,
      loginWithPassword,
      registerWithEmail,
      startSocialLogin,
      verifyEmailToken,
      resendVerificationEmail,
      getMfaSetup,
      verifyMfaCode,
      requestMfaCode,
      logout,
      redirectPathForRole,
    }),
    [
      state,
      loginWithPassword,
      registerWithEmail,
      startSocialLogin,
      verifyEmailToken,
      resendVerificationEmail,
      getMfaSetup,
      verifyMfaCode,
      requestMfaCode,
      logout,
      redirectPathForRole,
    ],
  )

  return createElement(AuthContext.Provider, { value }, children)
}

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }

  return context
}
