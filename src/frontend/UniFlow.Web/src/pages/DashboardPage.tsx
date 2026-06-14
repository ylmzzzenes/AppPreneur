import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { aiApi, dashboardApi, getErrorMessage, tasksApi } from '../api/services';
import type { DashboardTodayResponse, TaskItemStatus, WeeklySummaryResponse } from '../api/types';
import { EmptyState } from '../components/EmptyState';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { AiBadge, StatusBadge } from '../components/ui/Badge';
import { StatCard } from '../components/ui/StatCard';
import { IconSparkle } from '../components/ui/Icons';
import { useToast } from '../context/ToastContext';
import { tryShowTaskFeedback } from '../utils/taskFeedback';

export function DashboardPage() {
  const navigate = useNavigate();
  const { profile } = useAuth();
  const { showToast } = useToast();
  const [dashboard, setDashboard] = useState<DashboardTodayResponse | null>(null);
  const [summary, setSummary] = useState<WeeklySummaryResponse | null>(null);
  const [error, setError] = useState('');
  const [weeklyError, setWeeklyError] = useState('');
  const [loading, setLoading] = useState(true);
  const [weeklyLoading, setWeeklyLoading] = useState(false);
  const [statusBusy, setStatusBusy] = useState<number | null>(null);

  const loadWeekly = useCallback(async () => {
    setWeeklyLoading(true);
    setWeeklyError('');
    const result = await aiApi.weeklySummary();
    if (!result.isSuccess) {
      setWeeklyError(getErrorMessage(result, 'Haftalık özet alınamadı.'));
      setSummary(null);
    } else {
      setSummary(result.data ?? null);
    }
    setWeeklyLoading(false);
  }, []);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    const todayResult = await dashboardApi.today();
    if (!todayResult.isSuccess) {
      setError(getErrorMessage(todayResult, 'Dashboard yüklenemedi.'));
      setDashboard(null);
    } else {
      setDashboard(todayResult.data ?? null);
    }
    setLoading(false);
    void loadWeekly();
  }, [loadWeekly]);

  useEffect(() => {
    void load();
  }, [load]);

  async function changeStatus(taskId: number, status: TaskItemStatus) {
    setStatusBusy(taskId);
    const result = await tasksApi.updateStatus(taskId, status);
    if (!result.isSuccess) {
      showToast(getErrorMessage(result, 'Durum güncellenemedi.'), 'error');
      setStatusBusy(null);
      return;
    }
    await load();
    setStatusBusy(null);
    void tryShowTaskFeedback(taskId, status, (message, nextAction, isFallback) => {
      showToast(`${message}${nextAction ? ` — ${nextAction}` : ''}${isFallback ? ' (yedek)' : ''}`, 'info');
    });
  }

  if (loading) return <PageLoader label="Bugünkü plan yükleniyor..." />;

  const total = dashboard ? dashboard.completedTodayCount + dashboard.pendingTodayCount : 0;
  const progress = total > 0 ? Math.round((dashboard!.completedTodayCount / total) * 100) : 0;
  const greeting = profile?.displayName ? `Merhaba, ${profile.displayName.split(' ')[0]}!` : 'Merhaba!';

  return (
    <div className="page">
      <div className="welcome-hero">
        <div>
          <h2>{greeting}</h2>
          <p>Bugünkü akademik planınız hazır. Odaklanın, adım adım ilerleyin.</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn btn-secondary" onClick={() => navigate('/study-plan')}>Çalışma Planı</button>
          <button type="button" className="btn btn-primary" onClick={() => navigate('/tasks/new')}>+ Görev Ekle</button>
        </div>
      </div>

      <ErrorBanner message={error} />

      {dashboard && (
        <>
          <div className="grid-3">
            <StatCard label="Geciken" value={dashboard.overdueTasksCount} tone="danger" />
            <StatCard label="Tamamlanan" value={dashboard.completedTodayCount} tone="success" />
            <StatCard label="Bekleyen" value={dashboard.pendingTodayCount} tone="warning" />
          </div>
          {total > 0 && (
            <div className="card" style={{ padding: '1rem 1.25rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', fontWeight: 600 }}>
                <span className="muted">Bugünkü ilerleme</span>
                <span>{progress}%</span>
              </div>
              <div className="progress-bar"><div className="progress-fill" style={{ width: `${progress}%` }} /></div>
            </div>
          )}
        </>
      )}

      {dashboard && (
        <section className="card highlight-card ai-card card-elevated">
          <div className="card-header-row">
            <h2><span className="inline-icon"><IconSparkle size={18} /></span> Günün AI mesajı</h2>
            <span className="badge badge-pending">{dashboard.aiMood || 'Nötr'}</span>
          </div>
          <p style={{ fontSize: '1.05rem', lineHeight: 1.6, margin: '0 0 0.75rem' }}>{dashboard.dailyMessage || 'Bugün için mesaj yok.'}</p>
          <p className="muted small">Kişilik tonu: {dashboard.personalityVibe || '—'}</p>
        </section>
      )}

      <section className="card card-elevated">
        <div className="card-header-row">
          <h2>Öncelikli 3 görev</h2>
          <Link to="/tasks" className="link">Tüm görevler →</Link>
        </div>
        {!dashboard?.bigThreeTasks.length ? (
          <EmptyState icon="🎯" title="Bugün için öncelikli görev yok" description="Müfredat tarayarak veya manuel görev ekleyerek başlayın." actionLabel="Müfredat tara" onAction={() => navigate('/syllabus')} />
        ) : (
          <ul className="task-list">
            {dashboard.bigThreeTasks.map((task) => (
              <li key={task.id} className="task-row">
                <div className="task-row-main">
                  <strong>{task.title}</strong>
                  <div className="task-meta">
                    <span className="muted">{task.courseCode}</span>
                    <StatusBadge status={task.status} />
                  </div>
                </div>
                <div className="btn-group">
                  <button type="button" className="btn btn-sm btn-success" disabled={statusBusy === task.id} onClick={() => void changeStatus(task.id, 'Done')}>Tamamla</button>
                  <button type="button" className="btn btn-sm btn-secondary" disabled={statusBusy === task.id} onClick={() => void changeStatus(task.id, 'Pending')}>Beklet</button>
                  <button type="button" className="btn btn-sm btn-warning" disabled={statusBusy === task.id} onClick={() => void changeStatus(task.id, 'Missed')}>Kaçırıldı</button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="card card-elevated">
        <div className="card-header-row">
          <h2>Haftalık özet</h2>
          <button type="button" className="btn btn-ghost btn-sm" disabled={weeklyLoading} onClick={() => void loadWeekly()}>
            {weeklyLoading ? 'Yükleniyor...' : 'Yenile'}
          </button>
        </div>
        {weeklyError && <p className="error-text banner banner-error">{weeklyError}</p>}
        {weeklyLoading && !summary && (
          <div>
            <div className="skeleton skeleton-line" style={{ width: '90%' }} />
            <div className="skeleton skeleton-line" style={{ width: '70%' }} />
          </div>
        )}
        {summary && (
          <>
            <p style={{ lineHeight: 1.65 }}>{summary.summary}</p>
            <p className="muted" style={{ marginTop: '0.75rem' }}>Gelecek hafta odağı: <strong>{summary.nextWeekFocus || '—'}</strong></p>
            <AiBadge fallback={summary.isFallback} />
          </>
        )}
        {!summary && !weeklyLoading && !weeklyError && <p className="muted">Haftalık özet henüz yüklenmedi.</p>}
      </section>
    </div>
  );
}
