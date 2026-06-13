import { useState, type FormEvent } from 'react';
import { aiApi, getErrorMessage, syllabusApi } from '../api/services';
import type { SyllabusDetectedItem, SyllabusScanResponse } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { LoadingSpinner } from '../components/LoadingSpinner';

export function SyllabusPage() {
  const [courseCode, setCourseCode] = useState('');
  const [courseTitle, setCourseTitle] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [scan, setScan] = useState<SyllabusScanResponse | null>(null);
  const [items, setItems] = useState<Array<SyllabusDetectedItem & { isSelected: boolean }>>([]);
  const [chatMessage, setChatMessage] = useState('');
  const [chatReply, setChatReply] = useState('');
  const [error, setError] = useState('');
  const [info, setInfo] = useState('');
  const [loading, setLoading] = useState(false);
  const [confirming, setConfirming] = useState(false);

  async function handleScan(event: FormEvent) {
    event.preventDefault();

    if (!file) {
      setError('Lütfen bir dosya seçin.');
      return;
    }

    setLoading(true);
    setError('');
    setInfo('');
    setScan(null);
    setItems([]);

    try {
      const result = await syllabusApi.scan(courseCode.trim(), courseTitle.trim(), file);

      if (!result.isSuccess || !result.data) {
        setError(getErrorMessage(result, 'Müfredat taranamadı.'));
        return;
      }

      setScan(result.data);
      setItems(
        result.data.detectedItems.map((item) => ({ ...item, isSelected: true })),
      );
      setInfo(`${result.data.detectedItems.length} öğe tespit edildi.`);
    } finally {
      setLoading(false);
    }
  }

  async function handleConfirm() {
    if (!scan) {
      return;
    }

    setConfirming(true);
    setError('');
    setInfo('');

    try {
      const selected = items.filter((item) => item.isSelected).map(({ isSelected: _, ...item }) => item);
      const result = await syllabusApi.confirm({
        scanId: scan.scanId,
        courseCode: scan.courseCode,
        courseTitle: scan.courseTitle,
        items: selected,
      });

      if (!result.isSuccess || !result.data) {
        setError(getErrorMessage(result, 'Müfredat onaylanamadı.'));
        return;
      }

      setInfo(`${result.data.taskCount} görev oluşturuldu.`);
    } finally {
      setConfirming(false);
    }
  }

  async function handleChat(event: FormEvent) {
    event.preventDefault();
    if (!chatMessage.trim()) {
      return;
    }

    setLoading(true);
    setError('');
    setChatReply('');

    try {
      const result = await aiApi.chat(chatMessage.trim());

      if (!result.isSuccess) {
        setChatReply('AI şu an yanıt veremiyor. Lütfen daha sonra tekrar deneyin.');
        return;
      }

      setChatReply(result.data ?? 'Yanıt alınamadı.');
    } finally {
      setLoading(false);
    }
  }

  function toggleItem(index: number) {
    setItems((prev) =>
      prev.map((item, i) => (i === index ? { ...item, isSelected: !item.isSelected } : item)),
    );
  }

  return (
    <div className="page">
      <h1>Müfredat Tarama</h1>
      <ErrorBanner message={error} />
      {info && <div className="banner banner-info">{info}</div>}

      <form className="card form-grid" onSubmit={handleScan}>
        <h2>Dosya yükle</h2>
        <label>
          Ders kodu
          <input
            type="text"
            value={courseCode}
            onChange={(e) => setCourseCode(e.target.value)}
            required
          />
        </label>
        <label>
          Ders adı
          <input
            type="text"
            value={courseTitle}
            onChange={(e) => setCourseTitle(e.target.value)}
            required
          />
        </label>
        <label>
          Müfredat dosyası
          <input
            type="file"
            accept=".pdf,.png,.jpg,.jpeg,.txt,.md"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
            required
          />
        </label>
        <button type="submit" className="btn-primary" disabled={loading}>
          {loading ? 'Taranıyor...' : 'Tara'}
        </button>
        {loading && <LoadingSpinner label="OCR ve AI analizi çalışıyor..." />}
      </form>

      {scan && (
        <section className="card">
          <h2>Tespit edilen öğeler</h2>
          <p className="muted">{scan.sourceSummary}</p>
          {items.length === 0 ? (
            <p className="muted">Tespit edilen öğe yok.</p>
          ) : (
            <ul className="task-list">
              {items.map((item, index) => (
                <li key={`${item.title}-${index}`}>
                  <label className="checkbox-row">
                    <input
                      type="checkbox"
                      checked={item.isSelected}
                      onChange={() => toggleItem(index)}
                    />
                    <span>
                      <strong>{item.title}</strong>
                      {item.dueDate && (
                        <span className="muted"> · {new Date(item.dueDate).toLocaleDateString('tr-TR')}</span>
                      )}
                    </span>
                  </label>
                </li>
              ))}
            </ul>
          )}
          <button
            type="button"
            className="btn-primary"
            disabled={confirming || items.length === 0}
            onClick={() => void handleConfirm()}
          >
            {confirming ? 'Onaylanıyor...' : 'Seçilenleri görev olarak ekle'}
          </button>
        </section>
      )}

      <section className="card form-grid">
        <h2>AI Asistan (isteğe bağlı)</h2>
        <form onSubmit={handleChat}>
          <label>
            Mesajınız
            <textarea
              value={chatMessage}
              onChange={(e) => setChatMessage(e.target.value)}
              rows={3}
              placeholder="Bugün neye odaklanmalıyım?"
            />
          </label>
          <button type="submit" className="btn-secondary" disabled={loading}>
            Gönder
          </button>
        </form>
        {chatReply && <p className="chat-reply">{chatReply}</p>}
      </section>
    </div>
  );
}
