import type { SongStatus } from '../types/song'

const statusLabel: Record<SongStatus, string> = {
  PENDING: 'Processando',
  COMPLETED: 'Concluída',
  NOT_FOUND: 'Não encontrado',
}

const statusClass: Record<SongStatus, string> = {
  PENDING: 'pending',
  COMPLETED: 'completed',
  NOT_FOUND: 'not-found',
}

interface Props {
  status: SongStatus
}

export function StatusBadge({ status }: Props) {
  return <span className={`status-badge ${statusClass[status]}`}>{statusLabel[status]}</span>
}
