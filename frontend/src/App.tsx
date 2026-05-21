import { Navigate, Route, Routes, useParams } from 'react-router-dom'
import { AuthProvider } from './hooks/useAuth'
import { NotificationProvider } from './hooks/useNotifications'
import { ToastContainer } from './components/notifications/ToastContainer'
import { NotificationHistory } from './components/notifications/NotificationHistory'
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
import { DocumentUploadPage } from './pages/documents/DocumentUploadPage'
import { DocumentListPage } from './pages/documents/DocumentListPage'
import { PatientViewPage } from './pages/clinical/PatientViewPage'
import { CodeMappingPage } from './pages/clinical/CodeMappingPage'
import { UserManagementPage } from './pages/admin/UserManagementPage'

const DashboardPage = () => {
  const { role = 'patient' } = useParams<{ role: string }>()

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4 py-8 text-foreground">
      <section className="w-full max-w-xl rounded-xl border border-border bg-card p-6 text-center shadow-sm">
        <h1 className="text-3xl font-semibold tracking-tight">PropelIQ Dashboard</h1>
        <p className="mt-3 text-muted-foreground">
          Signed in successfully. Active role: <span className="font-medium text-foreground">{role}</span>
        </p>
      </section>
    </main>
  )
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
          <Route path="/dashboard/:role" element={<DashboardPage />} />
          <Route path="/booking/history" element={<AppointmentHistoryPage />} />
          <Route path="/booking/search" element={<AppointmentSearchPage />} />
          <Route path="/booking/walk-in" element={<WalkInBookingPage />} />
          <Route path="/queue/same-day" element={<SameDayQueuePage />} />
          <Route path="/booking/appointments/:appointmentId" element={<AppointmentDetailPage />} />
          <Route path="/booking/confirm" element={<BookingConfirmationPage />} />
          <Route path="/intake" element={<IntakePage />} />
          <Route path="/intake/ai" element={<AiIntakePage />} />
          <Route path="/intake/manual" element={<ManualIntakePage />} />
          <Route path="/documents/upload" element={<DocumentUploadPage />} />
          <Route path="/documents" element={<DocumentListPage />} />
          <Route path="/clinical/patient/:patientId" element={<PatientViewPage />} />
          <Route path="/clinical/patient/:patientId/codes" element={<CodeMappingPage />} />
          <Route path="/admin/users" element={<UserManagementPage />} />
          <Route path="*" element={<Navigate to="/auth/login" replace />} />
        </Routes>
      </AuthProvider>
    </NotificationProvider>
  )
}

export default App
