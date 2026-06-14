import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { coursesApi, getErrorMessage } from '../api/services';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { PageHeader } from '../components/ui/PageHeader';
import { useToast } from '../context/ToastContext';

const COLOR_PRESETS = ['#6366f1', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981', '#3b82f6'];

export function CourseFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const isEdit = Boolean(id);
  const courseId = id ? Number(id) : null;

  const [code, setCode] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [color, setColor] = useState('#6366f1');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!isEdit || !courseId) return;
    let cancelled = false;
    async function load() {
      const result = await coursesApi.get(courseId!);
      if (!cancelled) {
        if (!result.isSuccess || !result.data) setError(getErrorMessage(result, 'Ders yüklenemedi.'));
        else {
          setCode(result.data.code);
          setTitle(result.data.title);
          setDescription(result.data.description ?? '');
          setColor(result.data.color ?? '#6366f1');
        }
        setLoading(false);
      }
    }
    void load();
    return () => { cancelled = true; };
  }, [isEdit, courseId]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSaving(true);
    setError('');
    const payload = { code: code.trim(), title: title.trim(), description: description.trim() || undefined, color: color.trim() || undefined };
    const result = isEdit && courseId ? await coursesApi.update(courseId, payload) : await coursesApi.create(payload);
    if (!result.isSuccess) {
      setError(getErrorMessage(result, 'Ders kaydedilemedi.'));
      setSaving(false);
      return;
    }
    showToast(isEdit ? 'Ders güncellendi.' : 'Ders oluşturuldu.', 'success');
    navigate('/courses');
  }

  if (loading) return <PageLoader label="Ders yükleniyor..." />;

  return (
    <div className="page">
      <PageHeader
        title={isEdit ? 'Dersi Düzenle' : 'Yeni Ders'}
        subtitle={isEdit ? 'Ders bilgilerini güncelleyin' : 'Yeni bir ders ekleyin'}
        actions={<button type="button" className="btn btn-secondary" onClick={() => navigate('/courses')}>← İptal</button>}
      />
      <ErrorBanner message={error} />
      <form className="card card-elevated form-grid" onSubmit={handleSubmit}>
        <div className="grid-2" style={{ gap: '0 1rem' }}>
          <label>Ders kodu<input value={code} onChange={(e) => setCode(e.target.value)} required placeholder="CS101" /></label>
          <label>Ders adı<input value={title} onChange={(e) => setTitle(e.target.value)} required placeholder="Giriş Programlama" /></label>
        </div>
        <label>Açıklama<textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} placeholder="Ders hakkında kısa not" /></label>
        <label>
          Renk
          <div className="btn-group" style={{ marginTop: '0.35rem' }}>
            {COLOR_PRESETS.map((c) => (
              <button
                key={c}
                type="button"
                onClick={() => setColor(c)}
                style={{
                  width: 32, height: 32, borderRadius: '50%', background: c,
                  border: color === c ? '3px solid var(--text)' : '2px solid var(--border)',
                  cursor: 'pointer',
                }}
                aria-label={`Renk ${c}`}
              />
            ))}
          </div>
        </label>
        <button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Kaydediliyor...' : 'Kaydet'}</button>
      </form>
    </div>
  );
}
