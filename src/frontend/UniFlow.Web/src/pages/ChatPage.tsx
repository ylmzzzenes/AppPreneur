import { useEffect, useRef, useState, type FormEvent } from 'react';
import { aiApi, getErrorMessage } from '../api/services';
import type { ChatMessage } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { EmptyState } from '../components/EmptyState';

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

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const text = input.trim();
    if (!text || thinking) return;
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

  return (
    <div className="page chat-page">
      <div className="page-header">
        <h1>Sohbet</h1>
        <p className="muted">Sarkastik Dahi — akademik koçunuz</p>
      </div>
      <ErrorBanner message={error} />
      <section className="card chat-panel">
        {messages.length === 0 && !thinking && (
          <EmptyState title="Merhaba!" description="Bugün neye odaklanmalıyım? veya sınavıma nasıl hazırlanmalıyım? diye sorabilirsiniz." />
        )}
        <div className="chat-messages">
          {messages.map((m) => (
            <div key={m.id} className={`chat-bubble chat-${m.role}`}>{m.content}</div>
          ))}
          {thinking && <div className="chat-bubble chat-assistant typing">Sarkastik Dahi yazıyor...</div>}
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
