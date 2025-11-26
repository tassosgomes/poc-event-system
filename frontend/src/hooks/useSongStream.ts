import { useCallback, useEffect, useRef, useState } from 'react'
import { songApi } from '../api/songApi'
import { appConfig } from '../config'
import type { Song } from '../types/song'

type ConnectionStatus = 'connecting' | 'connected' | 'disconnected'

export function useSongStream() {
  const [songs, setSongs] = useState<Song[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('connecting')
  const eventSourceRef = useRef<EventSource | null>(null)
  const retryTimeoutRef = useRef<number | null>(null)

  const upsertSong = useCallback((incoming: Song) => {
    setSongs((prev) => {
      const index = prev.findIndex((song) => song.id === incoming.id)
      if (index === -1) {
        return [incoming, ...prev]
      }

      const clone = [...prev]
      clone[index] = incoming
      return clone
    })
  }, [])

  const refresh = useCallback(async () => {
    try {
      setLoading(true)
      const data = await songApi.listSongs()
      setSongs(data)
      setError(null)
    } catch (err) {
      console.error('Failed to load songs', err)
      setError('Não foi possível carregar as músicas. Verifique a API.')
    } finally {
      setLoading(false)
    }
  }, [])

  const connectToStream = useCallback(() => {
    if (eventSourceRef.current) {
      eventSourceRef.current.close()
    }

    setConnectionStatus('connecting')
    const source = new EventSource(`${appConfig.apiBaseUrl}/songs/stream`)
    eventSourceRef.current = source

    source.onopen = () => {
      setConnectionStatus('connected')
      setError(null)
    }

    source.addEventListener('song-update', (event) => {
      try {
        const parsed: Song = JSON.parse(event.data)
        upsertSong(parsed)
      } catch (err) {
        console.error('Failed to parse SSE payload', err)
      }
    })

    source.onerror = () => {
      setConnectionStatus('disconnected')
      setError('Conexão em tempo real perdida. Tentando reconectar...')
      source.close()

      if (retryTimeoutRef.current) {
        window.clearTimeout(retryTimeoutRef.current)
      }

      retryTimeoutRef.current = window.setTimeout(() => {
        retryTimeoutRef.current = null
        connectToStream()
      }, 4000)
    }
  }, [upsertSong])

  useEffect(() => {
    refresh()
    connectToStream()

    return () => {
      eventSourceRef.current?.close()
      if (retryTimeoutRef.current) {
        window.clearTimeout(retryTimeoutRef.current)
      }
    }
  }, [connectToStream, refresh])

  return {
    songs,
    loading,
    error,
    refresh,
    connectionStatus,
  }
}
