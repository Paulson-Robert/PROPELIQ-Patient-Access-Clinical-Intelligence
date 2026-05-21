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

  it('renders social sign-in options', async () => {
    render(
      <MemoryRouter initialEntries={['/auth/login']}>
        <App />
      </MemoryRouter>,
    )

    expect(
      await screen.findByRole('button', { name: 'Continue with Google' }),
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
      await screen.findByText('Too many failed attempts — please log in again.'),
    ).toBeInTheDocument()
  })
})
