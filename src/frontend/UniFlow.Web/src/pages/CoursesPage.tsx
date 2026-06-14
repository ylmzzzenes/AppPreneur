import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { coursesApi, getErrorMessage } from '../api/services';
import type { Course } from '../api/types';
import { ConfirmModal } from '../components/ConfirmModal';
import { EmptyState } from '../components/EmptyState';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { PageHeader } from '../components/ui/PageHeader';
import { useToast } from '../context/ToastContext';

export function CoursesPage() {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [courses, setCourses] = useState<Course[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);

  async function load() {
    setLoading(true);
    setError('');
    const result = await coursesApi.list();
    if (!result.isSuccess) setError(getErrorMessage(result, 'Dersler yüklenemedi.'));
    else setCourses(result.data ?? []);
    setLoading(false);
  }

  useEffect(() => { void load(); }, []);

  async function confirmDelete() {
    if (!deleteId) return;
    setBusyId(deleteId);
    const result = await coursesApi.remove(deleteId);
    if (!result.isSuccess) showToast(getErrorMessage(result), 'error');
    else {
      showToast('Ders silindi.', 'success');
      await load();
    }
    setBusyId(null);
    setDeleteId(null);
  }

  if (loading) return <PageLoader label="Dersler yükleniyor..." />;

  return (
    <div className="page">
      <PageHeader
        title="Dersler"
        subtitle={`${courses.length} ders kayıtlı`}
        actions={<button type="button" className="btn btn-primary" onClick={() => navigate('/courses/new')}>+ Ders Ekle</button>}
      />
      <ErrorBanner message={error} />
      <section className="card card-elevated">
        {courses.length === 0 ? (
          <EmptyState icon="📚" title="Henüz ders yok" description="İlk dersinizi ekleyerek başlayın." actionLabel="Ders ekle" onAction={() => navigate('/courses/new')} />
        ) : (
          <div className="course-grid">
            {courses.map((c) => (
              <article key={c.id} className="course-card">
                <div className="course-card-head">
                  <h3>{c.code}</h3>
                </div>
                <div className="course-card-body">
                  <p>{c.title}</p>
                  <div className="course-stats">
                    <span className="course-stat">{c.activeTaskCount} aktif görev</span>
                    <span className="course-stat">{c.taskCount} toplam</span>
                  </div>
                  <div className="btn-group">
                    <Link to={`/courses/${c.id}/edit`} className="btn btn-sm btn-secondary">Düzenle</Link>
                    <button type="button" className="btn btn-sm btn-danger" disabled={busyId === c.id} onClick={() => setDeleteId(c.id)}>Sil</button>
                  </div>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>

      <ConfirmModal
        open={deleteId !== null}
        title="Dersi sil"
        message="Bu ders ve ilişkili veriler silinecek. Emin misiniz?"
        confirmLabel="Sil"
        busy={busyId !== null}
        onConfirm={() => void confirmDelete()}
        onCancel={() => setDeleteId(null)}
      />
    </div>
  );
}
