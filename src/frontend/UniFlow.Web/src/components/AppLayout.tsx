import { useState } from 'react';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import {
  IconBrain,
  IconChat,
  IconCourses,
  IconDashboard,
  IconLogout,
  IconMenu,
  IconSyllabus,
  IconTasks,
} from './ui/Icons';

const NAV = [
  { to: '/dashboard', label: 'Bugün', Icon: IconDashboard },
  { to: '/tasks', label: 'Görevler', Icon: IconTasks },
  { to: '/courses', label: 'Dersler', Icon: IconCourses },
  { to: '/chat', label: 'Sohbet', Icon: IconChat },
  { to: '/syllabus', label: 'Müfredat', Icon: IconSyllabus },
  { to: '/study-plan', label: 'Çalışma Planı', Icon: IconBrain },
];

const PAGE_TITLES: Record<string, string> = {
  '/dashboard': 'Bugün',
  '/tasks': 'Görevler',
  '/courses': 'Dersler',
  '/chat': 'Sohbet',
  '/syllabus': 'Müfredat',
  '/study-plan': 'Çalışma Planı',
  '/profile': 'Profil',
};

export function AppLayout() {
  const { profile, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);
  const initial = profile?.displayName?.charAt(0)?.toUpperCase() ?? '?';

  const pageTitle = Object.entries(PAGE_TITLES).find(([path]) => location.pathname.startsWith(path))?.[1] ?? 'UniFlow';

  function closeMenu() {
    setMenuOpen(false);
  }

  return (
    <div className="app-layout">
      <div className={`sidebar-overlay${menuOpen ? ' open' : ''}`} onClick={closeMenu} aria-hidden />
      <aside className={`sidebar${menuOpen ? ' open' : ''}`}>
        <div className="sidebar-brand">
          <span className="brand-logo">U</span>
          <div>
            <strong>UniFlow</strong>
            <small>Akademik planlama</small>
          </div>
        </div>
        <nav className="sidebar-nav">
          {NAV.map(({ to, label, Icon }) => (
            <NavLink
              key={to}
              to={to}
              onClick={closeMenu}
              className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
            >
              <span className="nav-icon"><Icon size={18} /></span>
              {label}
            </NavLink>
          ))}
        </nav>
        <button type="button" className="nav-item profile-link" onClick={() => { closeMenu(); navigate('/profile'); }}>
          <span className="avatar">{initial}</span>
          <span>
            <strong>{profile?.displayName ?? 'Profil'}</strong>
            <small>{profile?.email}</small>
          </span>
        </button>
      </aside>

      <div className="main-panel">
        <header className="topbar">
          <div className="topbar-actions">
            <button type="button" className="btn btn-ghost btn-sm mobile-menu-btn" onClick={() => setMenuOpen(true)} aria-label="Menü">
              <IconMenu size={20} />
            </button>
            <span className="topbar-title">{pageTitle}</span>
          </div>
          <button type="button" className="btn btn-ghost btn-sm" onClick={logout}>
            <IconLogout size={16} /> Çıkış
          </button>
        </header>
        <main className="page-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
