import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { coursesApi, getErrorMessage, tasksApi } from '../api/services';
import type { Course, TaskItemStatus } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { useToast } from '../context/ToastContext';

export function TaskFormPage() {
  const { id } = useParams();
  const [search] = useSearchParams();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const isEdit = Boolean(id);
  const taskId = id ? Number(id) : null;

  const [courses, setCourses] = useState<Course[]>([]);
  const [courseId, setCourseId] = useState(Number(search.get('courseId')) || 0);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [estimatedMinutes, setEstimatedMinutes] = useState<number | ''>('');
  const [priorityScore, setPriorityScore] = useState<number | ''>('');
  const [status, setStatus] = useState<TaskItemStatus>('Pending');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false;
    async function init() {
      setLoading(true);
      const coursesResult = await coursesApi.list();
      if (!cancelled && coursesResult.isSuccess && coursesResult.data) {
        setCourses(coursesResult.data);
        const initialCourse = Number(search.get('courseId')) || coursesResult.data[0]?.id || 0;
        if (!isEdit) setCourseId(initialCourse);
      }
      if (isEdit && taskId) {
        const taskResult = await tasksApi.get(taskId);
        if (!cancelled) {
          if (!taskResult.isSuccess || !taskResult.data) {
            setError(getErrorMessage(taskResult, 'Görev yüklenemedi.'));
          } else {
            const t = taskResult.data;
            setCourseId(t.courseId);
            setTitle(t.title);
            setDescription(t.description ?? '');
            setDueDate(t.dueDate ? t.dueDate.slice(0, 10) : '');
            setEstimatedMinutes(t.estimatedMinutes ?? '');
            setPriorityScore(t.priorityScore ?? '');
            setStatus(t.status);
          }
        }
      }
      if (!cancelled) setLoading(false);
    }
    void init();
    return () => { cancelled = true; };
  }, [isEdit, taskId, search]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!courseId || !title.trim()) {
      setError('Ders ve başlık zorunludur.');
      return;
    }
    setSaving(true);
    setError('');
    const payload = {
      courseId,
      title: title.trim(),
      description: description.trim() || undefined,
      dueDate: dueDate ? new Date(dueDate).toISOString() : undefined,
      estimatedMinutes: estimatedMinutes === '' ? undefined : Number(estimatedMinutes),
      priorityScore: priorityScore === '' ? undefined : Number(priorityScore),
      status,
    };
    const result = isEdit && taskId
      ? await tasksApi.update(taskId, payload)
      : await tasksApi.create(payload);
    if (!result.isSuccess) {
      setError(getErrorMessage(result, 'Görev kaydedilemedi.'));
      setSaving(false);
      return;
    }
    showToast(isEdit ? 'Görev güncellendi.' : 'Görev oluşturuldu.', 'success');
    navigate('/tasks');
  }

  if (loading) return <PageLoader label="Form yükleniyor..." />;

  return (
    <div className="page">
      <h1>{isEdit ? 'Görevi Düzenle' : 'Yeni Görev'}</h1>
      <ErrorBanner message={error} />
      <form className="card form-grid" onSubmit={handleSubmit}>
        <label>Ders
          <select value={courseId} onChange={(e) => setCourseId(Number(e.target.value))} required>
            <option value={0} disabled>Ders seçin</option>
            {courses.map((c) => <option key={c.id} value={c.id}>{c.code} — {c.title}</option>)}
          </select>
        </label>
        <label>Başlık<input value={title} onChange={(e) => setTitle(e.target.value)} required /></label>
        <label>Açıklama<textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} /></label>
        <label>Son tarih<input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} /></label>
        <label>Tahmini süre (dk)<input type="number" value={estimatedMinutes} onChange={(e) => setEstimatedMinutes(e.target.value ? Number(e.target.value) : '')} /></label>
        <label>Öncelik<input type="number" value={priorityScore} onChange={(e) => setPriorityScore(e.target.value ? Number(e.target.value) : '')} /></label>
        {isEdit && (
          <label>Durum
            <select value={status} onChange={(e) => setStatus(e.target.value as TaskItemStatus)}>
              <option value="Pending">Bekliyor</option>
              <option value="Done">Tamamlandı</option>
              <option value="Missed">Kaçırıldı</option>
            </select>
          </label>
        )}
        <div className="btn-group">
          <button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Kaydediliyor...' : 'Kaydet'}</button>
          <button type="button" className="btn btn-secondary" onClick={() => navigate('/tasks')}>İptal</button>
        </div>
      </form>
    </div>
  );
}
