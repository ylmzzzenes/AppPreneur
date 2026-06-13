import { useEffect, useState, type FormEvent } from 'react';
import { coursesApi, getErrorMessage, tasksApi } from '../api/services';
import type { Course, CreateTaskRequest, TaskItem, TaskItemStatus } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { LoadingSpinner } from '../components/LoadingSpinner';

const emptyForm: CreateTaskRequest = {
  courseId: 0,
  title: '',
  description: '',
  dueDate: '',
  estimatedMinutes: undefined,
  priorityScore: undefined,
  status: 'Pending',
};

export function TasksPage() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [form, setForm] = useState<CreateTaskRequest>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  async function loadData() {
    setLoading(true);
    setError('');

    try {
      const [tasksResult, coursesResult] = await Promise.all([tasksApi.list(), coursesApi.list()]);

      if (!tasksResult.isSuccess) {
        setError(getErrorMessage(tasksResult, 'Görevler yüklenemedi.'));
        return;
      }

      setTasks(tasksResult.data ?? []);

      if (coursesResult.isSuccess && coursesResult.data) {
        setCourses(coursesResult.data);
        if (!form.courseId && coursesResult.data.length > 0) {
          setForm((prev) => ({ ...prev, courseId: coursesResult.data![0].id }));
        }
      }
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function resetForm() {
    setEditingId(null);
    setForm({
      ...emptyForm,
      courseId: courses[0]?.id ?? 0,
    });
  }

  function startEdit(task: TaskItem) {
    setEditingId(task.id);
    setForm({
      courseId: task.courseId,
      title: task.title,
      description: task.description ?? '',
      dueDate: task.dueDate ? task.dueDate.slice(0, 10) : '',
      estimatedMinutes: task.estimatedMinutes,
      priorityScore: task.priorityScore,
      status: task.status,
    });
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!form.courseId || !form.title.trim()) {
      setError('Ders ve başlık zorunludur.');
      return;
    }

    setSaving(true);
    setError('');

    try {
      const payload = {
        ...form,
        title: form.title.trim(),
        description: form.description?.trim() || undefined,
        dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : undefined,
      };

      const result = editingId
        ? await tasksApi.update(editingId, {
            ...payload,
            status: (form.status ?? 'Pending') as TaskItemStatus,
          })
        : await tasksApi.create(payload);

      if (!result.isSuccess) {
        setError(getErrorMessage(result, 'Görev kaydedilemedi.'));
        return;
      }

      resetForm();
      await loadData();
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id: number) {
    setSaving(true);
    setError('');

    try {
      const result = await tasksApi.remove(id);
      if (!result.isSuccess) {
        setError(getErrorMessage(result, 'Görev silinemedi.'));
        return;
      }

      await loadData();
    } finally {
      setSaving(false);
    }
  }

  async function toggleStatus(task: TaskItem) {
    const nextStatus: TaskItemStatus = task.status === 'Done' ? 'Pending' : 'Done';
    setSaving(true);
    setError('');

    try {
      const result = await tasksApi.updateStatus(task.id, nextStatus);
      if (!result.isSuccess) {
        setError(getErrorMessage(result, 'Durum güncellenemedi.'));
        return;
      }

      await loadData();
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <LoadingSpinner label="Görevler yükleniyor..." />;
  }

  return (
    <div className="page">
      <h1>Görevler</h1>
      <ErrorBanner message={error} />

      <form className="card form-grid" onSubmit={handleSubmit}>
        <h2>{editingId ? 'Görevi düzenle' : 'Yeni görev'}</h2>
        <label>
          Ders
          <select
            value={form.courseId}
            onChange={(e) => setForm((prev) => ({ ...prev, courseId: Number(e.target.value) }))}
            required
          >
            <option value={0} disabled>
              Ders seçin
            </option>
            {courses.map((course) => (
              <option key={course.id} value={course.id}>
                {course.code} — {course.title}
              </option>
            ))}
          </select>
        </label>
        <label>
          Başlık
          <input
            type="text"
            value={form.title}
            onChange={(e) => setForm((prev) => ({ ...prev, title: e.target.value }))}
            required
          />
        </label>
        <label>
          Açıklama
          <textarea
            value={form.description ?? ''}
            onChange={(e) => setForm((prev) => ({ ...prev, description: e.target.value }))}
            rows={3}
          />
        </label>
        <label>
          Son tarih
          <input
            type="date"
            value={form.dueDate ?? ''}
            onChange={(e) => setForm((prev) => ({ ...prev, dueDate: e.target.value }))}
          />
        </label>
        <div className="form-actions">
          <button type="submit" className="btn-primary" disabled={saving}>
            {saving ? 'Kaydediliyor...' : editingId ? 'Güncelle' : 'Oluştur'}
          </button>
          {editingId && (
            <button type="button" className="btn-secondary" onClick={resetForm}>
              İptal
            </button>
          )}
        </div>
      </form>

      <section className="card">
        <h2>Tüm görevler ({tasks.length})</h2>
        {tasks.length === 0 ? (
          <p className="muted">Henüz görev yok. Yukarıdan ekleyebilir veya müfredat tarayabilirsiniz.</p>
        ) : (
          <ul className="task-list">
            {tasks.map((task) => (
              <li key={task.id}>
                <div>
                  <strong>{task.title}</strong>
                  <span className="muted">
                    {task.courseCode} · {task.status}
                  </span>
                </div>
                <div className="row-actions">
                  <button type="button" className="btn-secondary" onClick={() => void toggleStatus(task)}>
                    {task.status === 'Done' ? 'Beklemeye al' : 'Tamamla'}
                  </button>
                  <button type="button" className="btn-secondary" onClick={() => startEdit(task)}>
                    Düzenle
                  </button>
                  <button type="button" className="btn-danger" onClick={() => void handleDelete(task.id)}>
                    Sil
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
