import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { getErrorMessage, syllabusApi } from '../api/services';
import { ErrorBanner } from '../components/ErrorBanner';
import { useSyllabusScan } from '../context/SyllabusScanContext';

export function SyllabusPage() {
  const navigate = useNavigate();
  const { setScan } = useSyllabusScan();
  const [courseCode, setCourseCode] = useState('');
  const [courseTitle, setCourseTitle] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!file) {
      setError('Lütfen bir dosya seçin.');
      return;
    }
    setLoading(true);
    setError('');
    const result = await syllabusApi.scan(courseCode.trim(), courseTitle.trim(), file);
    setLoading(false);
    if (!result.isSuccess || !result.data) {
      setError(getErrorMessage(result, 'Müfredat taranamadı. Production OCR Stub ise yalnızca metin dosyaları desteklenir; PDF/görsel için Gemini OCR anahtarı gerekir.'));
      return;
    }
    if (!result.data.detectedItems.length) {
      setError('Müfredatta görev bulunamadı. Farklı bir dosya deneyin.');
      return;
    }
    setScan(result.data);
    navigate('/syllabus/preview');
  }

  return (
    <div className="page">
      <h1>Müfredat Tarama</h1>
      <p className="muted">Ders müfredatınızı yükleyin; AI görevleri tespit etsin.</p>
      <ErrorBanner message={error} />
      <form className="card form-grid" onSubmit={handleSubmit}>
        <label>Ders kodu<input value={courseCode} onChange={(e) => setCourseCode(e.target.value)} required /></label>
        <label>Ders adı<input value={courseTitle} onChange={(e) => setCourseTitle(e.target.value)} required /></label>
        <label>
          Dosya
          <input type="file" accept=".pdf,.png,.jpg,.jpeg,.txt,.md" onChange={(e) => setFile(e.target.files?.[0] ?? null)} required />
        </label>
        <p className="muted small">Not: Sunucuda OCR Stub modundaysa yalnızca .txt/.md dosyaları çalışır. PDF veya görsel için Gemini OCR yapılandırması gerekir.</p>
        <button type="submit" className="btn btn-primary" disabled={loading}>{loading ? 'Taranıyor...' : 'Tara ve Önizle'}</button>
      </form>
    </div>
  );
}
