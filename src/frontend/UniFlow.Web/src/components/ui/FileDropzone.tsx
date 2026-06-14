import { useRef, useState, type DragEvent } from 'react';
import { IconUpload } from './Icons';

interface FileDropzoneProps {
  accept?: string;
  file: File | null;
  onFile: (file: File | null) => void;
}

export function FileDropzone({ accept = '.pdf,.png,.jpg,.jpeg,.txt,.md', file, onFile }: FileDropzoneProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [dragOver, setDragOver] = useState(false);

  function pick(f: File | undefined) {
    if (f) onFile(f);
  }

  function onDrop(e: DragEvent) {
    e.preventDefault();
    setDragOver(false);
    pick(e.dataTransfer.files[0]);
  }

  return (
    <div
      className={`dropzone${dragOver ? ' dropzone-active' : ''}${file ? ' dropzone-has-file' : ''}`}
      onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
      onDragLeave={() => setDragOver(false)}
      onDrop={onDrop}
      onClick={() => inputRef.current?.click()}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => e.key === 'Enter' && inputRef.current?.click()}
    >
      <input ref={inputRef} type="file" accept={accept} hidden onChange={(e) => pick(e.target.files?.[0])} />
      <IconUpload size={28} />
      {file ? (
        <>
          <strong>{file.name}</strong>
          <span className="muted">{(file.size / 1024).toFixed(1)} KB — değiştirmek için tıklayın</span>
        </>
      ) : (
        <>
          <strong>Dosya sürükleyin veya seçin</strong>
          <span className="muted">PDF, görsel veya metin (.txt, .md)</span>
        </>
      )}
    </div>
  );
}
