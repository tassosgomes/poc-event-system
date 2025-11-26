import type { Song } from '../types/song'
import { formatReleaseDate, formatViews } from '../utils/formatters'
import { StatusBadge } from './StatusBadge'
import { VideoPreview } from './VideoPreview'

interface Props {
  songs: Song[]
}

export function SongTable({ songs }: Props) {
  if (!songs.length) {
    return (
      <div className="empty-state">
        <strong>Nenhuma música cadastrada ainda.</strong>
        <p>Use a tela de cadastro para adicionar a primeira música.</p>
      </div>
    )
  }

  return (
    <div className="songs-table-wrapper">
      <table className="songs-table">
        <thead>
          <tr>
            <th>Título</th>
            <th>Artista</th>
            <th>Lançamento</th>
            <th>Gênero</th>
            <th>Status</th>
            <th>Vídeo</th>
            <th>Visualizações</th>
          </tr>
        </thead>
        <tbody>
          {songs.map((song) => (
            <tr key={song.id}>
              <td>{song.title}</td>
              <td>{song.artist}</td>
              <td>{formatReleaseDate(song.releaseDate)}</td>
              <td>{song.genre}</td>
              <td>
                <StatusBadge status={song.status} />
              </td>
              <td>
                {song.status === 'NOT_FOUND' ? (
                  <span>Não localizado</span>
                ) : (
                  <VideoPreview videoUrl={song.videoUrl ?? undefined} />
                )}
              </td>
              <td>{formatViews(song.views)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
