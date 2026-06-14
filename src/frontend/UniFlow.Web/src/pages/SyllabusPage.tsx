import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { getErrorMessage, syllabusApi } from '../api/services';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageHeader } from '../components/ui/PageHeader';
import { FileDropzone } from '../components/ui/FileDropzone';
import { StepIndicator } from '../components/ui/StepIndicator';
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
      <PageHeader title="Müfredat Tarama" subtitle="Ders müfredatınızı yükleyin; AI görevleri otomatik tespit etsin." />
      <StepIndicator steps={['Yükle & Tara', 'Önizle', 'Onayla']} current={0} />
      <ErrorBanner message={error} />
      <form className="card card-elevated form-grid form-grid-wide" onSubmit={handleSubmit}>
        <div className="grid-2" style={{ gap: '0 1rem' }}>
          <label>Ders kodu<input value={courseCode} onChange={(e) => setCourseCode(e.target.value)} required placeholder="CS101" /></label>
          <label>Ders adı<input value={courseTitle} onChange={(e) => setCourseTitle(e.target.value)} required placeholder="Giriş Programlama" /></label>
        </div>
        <label>
          Dosya
          <FileDropzone file={file} onFile={setFile} />
        </label>
        <div className="banner banner-info small">
          <strong>OCR notu:</strong> Sunucuda OCR Stub modundaysa yalnızca .txt/.md dosyaları çalışır. PDF veya görsel için Gemini OCR yapılandırması gerekir.
        </div>
        <button type="submit" className="btn btn-primary" disabled={loading || !file}>
          {loading ? 'Taranıyor...' : 'Tara ve Önizle →'}
        </button>
      </form>
    </div>
  );
}
