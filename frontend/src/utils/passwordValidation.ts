export interface PasswordRequirement {
  id: 'length' | 'uppercase' | 'lowercase' | 'digit' | 'special'
  label: string
  validator: (value: string) => boolean
}

export interface PasswordRequirementResult {
  id: PasswordRequirement['id']
  label: string
  passed: boolean
}

const HAS_UPPERCASE = /[A-Z]/
const HAS_LOWERCASE = /[a-z]/
const HAS_DIGIT = /\d/
const HAS_SPECIAL = /[^A-Za-z0-9]/

export const passwordRequirements: PasswordRequirement[] = [
  {
    id: 'length',
    label: 'At least 8 characters',
    validator: (value) => value.length >= 8,
  },
  {
    id: 'uppercase',
    label: 'Contains an uppercase letter',
    validator: (value) => HAS_UPPERCASE.test(value),
  },
  {
    id: 'lowercase',
    label: 'Contains a lowercase letter',
    validator: (value) => HAS_LOWERCASE.test(value),
  },
  {
    id: 'digit',
    label: 'Contains a number',
    validator: (value) => HAS_DIGIT.test(value),
  },
  {
    id: 'special',
    label: 'Contains a special character',
    validator: (value) => HAS_SPECIAL.test(value),
  },
]

export const validatePasswordComplexity = (
  value: string,
): PasswordRequirementResult[] =>
  passwordRequirements.map((requirement) => ({
    id: requirement.id,
    label: requirement.label,
    passed: requirement.validator(value),
  }))

export const isPasswordComplexityValid = (value: string): boolean =>
  validatePasswordComplexity(value).every((item) => item.passed)
