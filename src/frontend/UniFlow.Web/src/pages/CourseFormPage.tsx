import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { coursesApi, getErrorMessage } from '../api/services';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { useToast } from '../context/ToastContext';

export function CourseFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const isEdit = Boolean(id);
  const courseId = id ? Number(id) : null;

  const [code, setCode] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [color, setColor] = useState('');
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
          setColor(result.data.color ?? '');
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
      <h1>{isEdit ? 'Dersi Düzenle' : 'Yeni Ders'}</h1>
      <ErrorBanner message={error} />
      <form className="card form-grid" onSubmit={handleSubmit}>
        <label>Kod<input value={code} onChange={(e) => setCode(e.target.value)} required /></label>
        <label>Ad<input value={title} onChange={(e) => setTitle(e.target.value)} required /></label>
        <label>Açıklama<textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} /></label>
        <label>Renk (hex)<input value={color} onChange={(e) => setColor(e.target.value)} placeholder="#3B82F6" /></label>
        <div className="btn-group">
          <button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Kaydediliyor...' : 'Kaydet'}</button>
          <button type="button" className="btn btn-secondary" onClick={() => navigate('/courses')}>İptal</button>
        </div>
      </form>
    </div>
  );
}
