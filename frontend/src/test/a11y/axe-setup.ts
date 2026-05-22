import type { AxeResults } from 'axe-core'
import * as matchers from 'vitest-axe/matchers'
import { expect } from 'vitest'

// Extend vitest matchers with axe accessibility matchers
expect.extend(matchers)

// Module augmentation for vitest-axe custom matchers
declare module 'vitest' {
  // eslint-disable-next-line @typescript-eslint/no-empty-object-type
  interface Assertion<T> extends matchers.AxeMatchers {}
  // eslint-disable-next-line @typescript-eslint/no-empty-object-type
  interface AsymmetricMatchersContaining extends matchers.AxeMatchers {}
}

/**
 * Default axe-core configuration for CI testing.
 * Runs WCAG 2.1 Level AA checks with best-practices.
 */
export const axeConfig = {
  rules: {
    // Ensure colour contrast meets WCAG 2.1 AA
    'color-contrast': { enabled: true },
    // Ensure all interactive elements are keyboard accessible
    'keyboard-navigation': { enabled: true },
  },
  runOnly: {
    type: 'tag' as const,
    values: ['wcag2a', 'wcag2aa', 'best-practice'],
  },
}

/**
 * Helper to format axe violations into readable console output for CI.
 */
export function formatViolations(results: AxeResults): string {
  if (results.violations.length === 0) return 'No accessibility violations found.'

  return results.violations
    .map((violation) => {
      const nodes = violation.nodes
        .map((node) => `    - ${node.html}`)
        .join('\n')
      return `[${violation.impact}] ${violation.id}: ${violation.description}\n  Help: ${violation.helpUrl}\n  Elements:\n${nodes}`
    })
    .join('\n\n')
}
