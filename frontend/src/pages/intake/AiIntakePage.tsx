import { useCallback, useEffect, useRef, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { AlertCircle, Loader2 } from 'lucide-react'
import { ChatBubble } from '../../components/intake/ChatBubble'
import { IntakeProgress } from '../../components/intake/IntakeProgress'
import { IntakeSummary, type ExtractedField } from '../../components/intake/IntakeSummary'
import { useAuth } from '../../hooks/useAuth'
import {
  BookingError,
  bookingApi,
  type AiIntakeSubmitPayload,
  type AppointmentRecord,
} from '../../services/bookingApi'

// ---------------------------------------------------------------------------
// Intake question flow
// ---------------------------------------------------------------------------

interface IntakeQuestion {
  key: keyof ExtractedData
  prompt: string
  chips: string[]
}

interface ExtractedData {
  chronicConditions: string | null
  currentMedications: string | null
  allergies: string | null
  surgicalHistory: string | null
  reasonForVisit: string | null
}

const INTAKE_QUESTIONS: IntakeQuestion[] = [
  {
    key: 'chronicConditions',
    prompt:
      "Hi! I'll help you complete your intake. Let's start with your medical history. Do you have any chronic conditions such as diabetes, hypertension, or asthma?",
    chips: ['No chronic conditions', 'Type 2 diabetes', 'Let me list them'],
  },
  {
    key: 'currentMedications',
    prompt:
      'Are you currently taking any prescription medications? If so, please list the name and dosage.',
    chips: ['No medications', 'Not sure', 'Let me list them'],
  },
  {
    key: 'allergies',
    prompt: 'Do you have any known allergies to medications, foods, or environmental factors?',
    chips: ['No known allergies', 'Penicillin allergy', 'Let me list them'],
  },
  {
    key: 'surgicalHistory',
    prompt: 'Have you had any surgeries or major procedures in the past?',
    chips: ['No prior surgeries', 'Yes, in the past 5 years', 'Let me list them'],
  },
  {
    key: 'reasonForVisit',
    prompt:
      "Almost done! What is your primary reason for today's visit? Please describe your main concern or symptom.",
    chips: ['Routine check-up', 'Follow-up visit', 'New symptom'],
  },
]

const TOTAL_QUESTIONS = INTAKE_QUESTIONS.length

// Client-guided assistant typing delay (ms) for natural conversation pacing.
const AI_RESPONSE_DELAY_MS = 800

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface ChatMessage {
  id: string
  variant: 'ai' | 'user'
  text: string
}

type PageView = 'chat' | 'summary' | 'submitted'
type IntakeContextState = 'loading' | 'ready' | 'missing' | 'error'

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

const buildExtractedFields = (data: ExtractedData): ExtractedField[] => [
  { label: 'Chronic conditions', value: data.chronicConditions, confidence: data.chronicConditions ? 'high' : 'pending' },
  { label: 'Current medications', value: data.currentMedications, confidence: data.currentMedications ? 'high' : 'pending' },
  { label: 'Allergies', value: data.allergies, confidence: data.allergies ? 'high' : 'pending' },
  { label: 'Surgical history', value: data.surgicalHistory, confidence: data.surgicalHistory ? 'high' : 'pending' },
  { label: 'Reason for visit', value: data.reasonForVisit, confidence: data.reasonForVisit ? 'high' : 'pending' },
]

const pickDefaultAppointment = (appointments: AppointmentRecord[]): AppointmentRecord | null =>
  appointments
    .filter((appointment) => appointment.status === 'Scheduled')
    .sort((a, b) => `${a.date}T${a.startTime}`.localeCompare(`${b.date}T${b.startTime}`))[0] ?? null

const toSubmitPayload = (data: ExtractedData): AiIntakeSubmitPayload => ({
  chronicConditions: data.chronicConditions,
  currentMedications: data.currentMedications,
  allergies: data.allergies,
  surgicalHistory: data.surgicalHistory,
  reasonForVisit: data.reasonForVisit?.trim() ?? '',
})

// ---------------------------------------------------------------------------
// Props
// ---------------------------------------------------------------------------

interface AiIntakePageProps {
  /** Called whenever the user answers a question so the parent can track
   *  partial AI data for bridging to manual mode (AC-02). */
  onExtractedDataChange?: (data: Partial<ExtractedData>) => void
  /** Called when the user clicks "Manual form" toggle (AC-01). */
  onSwitchToManual?: () => void
  /** Called when the AI becomes unavailable so the parent can auto-switch
   *  to manual mode (AC-03). */
  onAiUnavailable?: () => void
}

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

export const AiIntakePage = ({
  onExtractedDataChange,
  onSwitchToManual,
  onAiUnavailable,
}: AiIntakePageProps = {}) => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { user } = useAuth()
  const queryAppointmentId = searchParams.get('appointmentId')

  const [view, setView] = useState<PageView>('chat')
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [appointmentId, setAppointmentId] = useState<string | null>(queryAppointmentId)
  const [contextState, setContextState] = useState<IntakeContextState>(
    queryAppointmentId ? 'ready' : 'loading',
  )
  const [contextMessage, setContextMessage] = useState<string | null>(null)
  const [isAiTyping, setIsAiTyping] = useState(false)
  const [inputValue, setInputValue] = useState('')
  const [questionIndex, setQuestionIndex] = useState(0)
  const [extractedData, setExtractedData] = useState<ExtractedData>({
    chronicConditions: null,
    currentMedications: null,
    allergies: null,
    surgicalHistory: null,
    reasonForVisit: null,
  })
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [aiUnavailable, setAiUnavailable] = useState(false)

  const messagesEndRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const typingTimerRef = useRef<number | null>(null)
  const appointmentQuery = appointmentId ? `?appointmentId=${encodeURIComponent(appointmentId)}` : ''

  const clearTypingTimer = useCallback(() => {
    if (typingTimerRef.current !== null) {
      window.clearTimeout(typingTimerRef.current)
      typingTimerRef.current = null
    }
  }, [])

  useEffect(() => {
    if (queryAppointmentId) {
      setAppointmentId(queryAppointmentId)
      setContextState('ready')
      setContextMessage(null)
      return
    }

    if (user?.role === 'staff') {
      setAppointmentId(null)
      setContextState('missing')
      setContextMessage('Open intake from a patient row in the same-day queue so the AI intake is tied to an appointment.')
      return
    }

    let cancelled = false

    const resolveAppointment = async () => {
      setContextState('loading')
      setContextMessage(null)

      try {
        const appointments = await bookingApi.getMyAppointments('Scheduled')
        if (cancelled) return

        const nextAppointment = pickDefaultAppointment(appointments)
        if (!nextAppointment) {
          setAppointmentId(null)
          setContextState('missing')
          setContextMessage('Book or select an upcoming appointment before completing pre-visit intake.')
          return
        }

        setAppointmentId(nextAppointment.id)
        setContextState('ready')
      } catch {
        if (cancelled) return
        setAppointmentId(null)
        setContextState('error')
        setContextMessage('Unable to load your upcoming appointments. Please try again.')
      }
    }

    void resolveAppointment()

    return () => {
      cancelled = true
    }
  }, [queryAppointmentId, user?.role])

  // Post the first AI question once the appointment context is ready.
  useEffect(() => {
    if (contextState !== 'ready' || messages.length > 0) return

    setIsAiTyping(true)
    typingTimerRef.current = window.setTimeout(() => {
      try {
        const first = INTAKE_QUESTIONS[0]
        setMessages([{ id: 'ai-0', variant: 'ai', text: first.prompt }])
      } catch {
        setAiUnavailable(true)
        onAiUnavailable?.()
      } finally {
        setIsAiTyping(false)
      }
    }, AI_RESPONSE_DELAY_MS)

    return clearTypingTimer
  }, [clearTypingTimer, contextState, messages.length])

  // Scroll to latest message
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, isAiTyping])

  const completedAnswers = Object.values(extractedData).filter(Boolean).length

  const postAiMessage = useCallback(
    (nextIndex: number) => {
      if (nextIndex >= TOTAL_QUESTIONS) {
        // All questions answered — move to summary
        setView('summary')
        return
      }

      setIsAiTyping(true)
      typingTimerRef.current = window.setTimeout(() => {
        try {
          const next = INTAKE_QUESTIONS[nextIndex]
          setMessages((prev) => [
            ...prev,
            { id: `ai-${nextIndex}`, variant: 'ai', text: next.prompt },
          ])
          setIsAiTyping(false)
        } catch {
          setIsAiTyping(false)
          setAiUnavailable(true)
          onAiUnavailable?.()
        }
      }, AI_RESPONSE_DELAY_MS)
    },
    [],
  )

  const handleUserResponse = useCallback(
    (responseText: string) => {
      const trimmed = responseText.trim()
      if (!trimmed || questionIndex >= TOTAL_QUESTIONS) return

      const currentQuestion = INTAKE_QUESTIONS[questionIndex]
      const msgId = `user-${questionIndex}`

      setMessages((prev) => [...prev, { id: msgId, variant: 'user', text: trimmed }])
      const updated = { ...extractedData, [currentQuestion.key]: trimmed }
      setExtractedData(updated)
      onExtractedDataChange?.(updated)
      setInputValue('')
      setSubmitError(null)

      const nextIndex = questionIndex + 1
      setQuestionIndex(nextIndex)
      postAiMessage(nextIndex)
    },
    [questionIndex, postAiMessage, extractedData],
  )

  const handleSend = () => handleUserResponse(inputValue)

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault()
      handleSend()
    }
  }

  const handleSubmit = async () => {
    const payload = toSubmitPayload(extractedData)

    if (!appointmentId) {
      setSubmitError('Choose an appointment before submitting intake.')
      return
    }

    if (!payload.reasonForVisit) {
      setSubmitError('Reason for visit is required.')
      return
    }

    setIsSubmitting(true)
    setSubmitError(null)

    try {
      await bookingApi.submitAiIntake(appointmentId, payload)
      setView('submitted')
    } catch (error) {
      const message =
        error instanceof BookingError
          ? error.message
          : 'Unable to submit intake right now. Please try again.'

      setSubmitError(message)
    } finally {
      setIsSubmitting(false)
    }
  }

  if (contextState === 'loading') {
    return (
      <main
        className="flex min-h-screen items-center justify-center bg-background px-4 py-8 text-foreground"
        id="main-content"
      >
        <div className="flex items-center gap-3 text-sm text-muted-foreground" role="status">
          <Loader2 className="h-5 w-5 animate-spin text-primary" aria-hidden="true" />
          Preparing intake...
        </div>
      </main>
    )
  }

  if (contextState === 'missing' || contextState === 'error') {
    const isStaff = user?.role === 'staff'
    const primaryTarget = isStaff ? '/queue/same-day' : '/booking/history'
    const primaryLabel = isStaff ? 'Open same-day queue' : 'Choose appointment'

    return (
      <main
        className="flex min-h-screen items-center justify-center bg-background px-4 py-8 text-foreground"
        id="main-content"
      >
        <section className="w-full max-w-md rounded-xl border border-border bg-card p-6 text-center shadow-sm">
          <AlertCircle className="mx-auto h-10 w-10 text-amber-600" aria-hidden="true" />
          <h1 className="mt-3 text-lg font-semibold text-foreground">Appointment needed</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            {contextMessage ?? 'Select an appointment before completing intake.'}
          </p>
          <Link
            to={primaryTarget}
            className="mt-5 inline-flex w-full items-center justify-center rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
          >
            {primaryLabel}
          </Link>
        </section>
      </main>
    )
  }

  // -------------------------------------------------------------------------
  // Fallback: AI unavailable
  // -------------------------------------------------------------------------
  if (aiUnavailable) {
    return (
      <main
        className="flex min-h-screen flex-col items-center justify-center gap-6 bg-background px-4 text-foreground"
        id="main-content"
        aria-live="polite"
      >
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-sm text-center">
          <h1 className="text-lg font-semibold text-foreground">AI assistant unavailable</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            The AI assistant is temporarily unavailable. You can complete your intake using the
            manual form instead.
          </p>
          <button
            type="button"
            className="mt-4 w-full rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
            onClick={() => onSwitchToManual ? onSwitchToManual() : navigate(`/intake/manual${appointmentQuery}`)}
          >
            Use manual form
          </button>
        </div>
      </main>
    )
  }

  // -------------------------------------------------------------------------
  // Submitted confirmation
  // -------------------------------------------------------------------------
  if (view === 'submitted') {
    return (
      <main
        className="flex min-h-screen flex-col items-center justify-center gap-6 bg-background px-4 text-foreground"
        id="main-content"
        aria-live="polite"
      >
        <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-sm text-center">
          <h1 className="text-lg font-semibold text-foreground">Intake submitted</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Your intake information has been recorded. Your care team will review it before your
            appointment.
          </p>
          <button
            type="button"
            className="mt-4 w-full rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
            onClick={() => navigate(user?.role === 'staff' ? '/dashboard/staff' : '/dashboard/patient')}
          >
            Return to dashboard
          </button>
        </div>
      </main>
    )
  }

  // -------------------------------------------------------------------------
  // Summary view (AC-04)
  // -------------------------------------------------------------------------
  if (view === 'summary') {
    return (
      <main className="min-h-screen bg-background text-foreground" id="main-content">
        <div className="mx-auto max-w-2xl px-4 py-8">
          <div className="mb-6">
            <IntakeProgress completedCount={TOTAL_QUESTIONS} totalCount={TOTAL_QUESTIONS} />
          </div>
          {submitError && (
            <div
              className="mb-5 rounded-md border border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive"
              role="alert"
            >
              {submitError}
            </div>
          )}
          <IntakeSummary
            fields={buildExtractedFields(extractedData)}
            isSubmitting={isSubmitting}
            onConfirm={handleSubmit}
            onBack={() => setView('chat')}
          />
        </div>
      </main>
    )
  }

  // -------------------------------------------------------------------------
  // Chat view (AC-01, AC-02, AC-03)
  // -------------------------------------------------------------------------
  const currentQuestion = questionIndex < TOTAL_QUESTIONS ? INTAKE_QUESTIONS[questionIndex] : null

  return (
    <main
      className="flex min-h-screen flex-col bg-background text-foreground"
      id="main-content"
    >
      {/* Header */}
      <header className="flex items-center justify-between border-b border-border bg-card px-4 py-3">
        <div className="flex items-center gap-3">
          <button
            type="button"
            aria-label="Back to dashboard"
            className="rounded-md p-1 text-muted-foreground hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            onClick={() => navigate(user?.role === 'staff' ? '/dashboard/staff' : '/dashboard/patient')}
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.5"
              aria-hidden="true"
            >
              <polyline points="15 18 9 12 15 6" />
            </svg>
          </button>
          <h1 className="text-base font-semibold text-foreground">Pre-visit intake</h1>
        </div>

        {/* Mode toggle (AC-01 wireframe) */}
        <div className="flex items-center gap-1 rounded-lg border border-border bg-muted p-1" role="tablist">
          <span
            role="tab"
            aria-selected="true"
            className="rounded-md bg-card px-3 py-1 text-xs font-medium text-foreground shadow-sm"
          >
            AI assistant
          </span>
          <button
            type="button"
            role="tab"
            aria-selected="false"
            className="rounded-md px-3 py-1 text-xs font-medium text-muted-foreground hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            onClick={() => onSwitchToManual ? onSwitchToManual() : navigate(`/intake/manual${appointmentQuery}`)}
          >
            Manual form
          </button>
        </div>
      </header>

      {/* Progress bar (AC-03) */}
      <div className="border-b border-border bg-card px-4 py-2" aria-label="Intake progress">
        <IntakeProgress completedCount={completedAnswers} totalCount={TOTAL_QUESTIONS} />
      </div>

      {/* Two-column layout */}
      <div className="flex flex-1 overflow-hidden">
        {/* Chat panel */}
        <section className="flex flex-1 flex-col overflow-hidden border-r border-border">
          {/* Message list */}
          <div
            role="log"
            aria-label="Intake conversation"
            aria-live="polite"
            className="flex flex-1 flex-col gap-3 overflow-y-auto px-4 py-4"
          >
            {messages.map((msg) => (
              <ChatBubble key={msg.id} variant={msg.variant}>
                {msg.text}
              </ChatBubble>
            ))}
            {isAiTyping && <ChatBubble variant="ai" isTyping />}
            <div ref={messagesEndRef} />
          </div>

          {/* Suggestion chips (AC-02) */}
          {currentQuestion && !isAiTyping && (
            <div
              className="flex flex-wrap gap-2 px-4 pb-3"
              aria-label="Quick response suggestions"
            >
              {currentQuestion.chips.map((chip) => (
                <button
                  key={chip}
                  type="button"
                  className="inline-flex min-h-[32px] items-center rounded-full border border-border bg-background px-3 py-1 text-xs text-foreground transition-colors hover:border-primary hover:bg-primary/5 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                  onClick={() => handleUserResponse(chip)}
                >
                  {chip}
                </button>
              ))}
            </div>
          )}

          {/* Input area (AC-02) */}
          <div className="flex gap-2 border-t border-border bg-background px-4 py-3">
            <input
              ref={inputRef}
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="Type your response…"
              aria-label="Your message"
              disabled={isAiTyping || questionIndex >= TOTAL_QUESTIONS}
              className="flex-1 rounded-lg border border-input bg-card px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:border-ring focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            />
            <button
              type="button"
              aria-label="Send message"
              disabled={!inputValue.trim() || isAiTyping || questionIndex >= TOTAL_QUESTIONS}
              onClick={handleSend}
              className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden="true"
              >
                <line x1="22" y1="2" x2="11" y2="13" />
                <polygon points="22 2 15 22 11 13 2 9 22 2" />
              </svg>
            </button>
          </div>
        </section>

        {/* Extracted data panel (desktop only) */}
        <aside
          className="hidden w-[360px] shrink-0 flex-col md:flex"
          aria-label="Extracted clinical data"
        >
          <div className="border-b border-border px-4 py-3 text-sm font-semibold text-foreground">
            Extracted data
          </div>
          <div className="flex flex-1 flex-col gap-2 overflow-y-auto p-4">
            {buildExtractedFields(extractedData).map((field) => (
              <div
                key={field.label}
                className="rounded-lg border border-border bg-card p-3 shadow-sm"
              >
                <div className="mb-0.5 text-xs text-muted-foreground">{field.label}</div>
                <div className="text-sm font-medium text-foreground">
                  {field.value ?? (
                    <span className="italic text-muted-foreground">Awaiting response…</span>
                  )}
                </div>
                {field.confidence !== 'pending' && (
                  <div className="mt-1.5">
                    <span className="inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-xs font-medium text-green-700 bg-green-50 border border-green-200">
                      <svg
                        width="10"
                        height="10"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        aria-hidden="true"
                      >
                        <polyline points="20 6 9 17 4 12" />
                      </svg>
                      High confidence
                    </span>
                  </div>
                )}
              </div>
            ))}
          </div>
          <div className="border-t border-border p-4">
            <button
              type="button"
              disabled={completedAnswers < TOTAL_QUESTIONS}
              onClick={() => setView('summary')}
              className="w-full rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Review &amp; submit
            </button>
          </div>
        </aside>
      </div>
    </main>
  )
}
