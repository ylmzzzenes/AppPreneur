import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { aiApi, dashboardApi, getErrorMessage, tasksApi } from '../api/services';
import type { DashboardTodayResponse, TaskItemStatus, WeeklySummaryResponse } from '../api/types';
import { EmptyState } from '../components/EmptyState';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { TASK_STATUS_LABELS } from '../constants/personality';
import { useToast } from '../context/ToastContext';
import { tryShowTaskFeedback } from '../utils/taskFeedback';

export function DashboardPage() {
  const navigate = useNavigate();
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

  return (
    <div className="page">
      <div className="page-header">
        <h1>Bugün</h1>
        <div className="page-actions">
          <button type="button" className="btn btn-secondary" onClick={() => navigate('/study-plan')}>Çalışma Planı</button>
          <button type="button" className="btn btn-primary" onClick={() => navigate('/tasks/new')}>Görev Ekle</button>
        </div>
      </div>
      <ErrorBanner message={error} />

      {dashboard && (
        <div className="grid-3">
          <div className="stat-card"><span>Geciken</span><strong>{dashboard.overdueTasksCount}</strong></div>
          <div className="stat-card"><span>Tamamlanan</span><strong>{dashboard.completedTodayCount}</strong></div>
          <div className="stat-card"><span>Bekleyen</span><strong>{dashboard.pendingTodayCount}</strong></div>
        </div>
      )}

      {dashboard && (
        <section className="card highlight-card">
          <h2>Günün AI mesajı</h2>
          <p>{dashboard.dailyMessage || 'Bugün için mesaj yok.'}</p>
          <p className="muted">Mod: {dashboard.aiMood || '—'} · Kişilik: {dashboard.personalityVibe || '—'}</p>
        </section>
      )}

      <section className="card">
        <div className="card-header-row">
          <h2>Öncelikli 3 görev</h2>
          <Link to="/tasks" className="link">Tüm görevler</Link>
        </div>
        {!dashboard?.bigThreeTasks.length ? (
          <EmptyState title="Bugün için öncelikli görev yok" description="Müfredat tarayarak veya manuel görev ekleyerek başlayın." actionLabel="Müfredat tara" onAction={() => navigate('/syllabus')} />
        ) : (
          <ul className="task-list">
            {dashboard.bigThreeTasks.map((task) => (
              <li key={task.id} className="task-row">
                <div>
                  <strong>{task.title}</strong>
                  <span className="muted">{task.courseCode} · {TASK_STATUS_LABELS[task.status] ?? task.status}</span>
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

      <section className="card">
        <div className="card-header-row">
          <h2>Haftalık özet</h2>
          <button type="button" className="btn btn-ghost" disabled={weeklyLoading} onClick={() => void loadWeekly()}>Yenile</button>
        </div>
        {weeklyLoading && <p className="muted">Özet yükleniyor...</p>}
        {weeklyError && <p className="error-text">{weeklyError}</p>}
        {summary && (
          <>
            <p>{summary.summary}</p>
            <p className="muted">Gelecek hafta odağı: {summary.nextWeekFocus || '—'}</p>
            {summary.isFallback && <p className="muted">AI geçici olarak kullanılamıyor; yedek özet gösteriliyor.</p>}
          </>
        )}
        {!summary && !weeklyLoading && !weeklyError && <p className="muted">Haftalık özet henüz yüklenmedi.</p>}
      </section>
    </div>
  );
}
