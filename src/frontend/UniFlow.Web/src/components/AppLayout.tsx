import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

const NAV = [
  { to: '/dashboard', label: 'Bugün', icon: '📅' },
  { to: '/tasks', label: 'Görevler', icon: '✅' },
  { to: '/courses', label: 'Dersler', icon: '📚' },
  { to: '/chat', label: 'Sohbet', icon: '💬' },
  { to: '/syllabus', label: 'Müfredat', icon: '📄' },
  { to: '/study-plan', label: 'Çalışma Planı', icon: '🧠' },
];

export function AppLayout() {
  const { profile, logout } = useAuth();
  const navigate = useNavigate();
  const initial = profile?.displayName?.charAt(0)?.toUpperCase() ?? '?';

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="brand-logo">U</span>
          <div>
            <strong>UniFlow</strong>
            <small>Akademik planlama</small>
          </div>
        </div>
        <nav className="sidebar-nav">
          {NAV.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
            >
              <span className="nav-icon">{item.icon}</span>
              {item.label}
            </NavLink>
          ))}
        </nav>
        <button type="button" className="nav-item profile-link" onClick={() => navigate('/profile')}>
          <span className="avatar">{initial}</span>
          <span>
            <strong>{profile?.displayName ?? 'Profil'}</strong>
            <small>{profile?.email}</small>
          </span>
        </button>
      </aside>

      <div className="main-panel">
        <header className="topbar">
          <div />
          <button type="button" className="btn btn-ghost" onClick={logout}>
            Çıkış
          </button>
        </header>
        <main className="page-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
