import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { InsuranceForm } from '../../components/booking/InsuranceForm'

describe('InsuranceForm', () => {
  it('shows a non-blocking warning for unusual policy format', () => {
    const onChange = vi.fn()

    render(
      <InsuranceForm
        value={{ provider: 'Blue Cross Blue Shield', policyNumber: '!!bad' }}
        onChange={onChange}
      />,
    )

    expect(
      screen.getByText('Policy number format looks unusual. Example format: BCBS-882341.'),
    ).toBeInTheDocument()
  })

  it('does not show warning for valid provider and policy number', () => {
    const onChange = vi.fn()

    render(
      <InsuranceForm
        value={{ provider: 'Blue Cross Blue Shield', policyNumber: 'BCBS-882341' }}
        onChange={onChange}
      />,
    )

    expect(
      screen.queryByText('Policy number format looks unusual. Example format: BCBS-882341.'),
    ).not.toBeInTheDocument()
  })

  it('emits changed insurance values', () => {
    const onChange = vi.fn()

    render(
      <InsuranceForm
        value={{ provider: '', policyNumber: '' }}
        onChange={onChange}
      />,
    )

    fireEvent.change(screen.getByLabelText('Insurance provider'), {
      target: { value: 'Aetna' },
    })
    fireEvent.change(screen.getByLabelText('Policy number'), {
      target: { value: 'AET-12345' },
    })

    expect(onChange).toHaveBeenCalledWith({ provider: 'Aetna', policyNumber: '' })
    expect(onChange).toHaveBeenCalledWith({ provider: '', policyNumber: 'AET-12345' })
  })
})
