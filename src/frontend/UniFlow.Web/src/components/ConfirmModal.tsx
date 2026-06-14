interface ConfirmModalProps {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
  busy?: boolean;
}

export function ConfirmModal({ open, title, message, confirmLabel = 'Sil', onConfirm, onCancel, busy }: ConfirmModalProps) {
  if (!open) return null;
  return (
    <div className="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="confirm-title">
      <div className="modal-card">
        <h3 id="confirm-title">{title}</h3>
        <p>{message}</p>
        <div className="modal-actions">
          <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={busy}>İptal</button>
          <button type="button" className="btn btn-danger" onClick={onConfirm} disabled={busy}>
            {busy ? 'İşleniyor...' : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
