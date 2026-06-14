import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { IconSparkle } from './ui/Icons';

interface AuthShellProps {
  children: ReactNode;
  wide?: boolean;
}

export function AuthShell({ children, wide }: AuthShellProps) {
  return (
    <div className="auth-layout">
      <aside className="auth-hero">
        <div className="auth-hero-inner">
          <Link to="/login" className="auth-brand">
            <span className="brand-mark">U</span>
            <div>
              <strong>UniFlow</strong>
              <span>Akademik planlama platformu</span>
            </div>
          </Link>
          <h2>Üniversite hayatınızı akıllıca planlayın</h2>
          <p>AI destekli görev yönetimi, müfredat tarama, haftalık özet ve kişisel çalışma planı — tek yerde.</p>
          <ul className="auth-features">
            <li><IconSparkle size={16} /> Günlük kişisel AI mesajları</li>
            <li><IconSparkle size={16} /> Müfredattan otomatik görev çıkarma</li>
            <li><IconSparkle size={16} /> Sarkastik Dahi ile sohbet</li>
          </ul>
        </div>
      </aside>
      <main className={`auth-main${wide ? ' auth-main-wide' : ''}`}>{children}</main>
    </div>
  );
}
