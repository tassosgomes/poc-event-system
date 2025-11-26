import { Link } from 'react-router-dom'
import { ConnectionIndicator } from '../components/ConnectionIndicator'
import { ErrorBanner } from '../components/ErrorBanner'
import { LoadingState } from '../components/LoadingState'
import { SongTable } from '../components/SongTable'
import { useSongStream } from '../hooks/useSongStream'

export function SongListPage() {
  const { songs, loading, error, refresh, connectionStatus } = useSongStream()

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Músicas cadastradas</h1>
          <p className="page-subtitle">Acompanhe o processamento em tempo real.</p>
        </div>
        <div className="table-actions">
          <ConnectionIndicator status={connectionStatus} />
          <Link className="button primary" to="/songs/new">
            Nova música
          </Link>
        </div>
      </div>

      {error && <ErrorBanner message={error} onAction={refresh} />}

      {loading ? <LoadingState message="Carregando lista..." /> : <SongTable songs={songs} />}
    </div>
  )
}
