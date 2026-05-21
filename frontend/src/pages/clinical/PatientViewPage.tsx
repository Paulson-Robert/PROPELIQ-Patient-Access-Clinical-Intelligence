import { useState } from 'react'
import { ArrowLeft, AlertTriangle } from 'lucide-react'
import { Link } from 'react-router-dom'
import { cn } from '../../lib/utils'
import { DataCategoryCard } from '../../components/clinical/DataCategoryCard'
import { VerificationBadge, type VerificationStatus } from '../../components/clinical/VerificationBadge'
import { RiskTierBadge } from '../../components/clinical/RiskTierBadge'
import type { RiskLevel } from '../../services/queueApi'

// --- Types ---

type RiskTier = 'low' | 'medium' | 'high'
type DataSource = 'EHR' | 'Intake' | 'Insurance' | 'Lab'

interface PatientDemographics {
  id: string
  name: string
  initials: string
  dob: string
  age: number
  sex: string
  mrn: string
  insurance: {
    provider: string
    memberId: string
    verificationStatus: VerificationStatus
  }
  riskTier: RiskTier
}

interface VitalEntry {
  label: string
  value: string
  source: DataSource
  hasConflict?: boolean
}

interface MedicationEntry {
  id: string
  name: string
  dose: string
  frequency: string
  prescriber: string
  source: DataSource
}

interface AllergyEntry {
  id: string
  allergen: string
  reaction: string
  severity: 'mild' | 'moderate' | 'severe'
  source: DataSource
}

interface DiagnosisEntry {
  id: string
  code: string
  description: string
  onset: string
  source: DataSource
}

interface ProcedureEntry {
  id: string
  code: string
  description: string
  date: string
  source: DataSource
}

// --- Mock data (AC-01: aggregated from multiple sources — EHR, Intake, Insurance) ---
// Inferred decision: No patient API endpoint is in scope for this task (frontend only).
// Static mock data is used to demonstrate 360° aggregation per SCR-019.
const MOCK_PATIENT: PatientDemographics = {
  id: 'pat-001',
  name: 'Maria Santos',
  initials: 'MS',
  dob: 'Mar 15, 1988',
  age: 36,
  sex: 'Female',
  mrn: 'PAT-001',
  insurance: {
    provider: 'Blue Cross Blue Shield',
    memberId: 'BCBS-882341',
    verificationStatus: 'verified',
  },
  riskTier: 'high',
}

const MOCK_VITALS: VitalEntry[] = [
  { label: 'Blood pressure', value: '142/88 mmHg', source: 'EHR', hasConflict: true },
  { label: 'Heart rate', value: '78 bpm', source: 'EHR' },
  { label: 'Temperature', value: '98.6 °F', source: 'EHR' },
  { label: 'SpO₂', value: '97%', source: 'EHR' },
  { label: 'Weight', value: '165 lbs', source: 'Intake' },
  { label: 'Height', value: '5′ 6″', source: 'EHR' },
]

const MOCK_MEDICATIONS: MedicationEntry[] = [
  {
    id: 'med-001',
    name: 'Lisinopril',
    dose: '10 mg',
    frequency: 'Once daily',
    prescriber: 'Dr. Chen',
    source: 'EHR',
  },
  {
    id: 'med-002',
    name: 'Metformin',
    dose: '500 mg',
    frequency: 'Twice daily',
    prescriber: 'Dr. Patel',
    source: 'EHR',
  },
]

const MOCK_ALLERGIES: AllergyEntry[] = [
  { id: 'alg-001', allergen: 'Penicillin', reaction: 'Hives', severity: 'moderate', source: 'EHR' },
  { id: 'alg-002', allergen: 'Sulfa drugs', reaction: 'Rash', severity: 'mild', source: 'Intake' },
]

const MOCK_DIAGNOSES: DiagnosisEntry[] = [
  { id: 'dx-001', code: 'E11.9', description: 'Type 2 diabetes mellitus', onset: 'Jan 2019', source: 'EHR' },
  { id: 'dx-002', code: 'I10', description: 'Essential hypertension', onset: 'Mar 2021', source: 'EHR' },
]

const MOCK_PROCEDURES: ProcedureEntry[] = []

// --- Helpers ---

// Map PatientViewPage's lowercase RiskTier to the canonical RiskLevel type
const RISK_TIER_MAP: Record<RiskTier, RiskLevel> = {
  low: 'Low',
  medium: 'Medium',
  high: 'High',
}

const SEVERITY_CONFIG: Record<AllergyEntry['severity'], string> = {
  mild: 'text-emerald-700',
  moderate: 'text-amber-700',
  severe: 'text-destructive',
}

type TabKey = 'vitals' | 'medications' | 'allergies' | 'diagnoses' | 'procedures'

const TABS: { key: TabKey; label: string }[] = [
  { key: 'vitals', label: 'Vitals' },
  { key: 'medications', label: 'Medications' },
  { key: 'allergies', label: 'Allergies' },
  { key: 'diagnoses', label: 'Diagnoses' },
  { key: 'procedures', label: 'Procedures' },
]

// --- Sub-components ---

const VitalsPanel = () => (
  <DataCategoryCard title="Vitals — Last recorded Jan 20, 2025" isEmpty={MOCK_VITALS.length === 0}>
    <dl className="grid grid-cols-2 gap-4 sm:grid-cols-3">
      {MOCK_VITALS.map((vital) => (
        <div key={vital.label}>
          <dt className="text-xs text-muted-foreground">{vital.label}</dt>
          <dd className="font-medium text-foreground">{vital.value}</dd>
          {vital.hasConflict ? (
            <span className="mt-0.5 inline-flex items-center gap-1 text-xs text-amber-600">
              <AlertTriangle className="h-3 w-3" aria-hidden="true" />
              Conflict
            </span>
          ) : (
            <span className="text-xs text-muted-foreground">Source: {vital.source}</span>
          )}
        </div>
      ))}
    </dl>
  </DataCategoryCard>
)

const MedicationsPanel = () => (
  <DataCategoryCard title="Medications" isEmpty={MOCK_MEDICATIONS.length === 0}>
    <ul className="divide-y divide-border" aria-label="Medication list">
      {MOCK_MEDICATIONS.map((med) => (
        <li key={med.id} className="flex items-start justify-between gap-4 py-3 first:pt-0 last:pb-0">
          <div>
            <p className="text-sm font-medium text-foreground">{med.name}</p>
            <p className="text-xs text-muted-foreground">
              {med.dose} · {med.frequency} · {med.prescriber}
            </p>
          </div>
          <span className="shrink-0 text-xs text-muted-foreground">Source: {med.source}</span>
        </li>
      ))}
    </ul>
  </DataCategoryCard>
)

const AllergiesPanel = () => (
  <DataCategoryCard title="Allergies" isEmpty={MOCK_ALLERGIES.length === 0}>
    <ul className="divide-y divide-border" aria-label="Allergy list">
      {MOCK_ALLERGIES.map((allergy) => (
        <li key={allergy.id} className="flex items-start justify-between gap-4 py-3 first:pt-0 last:pb-0">
          <div>
            <p className="text-sm font-medium text-foreground">{allergy.allergen}</p>
            <p className="text-xs text-muted-foreground">Reaction: {allergy.reaction}</p>
          </div>
          <div className="flex shrink-0 flex-col items-end gap-1">
            <span className={cn('text-xs font-medium capitalize', SEVERITY_CONFIG[allergy.severity])}>
              {allergy.severity}
            </span>
            <span className="text-xs text-muted-foreground">Source: {allergy.source}</span>
          </div>
        </li>
      ))}
    </ul>
  </DataCategoryCard>
)

const DiagnosesPanel = () => (
  <DataCategoryCard title="Diagnoses" isEmpty={MOCK_DIAGNOSES.length === 0}>
    <ul className="divide-y divide-border" aria-label="Diagnosis list">
      {MOCK_DIAGNOSES.map((dx) => (
        <li key={dx.id} className="flex items-start justify-between gap-4 py-3 first:pt-0 last:pb-0">
          <div>
            <p className="text-sm font-medium text-foreground">{dx.description}</p>
            <p className="text-xs text-muted-foreground">
              ICD-10: {dx.code} · Onset: {dx.onset}
            </p>
          </div>
          <span className="shrink-0 text-xs text-muted-foreground">Source: {dx.source}</span>
        </li>
      ))}
    </ul>
  </DataCategoryCard>
)

const ProceduresPanel = () => (
  // Edge case: MOCK_PROCEDURES is empty — shows "No records found" via isEmpty prop
  <DataCategoryCard title="Procedures" isEmpty={MOCK_PROCEDURES.length === 0}>
    <ul className="divide-y divide-border" aria-label="Procedure list">
      {MOCK_PROCEDURES.map((proc) => (
        <li key={proc.id} className="flex items-start justify-between gap-4 py-3 first:pt-0 last:pb-0">
          <div>
            <p className="text-sm font-medium text-foreground">{proc.description}</p>
            <p className="text-xs text-muted-foreground">
              {proc.code} · {proc.date}
            </p>
          </div>
          <span className="shrink-0 text-xs text-muted-foreground">Source: {proc.source}</span>
        </li>
      ))}
    </ul>
  </DataCategoryCard>
)

const TAB_PANELS: Record<TabKey, JSX.Element> = {
  vitals: <VitalsPanel />,
  medications: <MedicationsPanel />,
  allergies: <AllergiesPanel />,
  diagnoses: <DiagnosesPanel />,
  procedures: <ProceduresPanel />,
}

// --- Page ---

export const PatientViewPage = () => {
  const [activeTab, setActiveTab] = useState<TabKey>('vitals')
  const patient = MOCK_PATIENT

  return (
    <main className="min-h-screen bg-background text-foreground" id="main-content">
      <a
        href="#patient-data"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground"
      >
        Skip to patient data
      </a>

      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
        {/* Page header */}
        <header className="mb-6 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <Link
              to="/queue/same-day"
              className="inline-flex items-center justify-center rounded-md p-1.5 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              aria-label="Back to queue"
            >
              <ArrowLeft className="h-5 w-5" aria-hidden="true" />
            </Link>
            <h1 className="text-xl font-semibold tracking-tight">Patient view</h1>
          </div>
        </header>

        {/* AC-01 + AC-03: Demographics — patient identity section */}
        <section
          className="mb-6 flex flex-wrap items-center gap-4 rounded-xl border border-border bg-card p-4"
          aria-label="Patient demographics"
        >
          {/* Avatar */}
          <div
            className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-primary/10 text-lg font-semibold text-primary"
            aria-hidden="true"
          >
            {patient.initials}
          </div>

          {/* Details */}
          <div className="min-w-0 flex-1">
            <p className="text-base font-semibold text-foreground">{patient.name}</p>
            <p className="text-sm text-muted-foreground">
              DOB: {patient.dob} ({patient.age} y/o) · {patient.sex} · MRN: {patient.mrn}
            </p>
            {/* AC-02: insurance verification badge */}
            <p className="mt-1 flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
              <span>
                Insurance: {patient.insurance.provider} — {patient.insurance.memberId}
              </span>
              <VerificationBadge status={patient.insurance.verificationStatus} />
            </p>
          </div>

          {/* Risk tier — AC-03: RiskTierBadge in patient view */}
          <RiskTierBadge
            tier={RISK_TIER_MAP[patient.riskTier]}
            className="shrink-0 px-3 py-1 text-xs font-semibold"
          />
        </section>

        {/* Category tabs — AC-03: data grouped by category */}
        <div id="patient-data">
        <div
          role="tablist"
          aria-label="Patient data categories"
          className="mb-4 flex gap-1 overflow-x-auto rounded-lg border border-border bg-muted/30 p-1"
        >
          {TABS.map(({ key, label }) => (
            <button
              key={key}
              role="tab"
              aria-selected={activeTab === key}
              aria-controls={`tabpanel-${key}`}
              id={`tab-${key}`}
              type="button"
              onClick={() => setActiveTab(key)}
              className={cn(
                'whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-ring',
                activeTab === key
                  ? 'bg-background text-foreground shadow-sm'
                  : 'text-muted-foreground hover:text-foreground',
              )}
            >
              {label}
            </button>
          ))}
        </div>

        {/* Tab panel — AC-01: aggregated data from all sources */}
        <div
          id={`tabpanel-${activeTab}`}
          role="tabpanel"
          aria-labelledby={`tab-${activeTab}`}
        >
          {TAB_PANELS[activeTab]}
        </div>
        </div>
      </div>
    </main>
  )
}
