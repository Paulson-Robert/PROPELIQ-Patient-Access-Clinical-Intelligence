import { StrictMode } from 'react'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { DocumentListPage } from '../../pages/documents/DocumentListPage'
import { DocumentUploadPage } from '../../pages/documents/DocumentUploadPage'
import { bookingApi } from '../../services/bookingApi'
import { AuthProvider } from '../../hooks/useAuth'
import App from '../../App'

describe('document upload flow', () => {
  afterEach(() => {
    sessionStorage.clear()
  })

  it('retains uploaded documents and lists them in the documents view', async () => {
    render(
      <StrictMode>
        <MemoryRouter initialEntries={['/documents/upload']}>
          <AuthProvider>
            <Routes>
              <Route path="/documents/upload" element={<DocumentUploadPage />} />
              <Route path="/documents" element={<DocumentListPage />} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      </StrictMode>,
    )

    const fileName = `visit-summary-${Date.now()}.pdf`
    const file = new File(['clinical summary'], fileName, { type: 'application/pdf' })
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement

    fireEvent.change(fileInput, { target: { files: [file] } })

    expect(
      await screen.findByLabelText(`${fileName}: Uploaded`),
    ).toBeInTheDocument()

    fireEvent.click(screen.getByRole('link', { name: /view all documents/i }))

    expect(
      await screen.findByRole('heading', { name: 'Documents' }),
    ).toBeInTheDocument()

    const fileCell = await screen.findByText(fileName)
    const row = fileCell.closest('tr')
    expect(row).not.toBeNull()
    expect(within(row as HTMLTableRowElement).getByText('PDF')).toBeInTheDocument()
    expect(within(row as HTMLTableRowElement).getByText('Completed')).toBeInTheDocument()
  })

  it('deletes uploaded documents through the API so they stay removed after refetch', async () => {
    const fileName = `delete-me-${Date.now()}.pdf`
    const file = new File(['delete this clinical summary'], fileName, { type: 'application/pdf' })
    await bookingApi.uploadDocument(file)

    const renderList = () =>
      render(
        <MemoryRouter initialEntries={['/documents']}>
          <AuthProvider>
            <Routes>
              <Route path="/documents" element={<DocumentListPage />} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>,
      )

    const firstRender = renderList()

    const fileCell = await screen.findByText(fileName)
    const row = fileCell.closest('tr')
    expect(row).not.toBeNull()

    fireEvent.click(
      within(row as HTMLTableRowElement).getByRole('button', { name: `Delete ${fileName}` }),
    )
    fireEvent.click(await screen.findByRole('button', { name: 'Delete document' }))

    await waitFor(() => {
      expect(screen.queryByText(fileName)).not.toBeInTheDocument()
    })

    firstRender.unmount()
    renderList()

    await waitFor(() => {
      expect(screen.queryByText(/Loading documents/)).not.toBeInTheDocument()
    })
    expect(screen.queryByText(fileName)).not.toBeInTheDocument()
  })

  it('lets staff select a patient and upload into that patient document list', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/login']}>
        <App />
      </MemoryRouter>,
    )

    fireEvent.click(await screen.findByRole('tab', { name: 'Create account' }))
    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: `staff-docs-${Date.now()}@example.com` },
    })
    fireEvent.click(screen.getByLabelText('Staff'))
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'Password123!' },
    })
    fireEvent.change(screen.getByLabelText('Confirm password'), {
      target: { value: 'Password123!' },
    })
    fireEvent.click(screen.getByLabelText(/I agree/i))
    fireEvent.click(screen.getByRole('button', { name: 'Create account' }))

    expect(await screen.findByRole('heading', { name: /Good morning/i })).toBeInTheDocument()

    fireEvent.click(screen.getAllByRole('link', { name: 'Documents' })[0])

    expect(await screen.findByRole('heading', { name: 'Documents' })).toBeInTheDocument()
    expect(screen.getByText('Select a patient')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Patient search'), {
      target: { value: 'Maria' },
    })
    fireEvent.click(await screen.findByRole('option', { name: /Maria Santos/i }))

    expect(await screen.findByText(/Viewing documents for/i)).toHaveTextContent('Maria Santos')

    fireEvent.click(screen.getByRole('link', { name: 'Upload' }))

    expect(await screen.findByRole('heading', { name: 'Upload documents' })).toBeInTheDocument()
    expect(screen.getByText(/Uploading for/i)).toHaveTextContent('Maria Santos')

    const fileName = `staff-upload-${Date.now()}.pdf`
    const file = new File(['staff clinical summary'], fileName, { type: 'application/pdf' })
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement

    fireEvent.change(fileInput, { target: { files: [file] } })

    expect(await screen.findByLabelText(`${fileName}: Uploaded`)).toBeInTheDocument()

    fireEvent.click(screen.getByRole('link', { name: /view all documents/i }))

    expect(await screen.findByRole('heading', { name: 'Documents' })).toBeInTheDocument()
    expect(await screen.findByText(fileName)).toBeInTheDocument()
  })
})
