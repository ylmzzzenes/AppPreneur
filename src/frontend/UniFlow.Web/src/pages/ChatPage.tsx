import { useEffect, useRef, useState, type FormEvent } from 'react';
import { aiApi, getErrorMessage } from '../api/services';
import type { ChatMessage } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { EmptyState } from '../components/EmptyState';
import { PageHeader } from '../components/ui/PageHeader';

const SUGGESTIONS = [
  'Bugün neye odaklanmalıyım?',
  'Sınavıma nasıl hazırlanmalıyım?',
  'Bu hafta planımı gözden geçir',
  'Motivasyonumu kaybettim, ne yapmalıyım?',
];

function createMessage(role: 'user' | 'assistant', content: string): ChatMessage {
  return { id: crypto.randomUUID(), role, content, createdAt: new Date().toISOString() };
}

export function ChatPage() {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [error, setError] = useState('');
  const [thinking, setThinking] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, thinking]);

  async function send(text: string) {
    if (!text.trim() || thinking) return;
    setInput('');
    setError('');
    setMessages((prev) => [...prev, createMessage('user', text)]);
    setThinking(true);
    const result = await aiApi.chat(text);
    setThinking(false);
    if (!result.isSuccess) {
      setError(getErrorMessage(result, 'AI yanıt veremedi.'));
      setMessages((prev) => [...prev, createMessage('assistant', 'Şu an yanıt veremiyorum. Lütfen daha sonra tekrar deneyin.')]);
      return;
    }
    setMessages((prev) => [...prev, createMessage('assistant', result.data ?? 'Yanıt alınamadı.')]);
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    await send(input);
  }

  return (
    <div className="page chat-page">
      <PageHeader title="Sohbet" subtitle="Sarkastik Dahi — akademik koçunuz" />
      <ErrorBanner message={error} />
      <section className="card card-elevated chat-panel">
        {messages.length === 0 && !thinking && (
          <>
            <EmptyState icon="💬" title="Merhaba!" description="Akademik planlama, motivasyon veya çalışma stratejileri hakkında soru sorabilirsiniz." />
            <div className="chat-suggestions">
              {SUGGESTIONS.map((s) => (
                <button key={s} type="button" className="chat-chip" onClick={() => void send(s)}>{s}</button>
              ))}
            </div>
          </>
        )}
        <div className="chat-messages">
          {messages.map((m) => (
            <div key={m.id} className={`chat-bubble chat-${m.role}`}>{m.content}</div>
          ))}
          {thinking && <div className="chat-bubble chat-assistant typing">Sarkastik Dahi düşünüyor...</div>}
          <div ref={bottomRef} />
        </div>
        <form className="chat-input-row" onSubmit={handleSubmit}>
          <textarea value={input} onChange={(e) => setInput(e.target.value)} placeholder="Mesajınızı yazın..." rows={2} maxLength={4000} />
          <button type="submit" className="btn btn-primary" disabled={thinking || !input.trim()}>Gönder</button>
        </form>
      </section>
    </div>
  );
}
