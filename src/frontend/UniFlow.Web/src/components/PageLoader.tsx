interface PageLoaderProps {
  label?: string;
}

export function PageLoader({ label = 'Yükleniyor...' }: PageLoaderProps) {
  return (
    <div className="page-loader" role="status" aria-live="polite">
      <div className="spinner" />
      <p>{label}</p>
    </div>
  );
}
