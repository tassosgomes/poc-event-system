import { Link } from 'react-router-dom'
import { SongForm } from '../components/SongForm'

export function CreateSongPage() {
  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Cadastrar música</h1>
          <p className="page-subtitle">Preencha os dados para iniciar o enriquecimento.</p>
        </div>
        <Link className="button secondary" to="/songs">
          Voltar para listagem
        </Link>
      </div>

      <div className="card">
        <SongForm />
      </div>
    </div>
  )
}
