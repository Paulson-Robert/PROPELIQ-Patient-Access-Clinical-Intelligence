import { Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from './hooks/useAuth'
import { NotificationProvider } from './hooks/useNotifications'
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

// Redirects /dashboard to the landing page for the authenticated user's role
const DashboardRedirect = () => {
  const { user } = useAuth()
  const role = user?.role ?? 'patient'
  return <Navigate to={`/dashboard/${role}`} replace />
}

function App() {
  return (
    <NotificationProvider>
      <AuthProvider>
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
          <Route path="/dashboard/admin" element={<AdminDashboardPage />} />
          <Route path="/dashboard/staff" element={<AppLayout><StaffDashboardPage /></AppLayout>} />
          <Route path="/dashboard" element={<DashboardRedirect />} />
          <Route path="/booking/history" element={<AppLayout><AppointmentHistoryPage /></AppLayout>} />
          <Route path="/booking/search" element={<AppLayout><AppointmentSearchPage /></AppLayout>} />
          <Route path="/booking/walk-in" element={<WalkInBookingPage />} />
          <Route path="/queue/same-day" element={<SameDayQueuePage />} />
          <Route path="/booking/appointments/:appointmentId" element={<AppLayout><AppointmentDetailPage /></AppLayout>} />
          <Route path="/booking/confirm" element={<AppLayout><BookingConfirmationPage /></AppLayout>} />
          <Route path="/intake" element={<IntakePage />} />
          <Route path="/intake/history" element={<IntakeListPage />} />
          <Route path="/intake/ai" element={<AiIntakePage />} />
          <Route path="/intake/manual" element={<ManualIntakePage />} />
          <Route path="/documents/upload" element={<DocumentUploadPage />} />
          <Route path="/documents" element={<DocumentListPage />} />
          <Route path="/clinical/patient/:patientId" element={<PatientViewPage />} />
          <Route path="/clinical/patient/:patientId/codes" element={<CodeMappingPage />} />
          <Route path="/admin/users" element={<UserManagementPage />} />
          <Route path="/admin/audit-log" element={<AuditLogPage />} />
          <Route path="/admin/metrics" element={<MetricsDashboardPage />} />
          <Route path="*" element={<Navigate to="/auth/login" replace />} />
        </Routes>
      </AuthProvider>
    </NotificationProvider>
  )
}

export default App
