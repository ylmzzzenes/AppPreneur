import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { getErrorMessage, usersApi } from '../api/services';
import type { PersonalityVibe } from '../api/types';
import { ErrorBanner } from '../components/ErrorBanner';
import { PageLoader } from '../components/PageLoader';
import { PageHeader } from '../components/ui/PageHeader';
import { PERSONALITY_OPTIONS } from '../constants/personality';
import { useToast } from '../context/ToastContext';

export function ProfilePage() {
  const { profile, refreshProfile } = useAuth();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [displayName, setDisplayName] = useState('');
  const [major, setMajor] = useState('');
  const [academicGoal, setAcademicGoal] = useState('');
  const [personalityVibe, setPersonalityVibe] = useState<PersonalityVibe>('Friendly');
  const [dailyMinutes, setDailyMinutes] = useState(120);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (profile) {
      setDisplayName(profile.displayName);
      setMajor(profile.major ?? '');
      setAcademicGoal(profile.academicGoal ?? '');
      setPersonalityVibe(profile.personalityVibe);
      setDailyMinutes(profile.dailyStudyTargetMinutes ?? 120);
      setLoading(false);
    }
  }, [profile]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSaving(true);
    setError('');
    const result = await usersApi.updateOnboarding({
      displayName: displayName.trim(),
      major: major.trim() || undefined,
      academicGoal: academicGoal.trim() || undefined,
      personalityVibe,
      dailyStudyTargetMinutes: dailyMinutes,
    });
    if (!result.isSuccess) {
      setError(getErrorMessage(result, 'Profil kaydedilemedi.'));
    } else {
      await refreshProfile();
      showToast('Profil güncellendi.', 'success');
    }
    setSaving(false);
  }

  if (loading) return <PageLoader label="Profil yükleniyor..." />;

  const initial = profile?.displayName?.charAt(0)?.toUpperCase() ?? '?';

  return (
    <div className="page">
      <PageHeader title="Profil" subtitle="Hesap ve tercih ayarlarınız" actions={<button type="button" className="btn btn-secondary" onClick={() => navigate('/dashboard')}>← Geri</button>} />

      <div className="profile-hero">
        <span className="avatar">{initial}</span>
        <div>
          <h2>{profile?.displayName}</h2>
          <p>{profile?.email}</p>
          {profile?.major && <p className="small">{profile.major}</p>}
        </div>
      </div>

      <ErrorBanner message={error} />
      <form className="card card-elevated form-grid" onSubmit={handleSubmit}>
        <label>E-posta<input value={profile?.email ?? ''} disabled /></label>
        <label>Görünen ad<input value={displayName} onChange={(e) => setDisplayName(e.target.value)} required /></label>
        <label>Bölüm<input value={major} onChange={(e) => setMajor(e.target.value)} placeholder="Bölümünüz" /></label>
        <label>Akademik hedef<textarea value={academicGoal} onChange={(e) => setAcademicGoal(e.target.value)} rows={3} /></label>
        <label>AI kişilik tonu
          <select value={personalityVibe} onChange={(e) => setPersonalityVibe(e.target.value as PersonalityVibe)}>
            {PERSONALITY_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </label>
        <label>Günlük çalışma hedefi (dk)<input type="number" min={15} max={720} value={dailyMinutes} onChange={(e) => setDailyMinutes(Number(e.target.value))} /></label>
        <button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Kaydediliyor...' : 'Değişiklikleri Kaydet'}</button>
      </form>
    </div>
  );
}
