interface LoadingSpinnerProps {
  label?: string;
}

export function LoadingSpinner({ label = 'Yükleniyor...' }: LoadingSpinnerProps) {
  return (
    <div className="loading" role="status" aria-live="polite">
      <div className="spinner" />
      <span>{label}</span>
    </div>
  );
}
