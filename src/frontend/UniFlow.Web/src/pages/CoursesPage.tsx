import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { coursesApi, getErrorMessage } from '../api/services';
import type { Course } from '../api/types';
import { EmptyState } from '../components/EmptyState';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { useToast } from '../context/ToastContext';

export function CoursesPage() {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [courses, setCourses] = useState<Course[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<number | null>(null);

  async function load() {
    setLoading(true);
    setError('');
    const result = await coursesApi.list();
    if (!result.isSuccess) setError(getErrorMessage(result, 'Dersler yüklenemedi.'));
    else setCourses(result.data ?? []);
    setLoading(false);
  }

  useEffect(() => { void load(); }, []);

  async function remove(id: number) {
    if (!window.confirm('Bu dersi silmek istediğinize emin misiniz?')) return;
    setBusyId(id);
    const result = await coursesApi.remove(id);
    if (!result.isSuccess) showToast(getErrorMessage(result), 'error');
    else {
      showToast('Ders silindi.', 'success');
      await load();
    }
    setBusyId(null);
  }

  if (loading) return <PageLoader label="Dersler yükleniyor..." />;

  return (
    <div className="page">
      <div className="page-header">
        <h1>Dersler</h1>
        <button type="button" className="btn btn-primary" onClick={() => navigate('/courses/new')}>Ders Ekle</button>
      </div>
      <ErrorBanner message={error} />
      <section className="card">
        {courses.length === 0 ? (
          <EmptyState title="Henüz ders yok" description="İlk dersinizi ekleyerek başlayın." actionLabel="Ders ekle" onAction={() => navigate('/courses/new')} />
        ) : (
          <div className="course-grid">
            {courses.map((c) => (
              <article key={c.id} className="course-card">
                <h3>{c.code}</h3>
                <p>{c.title}</p>
                <p className="muted">{c.activeTaskCount} aktif / {c.taskCount} toplam görev</p>
                <div className="btn-group">
                  <Link to={`/courses/${c.id}/edit`} className="btn btn-sm btn-secondary">Düzenle</Link>
                  <button type="button" className="btn btn-sm btn-danger" disabled={busyId === c.id} onClick={() => void remove(c.id)}>Sil</button>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
