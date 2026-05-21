import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ChevronLeft, CheckCircle } from 'lucide-react'
import { IntakeStepper } from '../../components/intake/IntakeStepper'

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

const DRAFT_KEY = 'intake-manual-draft'

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

const readDraft = (): IntakeFormData | null => {
  try {
    const raw = localStorage.getItem(DRAFT_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as unknown
    if (typeof parsed !== 'object' || parsed === null) return null
    return parsed as IntakeFormData
  } catch {
    return null
  }
}

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

export const ManualIntakePage = () => {
  const navigate = useNavigate()

  const [currentStep, setCurrentStep] = useState(0)
  const [formData, setFormData] = useState<IntakeFormData>(() => readDraft() ?? EMPTY_FORM)
  const [validationError, setValidationError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSubmitted, setIsSubmitted] = useState(false)

  // AC-03 + Edge case: Save draft to localStorage on every change.
  // localStorage.setItem is synchronous, so data persists even on browser close.
  useEffect(() => {
    localStorage.setItem(DRAFT_KEY, JSON.stringify(formData))
  }, [formData])

  const updateField = useCallback(
    <K extends keyof IntakeFormData>(key: K, value: IntakeFormData[K]) => {
      setFormData((prev) => ({ ...prev, [key]: value }))
      if (key === 'reasonForVisit') setValidationError(null)
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

  const handleSubmit = useCallback(() => {
    if (!formData.reasonForVisit.trim()) {
      setValidationError('Primary reason for visit is required.')
      return
    }
    setIsSubmitting(true)
    // Inferred decision: no intake submission API endpoint defined in task scope.
    // Simulating async submission and clearing draft on success.
    // logged: implement-tasks:Step3 | decision | ManualIntakePage.tsx | UC-010 | Simulated intake submission with 600ms delay; no backend endpoint specified in task or upstream spec.
    setTimeout(() => {
      localStorage.removeItem(DRAFT_KEY)
      setIsSubmitting(false)
      setIsSubmitted(true)
    }, 600)
  }, [formData.reasonForVisit])

  const handleFormSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (currentStep < 4) {
      handleNext()
    } else {
      handleSubmit()
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
            onClick={() => navigate('/dashboard/patient')}
          >
            Back to dashboard
          </button>
        </section>
      </main>
    )
  }

  const stepProps: StepProps = {
    data: formData,
    onChange: updateField,
    validationError,
  }

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
            <a
              href="/intake/ai"
              className="rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              AI assistant
            </a>
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
            </div>

            <form onSubmit={handleFormSubmit} noValidate aria-labelledby="step-heading">
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
