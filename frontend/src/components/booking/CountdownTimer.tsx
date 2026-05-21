import { useEffect, useRef, useState } from 'react'

interface CountdownTimerProps {
  totalSeconds: number
  secondsRemaining?: number
  size?: 'sm' | 'md'
  onExpire?: () => void
}

// SVG ring: r=42, circumference = 2 * π * 42 ≈ 263.9
const RADIUS = 42
const CIRCUMFERENCE = 2 * Math.PI * RADIUS

const SM_RADIUS = 13
const SM_CIRCUMFERENCE = 2 * Math.PI * SM_RADIUS

export const CountdownTimer = ({
  totalSeconds,
  secondsRemaining: externalSecondsRemaining,
  size = 'md',
  onExpire,
}: CountdownTimerProps) => {
  const isControlled = typeof externalSecondsRemaining === 'number'
  const [internalSeconds, setInternalSeconds] = useState(totalSeconds)
  const onExpireRef = useRef(onExpire)
  onExpireRef.current = onExpire

  useEffect(() => {
    if (isControlled) return

    setInternalSeconds(totalSeconds)

    const interval = window.setInterval(() => {
      setInternalSeconds((prev) => {
        if (prev <= 1) {
          window.clearInterval(interval)
          onExpireRef.current?.()
          return 0
        }
        return prev - 1
      })
    }, 1000)

    return () => window.clearInterval(interval)
  }, [totalSeconds, isControlled])

  const seconds = isControlled ? externalSecondsRemaining : internalSeconds
  const progress = Math.max(0, Math.min(1, seconds / totalSeconds))

  if (size === 'sm') {
    const dashOffset = SM_CIRCUMFERENCE * (1 - progress)

    return (
      <svg
        width="32"
        height="32"
        viewBox="0 0 32 32"
        aria-label={`${seconds} seconds remaining`}
        role="timer"
        data-uxr="UXR-502"
      >
        <circle
          cx="16"
          cy="16"
          r={SM_RADIUS}
          fill="none"
          stroke="hsl(var(--border))"
          strokeWidth="2.5"
        />
        <circle
          cx="16"
          cy="16"
          r={SM_RADIUS}
          fill="none"
          stroke="hsl(var(--primary))"
          strokeWidth="2.5"
          strokeDasharray={SM_CIRCUMFERENCE}
          strokeDashoffset={dashOffset}
          strokeLinecap="round"
          transform="rotate(-90 16 16)"
        />
      </svg>
    )
  }

  const dashOffset = CIRCUMFERENCE * (1 - progress)

  return (
    <div
      className="relative inline-flex items-center justify-center"
      data-uxr="UXR-502"
    >
      <svg
        width="96"
        height="96"
        viewBox="0 0 96 96"
        aria-label={`${seconds} seconds remaining`}
        role="timer"
      >
        <circle
          cx="48"
          cy="48"
          r={RADIUS}
          fill="none"
          stroke="hsl(var(--border))"
          strokeWidth="4"
        />
        <circle
          cx="48"
          cy="48"
          r={RADIUS}
          fill="none"
          stroke="hsl(var(--primary))"
          strokeWidth="4"
          strokeDasharray={CIRCUMFERENCE}
          strokeDashoffset={dashOffset}
          strokeLinecap="round"
          transform="rotate(-90 48 48)"
        />
      </svg>
      <span
        className="absolute text-xl font-semibold tabular-nums text-foreground"
        aria-hidden="true"
      >
        {seconds}s
      </span>
    </div>
  )
}
