import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getErrorMessage, syllabusApi } from '../api/services';
import type { SyllabusDetectedItem } from '../api/types';
import { EmptyState } from '../components/EmptyState';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageHeader } from '../components/ui/PageHeader';
import { StepIndicator } from '../components/ui/StepIndicator';
import { useSyllabusScan } from '../context/SyllabusScanContext';
import { useToast } from '../context/ToastContext';

export function SyllabusPreviewPage() {
  const navigate = useNavigate();
  const { scan, clearScan } = useSyllabusScan();
  const { showToast } = useToast();
  const [items, setItems] = useState<Array<SyllabusDetectedItem & { selected: boolean }>>([]);
  const [error, setError] = useState('');
  const [confirming, setConfirming] = useState(false);

  useEffect(() => {
    if (!scan) return;
    setItems(scan.detectedItems.map((i) => ({ ...i, selected: true })));
  }, [scan]);

  if (!scan) {
    return (
      <div className="page">
        <EmptyState icon="📄" title="Tarama oturumu bulunamadı" description="Önce müfredat dosyası tarayın." actionLabel="Müfredat tara" onAction={() => navigate('/syllabus')} />
      </div>
    );
  }

  const selectedCount = items.filter((i) => i.selected).length;

  function toggle(index: number) {
    setItems((prev) => prev.map((item, i) => (i === index ? { ...item, selected: !item.selected } : item)));
  }

  function toggleAll(selected: boolean) {
    setItems((prev) => prev.map((item) => ({ ...item, selected })));
  }

  async function confirm() {
    const selected = items.filter((i) => i.selected).map(({ selected: _, ...item }) => item);
    if (!selected.length) {
      setError('En az bir öğe seçmelisiniz.');
      return;
    }
    setConfirming(true);
    setError('');
    const result = await syllabusApi.confirm({
      scanId: scan!.scanId,
      courseCode: scan!.courseCode,
      courseTitle: scan!.courseTitle,
      items: selected,
    });
    setConfirming(false);
    if (!result.isSuccess || !result.data) {
      setError(getErrorMessage(result, 'Müfredat onaylanamadı.'));
      return;
    }
    clearScan();
    showToast(`${result.data.taskCount} görev oluşturuldu.`, 'success');
    navigate('/dashboard');
  }

  return (
    <div className="page">
      <PageHeader
        title="Müfredat Önizleme"
        subtitle={`${scan.courseCode} — ${scan.courseTitle}`}
        actions={<button type="button" className="btn btn-secondary" onClick={() => navigate('/syllabus')}>← Geri</button>}
      />
      <StepIndicator steps={['Yükle & Tara', 'Önizle', 'Onayla']} current={1} />
      <p className="muted small">{scan.sourceSummary}</p>
      <ErrorBanner message={error} />
      <section className="card card-elevated">
        <div className="card-header-row">
          <h2>{selectedCount} / {items.length} öğe seçili</h2>
          <div className="btn-group">
            <button type="button" className="btn btn-sm btn-secondary" onClick={() => toggleAll(true)}>Tümünü seç</button>
            <button type="button" className="btn btn-sm btn-ghost" onClick={() => toggleAll(false)}>Temizle</button>
          </div>
        </div>
        <ul className="task-list">
          {items.map((item, index) => (
            <li key={`${item.title}-${index}`} className="task-row">
              <label className="checkbox-row" style={{ margin: 0, flex: 1 }}>
                <input type="checkbox" checked={item.selected} onChange={() => toggle(index)} />
                <span className="task-row-main">
                  <strong>{item.title}</strong>
                  {item.dueDate && <span className="muted">📅 {new Date(item.dueDate).toLocaleDateString('tr-TR')}</span>}
                </span>
              </label>
            </li>
          ))}
        </ul>
        <button type="button" className="btn btn-primary btn-block" style={{ marginTop: '1rem' }} disabled={confirming || selectedCount === 0} onClick={() => void confirm()}>
          {confirming ? 'Onaylanıyor...' : `Seçilen ${selectedCount} görevi ekle`}
        </button>
      </section>
    </div>
  );
}
