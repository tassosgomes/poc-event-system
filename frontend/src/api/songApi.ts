import type { CreateSongPayload, Song } from '../types/song'
import { httpClient } from './httpClient'

export const songApi = {
  async listSongs() {
    const { data } = await httpClient.get<Song[]>('/songs')
    return data
  },
  async createSong(payload: CreateSongPayload) {
    const { data } = await httpClient.post<Song>('/songs', payload)
    return data
  },
}
