import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { AlertCircle, ChevronLeft, CheckCircle, Loader2 } from 'lucide-react'
import { IntakeStepper } from '../../components/intake/IntakeStepper'
import { useAuth } from '../../hooks/useAuth'
import {
  BookingError,
  bookingApi,
  type AppointmentRecord,
  type ManualIntakeDraft,
  type ManualIntakeDraftPayload,
  type ManualIntakeSubmitPayload,
} from '../../services/bookingApi'

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

const DRAFT_KEY = 'intake-manual-draft'
const DRAFT_SAVE_DELAY_MS = 500

const STEP_LABELS = ['Medical history', 'Symptoms', 'Medications', 'Allergies', 'Reason for visit']

const SEVERITY_OPTIONS = [
  { value: '', label: 'Select severity' },
  { value: '1', label: '1 — Minimal' },
  { value: '2', label: '2' },
  { value: '3', label: '3' },
  { value: '4', label: '4' },
  { value: '5', label: '5 — Moderate' },
  { value: '6', label: '6' },
  { value: '7', label: '7' },
  { value: '8', label: '8' },
  { value: '9', label: '9' },
  { value: '10', label: '10 — Severe' },
]

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface IntakeFormData {
  chronicConditions: string
  pastSurgeries: string
  familyHistory: string
  symptoms: string
  symptomOnset: string
  symptomSeverity: string
  currentMedications: string
  knownAllergies: string
  reasonForVisit: string
}

type IntakeContextState = 'loading' | 'ready' | 'missing' | 'error'
type DraftSaveState = 'idle' | 'saving' | 'saved' | 'error'

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

const EMPTY_FORM: IntakeFormData = {
  chronicConditions: '',
  pastSurgeries: '',
  familyHistory: '',
  symptoms: '',
  symptomOnset: '',
  symptomSeverity: '',
  currentMedications: '',
  knownAllergies: '',
  reasonForVisit: '',
}

const readDraft = (key = DRAFT_KEY): IntakeFormData | null => {
  try {
    const raw = localStorage.getItem(key)
    if (!raw) return null
    const parsed = JSON.parse(raw) as unknown
    if (typeof parsed !== 'object' || parsed === null) return null
    return parsed as IntakeFormData
  } catch {
    return null
  }
}

const draftKeyForAppointment = (appointmentId: string | null): string =>
  appointmentId ? `${DRAFT_KEY}:${appointmentId}` : DRAFT_KEY

const hasAnyFormData = (data: IntakeFormData): boolean =>
  Object.values(data).some((value) => value.trim().length > 0)

const fromDraftDto = (draft: ManualIntakeDraft): IntakeFormData => ({
  chronicConditions: draft.chronicConditions ?? '',
  pastSurgeries: draft.pastSurgeries ?? '',
  familyHistory: draft.familyHistory ?? '',
  symptoms: draft.symptomsDescription ?? '',
  symptomOnset: draft.symptomOnset ?? '',
  symptomSeverity: draft.symptomSeverity ?? '',
  currentMedications: draft.currentMedications ?? '',
  knownAllergies: draft.knownAllergies ?? '',
  reasonForVisit: draft.reasonForVisit ?? '',
})

const toDraftPayload = (data: IntakeFormData): ManualIntakeDraftPayload => ({
  chronicConditions: data.chronicConditions,
  pastSurgeries: data.pastSurgeries,
  familyHistory: data.familyHistory,
  symptomsDescription: data.symptoms,
  symptomOnset: data.symptomOnset,
  symptomSeverity: data.symptomSeverity,
  currentMedications: data.currentMedications,
  knownAllergies: data.knownAllergies,
  reasonForVisit: data.reasonForVisit,
})

const toSubmitPayload = (data: IntakeFormData): ManualIntakeSubmitPayload => ({
  ...toDraftPayload(data),
  reasonForVisit: data.reasonForVisit.trim(),
})

const pickDefaultAppointment = (appointments: AppointmentRecord[]): AppointmentRecord | null =>
  appointments
    .filter((appointment) => appointment.status === 'Scheduled')
    .sort((a, b) => `${a.date}T${a.startTime}`.localeCompare(`${b.date}T${b.startTime}`))[0] ?? null

// ---------------------------------------------------------------------------
// Shared field/label styles
// ---------------------------------------------------------------------------

const LABEL_CLASS = 'text-xs font-medium text-muted-foreground'

const INPUT_CLASS =
  'mt-1 min-h-[44px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring'

const TEXTAREA_CLASS =
  'mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring resize-y'

const TEXTAREA_ERROR_CLASS =
  'mt-1 w-full rounded-md border border-destructive bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring resize-y'

const SELECT_CLASS =
  'mt-1 min-h-[44px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring'

// ---------------------------------------------------------------------------
// Step panels
// ---------------------------------------------------------------------------

interface StepProps {
  data: IntakeFormData
  onChange: <K extends keyof IntakeFormData>(key: K, value: IntakeFormData[K]) => void
  validationError: string | null
}

const MedicalHistoryStep = ({ data, onChange }: StepProps) => (
  <section aria-labelledby="step-heading" className="flex flex-col gap-5">
    <div className="flex flex-col">
      <label htmlFor="chronicConditions" className={LABEL_CLASS}>
        Chronic conditions
      </label>
      <input
        id="chronicConditions"
        type="text"
        className={INPUT_CLASS}
        value={data.chronicConditions}
        onChange={(e) => onChange('chronicConditions', e.target.value)}
        placeholder="e.g., Diabetes, Asthma"
        autoComplete="off"
      />
    </div>

    <div className="flex flex-col">
      <label htmlFor="pastSurgeries" className={LABEL_CLASS}>
        Past surgeries
      </label>
      <textarea
        id="pastSurgeries"
        className={TEXTAREA_CLASS}
        rows={3}
        value={data.pastSurgeries}
        onChange={(e) => onChange('pastSurgeries', e.target.value)}
        placeholder="e.g., Appendectomy (2018)"
      />
    </div>

    <div className="flex flex-col">
      <label htmlFor="familyHistory" className={LABEL_CLASS}>
        Family history
      </label>
      <textarea
        id="familyHistory"
        className={TEXTAREA_CLASS}
        rows={3}
        value={data.familyHistory}
        onChange={(e) => onChange('familyHistory', e.target.value)}
        placeholder="e.g., Father — heart disease, Mother — diabetes"
      />
    </div>
  </section>
)

const SymptomsStep = ({ data, onChange }: StepProps) => (
  <section aria-labelledby="step-heading" className="flex flex-col gap-5">
    <div className="flex flex-col">
      <label htmlFor="symptoms" className={LABEL_CLASS}>
        Describe your symptoms
      </label>
      <textarea
        id="symptoms"
        className={TEXTAREA_CLASS}
        rows={4}
        value={data.symptoms}
        onChange={(e) => onChange('symptoms', e.target.value)}
        placeholder="e.g., Persistent headaches for the past 2 weeks, mild dizziness"
      />
    </div>

    <div className="grid gap-4 sm:grid-cols-2">
      <div className="flex flex-col">
        <label htmlFor="symptomOnset" className={LABEL_CLASS}>
          When did symptoms start?
        </label>
        <input
          id="symptomOnset"
          type="date"
          className={INPUT_CLASS}
          value={data.symptomOnset}
          onChange={(e) => onChange('symptomOnset', e.target.value)}
        />
      </div>

      <div className="flex flex-col">
        <label htmlFor="symptomSeverity" className={LABEL_CLASS}>
          Severity (1–10)
        </label>
        <select
          id="symptomSeverity"
          className={SELECT_CLASS}
          value={data.symptomSeverity}
          onChange={(e) => onChange('symptomSeverity', e.target.value)}
        >
          {SEVERITY_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
      </div>
    </div>
  </section>
)

const MedicationsStep = ({ data, onChange }: StepProps) => (
  <section aria-labelledby="step-heading" className="flex flex-col gap-5">
    <div className="flex flex-col">
      <label htmlFor="currentMedications" className={LABEL_CLASS}>
        Current medications
      </label>
      <textarea
        id="currentMedications"
        className={TEXTAREA_CLASS}
        rows={4}
        value={data.currentMedications}
        onChange={(e) => onChange('currentMedications', e.target.value)}
        placeholder="e.g., Metformin 500mg twice daily"
      />
    </div>
  </section>
)

const AllergiesStep = ({ data, onChange }: StepProps) => (
  <section aria-labelledby="step-heading" className="flex flex-col gap-5">
    <div className="flex flex-col">
      <label htmlFor="knownAllergies" className={LABEL_CLASS}>
        Known allergies
      </label>
      <input
        id="knownAllergies"
        type="text"
        className={INPUT_CLASS}
        value={data.knownAllergies}
        onChange={(e) => onChange('knownAllergies', e.target.value)}
        placeholder="e.g., Penicillin, Shellfish, Latex"
        autoComplete="off"
      />
    </div>
  </section>
)

const ReasonForVisitStep = ({ data, onChange, validationError }: StepProps) => (
  <section aria-labelledby="step-heading" className="flex flex-col gap-5">
    <div className="flex flex-col">
      <label htmlFor="reasonForVisit" className={LABEL_CLASS}>
        Primary reason{' '}
        <span className="text-destructive" aria-hidden="true">
          *
        </span>
      </label>
      <textarea
        id="reasonForVisit"
        className={validationError ? TEXTAREA_ERROR_CLASS : TEXTAREA_CLASS}
        rows={4}
        required
        value={data.reasonForVisit}
        onChange={(e) => onChange('reasonForVisit', e.target.value)}
        placeholder="e.g., Annual physical exam, follow-up on blood work results"
        aria-required="true"
        aria-describedby={validationError ? 'reason-error' : undefined}
        aria-invalid={validationError ? 'true' : undefined}
      />
      {validationError && (
        <p id="reason-error" role="alert" className="mt-1.5 text-xs text-destructive">
          {validationError}
        </p>
      )}
    </div>
  </section>
)

const STEP_HEADINGS = [
  'Medical history',
  'Current symptoms',
  'Medications',
  'Allergies',
  'Reason for visit',
]

const STEP_DESCRIPTIONS = [
  'Share your chronic conditions, past surgeries, and family medical history.',
  'Describe your current symptoms, when they started, and their severity.',
  'List your current prescription and over-the-counter medications.',
  'List any known allergies to medications, foods, or environmental factors.',
  'Tell us the primary reason for your visit today.',
]

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

interface ManualIntakePageProps {
  /** When provided, the AI assistant toggle calls this callback instead of
   *  navigating to /intake/ai — used by the parent IntakePage (AC-01). */
  onSwitchToAi?: () => void
}

export const ManualIntakePage = ({ onSwitchToAi }: ManualIntakePageProps = {}) => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { user } = useAuth()
  const queryAppointmentId = searchParams.get('appointmentId')

  const [currentStep, setCurrentStep] = useState(0)
  const [formData, setFormData] = useState<IntakeFormData>(() => readDraft() ?? EMPTY_FORM)
  const [appointmentId, setAppointmentId] = useState<string | null>(queryAppointmentId)
  const [contextState, setContextState] = useState<IntakeContextState>(
    queryAppointmentId ? 'ready' : 'loading',
  )
  const [contextMessage, setContextMessage] = useState<string | null>(null)
  const [draftLoaded, setDraftLoaded] = useState(false)
  const [hasUserEdited, setHasUserEdited] = useState(false)
  const [draftSaveState, setDraftSaveState] = useState<DraftSaveState>('idle')
  const [validationError, setValidationError] = useState<string | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSubmitted, setIsSubmitted] = useState(false)

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
      setContextMessage('Open intake from a patient row in the same-day queue so the form is tied to an appointment.')
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

  useEffect(() => {
    if (!appointmentId || contextState !== 'ready') return

    let cancelled = false

    const loadDraft = async () => {
      setDraftLoaded(false)
      setDraftSaveState('idle')

      try {
        const serverDraft = await bookingApi.getManualIntakeDraft(appointmentId)
        if (cancelled) return

        const localDraft =
          readDraft(draftKeyForAppointment(appointmentId)) ??
          readDraft() ??
          EMPTY_FORM

        setFormData(serverDraft ? fromDraftDto(serverDraft) : localDraft)
        setHasUserEdited(false)
      } catch {
        if (cancelled) return
        const localDraft =
          readDraft(draftKeyForAppointment(appointmentId)) ??
          readDraft() ??
          EMPTY_FORM

        setFormData(localDraft)
        setHasUserEdited(false)
        setDraftSaveState('error')
      } finally {
        if (!cancelled) setDraftLoaded(true)
      }
    }

    void loadDraft()

    return () => {
      cancelled = true
    }
  }, [appointmentId, contextState])

  // Keep a local fallback draft while the API-backed draft is the source of truth.
  useEffect(() => {
    localStorage.setItem(draftKeyForAppointment(appointmentId), JSON.stringify(formData))
  }, [appointmentId, formData])

  useEffect(() => {
    if (!appointmentId || !draftLoaded || !hasUserEdited || isSubmitted || !hasAnyFormData(formData)) return

    const timer = window.setTimeout(() => {
      setDraftSaveState('saving')
      void bookingApi
        .saveManualIntakeDraft(appointmentId, toDraftPayload(formData))
        .then(() => setDraftSaveState('saved'))
        .catch(() => setDraftSaveState('error'))
    }, DRAFT_SAVE_DELAY_MS)

    return () => window.clearTimeout(timer)
  }, [appointmentId, draftLoaded, formData, hasUserEdited, isSubmitted])

  const updateField = useCallback(
    <K extends keyof IntakeFormData>(key: K, value: IntakeFormData[K]) => {
      setFormData((prev) => ({ ...prev, [key]: value }))
      setHasUserEdited(true)
      if (key === 'reasonForVisit') setValidationError(null)
      setSubmitError(null)
    },
    [],
  )

  const validateCurrentStep = (): boolean => {
    if (currentStep === 4 && !formData.reasonForVisit.trim()) {
      setValidationError('Primary reason for visit is required.')
      return false
    }
    return true
  }

  const handleNext = () => {
    if (!validateCurrentStep()) return
    setCurrentStep((prev) => prev + 1)
    setValidationError(null)
  }

  const handleBack = () => {
    setCurrentStep((prev) => prev - 1)
    setValidationError(null)
  }

  const handleSubmit = useCallback(async () => {
    if (!formData.reasonForVisit.trim()) {
      setValidationError('Primary reason for visit is required.')
      return
    }

    if (!appointmentId) {
      setSubmitError('Choose an appointment before submitting intake.')
      return
    }

    setIsSubmitting(true)
    setSubmitError(null)

    try {
      await bookingApi.submitManualIntake(appointmentId, toSubmitPayload(formData))
      localStorage.removeItem(DRAFT_KEY)
      localStorage.removeItem(draftKeyForAppointment(appointmentId))
      setDraftSaveState('saved')
      setIsSubmitting(false)
      setIsSubmitted(true)
    } catch (error) {
      const message =
        error instanceof BookingError
          ? error.message
          : 'Unable to submit intake right now. Please try again.'

      setSubmitError(message)
      setIsSubmitting(false)
    }
  }, [appointmentId, formData])

  const handleFormSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (currentStep < 4) {
      handleNext()
    } else {
      void handleSubmit()
    }
  }

  if (isSubmitted) {
    return (
      <main
        className="flex min-h-screen items-center justify-center bg-background px-4 py-8"
        id="main-content"
        data-uxr="SCR-010"
      >
        <section
          className="w-full max-w-md rounded-xl border border-border bg-card p-8 text-center shadow-sm"
          aria-labelledby="success-heading"
        >
          <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-green-100">
            <CheckCircle className="h-6 w-6 text-green-600" aria-hidden="true" />
          </div>
          <h1 id="success-heading" className="text-lg font-semibold text-foreground">
            Intake submitted
          </h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Your pre-visit intake form has been submitted successfully.
          </p>
          <button
            type="button"
            className="mt-6 w-full rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
            onClick={() => navigate(user?.role === 'staff' ? '/dashboard/staff' : '/dashboard/patient')}
          >
            Back to dashboard
          </button>
        </section>
      </main>
    )
  }

  if (contextState === 'loading' || (contextState === 'ready' && !draftLoaded)) {
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

  const stepProps: StepProps = {
    data: formData,
    onChange: updateField,
    validationError,
  }
  const appointmentQuery = appointmentId ? `?appointmentId=${encodeURIComponent(appointmentId)}` : ''
  const aiHref = `/intake/ai${appointmentQuery}`

  return (
    <div
      className="flex min-h-screen flex-col bg-background"
      data-uxr="SCR-010"
    >
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
      >
        Skip to main content
      </a>

      <header className="border-b border-border bg-background px-6 py-4">
        <div className="mx-auto flex max-w-2xl items-center gap-3">
          <button
            type="button"
            onClick={() => navigate(-1)}
            aria-label="Go back"
            className="flex h-9 w-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          >
            <ChevronLeft className="h-5 w-5" aria-hidden="true" />
          </button>

          <h1 className="text-lg font-semibold text-foreground">Pre-visit intake</h1>

          <nav
            className="ml-auto flex items-center gap-1 rounded-lg border border-border bg-background p-1"
            aria-label="Intake mode"
          >
            {onSwitchToAi ? (
              <button
                type="button"
                className="rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                onClick={onSwitchToAi}
              >
                AI assistant
              </button>
            ) : (
              <a
                href={aiHref}
                className="rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              >
                AI assistant
              </a>
            )}
            <span
              className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground"
              aria-current="page"
            >
              Manual form
            </span>
          </nav>
        </div>
      </header>

      <main
        className="flex flex-1 flex-col items-center px-4 py-8"
        id="main-content"
      >
        <div className="w-full max-w-2xl">
          <IntakeStepper steps={STEP_LABELS} currentStep={currentStep} />

          <div className="mt-8 rounded-xl border border-border bg-card p-6 shadow-sm">
            <div className="mb-6">
              <h2
                id="step-heading"
                className="text-base font-semibold text-foreground"
              >
                {STEP_HEADINGS[currentStep]}
              </h2>
              <p className="mt-1 text-sm text-muted-foreground">
                {STEP_DESCRIPTIONS[currentStep]}
              </p>
              {draftSaveState === 'saving' && (
                <p className="mt-2 text-xs text-muted-foreground" role="status">
                  Saving draft...
                </p>
              )}
              {draftSaveState === 'saved' && (
                <p className="mt-2 text-xs text-emerald-700" role="status">
                  Draft saved
                </p>
              )}
              {draftSaveState === 'error' && (
                <p className="mt-2 text-xs text-amber-700" role="status">
                  Draft is saved locally. Server sync will retry when you make another change.
                </p>
              )}
            </div>

            <form onSubmit={handleFormSubmit} noValidate aria-labelledby="step-heading">
              {submitError && (
                <div
                  className="mb-5 rounded-md border border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive"
                  role="alert"
                >
                  {submitError}
                </div>
              )}
              {currentStep === 0 && <MedicalHistoryStep {...stepProps} />}
              {currentStep === 1 && <SymptomsStep {...stepProps} />}
              {currentStep === 2 && <MedicationsStep {...stepProps} />}
              {currentStep === 3 && <AllergiesStep {...stepProps} />}
              {currentStep === 4 && <ReasonForVisitStep {...stepProps} />}

              <div className="mt-8 flex gap-3">
                {currentStep > 0 && (
                  <button
                    type="button"
                    onClick={handleBack}
                    className="flex-1 rounded-lg border border-border bg-background px-4 py-2.5 text-sm font-medium text-foreground transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                  >
                    Back
                  </button>
                )}

                {currentStep < 4 ? (
                  <button
                    type="submit"
                    className="flex-1 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                  >
                    Next
                  </button>
                ) : (
                  <button
                    type="submit"
                    disabled={isSubmitting}
                    aria-busy={isSubmitting}
                    className="flex-1 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {isSubmitting ? 'Submitting…' : 'Submit intake'}
                  </button>
                )}
              </div>
            </form>
          </div>
        </div>
      </main>
    </div>
  )
}
