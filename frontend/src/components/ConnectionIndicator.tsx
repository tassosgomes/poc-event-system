type Status = 'connecting' | 'connected' | 'disconnected'

const labels: Record<Status, string> = {
  connecting: 'Conectando SSE',
  connected: 'SSE ativo',
  disconnected: 'SSE desconectado',
}

interface Props {
  status: Status
}

export function ConnectionIndicator({ status }: Props) {
  return (
    <span className="connection-indicator">
      <span className={`indicator-dot ${status}`} />
      {labels[status]}
    </span>
  )
}
