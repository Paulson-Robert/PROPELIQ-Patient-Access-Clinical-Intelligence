import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { CodeMappingPage } from '../../pages/clinical/CodeMappingPage'

const renderPage = (patientId = 'pat-001') =>
  render(
    <MemoryRouter initialEntries={[`/clinical/patient/${patientId}/codes`]}>
      <Routes>
        <Route path="/clinical/patient/:patientId/codes" element={<CodeMappingPage />} />
      </Routes>
    </MemoryRouter>,
  )

describe('CodeMappingPage', () => {
  it('AC-01: renders page heading with patient name', () => {
    renderPage()
    expect(
      screen.getByRole('heading', { name: /code mapping — maria santos/i }),
    ).toBeInTheDocument()
  })

  it('AC-01: renders suggested codes table with column headers', () => {
    renderPage()
    expect(screen.getByRole('table', { name: /AI-suggested medical codes/i })).toBeInTheDocument()
    expect(screen.getByText('Code')).toBeInTheDocument()
    expect(screen.getByText('Confidence')).toBeInTheDocument()
    expect(screen.getByText('Actions')).toBeInTheDocument()
  })

  it('AC-01: renders all four mock suggestions', () => {
    renderPage()
    expect(screen.getByText('E11.9')).toBeInTheDocument()
    expect(screen.getByText('I10')).toBeInTheDocument()
    expect(screen.getByText('99213')).toBeInTheDocument()
    expect(screen.getByText('R51.9')).toBeInTheDocument()
  })

  it('AC-01: renders confidence indicators with scores', () => {
    renderPage()
    expect(screen.getByLabelText('High confidence: 96%')).toBeInTheDocument()
    expect(screen.getByLabelText('High confidence: 94%')).toBeInTheDocument()
    expect(screen.getByLabelText('Medium confidence: 72%')).toBeInTheDocument()
    expect(screen.getByLabelText('Low confidence: 45%')).toBeInTheDocument()
  })

  it('manual entry form is always present', () => {
    renderPage()
    expect(screen.getByRole('heading', { name: /add code manually/i })).toBeInTheDocument()
    expect(screen.getByLabelText('ICD-10 / CPT code')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Add' })).toBeInTheDocument()
  })

  it('back link points to patient view', () => {
    renderPage()
    const backLink = screen.getByRole('link', { name: /back to patient view/i })
    expect(backLink).toHaveAttribute('href', '/clinical/patient/pat-001')
  })
})
