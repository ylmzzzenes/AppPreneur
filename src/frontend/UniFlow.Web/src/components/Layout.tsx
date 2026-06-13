import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function Layout() {
  const { logout } = useAuth();

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="brand">UniFlow</div>
        <nav className="nav-links">
          <NavLink to="/dashboard" className={({ isActive }) => (isActive ? 'active' : '')}>
            Bugün
          </NavLink>
          <NavLink to="/tasks" className={({ isActive }) => (isActive ? 'active' : '')}>
            Görevler
          </NavLink>
          <NavLink to="/syllabus" className={({ isActive }) => (isActive ? 'active' : '')}>
            Müfredat
          </NavLink>
        </nav>
        <button type="button" className="btn-secondary" onClick={logout}>
          Çıkış
        </button>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}
