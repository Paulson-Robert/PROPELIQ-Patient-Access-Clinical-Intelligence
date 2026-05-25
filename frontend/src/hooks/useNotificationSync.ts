import { useEffect, useRef } from 'react'
import { useAuth } from './useAuth'
import { useNotifications } from './useNotifications'
import { notificationApi } from '../services/notificationApi'
import { bookingApi } from '../services/bookingApi'

/**
 * Syncs backend notifications into the in-app NotificationHistory bell on mount.
 * - Staff/admin: loads from GET /api/notifications/staff
 * - Patients: loads from GET /api/notifications/my
 *
 * Must be rendered inside both <AuthProvider> and <NotificationProvider>.
 */
export const useNotificationSync = () => {
  const { user } = useAuth()
  const { addNotification } = useNotifications()
  const syncedRef = useRef(false)

  useEffect(() => {
    if (!user || syncedRef.current) return

    let cancelled = false
    syncedRef.current = true

    const sync = async () => {
      try {
        if (user.role === 'staff' || user.role === 'admin') {
          const page = await notificationApi.getStaffNotifications(1, 20)
          if (cancelled) return

          // Add unread notifications newest-first so they appear at top of history
          const unread = [...page.items].reverse().filter((n) => !n.isRead)
          for (const n of unread) {
            addNotification({
              variant: n.variant,
              title: n.title,
              message: n.message ?? undefined,
            })
          }
        } else if (user.role === 'patient') {
          const notifications = await bookingApi.getMyNotifications()
          if (cancelled) return

          const toShow = [...notifications].reverse().slice(0, 5)
          for (const n of toShow) {
            addNotification({
              variant: 'info',
              title: formatPatientNotifTitle(n.notificationType),
              message: `Sent via ${n.channel.toLowerCase()} · ${n.status.toLowerCase()}`,
            })
          }
        }
      } catch {
        // Non-critical — bell stays empty if fetch fails
      }
    }

    void sync()
    return () => { cancelled = true }
  }, [user?.email, user?.role, addNotification])
}

const formatPatientNotifTitle = (type: string): string => {
  switch (type) {
    case 'AppointmentReminder': return 'Appointment reminder'
    case 'BookingConfirmation': return 'Booking confirmed'
    case 'CancellationConfirmation': return 'Appointment cancelled'
    case 'DocumentProcessed': return 'Document processed'
    default: return 'Notification'
  }
}
