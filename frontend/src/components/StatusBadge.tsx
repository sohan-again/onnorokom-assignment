const COLORS: Record<string, string> = {
  HELD: 'text-bg-warning',
  CONFIRMED: 'text-bg-success',
  EXPIRED: 'text-bg-secondary',
  CANCELLED: 'text-bg-danger',
}

export default function StatusBadge({ status }: { status: string }) {
  return <span className={'badge ' + (COLORS[status] ?? 'text-bg-light')}>{status}</span>
}
