import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getErrorMessage, syllabusApi } from '../api/services';
import type { SyllabusDetectedItem } from '../api/types';
import { EmptyState } from '../components/EmptyState';
import { ErrorBanner } from '../components/ErrorBanner';
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
        <EmptyState title="Tarama oturumu bulunamadı" description="Önce müfredat dosyası tarayın." actionLabel="Müfredat tara" onAction={() => navigate('/syllabus')} />
      </div>
    );
  }

  function toggle(index: number) {
    setItems((prev) => prev.map((item, i) => (i === index ? { ...item, selected: !item.selected } : item)));
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
      <div className="page-header">
        <h1>Müfredat Önizleme</h1>
        <button type="button" className="btn btn-secondary" onClick={() => navigate('/syllabus')}>Geri</button>
      </div>
      <p className="muted">{scan.sourceSummary}</p>
      <ErrorBanner message={error} />
      <section className="card">
        <ul className="task-list">
          {items.map((item, index) => (
            <li key={`${item.title}-${index}`}>
              <label className="checkbox-row">
                <input type="checkbox" checked={item.selected} onChange={() => toggle(index)} />
                <span>
                  <strong>{item.title}</strong>
                  {item.dueDate && <span className="muted"> · {new Date(item.dueDate).toLocaleDateString('tr-TR')}</span>}
                </span>
              </label>
            </li>
          ))}
        </ul>
        <button type="button" className="btn btn-primary" disabled={confirming} onClick={() => void confirm()}>
          {confirming ? 'Onaylanıyor...' : 'Seçilenleri Görev Olarak Ekle'}
        </button>
      </section>
    </div>
  );
}
