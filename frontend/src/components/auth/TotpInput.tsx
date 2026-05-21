import { useEffect, useMemo, useRef, type ClipboardEvent } from 'react'

interface TotpInputProps {
  value: string
  onChange: (value: string) => void
  length?: number
  disabled?: boolean
  label?: string
}

export const TotpInput = ({
  value,
  onChange,
  length = 6,
  disabled = false,
  label = 'One-time password',
}: TotpInputProps) => {
  const inputsRef = useRef<Array<HTMLInputElement | null>>([])

  const digits = useMemo(
    () => Array.from({ length }, (_, index) => value[index] ?? ''),
    [length, value],
  )

  useEffect(() => {
    inputsRef.current = inputsRef.current.slice(0, length)
  }, [length])

  const focusInput = (index: number) => {
    inputsRef.current[index]?.focus()
  }

  const updateValue = (index: number, nextDigit: string) => {
    const nextValue = digits.slice()
    nextValue[index] = nextDigit
    onChange(nextValue.join(''))
  }

  const handlePaste = (event: ClipboardEvent<HTMLInputElement>) => {
    event.preventDefault()

    const pastedDigits = event.clipboardData.getData('text').replace(/\D/g, '').slice(0, length)
    if (!pastedDigits) {
      return
    }

    const nextValue = Array.from({ length }, (_, index) => pastedDigits[index] ?? digits[index] ?? '')
    onChange(nextValue.join(''))
    focusInput(Math.min(pastedDigits.length, length - 1))
  }

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between gap-3 text-sm">
        <span className="font-medium">{label}</span>
        <span className="text-muted-foreground">
          {value.length}/{length}
        </span>
      </div>

      <div className="flex justify-between gap-2" role="group" aria-label={label}>
        {digits.map((digit, index) => (
          <input
            key={index}
            ref={(element) => {
              inputsRef.current[index] = element
            }}
            type="text"
            inputMode="numeric"
            autoComplete={index === 0 ? 'one-time-code' : 'off'}
            maxLength={1}
            value={digit}
            onChange={(event) => {
              const nextDigit = event.target.value.replace(/\D/g, '').slice(-1)
              updateValue(index, nextDigit)

              if (nextDigit && index < length - 1) {
                focusInput(index + 1)
              }
            }}
            onKeyDown={(event) => {
              if (event.key === 'Backspace' && !digit && index > 0) {
                focusInput(index - 1)
              }

              if (event.key === 'ArrowLeft' && index > 0) {
                focusInput(index - 1)
              }

              if (event.key === 'ArrowRight' && index < length - 1) {
                focusInput(index + 1)
              }
            }}
            onPaste={handlePaste}
            disabled={disabled}
            className="h-12 w-11 rounded-md border border-input bg-background text-center text-base font-semibold tracking-[0.4em] focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 disabled:cursor-not-allowed disabled:opacity-60"
            aria-label={`Digit ${index + 1}`}
          />
        ))}
      </div>
    </div>
  )
}