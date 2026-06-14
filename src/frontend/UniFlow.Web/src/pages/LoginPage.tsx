import { useState, type FormEvent } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';

export function LoginPage() {
  const { isAuthenticated, isBootstrapping, login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (isAuthenticated && !isBootstrapping) return <Navigate to="/dashboard" replace />;
  if (isBootstrapping) return <PageLoader label="Yönlendiriliyor..." />;

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const msg = await login({ email: email.trim(), password });
      if (msg) setError(msg);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-shell">
      <form className="auth-card card" onSubmit={handleSubmit}>
        <h1>Giriş Yap</h1>
        <p className="muted">UniFlow hesabınızla devam edin.</p>
        <ErrorBanner message={error} />
        <label>E-posta<input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoComplete="email" /></label>
        <label>Şifre<input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required autoComplete="current-password" /></label>
        <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
          {loading ? 'Giriş yapılıyor...' : 'Giriş Yap'}
        </button>
        <p className="muted center">Hesabınız yok mu? <Link to="/register">Kayıt olun</Link></p>
      </form>
    </div>
  );
}
