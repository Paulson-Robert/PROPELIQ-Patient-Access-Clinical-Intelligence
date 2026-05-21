export type ChatBubbleVariant = 'ai' | 'user'

interface TypingIndicatorProps {
  label?: string
}

const TypingIndicator = ({ label = 'AI is thinking...' }: TypingIndicatorProps) => (
  <div
    className="flex gap-1 items-center py-1"
    aria-label={label}
    role="status"
  >
    {[0, 1, 2].map((i) => (
      <span
        key={i}
        className="block h-2 w-2 rounded-full bg-muted-foreground animate-bounce"
        style={{ animationDelay: `${i * 0.15}s` }}
        aria-hidden="true"
      />
    ))}
    <span className="sr-only">{label}</span>
  </div>
)

interface ChatBubbleProps {
  variant: ChatBubbleVariant
  children?: React.ReactNode
  isTyping?: boolean
}

export const ChatBubble = ({ variant, children, isTyping = false }: ChatBubbleProps) => {
  const isAi = variant === 'ai'

  const containerClasses = [
    'flex w-full',
    isAi ? 'justify-start' : 'justify-end',
  ].join(' ')

  const bubbleClasses = [
    'max-w-[75%] rounded-2xl px-4 py-3 text-sm leading-relaxed',
    isAi
      ? 'rounded-tl-sm bg-muted text-foreground'
      : 'rounded-tr-sm bg-primary text-primary-foreground',
  ].join(' ')

  return (
    <div className={containerClasses} data-uxr="SCR-009">
      <div className={bubbleClasses}>
        {isTyping ? <TypingIndicator /> : children}
      </div>
    </div>
  )
}
