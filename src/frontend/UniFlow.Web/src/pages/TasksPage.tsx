import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { coursesApi, getErrorMessage, tasksApi } from '../api/services';
import type { Course, TaskItem, TaskItemStatus, TaskListResponse } from '../api/types';
import { TASK_STATUS_LABELS } from '../constants/personality';
import { EmptyState } from '../components/EmptyState';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { useToast } from '../context/ToastContext';
import { tryShowTaskFeedback } from '../utils/taskFeedback';

type FilterMode = 'today' | 'upcoming' | 'all';

export function TasksPage() {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [filter, setFilter] = useState<FilterMode>('today');
  const [statusFilter, setStatusFilter] = useState<TaskItemStatus | ''>('');
  const [courseFilter, setCourseFilter] = useState<number | ''>('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<number | null>(null);

  async function load() {
    setLoading(true);
    setError('');
    const coursesResult = await coursesApi.list();
    if (coursesResult.isSuccess && coursesResult.data) setCourses(coursesResult.data);

    let result;
    if (filter === 'today') result = await tasksApi.today();
    else if (filter === 'upcoming') result = await tasksApi.upcoming(14);
    else result = await tasksApi.list();

    if (!result.isSuccess) {
      setError(getErrorMessage(result, 'Görevler yüklenemedi.'));
      setTasks([]);
    } else if (filter === 'today') {
      setTasks((result.data as TaskListResponse)?.items ?? []);
    } else {
      setTasks((result.data as TaskItem[]) ?? []);
    }
    setLoading(false);
  }

  useEffect(() => {
    void load();
  }, [filter]);

  const filtered = useMemo(() => {
    return tasks.filter((t) => {
      if (statusFilter && t.status !== statusFilter) return false;
      if (courseFilter && t.courseId !== courseFilter) return false;
      return true;
    });
  }, [tasks, statusFilter, courseFilter]);

  async function changeStatus(task: TaskItem, status: TaskItemStatus) {
    setBusyId(task.id);
    const result = await tasksApi.updateStatus(task.id, status);
    if (!result.isSuccess) {
      showToast(getErrorMessage(result), 'error');
    } else {
      await load();
      void tryShowTaskFeedback(task.id, status, (msg, next, fb) => {
        showToast(`${msg}${next ? ` — ${next}` : ''}${fb ? ' (yedek)' : ''}`, 'info');
      });
    }
    setBusyId(null);
  }

  async function removeTask(id: number) {
    if (!window.confirm('Bu görevi silmek istediğinize emin misiniz?')) return;
    setBusyId(id);
    const result = await tasksApi.remove(id);
    if (!result.isSuccess) showToast(getErrorMessage(result), 'error');
    else {
      showToast('Görev silindi.', 'success');
      await load();
    }
    setBusyId(null);
  }

  if (loading) return <PageLoader label="Görevler yükleniyor..." />;

  return (
    <div className="page">
      <div className="page-header">
        <h1>Görevler</h1>
        <button type="button" className="btn btn-primary" onClick={() => navigate('/tasks/new')}>Yeni Görev</button>
      </div>
      <ErrorBanner message={error} />

      <div className="filter-bar card">
        <div className="btn-group">
          {(['today', 'upcoming', 'all'] as FilterMode[]).map((f) => (
            <button key={f} type="button" className={`btn btn-sm ${filter === f ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setFilter(f)}>
              {f === 'today' ? 'Bugün' : f === 'upcoming' ? 'Yaklaşan' : 'Tümü'}
            </button>
          ))}
        </div>
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value as TaskItemStatus | '')}>
          <option value="">Tüm durumlar</option>
          <option value="Pending">Bekliyor</option>
          <option value="Done">Tamamlandı</option>
          <option value="Missed">Kaçırıldı</option>
        </select>
        <select value={courseFilter} onChange={(e) => setCourseFilter(e.target.value ? Number(e.target.value) : '')}>
          <option value="">Tüm dersler</option>
          {courses.map((c) => <option key={c.id} value={c.id}>{c.code} — {c.title}</option>)}
        </select>
      </div>

      <section className="card">
        {filtered.length === 0 ? (
          <EmptyState title="Görev bulunamadı" description="Filtreleri değiştirin veya yeni görev ekleyin." actionLabel="İlk görevi ekle" onAction={() => navigate('/tasks/new')} />
        ) : (
          <ul className="task-list">
            {filtered.map((task) => (
              <li key={task.id} className="task-row">
                <div>
                  <strong>{task.title}</strong>
                  <span className="muted">{task.courseCode} · {TASK_STATUS_LABELS[task.status]}</span>
                  {task.dueDate && <span className="muted"> · {new Date(task.dueDate).toLocaleDateString('tr-TR')}</span>}
                </div>
                <div className="btn-group">
                  <button type="button" className="btn btn-sm btn-success" disabled={busyId === task.id} onClick={() => void changeStatus(task, 'Done')}>Tamamla</button>
                  <Link to={`/tasks/${task.id}/edit`} className="btn btn-sm btn-secondary">Düzenle</Link>
                  <button type="button" className="btn btn-sm btn-danger" disabled={busyId === task.id} onClick={() => void removeTask(task.id)}>Sil</button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
