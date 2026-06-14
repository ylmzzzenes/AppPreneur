import { useState, type FormEvent } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { ErrorBanner } from '../components/ErrorBanner';

export function RegisterPage() {
  const { isAuthenticated, register } = useAuth();
  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [major, setMajor] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (isAuthenticated) return <Navigate to="/dashboard" replace />;

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const msg = await register({
        displayName: displayName.trim(),
        email: email.trim(),
        password,
        major: major.trim() || undefined,
      });
      if (msg) setError(msg);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-shell">
      <form className="auth-card card" onSubmit={handleSubmit}>
        <h1>Kayıt Ol</h1>
        <p className="muted">Üniversite planlamanıza başlayın.</p>
        <ErrorBanner message={error} />
        <label>Görünen ad<input type="text" value={displayName} onChange={(e) => setDisplayName(e.target.value)} required maxLength={100} /></label>
        <label>E-posta<input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required /></label>
        <label>Şifre<input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} /></label>
        <label>Bölüm (isteğe bağlı)<input type="text" value={major} onChange={(e) => setMajor(e.target.value)} maxLength={100} /></label>
        <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
          {loading ? 'Kayıt yapılıyor...' : 'Kayıt Ol'}
        </button>
        <p className="muted center">Zaten hesabınız var mı? <Link to="/login">Giriş yapın</Link></p>
      </form>
    </div>
  );
}
