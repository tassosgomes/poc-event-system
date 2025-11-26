import { Navigate, Route, Routes } from 'react-router-dom'
import { MainLayout } from './layout/MainLayout'
import { CreateSongPage } from './pages/CreateSongPage'
import { SongListPage } from './pages/SongListPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<MainLayout />}>
        <Route index element={<Navigate to="/songs" replace />} />
        <Route path="songs" element={<SongListPage />} />
        <Route path="songs/new" element={<CreateSongPage />} />
        <Route path="*" element={<Navigate to="/songs" replace />} />
      </Route>
    </Routes>
  )
}

export default App
