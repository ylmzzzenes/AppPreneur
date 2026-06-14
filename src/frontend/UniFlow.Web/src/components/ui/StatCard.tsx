import type { ReactNode } from 'react';

interface StatCardProps {
  label: string;
  value: number | string;
  icon?: ReactNode;
  tone?: 'default' | 'success' | 'warning' | 'danger';
}

export function StatCard({ label, value, icon, tone = 'default' }: StatCardProps) {
  return (
    <article className={`stat-card stat-${tone}`}>
      <div className="stat-top">
        <span className="stat-label">{label}</span>
        {icon && <span className="stat-icon">{icon}</span>}
      </div>
      <strong className="stat-value">{value}</strong>
    </article>
  );
}
