const PADDING = { top: 12, right: 8, bottom: 28, left: 36 }

export type ChartDataPoint = {
  label: string
  value: number
}

export interface TrendChartProps {
  type: 'line' | 'bar'
  data: ChartDataPoint[]
  title: string
  ariaLabel?: string
  height?: number
  valueFormatter?: (v: number) => string
}

const buildTicks = (max: number, count = 4): number[] => {
  if (max === 0) return [0]
  const step = Math.ceil(max / count)
  return Array.from({ length: count + 1 }, (_, i) => i * step)
}

export const TrendChart = ({
  type,
  data,
  title,
  ariaLabel,
  height = 200,
  valueFormatter = (v) => String(v),
}: TrendChartProps) => {
  const chartId = `chart-${title.replace(/\s+/g, '-').toLowerCase()}`

  if (data.length === 0) {
    return (
      <figure className="rounded-xl border border-border bg-card p-5 shadow-sm" aria-label={ariaLabel ?? title}>
        <figcaption className="mb-3 text-sm font-medium text-foreground">{title}</figcaption>
        <div
          className="flex items-center justify-center text-sm text-muted-foreground"
          style={{ height }}
          role="status"
          aria-live="polite"
        >
          No data available
        </div>
      </figure>
    )
  }

  const w = 500
  const h = height
  const innerW = w - PADDING.left - PADDING.right
  const innerH = h - PADDING.top - PADDING.bottom

  const maxVal = Math.max(...data.map((d) => d.value), 1)
  const ticks = buildTicks(maxVal)
  const gridMax = ticks[ticks.length - 1]

  const toX = (i: number) => PADDING.left + (i / (data.length - 1 || 1)) * innerW
  const toY = (v: number) => PADDING.top + innerH - (v / gridMax) * innerH

  const barWidth = Math.max(4, innerW / data.length - 4)

  return (
    <figure
      className="rounded-xl border border-border bg-card p-5 shadow-sm"
      aria-label={ariaLabel ?? title}
    >
      <figcaption className="mb-3 text-sm font-medium text-foreground">{title}</figcaption>
      <svg
        viewBox={`0 0 ${w} ${h}`}
        width="100%"
        height={h}
        role="img"
        aria-labelledby={chartId}
      >
        <title id={chartId}>{ariaLabel ?? title}</title>

        {/* Y-axis grid lines and labels */}
        {ticks.map((tick) => {
          const y = toY(tick)
          return (
            <g key={tick}>
              <line
                x1={PADDING.left}
                y1={y}
                x2={w - PADDING.right}
                y2={y}
                className="stroke-border"
                strokeWidth={1}
                strokeDasharray="3 3"
              />
              <text
                x={PADDING.left - 4}
                y={y + 4}
                textAnchor="end"
                className="fill-muted-foreground text-[10px]"
                style={{ fontSize: 10 }}
              >
                {valueFormatter(tick)}
              </text>
            </g>
          )
        })}

        {/* X-axis labels (first, middle, last) */}
        {[0, Math.floor((data.length - 1) / 2), data.length - 1]
          .filter((i, idx, arr) => arr.indexOf(i) === idx)
          .map((i) => (
            <text
              key={i}
              x={type === 'bar' ? PADDING.left + (i / data.length) * innerW + barWidth / 2 : toX(i)}
              y={h - 4}
              textAnchor="middle"
              className="fill-muted-foreground"
              style={{ fontSize: 10 }}
            >
              {data[i].label}
            </text>
          ))}

        {type === 'line' && (
          <>
            {/* Area fill */}
            <polyline
              points={[
                ...data.map((d, i) => `${toX(i)},${toY(d.value)}`),
                `${toX(data.length - 1)},${toY(0)}`,
                `${toX(0)},${toY(0)}`,
              ].join(' ')}
              className="fill-primary/10"
              stroke="none"
            />
            {/* Line */}
            <polyline
              points={data.map((d, i) => `${toX(i)},${toY(d.value)}`).join(' ')}
              fill="none"
              className="stroke-primary"
              strokeWidth={2}
              strokeLinejoin="round"
              strokeLinecap="round"
            />
            {/* Dots */}
            {data.map((d, i) => (
              <circle
                key={i}
                cx={toX(i)}
                cy={toY(d.value)}
                r={3}
                className="fill-primary stroke-card"
                strokeWidth={1.5}
              />
            ))}
          </>
        )}

        {type === 'bar' && (
          <>
            {data.map((d, i) => {
              const x = PADDING.left + (i / data.length) * innerW + (innerW / data.length - barWidth) / 2
              const barH = (d.value / gridMax) * innerH
              return (
                <rect
                  key={i}
                  x={x}
                  y={toY(d.value)}
                  width={barWidth}
                  height={barH}
                  rx={2}
                  className="fill-primary/80"
                />
              )
            })}
          </>
        )}
      </svg>
    </figure>
  )
}
