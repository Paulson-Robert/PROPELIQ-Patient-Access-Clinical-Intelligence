import type { PatientSearchResult } from '../../services/bookingApi'

export const patientFromSearchParams = (
  searchParams: URLSearchParams,
): PatientSearchResult | null => {
  const patientId = searchParams.get('patientId')?.trim()
  if (!patientId) return null

  return {
    id: patientId,
    name: searchParams.get('patientName')?.trim() || 'Selected patient',
    email: searchParams.get('patientEmail')?.trim() || undefined,
    phone: searchParams.get('patientPhone')?.trim() || undefined,
  }
}

export const patientToSearchParams = (
  patient: PatientSearchResult | null,
): URLSearchParams => {
  const params = new URLSearchParams()
  if (!patient) return params

  params.set('patientId', patient.id)
  params.set('patientName', patient.name)
  if (patient.email) params.set('patientEmail', patient.email)
  if (patient.phone) params.set('patientPhone', patient.phone)

  return params
}

export const documentPathForPatient = (
  path: string,
  patient: PatientSearchResult | null,
): string => {
  const params = patientToSearchParams(patient)
  const query = params.toString()
  return query ? `${path}?${query}` : path
}
