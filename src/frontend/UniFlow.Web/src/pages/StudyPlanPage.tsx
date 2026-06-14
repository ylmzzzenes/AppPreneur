import { useEffect, useState } from 'react';
import { coursesApi, getErrorMessage, aiApi } from '../api/services';
import type { Course, StudyPlanResponse } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { AiBadge } from '../components/ui/Badge';
import { PageHeader } from '../components/ui/PageHeader';
import { IconBrain } from '../components/ui/Icons';

export function StudyPlanPage() {
  const [courses, setCourses] = useState<Course[]>([]);
  const [courseId, setCourseId] = useState<number | ''>('');
  const [days, setDays] = useState(7);
  const [focus, setFocus] = useState('');
  const [plan, setPlan] = useState<StudyPlanResponse | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);

  useEffect(() => {
    void coursesApi.list().then((r) => {
      if (r.isSuccess && r.data) setCourses(r.data);
      setLoading(false);
    });
  }, []);

  async function generate() {
    setGenerating(true);
    setError('');
    const result = await aiApi.studyPlan({
      courseId: courseId === '' ? undefined : courseId,
      days,
      focus: focus.trim() || undefined,
    });
    if (!result.isSuccess) {
      setError(getErrorMessage(result, 'Çalışma planı oluşturulamadı.'));
      setPlan(null);
    } else {
      setPlan(result.data ?? null);
    }
    setGenerating(false);
  }

  if (loading) return <PageLoader label="Dersler yükleniyor..." />;

  return (
    <div className="page">
      <PageHeader title="Çalışma Planı" subtitle="AI destekli kişisel günlük çalışma planı oluşturun." />

      <section className="card highlight-card card-elevated">
        <div className="card-header-row">
          <h2><span className="inline-icon"><IconBrain size={18} /></span> Plan oluştur</h2>
        </div>
        <ErrorBanner message={error} />
        <div className="grid-2" style={{ gap: '0 1rem' }}>
          <label>Gün sayısı
            <select value={days} onChange={(e) => setDays(Number(e.target.value))}>
              <option value={3}>3 gün</option>
              <option value={7}>7 gün</option>
              <option value={14}>14 gün</option>
            </select>
          </label>
          <label>Ders (isteğe bağlı)
            <select value={courseId} onChange={(e) => setCourseId(e.target.value ? Number(e.target.value) : '')}>
              <option value="">Tüm dersler</option>
              {courses.map((c) => <option key={c.id} value={c.id}>{c.code} — {c.title}</option>)}
            </select>
          </label>
        </div>
        <label>Odak alanı<textarea value={focus} onChange={(e) => setFocus(e.target.value)} rows={2} placeholder="Örn: final haftası, proje teslimi, vize hazırlığı" /></label>
        <button type="button" className="btn btn-primary" disabled={generating} onClick={() => void generate()}>
          {generating ? 'Plan oluşturuluyor...' : '✨ Plan Oluştur'}
        </button>
      </section>

      {generating && (
        <div className="card">
          <div className="skeleton skeleton-card" />
          <div className="skeleton skeleton-card" />
          <div className="skeleton skeleton-line" style={{ width: '60%' }} />
        </div>
      )}

      {plan && !generating && (
        <section className="card card-elevated">
          <div className="card-header-row">
            <h2>{plan.title}</h2>
            <AiBadge fallback={plan.isFallback} />
          </div>
          <p style={{ lineHeight: 1.6 }}>{plan.summary}</p>
          <div className="plan-days">
            {plan.days.map((day) => (
              <article key={day.date} className="plan-day-card">
                <h3>📅 {day.date} — {day.focus}</h3>
                <ul>{day.tasks.map((t) => <li key={t.title}><strong>{t.title}</strong> ({t.estimatedMinutes} dk) — {t.reason}</li>)}</ul>
                {day.tip && <p className="muted small">💡 {day.tip}</p>}
              </article>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
