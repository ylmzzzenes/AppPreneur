import { useEffect, useState } from 'react';
import { coursesApi, getErrorMessage, aiApi } from '../api/services';
import type { Course, StudyPlanResponse } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';

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
      <h1>Çalışma Planı</h1>
      <p className="muted">AI destekli günlük çalışma planı oluşturun.</p>
      <ErrorBanner message={error} />

      <section className="card form-grid">
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
        <label>Odak<textarea value={focus} onChange={(e) => setFocus(e.target.value)} rows={2} placeholder="Örn: final haftası, proje teslimi" /></label>
        <button type="button" className="btn btn-primary" disabled={generating} onClick={() => void generate()}>
          {generating ? 'Plan oluşturuluyor...' : 'Plan Oluştur'}
        </button>
      </section>

      {plan && (
        <section className="card">
          <h2>{plan.title}</h2>
          <p>{plan.summary}</p>
          {plan.isFallback && <p className="muted">AI geçici olarak kullanılamıyor; yedek plan gösteriliyor.</p>}
          <div className="plan-days">
            {plan.days.map((day) => (
              <article key={day.date} className="plan-day-card">
                <h3>{day.date} — {day.focus}</h3>
                <ul>{day.tasks.map((t) => <li key={t.title}><strong>{t.title}</strong> ({t.estimatedMinutes} dk) — {t.reason}</li>)}</ul>
                {day.tip && <p className="muted">İpucu: {day.tip}</p>}
              </article>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
