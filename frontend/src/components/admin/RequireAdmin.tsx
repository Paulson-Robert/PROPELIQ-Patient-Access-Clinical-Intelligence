import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'

interface RequireAdminProps {
  children: ReactNode
}

/**
 * Route guard that redirects unauthenticated users to /auth/login
 * and non-admin users to their role-appropriate dashboard.
 */
export const RequireAdmin = ({ children }: RequireAdminProps) => {
  const { user } = useAuth()

  if (!user) {
    return <Navigate to="/auth/login" replace />
  }

  if (user.role !== 'admin') {
    return <Navigate to={`/dashboard/${user.role}`} replace />
  }

  return <>{children}</>
}
