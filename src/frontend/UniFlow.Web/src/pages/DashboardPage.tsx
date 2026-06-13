import { useEffect, useState } from 'react';
import { aiApi, dashboardApi, getErrorMessage } from '../api/services';
import type { DashboardTodayResponse, WeeklySummaryResponse } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { LoadingSpinner } from '../components/LoadingSpinner';

export function DashboardPage() {
  const [dashboard, setDashboard] = useState<DashboardTodayResponse | null>(null);
  const [summary, setSummary] = useState<WeeklySummaryResponse | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      setError('');

      try {
        const [todayResult, summaryResult] = await Promise.all([
          dashboardApi.today(),
          aiApi.weeklySummary(),
        ]);

        if (cancelled) {
          return;
        }

        if (!todayResult.isSuccess) {
          setError(getErrorMessage(todayResult, 'Dashboard yüklenemedi.'));
          return;
        }

        setDashboard(todayResult.data ?? null);

        if (summaryResult.isSuccess && summaryResult.data) {
          setSummary(summaryResult.data);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, []);

  if (loading) {
    return <LoadingSpinner label="Bugünkü plan yükleniyor..." />;
  }

  return (
    <div className="page">
      <h1>Bugün</h1>
      <ErrorBanner message={error} />

      {dashboard && (
        <>
          <section className="card stats-grid">
            <div>
              <span className="stat-label">Geciken</span>
              <strong>{dashboard.overdueTasksCount}</strong>
            </div>
            <div>
              <span className="stat-label">Tamamlanan</span>
              <strong>{dashboard.completedTodayCount}</strong>
            </div>
            <div>
              <span className="stat-label">Bekleyen</span>
              <strong>{dashboard.pendingTodayCount}</strong>
            </div>
          </section>

          <section className="card">
            <h2>Günün mesajı</h2>
            <p>{dashboard.dailyMessage || 'Bugün için mesaj yok.'}</p>
            <p className="muted">
              AI modu: {dashboard.aiMood || '—'} · Kişilik: {dashboard.personalityVibe || '—'}
            </p>
          </section>

          <section className="card">
            <h2>Öncelikli 3 görev</h2>
            {dashboard.bigThreeTasks.length === 0 ? (
              <p className="muted">Bugün için öncelikli görev yok.</p>
            ) : (
              <ul className="task-list">
                {dashboard.bigThreeTasks.map((task) => (
                  <li key={task.id}>
                    <div>
                      <strong>{task.title}</strong>
                      <span className="muted">
                        {task.courseCode} · {task.status}
                      </span>
                    </div>
                    {task.dueDate && <time>{new Date(task.dueDate).toLocaleDateString('tr-TR')}</time>}
                  </li>
                ))}
              </ul>
            )}
          </section>
        </>
      )}

      <section className="card">
        <h2>Haftalık özet</h2>
        {summary ? (
          <>
            <p>{summary.summary}</p>
            {summary.isFallback && (
              <p className="muted">AI geçici olarak kullanılamıyor; yedek özet gösteriliyor.</p>
            )}
          </>
        ) : (
          <p className="muted">Haftalık özet şu an alınamadı.</p>
        )}
      </section>
    </div>
  );
}
