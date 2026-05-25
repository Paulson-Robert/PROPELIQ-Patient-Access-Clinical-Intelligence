import { Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from './hooks/useAuth'
import { NotificationProvider } from './hooks/useNotifications'
import { useNotificationSync } from './hooks/useNotificationSync'
import { ToastContainer } from './components/notifications/ToastContainer'
import { NotificationHistory } from './components/notifications/NotificationHistory'
import { AppLayout } from './components/layout/AppLayout'
import { PatientDashboardPage } from './pages/dashboard/PatientDashboardPage'
import { StaffDashboardPage } from './pages/dashboard/StaffDashboardPage'
import { AdminDashboardPage } from './pages/admin/AdminDashboardPage'
import { AppointmentSearchPage } from './pages/booking/AppointmentSearchPage'
import { AppointmentDetailPage } from './pages/booking/AppointmentDetailPage'
import { BookingConfirmationPage } from './pages/booking/BookingConfirmationPage'
import { AppointmentHistoryPage } from './pages/booking/AppointmentHistoryPage'
import { WalkInBookingPage } from './pages/walkin/WalkInBookingPage'
import { SameDayQueuePage } from './pages/queue/SameDayQueuePage'
import { LoginPage } from './pages/auth/LoginPage'
import { MfaSetupPage } from './pages/auth/MfaSetupPage'
import { MfaVerificationPage } from './pages/auth/MfaVerificationPage'
import { PasswordResetPage } from './pages/auth/PasswordResetPage'
import { AiIntakePage } from './pages/intake/AiIntakePage'
import { ManualIntakePage } from './pages/intake/ManualIntakePage'
import { IntakePage } from './pages/intake/IntakePage'
import { IntakeListPage } from './pages/intake/IntakeListPage'
import { DocumentUploadPage } from './pages/documents/DocumentUploadPage'
import { DocumentListPage } from './pages/documents/DocumentListPage'
import { PatientViewPage } from './pages/clinical/PatientViewPage'
import { CodeMappingPage } from './pages/clinical/CodeMappingPage'
import { UserManagementPage } from './pages/admin/UserManagementPage'
import { AuditLogPage } from './pages/admin/AuditLogPage'
import { MetricsDashboardPage } from './pages/admin/MetricsDashboardPage'
import { RequireAdmin } from './components/admin/RequireAdmin'

// Redirects /dashboard to the landing page for the authenticated user's role
const DashboardRedirect = () => {
  const { user } = useAuth()
  const role = user?.role ?? 'patient'
  return <Navigate to={`/dashboard/${role}`} replace />
}

// Syncs backend notifications into the bell history when the user is authenticated.
const NotificationSyncLoader = () => {
  useNotificationSync()
  return null
}

function App() {
  return (
    <NotificationProvider>
      <AuthProvider>
        <NotificationSyncLoader />
        <a
          href="#main-content"
          className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
        >
          Skip to main content
        </a>
        <header className="fixed right-0 top-0 z-40 flex items-center p-2">
          <NotificationHistory />
        </header>
        <ToastContainer />
        <Routes>
          <Route path="/" element={<Navigate to="/auth/login" replace />} />
          <Route path="/auth/login" element={<LoginPage />} />
          <Route path="/auth/password-reset" element={<PasswordResetPage />} />
          <Route path="/auth/mfa/verify" element={<MfaVerificationPage />} />
          <Route path="/auth/mfa/setup" element={<MfaSetupPage />} />
          <Route path="/dashboard/patient" element={<AppLayout><PatientDashboardPage /></AppLayout>} />
          <Route path="/dashboard/admin" element={<RequireAdmin><AdminDashboardPage /></RequireAdmin>} />
          <Route path="/dashboard/staff" element={<AppLayout><StaffDashboardPage /></AppLayout>} />
          <Route path="/dashboard" element={<DashboardRedirect />} />
          <Route path="/booking/history" element={<AppLayout><AppointmentHistoryPage /></AppLayout>} />
          <Route path="/booking/search" element={<AppLayout><AppointmentSearchPage /></AppLayout>} />
          <Route path="/booking/walk-in" element={<AppLayout childrenOwnMain><WalkInBookingPage /></AppLayout>} />
          <Route path="/queue/same-day" element={<AppLayout childrenOwnMain><SameDayQueuePage /></AppLayout>} />
          <Route path="/booking/appointments/:appointmentId" element={<AppLayout><AppointmentDetailPage /></AppLayout>} />
          <Route path="/booking/confirm" element={<AppLayout><BookingConfirmationPage /></AppLayout>} />
          <Route path="/intake" element={<AppLayout childrenOwnMain><IntakePage /></AppLayout>} />
          <Route path="/intake/history" element={<AppLayout childrenOwnMain><IntakeListPage /></AppLayout>} />
          <Route path="/intake/ai" element={<AppLayout childrenOwnMain><AiIntakePage /></AppLayout>} />
          <Route path="/intake/manual" element={<AppLayout childrenOwnMain><ManualIntakePage /></AppLayout>} />
          <Route path="/documents/upload" element={<AppLayout childrenOwnMain><DocumentUploadPage /></AppLayout>} />
          <Route path="/documents" element={<AppLayout childrenOwnMain><DocumentListPage /></AppLayout>} />
          <Route path="/clinical/patient/:patientId" element={<AppLayout childrenOwnMain><PatientViewPage /></AppLayout>} />
          <Route path="/clinical/patient/:patientId/codes" element={<AppLayout childrenOwnMain><CodeMappingPage /></AppLayout>} />
          <Route path="/admin/users" element={<RequireAdmin><UserManagementPage /></RequireAdmin>} />
          <Route path="/admin/audit-log" element={<RequireAdmin><AuditLogPage /></RequireAdmin>} />
          <Route path="/admin/metrics" element={<RequireAdmin><MetricsDashboardPage /></RequireAdmin>} />
          <Route path="*" element={<Navigate to="/auth/login" replace />} />
        </Routes>
      </AuthProvider>
    </NotificationProvider>
  )
}

export default App
