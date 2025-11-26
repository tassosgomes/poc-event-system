import { useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { songApi } from '../api/songApi'
import type { CreateSongPayload } from '../types/song'
import { ErrorBanner } from './ErrorBanner'

const initialValues: CreateSongPayload = {
  title: '',
  artist: '',
  releaseDate: '',
  genre: '',
}

export function SongForm() {
  const [formValues, setFormValues] = useState<CreateSongPayload>(initialValues)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const navigate = useNavigate()

  const handleChange = (field: keyof CreateSongPayload) => (event: ChangeEvent<HTMLInputElement>) => {
    setFormValues((prev) => ({ ...prev, [field]: event.target.value }))
    if (error) {
      setError(null)
    }
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!formValues.title || !formValues.artist) {
      setError('Título e Artista são obrigatórios.')
      return
    }

    try {
      setSubmitting(true)
      setError(null)
      await songApi.createSong(formValues)
      setFormValues(initialValues)
      navigate('/songs')
    } catch (err) {
      console.error('Failed to create song', err)
      setError('Não foi possível cadastrar a música. Tente novamente.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="song-form" onSubmit={handleSubmit}>
      <div className="form-row">
        <div className="input-group">
          <label className="input-label" htmlFor="title">
            Título
          </label>
          <input
            id="title"
            name="title"
            type="text"
            className="input-control"
            placeholder="Ex.: Bohemian Rhapsody"
            value={formValues.title}
            onChange={handleChange('title')}
            required
          />
        </div>
        <div className="input-group">
          <label className="input-label" htmlFor="artist">
            Artista
          </label>
          <input
            id="artist"
            name="artist"
            type="text"
            className="input-control"
            placeholder="Ex.: Queen"
            value={formValues.artist}
            onChange={handleChange('artist')}
            required
          />
        </div>
      </div>

      <div className="form-row">
        <div className="input-group">
          <label className="input-label" htmlFor="releaseDate">
            Data de lançamento
          </label>
          <input
            id="releaseDate"
            name="releaseDate"
            type="date"
            className="input-control"
            value={formValues.releaseDate}
            onChange={handleChange('releaseDate')}
            required
          />
        </div>
        <div className="input-group">
          <label className="input-label" htmlFor="genre">
            Gênero
          </label>
          <input
            id="genre"
            name="genre"
            type="text"
            className="input-control"
            placeholder="Ex.: Rock"
            value={formValues.genre}
            onChange={handleChange('genre')}
            required
          />
        </div>
      </div>

      {error && <ErrorBanner message={error} />}

      <div>
        <button type="submit" className="button primary" disabled={submitting}>
          {submitting ? 'Enviando...' : 'Cadastrar música'}
        </button>
      </div>
    </form>
  )
}
