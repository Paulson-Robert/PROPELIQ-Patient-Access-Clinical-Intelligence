import { render } from '@testing-library/react'
import { axe } from 'vitest-axe'
import { describe, it, expect } from 'vitest'
import { InlineError } from '../components/shared/InlineError'
import { ErrorBanner, type NetworkError } from '../components/shared/ErrorBanner'
import '../test/a11y/axe-setup'

describe('Accessibility: InlineError', () => {
  it('has no axe violations', async () => {
    const { container } = render(
      <InlineError
        id="test-error"
        message="This field is required"
        suggestion="Please enter a value"
      />,
    )

    const results = await axe(container)
    expect(results).toHaveNoViolations()
  })
})

describe('Accessibility: ErrorBanner', () => {
  const errors: NetworkError[] = [
    { id: '1', message: 'Network connection lost', severity: 'critical', retryable: true },
    { id: '2', message: 'Request timed out', severity: 'warning', retryable: true },
  ]

  it('has no axe violations with multiple errors', async () => {
    const { container } = render(
      <ErrorBanner
        errors={errors}
        onRetry={() => {}}
        onDismiss={() => {}}
      />,
    )

    const results = await axe(container)
    expect(results).toHaveNoViolations()
  })

  it('has no axe violations when empty', async () => {
    const { container } = render(<ErrorBanner errors={[]} />)

    const results = await axe(container)
    expect(results).toHaveNoViolations()
  })
})
