export type SongStatus = 'PENDING' | 'COMPLETED' | 'NOT_FOUND'

export interface Song {
  id: string
  title: string
  artist: string
  releaseDate: string
  genre: string
  videoUrl?: string | null
  views?: number | null
  status: SongStatus
}

export interface CreateSongPayload {
  title: string
  artist: string
  releaseDate: string
  genre: string
}
