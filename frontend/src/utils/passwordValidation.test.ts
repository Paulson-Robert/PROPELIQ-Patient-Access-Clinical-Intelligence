import { describe, expect, it } from 'vitest'
import {
  isPasswordComplexityValid,
  validatePasswordComplexity,
} from './passwordValidation'

describe('passwordValidation', () => {
  it('fails each requirement when password is weak', () => {
    const results = validatePasswordComplexity('abc')

    expect(results.find((item) => item.id === 'length')?.passed).toBe(false)
    expect(results.find((item) => item.id === 'uppercase')?.passed).toBe(false)
    expect(results.find((item) => item.id === 'digit')?.passed).toBe(false)
    expect(results.find((item) => item.id === 'special')?.passed).toBe(false)
  })

  it('passes when password meets complexity requirements', () => {
    expect(isPasswordComplexityValid('StrongPass1!')).toBe(true)
  })

  it('fails when one requirement is missing', () => {
    expect(isPasswordComplexityValid('StrongPass12')).toBe(false)
  })
})
