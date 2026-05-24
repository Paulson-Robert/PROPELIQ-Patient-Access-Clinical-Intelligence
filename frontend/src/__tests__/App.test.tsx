import { fireEvent, render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import App from '../App'

describe('App', () => {
  it('renders the authentication entry title', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/login']}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByRole('heading', { level: 2, name: 'Welcome back' }),
    ).toBeInTheDocument()
  })

  it('does not render social sign-in options', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/login']}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByRole('heading', { level: 2, name: 'Welcome back' }),
    ).toBeInTheDocument()
    expect(
      screen.queryByRole('button', { name: 'Continue with Google' }),
    ).not.toBeInTheDocument()
    expect(
      screen.queryByRole('button', { name: 'Continue with Microsoft' }),
    ).not.toBeInTheDocument()
  })

  it('registers staff users into the staff dashboard', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/login']}>
        <App />
      </MemoryRouter>,
    )

    fireEvent.click(await screen.findByRole('tab', { name: 'Create account' }))
    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'new.user@example.com' },
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

    expect(
      await screen.findByRole('heading', { name: /Good morning/i }),
    ).toBeInTheDocument()
  })

  it('routes staff users into MFA verification after login', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/login']}>
        <App />
      </MemoryRouter>,
    )

    fireEvent.change(screen.getByLabelText('Email address'), {
      target: { value: 'staff@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'Password123!' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Log in' }))

    expect(
      await screen.findByRole('heading', { name: 'Verify your identity' }),
    ).toBeInTheDocument()
  })

  it('renders the MFA setup screen', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/mfa/setup?email=first.admin@example.com&role=admin&method=totp']}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByRole('heading', { name: 'Set up two-factor authentication' }),
    ).toBeInTheDocument()
  })

  it('shows a lockout message after too many failed attempts', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/login?session=terminated']}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByText('Too many failed attempts - please log in again.'),
    ).toBeInTheDocument()
  })

  it('navigates to password reset from forgot password action', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/login']}>
        <App />
      </MemoryRouter>,
    )

    fireEvent.click(await screen.findByRole('button', { name: 'Forgot password?' }))

    expect(
      await screen.findByRole('heading', { level: 2, name: 'Reset your password' }),
    ).toBeInTheDocument()
  })

  it('renders the password reset route directly', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/password-reset']}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByRole('button', { name: 'Send verification code' }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Reset password' })).toBeDisabled()
  })

  it('renders intake history inside the app navigation shell', async () => {
    render(
      <MemoryRouter initialEntries={['/intake/history']}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByRole('heading', { name: 'Intake history' }),
    ).toBeInTheDocument()
    expect(screen.getByRole('navigation', { name: 'Main navigation' })).toBeInTheDocument()
  })

  it('renders documents inside the app navigation shell', async () => {
    render(
      <MemoryRouter initialEntries={['/documents']}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByRole('heading', { name: 'Documents' }),
    ).toBeInTheDocument()
    expect(screen.getByRole('navigation', { name: 'Main navigation' })).toBeInTheDocument()
  })

  it.each([
    ['/queue/same-day', 'Same-day queue'],
    ['/booking/walk-in', 'Walk-in booking'],
    ['/clinical/patient/pat-001', 'Patient view'],
    ['/clinical/patient/pat-001/codes', /Code mapping/i],
  ])('renders %s inside the app navigation shell', async (path, headingName) => {
    render(
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByRole('heading', { name: headingName }),
    ).toBeInTheDocument()
    expect(screen.getByRole('navigation', { name: 'Main navigation' })).toBeInTheDocument()
    expect(document.querySelectorAll('#main-content')).toHaveLength(1)
  })
})
