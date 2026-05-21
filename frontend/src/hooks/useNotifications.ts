import {
  createElement,
  createContext,
  useCallback,
  useContext,
  useMemo,
  useReducer,
  type PropsWithChildren,
} from 'react'

export type NotificationVariant = 'info' | 'success' | 'warning' | 'error'

export interface Notification {
  id: string
  variant: NotificationVariant
  title: string
  message?: string
  createdAt: number
}

export type AddNotificationPayload = Pick<Notification, 'variant' | 'title'> &
  Partial<Pick<Notification, 'message'>>

interface NotificationState {
  active: Notification[]
  history: Notification[]
}

type NotificationAction =
  | { type: 'add'; payload: AddNotificationPayload }
  | { type: 'dismiss'; id: string }
  | { type: 'dismiss-all' }
  | { type: 'clear-history' }

/** Maximum number of simultaneous active toasts before oldest is evicted to history. */
const MAX_ACTIVE = 10

const notificationReducer = (
  state: NotificationState,
  action: NotificationAction,
): NotificationState => {
  switch (action.type) {
    case 'add': {
      const notification: Notification = {
        id: `notif-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
        createdAt: Date.now(),
        ...action.payload,
      }
      if (state.active.length >= MAX_ACTIVE) {
        const evicted = state.active[0]
        return {
          active: [...state.active.slice(1), notification],
          history: [evicted, ...state.history],
        }
      }
      return { ...state, active: [...state.active, notification] }
    }
    case 'dismiss': {
      const dismissed = state.active.find((n) => n.id === action.id)
      return {
        active: state.active.filter((n) => n.id !== action.id),
        history: dismissed ? [dismissed, ...state.history] : state.history,
      }
    }
    case 'dismiss-all': {
      return {
        active: [],
        history: [...state.active.reverse(), ...state.history],
      }
    }
    case 'clear-history': {
      return { ...state, history: [] }
    }
    default:
      return state
  }
}

const initialState: NotificationState = {
  active: [],
  history: [],
}

interface NotificationContextValue {
  active: Notification[]
  history: Notification[]
  addNotification: (payload: AddNotificationPayload) => void
  dismiss: (id: string) => void
  dismissAll: () => void
  clearHistory: () => void
}

const NotificationContext = createContext<NotificationContextValue | undefined>(undefined)

export const NotificationProvider = ({ children }: PropsWithChildren) => {
  const [state, dispatch] = useReducer(notificationReducer, initialState)

  const addNotification = useCallback((payload: AddNotificationPayload) => {
    dispatch({ type: 'add', payload })
  }, [])

  const dismiss = useCallback((id: string) => {
    dispatch({ type: 'dismiss', id })
  }, [])

  const dismissAll = useCallback(() => {
    dispatch({ type: 'dismiss-all' })
  }, [])

  const clearHistory = useCallback(() => {
    dispatch({ type: 'clear-history' })
  }, [])

  const value = useMemo(
    () => ({ ...state, addNotification, dismiss, dismissAll, clearHistory }),
    [state, addNotification, dismiss, dismissAll, clearHistory],
  )

  return createElement(NotificationContext.Provider, { value }, children)
}

export const useNotifications = (): NotificationContextValue => {
  const ctx = useContext(NotificationContext)
  if (!ctx) {
    throw new Error('useNotifications must be used within a NotificationProvider')
  }
  return ctx
}
