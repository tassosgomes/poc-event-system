import { NavLink, Outlet } from 'react-router-dom'

export function MainLayout() {
  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="app-title">Music Event System</div>
        <nav className="nav-links">
          <NavLink className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`} to="/songs">
            Listagem
          </NavLink>
          <NavLink className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`} to="/songs/new">
            Cadastro
          </NavLink>
        </nav>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  )
}
